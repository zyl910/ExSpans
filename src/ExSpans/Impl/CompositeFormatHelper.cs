using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

namespace Zyl.ExSpans.Impl {
#if NET8_0_OR_GREATER

    /// <summary>
    /// <see cref="CompositeFormat"/> Helper.
    /// </summary>
    public static class CompositeFormatHelper {

        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_formattedCount")]
        internal extern static ref int Getter_formattedCount(CompositeFormat format);
        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_literalLength")]
        internal extern static ref int Getter_literalLength(CompositeFormat format);
        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_segments")]
        internal extern static ref (string? Literal, int ArgIndex, int Alignment, string? Format)[] Getter_segments(CompositeFormat format);

        [UnsafeAccessor(UnsafeAccessorKind.Method, Name = nameof(ValidateNumberOfArgs))]
        internal extern static bool CallValidateNumberOfArgs(CompositeFormat format, int numArgs);

        /// <summary>
        /// Get _formattedCount value.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <returns>Returns value.</returns>
        public static int GetFormattedCount(CompositeFormat format) {
            return Getter_formattedCount(format);
        }

        /// <summary>
        /// Get _literalLength value.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <returns>Returns value.</returns>
        public static int GetLiteralLength(CompositeFormat format) {
            return Getter_literalLength(format);
        }

        /// <summary>
        /// Get _segments value.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <returns>Returns value.</returns>
        public static (string? Literal, int ArgIndex, int Alignment, string? Format)[] GetSegments(CompositeFormat format) {
            return Getter_segments(format);
        }

        /// <summary>Throws an exception if the specified number of arguments is fewer than the number required.</summary>
        /// <param name="format">The format.</param>
        /// <param name="numArgs">The number of arguments provided by the caller.</param>
        /// <exception cref="FormatException">An insufficient number of arguments were provided.</exception>
        public static void ValidateNumberOfArgs(CompositeFormat format, int numArgs) {
            CallValidateNumberOfArgs(format, numArgs);
        }
    }

#endif // NET8_0_OR_GREATER
}
