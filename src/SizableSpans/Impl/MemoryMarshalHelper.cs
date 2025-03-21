using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Zyl.SizableSpans.Impl {
    /// <summary>
    /// MemoryMarshal Helper.
    /// </summary>
    public static class MemoryMarshalHelper {

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER

        /// <summary>
        /// [Saturating] Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        /// <typeparam name="T">The type of the data items.</typeparam>
        /// <param name="reference">A reference to data.</param>
        /// <param name="length">The number of T elements that reference contains.</param>
        /// <returns>A read-only span.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<T> CreateReadOnlySpanSaturating<T>(ref T reference, TSize length) {
            int len = int.MaxValue;
            if (IntPtrs.LessThan(length, (uint)len)) {
                len = (int)length;
            }
            return MemoryMarshal.CreateReadOnlySpan(ref reference, len);
        }

        /// <summary>
        /// [Saturating] Creates a new span over a portion of a regular managed object.
        /// </summary>
        /// <typeparam name="T">The type of the data items.</typeparam>
        /// <param name="reference">A reference to data.</param>
        /// <param name="length">The number of T elements that reference contains.</param>
        /// <returns>A span.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> CreateSpanSaturating<T>(ref T reference, TSize length) {
            int len = int.MaxValue;
            if (IntPtrs.LessThan(length, (uint)len)) {
                len = (int)length;
            }
            return MemoryMarshal.CreateSpan(ref reference, len);
        }

#endif // NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER

    }
}
