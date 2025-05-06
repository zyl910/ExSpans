using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Zyl.ExSpans.Impl;
using Zyl.ExSpans.Reflection;

namespace Zyl.ExSpans {

    /// <summary>Provides a handler used by the language compiler to format interpolated strings into character ExSpans.</summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [InterpolatedStringHandler]
    public ref struct ExTryWriteInterpolatedStringHandler {
        // Implementation note:
        // As this type is only intended to be targeted by the compiler, public APIs eschew argument validation logic
        // in a variety of places, e.g. allowing a null input when one isn't expected to produce a NullReferenceException rather
        // than an ArgumentNullException.

        /// <summary>The destination buffer.</summary>
        private readonly ExSpan<char> _destination;
        /// <summary>Optional provider to pass to IFormattable.ToString or ISpanFormattable.TryFormat calls.</summary>
        private readonly IFormatProvider? _provider;
        /// <summary>The number of characters written to <see cref="_destination"/>.</summary>
        internal TSize _pos;
        /// <summary>true if all formatting operations have succeeded; otherwise, false.</summary>
        internal bool _success;
        /// <summary>Whether <see cref="_provider"/> provides an ICustomFormatter.</summary>
        /// <remarks>
        /// Custom formatters are very rare.  We want to support them, but it's ok if we make them more expensive
        /// in order to make them as pay-for-play as possible.  So, we avoid adding another reference type field
        /// to reduce the size of the handler and to reduce required zero'ing, by only storing whether the provider
        /// provides a formatter, rather than actually storing the formatter.  This in turn means, if there is a
        /// formatter, we pay for the extra interface call on each AppendFormatted that needs it.
        /// </remarks>
        private readonly bool _hasCustomFormatter;

        /// <summary>Creates a handler used to write an interpolated string into a <see cref="ExSpan{Char}"/>.</summary>
        /// <param name="literalLength">The number of constant characters outside of interpolation expressions in the interpolated string.</param>
        /// <param name="formattedCount">The number of interpolation expressions in the interpolated string.</param>
        /// <param name="destination">The destination buffer.</param>
        /// <param name="shouldAppend">Upon return, true if the destination may be long enough to support the formatting, or false if it won't be.</param>
        /// <remarks>This is intended to be called only by compiler-generated code. Arguments are not validated as they'd otherwise be for members intended to be used directly.</remarks>
        public ExTryWriteInterpolatedStringHandler(int literalLength, int formattedCount, ExSpan<char> destination, out bool shouldAppend) {
            _destination = destination;
            _provider = null;
            _pos = 0;
            _success = shouldAppend = destination.Length >= literalLength;
            _hasCustomFormatter = false;
        }

        /// <summary>Creates a handler used to write an interpolated string into a <see cref="ExSpan{Char}"/>.</summary>
        /// <param name="literalLength">The number of constant characters outside of interpolation expressions in the interpolated string.</param>
        /// <param name="formattedCount">The number of interpolation expressions in the interpolated string.</param>
        /// <param name="destination">The destination buffer.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="shouldAppend">Upon return, true if the destination may be long enough to support the formatting, or false if it won't be.</param>
        /// <remarks>This is intended to be called only by compiler-generated code. Arguments are not validated as they'd otherwise be for members intended to be used directly.</remarks>
        public ExTryWriteInterpolatedStringHandler(int literalLength, int formattedCount, ExSpan<char> destination, IFormatProvider? provider, out bool shouldAppend) {
            _destination = destination;
            _provider = provider;
            _pos = 0;
            _success = shouldAppend = destination.Length >= literalLength;
            _hasCustomFormatter = provider is not null
#if TODO
                && DefaultInterpolatedStringHandler.HasCustomFormatter(provider)
#endif // TODO
                ;
        }

        /// <summary>Writes the specified string to the handler.</summary>
        /// <param name="value">The string to write.</param>
        /// <returns>true if the value could be formatted to the span; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AppendLiteral(string value) {
            if (value.TryCopyTo(_destination.Slice(_pos))) {
                _pos += value.Length;
                return true;
            }

            return Fail();
        }

