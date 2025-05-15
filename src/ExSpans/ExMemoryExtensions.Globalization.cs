using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Zyl.ExSpans.Exceptions;
using Zyl.ExSpans.Extensions;
using Zyl.ExSpans.Impl;
#if NETCOREAPP3_0_OR_GREATER
using Zyl.ExSpans.Text;
#endif // NETCOREAPP3_0_OR_GREATER

namespace Zyl.ExSpans {
    partial class ExMemoryExtensions {
        /// <summary>
        /// Indicates whether the specified span contains only white-space characters.
        /// </summary>
        public static bool IsWhiteSpace(this ReadOnlyExSpan<char> span) {
            for (TSize i = (TSize)0; i < span.Length; i+=1) {
                if (!char.IsWhiteSpace(span[i]))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Returns a value indicating whether the specified <paramref name="value"/> occurs within the <paramref name="span"/>.
        /// </summary>
        /// <param name="span">The source span.</param>
        /// <param name="value">The value to seek within the source span.</param>
        /// <param name="comparisonType">One of the enumeration values that determines how the <paramref name="span"/> and <paramref name="value"/> are compared.</param>
        /// <exception cref="ExSpanTooLongException">Throws an exception if the length is out of the range of Int32.</exception>
        public static bool Contains(this ReadOnlyExSpan<char> span, ReadOnlyExSpan<char> value, StringComparison comparisonType) {
            return IndexOf(span, value, comparisonType) >= 0;
        }

        /// <summary>
        /// Determines whether this <paramref name="span"/> and the specified <paramref name="other"/> span have the same characters
        /// when compared using the specified <paramref name="comparisonType"/> option.
        /// </summary>
        /// <param name="span">The source span.</param>
        /// <param name="other">The value to compare with the source span.</param>
        /// <param name="comparisonType">One of the enumeration values that determines how the <paramref name="span"/> and <paramref name="other"/> are compared.</param>
        /// <exception cref="ExSpanTooLongException">Throws an exception if the length is out of the range of Int32.</exception>
        //[Intrinsic] // Unrolled and vectorized for half-constant input (Ordinal)
        public static bool Equals(this ReadOnlyExSpan<char> span, ReadOnlyExSpan<char> other, StringComparison comparisonType) {
#if INTERNAL && TODO
            StringHelper.CheckStringComparison(comparisonType);

            switch (comparisonType) {
                case StringComparison.CurrentCulture:
                case StringComparison.CurrentCultureIgnoreCase:
                    return CultureInfo.CurrentCulture.CompareInfo.Compare(span, other, string.GetCaseCompareOfComparisonCulture(comparisonType)) == 0;

#if NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_0_OR_GREATER || NET20
                case StringComparison.InvariantCulture:
                case StringComparison.InvariantCultureIgnoreCase:
                    return CompareInfo.Invariant.Compare(span, other, string.GetCaseCompareOfComparisonCulture(comparisonType)) == 0;
#endif // NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_0_OR_GREATER || NET20

                case StringComparison.Ordinal:
                    return EqualsOrdinal(span, other);

                default:
                    Debug.Assert(comparisonType == StringComparison.OrdinalIgnoreCase);
                    return EqualsOrdinalIgnoreCase(span, other);
            }
#endif // INTERNAL && TODO
            if (comparisonType == StringComparison.Ordinal) {
                return EqualsOrdinal(span, other);
            }
            ExSpanTooLongException.ThrowIfOutInt32(span.Length);
            ExSpanTooLongException.ThrowIfOutInt32(other.Length);
            return span.AsReadOnlySpan().Equals(other.AsReadOnlySpan(), comparisonType);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool EqualsOrdinal(this ReadOnlyExSpan<char> span, ReadOnlyExSpan<char> value) {
            if (span.Length != value.Length)
                return false;
            if (value.Length == (TSize)0)  // span.Length == value.Length == 0
                return true;
            return span.SequenceEqual(value);
        }

#if INTERNAL && TODO
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool EqualsOrdinalIgnoreCase(this ReadOnlyExSpan<char> span, ReadOnlyExSpan<char> value) {
            if (span.Length != value.Length)
                return false;
            if (value.Length == (TSize)0)  // span.Length == value.Length == 0
                return true;
            return Ordinal.EqualsIgnoreCase(ref ExMemoryMarshal.GetReference(span), ref ExMemoryMarshal.GetReference(value), span.Length);
        }
#endif // INTERNAL && TODO

        /// <summary>
        /// Compares the specified <paramref name="span"/> and <paramref name="other"/> using the specified <paramref name="comparisonType"/>,
        /// and returns an integer that indicates their relative position in the sort order.
        /// </summary>
        /// <param name="span">The source span.</param>
        /// <param name="other">The value to compare with the source span.</param>
        /// <param name="comparisonType">One of the enumeration values that determines how the <paramref name="span"/> and <paramref name="other"/> are compared.</param>
        /// <exception cref="ExSpanTooLongException">Throws an exception if the length is out of the range of Int32.</exception>
        public static int CompareTo(this ReadOnlyExSpan<char> span, ReadOnlyExSpan<char> other, StringComparison comparisonType) {
#if INTERNAL && TODO
            string.CheckStringComparison(comparisonType);
            switch (comparisonType) {
                case StringComparison.CurrentCulture:
                case StringComparison.CurrentCultureIgnoreCase:
                    return CultureInfo.CurrentCulture.CompareInfo.Compare(span, other, string.GetCaseCompareOfComparisonCulture(comparisonType));

                case StringComparison.InvariantCulture:
                case StringComparison.InvariantCultureIgnoreCase:
                    return CompareInfo.Invariant.Compare(span, other, string.GetCaseCompareOfComparisonCulture(comparisonType));

                case StringComparison.Ordinal:
                    if (span.Length == 0 || other.Length == 0)
                        return span.Length - other.Length;
                    return string.CompareOrdinal(span, other);

                default:
                    Debug.Assert(comparisonType == StringComparison.OrdinalIgnoreCase);
                    return Ordinal.CompareStringIgnoreCase(ref ExMemoryMarshal.GetReference(span), span.Length, ref ExMemoryMarshal.GetReference(other), other.Length);
            }
#endif // INTERNAL && TODO
            ExSpanTooLongException.ThrowIfOutInt32(span.Length);
            ExSpanTooLongException.ThrowIfOutInt32(other.Length);
            return span.AsReadOnlySpan().IndexOf(other.AsReadOnlySpan(), comparisonType);
        }

        /// <summary>
        /// Reports the zero-based index of the first occurrence of the specified <paramref name="value"/> in the current <paramref name="span"/>.
        /// </summary>
        /// <param name="span">The source span.</param>
        /// <param name="value">The value to seek within the source span.</param>
        /// <param name="comparisonType">One of the enumeration values that determines how the <paramref name="span"/> and <paramref name="value"/> are compared.</param>
        /// <exception cref="ExSpanTooLongException">Throws an exception if the length is out of the range of Int32.</exception>
        public static TSize IndexOf(this ReadOnlyExSpan<char> span, ReadOnlyExSpan<char> value, StringComparison comparisonType) {
            //string.CheckStringComparison(comparisonType);

            if (comparisonType == StringComparison.Ordinal) {
                return ExSpanHelpers.IndexOf(ref ExMemoryMarshal.GetReference(span), span.Length, ref ExMemoryMarshal.GetReference(value), value.Length);
            }

#if INTERNAL && TODO
            switch (comparisonType) {
                case StringComparison.CurrentCulture:
                case StringComparison.CurrentCultureIgnoreCase:
                    return CultureInfo.CurrentCulture.CompareInfo.IndexOf(span, value, string.GetCaseCompareOfComparisonCulture(comparisonType));

                case StringComparison.InvariantCulture:
                case StringComparison.InvariantCultureIgnoreCase:
                    return CompareInfo.Invariant.IndexOf(span, value, string.GetCaseCompareOfComparisonCulture(comparisonType));

                default:
                    Debug.Assert(comparisonType == StringComparison.OrdinalIgnoreCase);
                    return Ordinal.IndexOfOrdinalIgnoreCase(span, value);
            }
#endif // INTERNAL && TODO
            ExSpanTooLongException.ThrowIfOutInt32(span.Length);
            ExSpanTooLongException.ThrowIfOutInt32(value.Length);
            return span.AsReadOnlySpan().IndexOf(value.AsReadOnlySpan(), comparisonType);
        }

#if NETCOREAPP3_0_OR_GREATER
        /// <summary>
        /// Reports the zero-based index of the last occurrence of the specified <paramref name="value"/> in the current <paramref name="span"/>.
        /// </summary>
        /// <param name="span">The source span.</param>
        /// <param name="value">The value to seek within the source span.</param>
        /// <param name="comparisonType">One of the enumeration values that determines how the <paramref name="span"/> and <paramref name="value"/> are compared.</param>
        /// <exception cref="ExSpanTooLongException">Throws an exception if the length is out of the range of Int32.</exception>
        public static TSize LastIndexOf(this ReadOnlyExSpan<char> span, ReadOnlyExSpan<char> value, StringComparison comparisonType) {
            //string.CheckStringComparison(comparisonType);

            if (comparisonType == StringComparison.Ordinal) {
                return ExSpanHelpers.LastIndexOf(
                    ref ExMemoryMarshal.GetReference(span),
                    span.Length,
                    ref ExMemoryMarshal.GetReference(value),
                    value.Length);
            }

#if INTERNAL && TODO
            switch (comparisonType) {
                case StringComparison.CurrentCulture:
                case StringComparison.CurrentCultureIgnoreCase:
                    return CultureInfo.CurrentCulture.CompareInfo.LastIndexOf(span, value, string.GetCaseCompareOfComparisonCulture(comparisonType));

                case StringComparison.InvariantCulture:
                case StringComparison.InvariantCultureIgnoreCase:
                    return CompareInfo.Invariant.LastIndexOf(span, value, string.GetCaseCompareOfComparisonCulture(comparisonType));

                default:
                    Debug.Assert(comparisonType == StringComparison.OrdinalIgnoreCase);
                    return Ordinal.LastIndexOfOrdinalIgnoreCase(span, value);
            }
#endif // INTERNAL && TODO
            ExSpanTooLongException.ThrowIfOutInt32(span.Length);
            ExSpanTooLongException.ThrowIfOutInt32(value.Length);
            return span.AsReadOnlySpan().LastIndexOf(value.AsReadOnlySpan(), comparisonType);
        }
#endif // NETCOREAPP3_0_OR_GREATER

        /// <summary>
        /// Copies the characters from the source span into the destination, converting each character to lowercase,
        /// using the casing rules of the specified culture.
        /// </summary>
        /// <param name="source">The source span.</param>
        /// <param name="destination">The destination span which contains the transformed characters.</param>
        /// <param name="culture">An object that supplies culture-specific casing rules.</param>
        /// <remarks>If <paramref name="culture"/> is null, <see cref="CultureInfo.CurrentCulture"/> will be used.</remarks>
        /// <returns>The number of characters written into the destination span. If the destination is too small, returns -1.</returns>
        /// <exception cref="InvalidOperationException">The source and destination buffers overlap.</exception>
        /// <exception cref="ExSpanTooLongException">Throws an exception if the length is out of the range of Int32.</exception>
        public static TSize ToLower(this ReadOnlyExSpan<char> source, ExSpan<char> destination, CultureInfo? culture) {
            if (source.Overlaps(destination))
                ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_SpanOverlappedOperation);

            culture ??= CultureInfo.CurrentCulture;

            // Assuming that changing case does not affect length
            if (destination.Length < source.Length)
                return -1;

#if INTERNAL && TODO
            if (GlobalizationMode.Invariant)
                InvariantModeCasing.ToLower(source, destination);
            else
                culture.TextInfo.ChangeCaseToLower(source, destination);
            return source.Length;
#endif // INTERNAL && TODO
            ExSpanTooLongException.ThrowIfOutInt32(source.Length);
            ExSpanTooLongException.ThrowIfOutInt32(destination.Length);
            return source.AsReadOnlySpan().ToLower(destination.AsSpan(), culture);
        }

        /// <summary>
        /// Copies the characters from the source span into the destination, converting each character to lowercase,
        /// using the casing rules of the invariant culture.
        /// </summary>
        /// <param name="source">The source span.</param>
        /// <param name="destination">The destination span which contains the transformed characters.</param>
        /// <returns>The number of characters written into the destination span. If the destination is too small, returns -1.</returns>
        /// <exception cref="InvalidOperationException">The source and destination buffers overlap.</exception>
        /// <exception cref="ExSpanTooLongException">Throws an exception if the length is out of the range of Int32.</exception>
        public static TSize ToLowerInvariant(this ReadOnlyExSpan<char> source, ExSpan<char> destination) {
            if (source.Overlaps(destination))
                ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_SpanOverlappedOperation);

            // Assuming that changing case does not affect length
            if (destination.Length < source.Length)
                return -1;

#if INTERNAL && TODO
            if (GlobalizationMode.Invariant)
                InvariantModeCasing.ToLower(source, destination);
            else
                TextInfo.Invariant.ChangeCaseToLower(source, destination);
            return source.Length;
#endif // INTERNAL && TODO
            ExSpanTooLongException.ThrowIfOutInt32(source.Length);
            ExSpanTooLongException.ThrowIfOutInt32(destination.Length);
            return source.AsReadOnlySpan().ToLowerInvariant(destination.AsSpan());
        }

        /// <summary>
        /// Copies the characters from the source span into the destination, converting each character to uppercase,
        /// using the casing rules of the specified culture.
        /// </summary>
        /// <param name="source">The source span.</param>
        /// <param name="destination">The destination span which contains the transformed characters.</param>
        /// <param name="culture">An object that supplies culture-specific casing rules.</param>
        /// <remarks>If <paramref name="culture"/> is null, <see cref="CultureInfo.CurrentCulture"/> will be used.</remarks>
        /// <returns>The number of characters written into the destination span. If the destination is too small, returns -1.</returns>
        /// <exception cref="InvalidOperationException">The source and destination buffers overlap.</exception>
        /// <exception cref="ExSpanTooLongException">Throws an exception if the length is out of the range of Int32.</exception>
        public static TSize ToUpper(this ReadOnlyExSpan<char> source, ExSpan<char> destination, CultureInfo? culture) {
            if (source.Overlaps(destination))
                ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_SpanOverlappedOperation);

            culture ??= CultureInfo.CurrentCulture;

            // Assuming that changing case does not affect length
            if (destination.Length < source.Length)
                return -1;

#if INTERNAL && TODO
            if (GlobalizationMode.Invariant)
                InvariantModeCasing.ToUpper(source, destination);
            else
                culture.TextInfo.ChangeCaseToUpper(source, destination);
            return source.Length;
#endif // INTERNAL && TODO
            ExSpanTooLongException.ThrowIfOutInt32(source.Length);
            ExSpanTooLongException.ThrowIfOutInt32(destination.Length);
            return source.AsReadOnlySpan().ToUpper(destination.AsSpan(), culture);
        }

