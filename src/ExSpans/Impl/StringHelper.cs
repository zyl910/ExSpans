using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace Zyl.ExSpans.Impl {
    /// <summary>
    /// <see cref="String"/> Helper.
    /// </summary>
    public static class StringHelper {

        /// <summary>
        /// Check <see cref="StringComparison"/>.
        /// </summary>
        /// <param name="comparisonType">Source comparisonType.</param>
        public static void CheckStringComparison(StringComparison comparisonType) {
            // Single comparison to check if comparisonType is within [CurrentCulture .. OrdinalIgnoreCase]
            if ((uint)comparisonType > (uint)StringComparison.OrdinalIgnoreCase) {
                throw new ArgumentOutOfRangeException(nameof(comparisonType));
            }
        }

        /// <summary>Gets whether the provider provides a custom formatter.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] // only used in a few hot path call sites
        public static bool HasCustomFormatter(IFormatProvider provider) {
            // From: DefaultInterpolatedStringHandler.HasCustomFormatter(provider)
            Debug.Assert(provider is not null);
            Debug.Assert(provider is not CultureInfo || provider.GetFormat(typeof(ICustomFormatter)) is null, "Expected CultureInfo to not provide a custom formatter");
            return
                provider!.GetType() != typeof(CultureInfo) && // optimization to avoid GetFormat in the majority case
                provider.GetFormat(typeof(ICustomFormatter)) != null;
        }
    }
}