        #region AppendFormatted
        // Design note:
        // This provides the same set of overloads and semantics as DefaultInterpolatedStringHandler.

        #region AppendFormatted T
        /// <summary>Writes the specified value to the handler.</summary>
        /// <param name="value">The value to write.</param>
        /// <typeparam name="T">The type of the value to write.</typeparam>
        public bool AppendFormatted<T>(T value) {
            // This method could delegate to AppendFormatted with a null format, but explicitly passing
            // default as the format to TryFormat helps to improve code quality in some cases when TryFormat is inlined,
            // e.g. for Int32 it enables the JIT to eliminate code in the inlined method based on a length check on the format.

            // If there's a custom formatter, always use it.
            if (_hasCustomFormatter) {
                return AppendCustomFormatter(value, format: null);
            }

            // Check first for IFormattable, even though we'll prefer to use ISpanFormattable, as the latter
            // derives from the former.  For value types, it won't matter as the type checks devolve into
            // JIT-time constants.  For reference types, they're more likely to implement IFormattable
            // than they are to implement ISpanFormattable: if they don't implement either, we save an
            // interface check over first checking for ISpanFormattable and then for IFormattable, and
            // if it only implements IFormattable, we come out even: only if it implements both do we
            // end up paying for an extra interface check.
            string? s;
            if (value is IFormattable) {
                // If the value can format itself directly into our buffer, do so.

                if (TypeHelper.IsEnum(typeof(T))) {
                    if (EnumHelper.TryFormatUnconstrained(value, _destination.Slice(_pos).AsSpan(), out int charsWritten)) {
                        _pos += charsWritten;
                        return true;
                    }

                    return Fail();
                }

#if NET6_0_OR_GREATER
                if (value is ISpanFormattable) {
                    if (((ISpanFormattable)value).TryFormat(_destination.Slice(_pos).AsSpan(), out int charsWritten, default, _provider)) // constrained call avoiding boxing for value types
                    {
                        _pos += charsWritten;
                        return true;
                    }

                    return Fail();
                }
#endif // NET6_0_OR_GREATER

                s = ((IFormattable)value).ToString(format: null, _provider); // constrained call avoiding boxing for value types
            } else {
                s = value?.ToString();
            }

            return s is null || AppendLiteral(s);
        }

        /// <summary>Writes the specified value to the handler.</summary>
        /// <param name="value">The value to write.</param>
        /// <param name="format">The format string.</param>
        /// <typeparam name="T">The type of the value to write.</typeparam>
        public bool AppendFormatted<T>(T value, string? format) {
            // If there's a custom formatter, always use it.
            if (_hasCustomFormatter) {
                return AppendCustomFormatter(value, format);
            }

            // Check first for IFormattable, even though we'll prefer to use ISpanFormattable, as the latter
            // derives from the former.  For value types, it won't matter as the type checks devolve into
            // JIT-time constants.  For reference types, they're more likely to implement IFormattable
            // than they are to implement ISpanFormattable: if they don't implement either, we save an
            // interface check over first checking for ISpanFormattable and then for IFormattable, and
            // if it only implements IFormattable, we come out even: only if it implements both do we
            // end up paying for an extra interface check.
            string? s;
            if (value is IFormattable) {
                // If the value can format itself directly into our buffer, do so.

                if (TypeHelper.IsEnum(typeof(T))) {
                    if (EnumHelper.TryFormatUnconstrained(value, _destination.Slice(_pos).AsSpan(), out int charsWritten, (format is not null) ? format.AsSpan() : default)) {
                        _pos += charsWritten;
                        return true;
                    }

                    return Fail();
                }

#if NET6_0_OR_GREATER
                if (value is ISpanFormattable) {
                    if (((ISpanFormattable)value).TryFormat(_destination.Slice(_pos).AsSpan(), out int charsWritten, format, _provider)) // constrained call avoiding boxing for value types
                    {
                        _pos += charsWritten;
                        return true;
                    }

                    return Fail();
                }
#endif // NET6_0_OR_GREATER

                s = ((IFormattable)value).ToString(format, _provider); // constrained call avoiding boxing for value types
            } else {
                s = value?.ToString();
            }

            return s is null || AppendLiteral(s);
        }

