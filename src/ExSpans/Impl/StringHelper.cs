using System;
using System.Collections.Generic;
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

    }
}
