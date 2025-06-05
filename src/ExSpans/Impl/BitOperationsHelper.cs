using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Zyl.ExSpans.Impl {
    /// <summary>
    /// <see cref="System.Numerics.BitOperations"/> Helper.
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

        /// <summary>
        /// Flip the bit at a specific position in a given value.
        /// Similar in behavior to the x86 instruction BTC (Bit Test and Complement).
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="index">The zero-based index of the bit to flip.
        /// Any value outside the range [0..31] is treated as congruent mod 32.</param>
        /// <returns>The new value.</returns>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint FlipBit(uint value, int index) {
            return value ^ (1u << index);
        }

        /// <summary>
        /// Flip the bit at a specific position in a given value.
        /// Similar in behavior to the x86 instruction BTC (Bit Test and Complement).
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="index">The zero-based index of the bit to flip.
        /// Any value outside the range [0..63] is treated as congruent mod 64.</param>
        /// <returns>The new value.</returns>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong FlipBit(ulong value, int index) {
            return value ^ (1ul << index);
        }
    }
}