        /// <summary>Writes the specified value to the handler.</summary>
        /// <param name="value">The value to write.</param>
        /// <param name="alignment">Minimum number of characters that should be written for this value.  If the value is negative, it indicates left-aligned and the required minimum is the absolute value.</param>
        /// <typeparam name="T">The type of the value to write.</typeparam>
        public bool AppendFormatted<T>(T value, int alignment) {
            TSize startingPos = _pos;
            if (AppendFormatted(value)) {
                return alignment == 0 || TryAppendOrInsertAlignmentIfNeeded(startingPos, alignment);
            }

            return Fail();
        }

        /// <summary>Writes the specified value to the handler.</summary>
        /// <param name="value">The value to write.</param>
        /// <param name="format">The format string.</param>
        /// <param name="alignment">Minimum number of characters that should be written for this value.  If the value is negative, it indicates left-aligned and the required minimum is the absolute value.</param>
        /// <typeparam name="T">The type of the value to write.</typeparam>
        public bool AppendFormatted<T>(T value, int alignment, string? format) {
            TSize startingPos = _pos;
            if (AppendFormatted(value, format)) {
                return alignment == 0 || TryAppendOrInsertAlignmentIfNeeded(startingPos, alignment);
            }

            return Fail();
        }
        #endregion

        #region AppendFormatted ReadOnlyExSpan<char>
        /// <summary>Writes the specified character ExSpan to the handler.</summary>
        /// <param name="value">The span to write.</param>
        public bool AppendFormatted(scoped ReadOnlyExSpan<char> value) {
            // Fast path for when the value fits in the current buffer
            if (value.TryCopyTo(_destination.Slice(_pos))) {
                _pos += value.Length;
                return true;
            }

            return Fail();
        }

        /// <summary>Writes the specified string of chars to the handler.</summary>
        /// <param name="value">The span to write.</param>
        /// <param name="alignment">Minimum number of characters that should be written for this value.  If the value is negative, it indicates left-aligned and the required minimum is the absolute value.</param>
        /// <param name="format">The format string.</param>
        public bool AppendFormatted(scoped ReadOnlyExSpan<char> value, int alignment = 0, string? format = null) {
            bool leftAlign = false;
            if (alignment < 0) {
                leftAlign = true;
                alignment = -alignment;
            }

            TSize paddingRequired = alignment - value.Length;
            if (paddingRequired <= 0) {
                // The value is as large or larger than the required amount of padding,
                // so just write the value.
                return AppendFormatted(value);
            }

            // Write the value along with the appropriate padding.
            Debug.Assert(alignment > value.Length);
            if (alignment <= _destination.Length - _pos) {
                if (leftAlign) {
                    value.CopyTo(_destination.Slice(_pos));
                    _pos += value.Length;
                    _destination.Slice(_pos, paddingRequired).Fill(' ');
                    _pos += paddingRequired;
                } else {
                    _destination.Slice(_pos, paddingRequired).Fill(' ');
                    _pos += paddingRequired;
                    value.CopyTo(_destination.Slice(_pos));
                    _pos += value.Length;
                }

                return true;
            }

            return Fail();
        }
        #endregion

        #region AppendFormatted string
        /// <summary>Writes the specified value to the handler.</summary>
        /// <param name="value">The value to write.</param>
        public bool AppendFormatted(string? value) {
            if (_hasCustomFormatter) {
                return AppendCustomFormatter(value, format: null);
            }

            if (value is null) {
                return true;
            }

            if (value.TryCopyTo(_destination.Slice(_pos))) {
                _pos += value.Length;
                return true;
            }

            return Fail();
        }