        /// <summary>
        /// Copies the characters from the source span into the destination, converting each character to uppercase
        /// using the casing rules of the invariant culture.
        /// </summary>
        /// <param name="source">The source span.</param>
        /// <param name="destination">The destination span which contains the transformed characters.</param>
        /// <returns>The number of characters written into the destination span. If the destination is too small, returns -1.</returns>
        /// <exception cref="InvalidOperationException">The source and destination buffers overlap.</exception>
        /// <exception cref="ExSpanTooLongException">Throws an exception if the length is out of the range of Int32.</exception>
        public static TSize ToUpperInvariant(this ReadOnlyExSpan<char> source, ExSpan<char> destination) {
            if (source.Overlaps(destination))
                ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_SpanOverlappedOperation);

            // Assuming that changing case does not affect length
            if (destination.Length < source.Length)
                return -1;

#if INTERNAL && TODO
            if (GlobalizationMode.Invariant)
                InvariantModeCasing.ToUpper(source, destination);
            else
                TextInfo.Invariant.ChangeCaseToUpper(source, destination);
            return source.Length;
#endif // INTERNAL && TODO
            ExSpanTooLongException.ThrowIfOutInt32(source.Length);
            ExSpanTooLongException.ThrowIfOutInt32(destination.Length);
            return source.AsReadOnlySpan().ToUpperInvariant(destination.AsSpan());
        }

