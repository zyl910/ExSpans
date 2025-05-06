using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Zyl.ExSpans.Impl {
    /// <summary>
    /// <see cref="Enum"/> Helper.
    /// </summary>
    public static class EnumHelper {

#if NET9_0_OR_GREATER
        [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = nameof(TryFormatUnconstrained))]
        internal extern static bool CallTryFormatUnconstrained<TEnum>(Enum? athis, TEnum value, Span<char> destination, out int charsWritten, [StringSyntax(StringSyntaxAttribute.EnumFormat)] ReadOnlySpan<char> format);
#endif // NET9_0_OR_GREATER

        private static bool TryFormat_UseFormate<TEnum>(TEnum value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default) {
            string formatString = format.ToString();
            string text = Enum.Format(typeof(TEnum), value!, formatString);
            bool flag = text.AsSpan().TryCopyTo(destination);
            if (flag) {
                charsWritten = text.Length;
            } else {
                charsWritten = 0;
            }
            return flag;
        }

        /// <summary>Tries to format the value of the enumerated type instance into the provided span of characters.</summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="value"></param>
        /// <param name="destination">The span into which to write the instance's value formatted as a span of characters.</param>
        /// <param name="charsWritten">When this method returns, contains the number of characters that were written in <paramref name="destination"/>.</param>
        /// <param name="format">A span containing the character that represents the standard format string that defines the acceptable format of destination. This may be empty, or "g", "d", "f", or "x".</param>
        /// <returns><see langword="true"/> if the formatting was successful; otherwise, <see langword="false"/> if the destination span wasn't large enough to contain the formatted value.</returns>
        /// <exception cref="FormatException">The format parameter contains an invalid value.</exception>
        public static bool TryFormat<TEnum>(TEnum value, Span<char> destination, out int charsWritten,
#if NET7_0_OR_GREATER
            [StringSyntax(StringSyntaxAttribute.EnumFormat)]
#endif // NET7_0_OR_GREATER
            ReadOnlySpan<char> format = default) where TEnum : struct {
#if NET8_0_OR_GREATER
            return Enum.TryFormat(value, destination, out charsWritten, format);
#else
            return TryFormat_UseFormate(value, destination, out charsWritten, format);
#endif // NET8_0_OR_GREATER
        }

        /// <summary>Tries to format the value of the enumerated type instance into the provided span of characters.</summary>
        /// <remarks>
        /// This is same as the implementation for <see cref="TryFormat"/>. It is separated out as <see cref="TryFormat"/> has constrains on the TEnum,
        /// and we internally want to use this method in cases where we dynamically validate a generic T is an enum rather than T implementing
        /// those constraints. It's a manual copy/paste right now to avoid pressure on the JIT's inlining mechanisms.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] // format is most frequently a constant, and we want it exposed to the implementation; this should be inlined automatically, anyway
        internal static bool TryFormatUnconstrained<TEnum>(TEnum value, Span<char> destination, out int charsWritten,
#if NET7_0_OR_GREATER
            [StringSyntax(StringSyntaxAttribute.EnumFormat)]
#endif // NET7_0_OR_GREATER
            ReadOnlySpan<char> format = default) {
#if NET9_0_OR_GREATER
            return CallTryFormatUnconstrained(null, value, destination, out charsWritten, format);
#else
            return TryFormat_UseFormate(value, destination, out charsWritten, format);
#endif // NET9_0_OR_GREATER
        }

    }
}