        /// <summary>Writes the specified value to the handler.</summary>
        /// <param name="value">The value to write.</param>
        /// <param name="alignment">Minimum number of characters that should be written for this value.  If the value is negative, it indicates left-aligned and the required minimum is the absolute value.</param>
        /// <param name="format">The format string.</param>
        public bool AppendFormatted(string? value, int alignment = 0, string? format = null) =>
            // Format is meaningless for strings and doesn't make sense for someone to specify.  We have the overload
            // simply to disambiguate between ROS<char> and object, just in case someone does specify a format, as
            // string is implicitly convertible to both. Just delegate to the T-based implementation.
            AppendFormatted<string?>(value, alignment, format);
        #endregion

        #region AppendFormatted object
        /// <summary>Writes the specified value to the handler.</summary>
        /// <param name="value">The value to write.</param>
        /// <param name="alignment">Minimum number of characters that should be written for this value.  If the value is negative, it indicates left-aligned and the required minimum is the absolute value.</param>
        /// <param name="format">The format string.</param>
        public bool AppendFormatted(object? value, int alignment = 0, string? format = null) =>
            // This overload is expected to be used rarely, only if either a) something strongly typed as object is
            // formatted with both an alignment and a format, or b) the compiler is unable to target type to T. It
            // exists purely to help make cases from (b) compile. Just delegate to the T-based implementation.
            AppendFormatted<object?>(value, alignment, format);
        #endregion
        #endregion

        /// <summary>Formats the value using the custom formatter from the provider.</summary>
        /// <param name="value">The value to write.</param>
        /// <param name="format">The format string.</param>
        /// <typeparam name="T">The type of the value to write.</typeparam>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private bool AppendCustomFormatter<T>(T value, string? format) {
            // This case is very rare, but we need to handle it prior to the other checks in case
            // a provider was used that supplied an ICustomFormatter which wanted to intercept the particular value.
            // We do the cast here rather than in the ctor, even though this could be executed multiple times per
            // formatting, to make the cast pay for play.
            Debug.Assert(_hasCustomFormatter);
            Debug.Assert(_provider != null);

            ICustomFormatter? formatter = (ICustomFormatter?)_provider!.GetFormat(typeof(ICustomFormatter));
            Debug.Assert(formatter != null, "An incorrectly written provider said it implemented ICustomFormatter, and then didn't");

            if (formatter is not null && formatter.Format(format, value, _provider) is string customFormatted) {
                return AppendLiteral(customFormatted);
            }

            return true;
        }

        /// <summary>Handles adding any padding required for aligning a formatted value in an interpolation expression.</summary>
        /// <param name="startingPos">The position at which the written value started.</param>
        /// <param name="alignment">Non-zero minimum number of characters that should be written for this value.  If the value is negative, it indicates left-aligned and the required minimum is the absolute value.</param>
        private bool TryAppendOrInsertAlignmentIfNeeded(TSize startingPos, int alignment) {
            Debug.Assert(startingPos >= 0 && startingPos <= _pos);
            Debug.Assert(alignment != 0);

            TSize charsWritten = _pos - startingPos;

            bool leftAlign = false;
            if (alignment < 0) {
                leftAlign = true;
                alignment = -alignment;
            }

            TSize paddingNeeded = alignment - charsWritten;
            if (paddingNeeded <= 0) {
                return true;
            }

            if (paddingNeeded <= _destination.Length - _pos) {
                if (leftAlign) {
                    _destination.Slice(_pos, paddingNeeded).Fill(' ');
                } else {
                    _destination.Slice(startingPos, charsWritten).CopyTo(_destination.Slice(startingPos + paddingNeeded));
                    _destination.Slice(startingPos, paddingNeeded).Fill(' ');
                }

                _pos += paddingNeeded;
                return true;
            }

            return Fail();
        }

        /// <summary>Marks formatting as having failed and returns false.</summary>
        private bool Fail() {
            _success = false;
            return false;
        }
    }

}