        /// <summary>
        /// Determines whether the end of the <paramref name="span"/> matches the specified <paramref name="value"/> when compared using the specified <paramref name="comparisonType"/> option.
        /// </summary>
        /// <param name="span">The source span.</param>
        /// <param name="value">The sequence to compare to the end of the source span.</param>
        /// <param name="comparisonType">One of the enumeration values that determines how the <paramref name="span"/> and <paramref name="value"/> are compared.</param>
        /// <exception cref="ExSpanTooLongException">Throws an exception if the length is out of the range of Int32.</exception>
        //[Intrinsic] // Unrolled and vectorized for half-constant input (Ordinal)
        public static bool EndsWith(this ReadOnlyExSpan<char> span, ReadOnlyExSpan<char> value, StringComparison comparisonType) {
#if INTERNAL && TODO
            string.CheckStringComparison(comparisonType);

            switch (comparisonType) {
                case StringComparison.CurrentCulture:
                case StringComparison.CurrentCultureIgnoreCase:
                    return CultureInfo.CurrentCulture.CompareInfo.IsSuffix(span, value, string.GetCaseCompareOfComparisonCulture(comparisonType));

                case StringComparison.InvariantCulture:
                case StringComparison.InvariantCultureIgnoreCase:
                    return CompareInfo.Invariant.IsSuffix(span, value, string.GetCaseCompareOfComparisonCulture(comparisonType));

                case StringComparison.Ordinal:
                    return span.EndsWith(value);

                default:
                    Debug.Assert(comparisonType == StringComparison.OrdinalIgnoreCase);
                    return span.EndsWithOrdinalIgnoreCase(value);
            }
#endif // INTERNAL && TODO
            if (comparisonType == StringComparison.Ordinal) {
                return span.EndsWith(value);
            }
            ExSpanTooLongException.ThrowIfOutInt32(span.Length);
            ExSpanTooLongException.ThrowIfOutInt32(value.Length);
            return span.AsReadOnlySpan().EndsWith(value.AsReadOnlySpan(), comparisonType);
        }

#if INTERNAL && TODO
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool EndsWithOrdinalIgnoreCase(this ReadOnlyExSpan<char> span, ReadOnlyExSpan<char> value)
            => value.Length <= span.Length
            && Ordinal.EqualsIgnoreCase(
                ref Unsafe.Add(ref ExMemoryMarshal.GetReference(span), span.Length - value.Length),
                ref ExMemoryMarshal.GetReference(value),
                value.Length);
#endif // INTERNAL && TODO

