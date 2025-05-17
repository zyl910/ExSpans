using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Zyl.ExSpans.Impl {
    /// <summary>
    /// <see cref="BitOperations"/> Helper.
    /// </summary>
    public static class BitOperationsHelper {

        /// <summary>
        /// Reset the lowest significant bit in the given value
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>Returns reset the lowest significant bit in the given value.</returns>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ResetLowestSetBit(uint value) {
            // It's lowered to BLSR on x86
            return value & (value - 1);
        }

        /// <inheritdoc cref="ResetLowestSetBit(uint)"/>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong ResetLowestSetBit(ulong value) {
            // It's lowered to BLSR on x86
            return value & (value - 1);
        }

    }
}
