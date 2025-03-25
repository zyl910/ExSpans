using System;
using System.Collections.Generic;
#if NETCOREAPP3_0_OR_GREATER
using System.Runtime.Intrinsics;
#endif // NETCOREAPP3_0_OR_GREATER
#if NET5_0_OR_GREATER
using System.Runtime.Intrinsics.Arm;
#endif // NET5_0_OR_GREATER
#if NET8_0_OR_GREATER
using System.Runtime.Intrinsics.Wasm;
#endif // NET8_0_OR_GREATER
#if NETCOREAPP3_0_OR_GREATER
using System.Runtime.Intrinsics.X86;
#endif // NETCOREAPP3_0_OR_GREATER
using System.Text;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Zyl.SizableSpans.Impl {
    /// <summary>
    /// Helper methods of <see cref="BitOperations"/> (<see cref="BitOperations"/> 的帮助方法).
    /// </summary>
    public static class BitOperationsHelper {

        /// <summary>
        /// Evaluate whether a given integral value is a power of 2.
        /// </summary>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPow2(int value) {
#if NET6_0_OR_GREATER
            return BitOperations.IsPow2(value);
#else
            return (value & (value - 1)) == 0 && value > 0;
#endif // NET6_0_OR_GREATER
        }

        /// <summary>
        /// Evaluate whether a given integral value is a power of 2.
        /// </summary>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        //[CLSCompliant(false)]
        public static bool IsPow2(uint value) {
#if NET6_0_OR_GREATER
            return BitOperations.IsPow2(value);
#else
            return (value & (value - 1)) == 0 && value > 0;
#endif // NET6_0_OR_GREATER
        }

        /// <summary>
        /// Evaluate whether a given integral value is a power of 2.
        /// </summary>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPow2(long value) {
#if NET6_0_OR_GREATER
            return BitOperations.IsPow2(value);
#else
            return (value & (value - 1)) == 0 && value > 0;
#endif // NET6_0_OR_GREATER
        }

        /// <summary>
        /// Evaluate whether a given integral value is a power of 2.
        /// </summary>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        //[CLSCompliant(false)]
        public static bool IsPow2(ulong value) {
#if NET6_0_OR_GREATER
            return BitOperations.IsPow2(value);
#else
            return (value & (value - 1)) == 0 && value > 0;
#endif // NET6_0_OR_GREATER
        }

    }
}