        /// <summary>
        /// Determines whether the beginning of the <paramref name="span"/> matches the specified <paramref name="value"/> when compared using the specified <paramref name="comparisonType"/> option.
        /// </summary>
        /// <param name="span">The source span.</param>
        /// <param name="value">The sequence to compare to the beginning of the source span.</param>
        /// <param name="comparisonType">One of the enumeration values that determines how the <paramref name="span"/> and <paramref name="value"/> are compared.</param>
        /// <exception cref="ExSpanTooLongException">Throws an exception if the length is out of the range of Int32.</exception>
        //[Intrinsic] // Unrolled and vectorized for half-constant input (Ordinal)
        public static bool StartsWith(this ReadOnlyExSpan<char> span, ReadOnlyExSpan<char> value, StringComparison comparisonType) {
#if INTERNAL && TODO
            string.CheckStringComparison(comparisonType);

            switch (comparisonType) {
                case StringComparison.CurrentCulture:
                case StringComparison.CurrentCultureIgnoreCase:
                    return CultureInfo.CurrentCulture.CompareInfo.IsPrefix(span, value, string.GetCaseCompareOfComparisonCulture(comparisonType));

                case StringComparison.InvariantCulture:
                case StringComparison.InvariantCultureIgnoreCase:
                    return CompareInfo.Invariant.IsPrefix(span, value, string.GetCaseCompareOfComparisonCulture(comparisonType));

                case StringComparison.Ordinal:
                    return span.StartsWith(value);

                default:
                    Debug.Assert(comparisonType == StringComparison.OrdinalIgnoreCase);
                    return span.StartsWithOrdinalIgnoreCase(value);
            }
#endif // INTERNAL && TODO
            if (comparisonType == StringComparison.Ordinal) {
                return span.StartsWith(value);
            }
            ExSpanTooLongException.ThrowIfOutInt32(span.Length);
            ExSpanTooLongException.ThrowIfOutInt32(value.Length);
            return span.AsReadOnlySpan().StartsWith(value.AsReadOnlySpan(), comparisonType);
        }

#if INTERNAL && TODO
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool StartsWithOrdinalIgnoreCase(this ReadOnlyExSpan<char> span, ReadOnlyExSpan<char> value)
            => value.Length <= span.Length
            && Ordinal.EqualsIgnoreCase(ref ExMemoryMarshal.GetReference(span), ref ExMemoryMarshal.GetReference(value), value.Length);
#endif // INTERNAL && TODO

#if NETCOREAPP3_0_OR_GREATER
        /// <summary>
        /// Returns an enumeration of <see cref="Rune"/> from the provided span.
        /// </summary>
        /// <remarks>
        /// Invalid sequences will be represented in the enumeration by <see cref="Rune.ReplacementChar"/>.
        /// </remarks>
        public static ExSpanRuneEnumerator EnumerateRunes(this ReadOnlyExSpan<char> span) {
            return new ExSpanRuneEnumerator(span);
        }

        /// <summary>
        /// Returns an enumeration of <see cref="Rune"/> from the provided span.
        /// </summary>
        /// <remarks>
        /// Invalid sequences will be represented in the enumeration by <see cref="Rune.ReplacementChar"/>.
        /// </remarks>
        [OverloadResolutionPriority(-1)]
        public static ExSpanRuneEnumerator EnumerateRunes(this ExSpan<char> span) {
            return new ExSpanRuneEnumerator(span);
        }

        /// <summary>
        /// Returns an enumeration of lines over the provided span.
        /// </summary>
        /// <remarks>
        /// It is recommended that protocol parsers not utilize this API. See the documentation
        /// for <see cref="string.ReplaceLineEndings()"/> for more information on how newline
        /// sequences are detected.
        /// </remarks>
        public static ExSpanLineEnumerator EnumerateLines(this ReadOnlyExSpan<char> span) {
            return new ExSpanLineEnumerator(span);
        }

        /// <summary>
        /// Returns an enumeration of lines over the provided span.
        /// </summary>
        /// <remarks>
        /// It is recommended that protocol parsers not utilize this API. See the documentation
        /// for <see cref="string.ReplaceLineEndings()"/> for more information on how newline
        /// sequences are detected.
        /// </remarks>
        [OverloadResolutionPriority(-1)]
        public static ExSpanLineEnumerator EnumerateLines(this ExSpan<char> span) {
            return new ExSpanLineEnumerator(span);
        }
#endif // NETCOREAPP3_0_OR_GREATER

    }
}
