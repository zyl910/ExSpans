﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Zyl.ExSpans.Extensions;

namespace Zyl.ExSpans.Impl {
    /// <summary>
    /// <see cref="MemoryMarshal"/> helper.
    /// </summary>
    public static class MemoryMarshalHelper {

        /// <summary>
        /// Get the saturating length of the span (取得跨度的饱和长度).
        /// </summary>
        /// <param name="length">The length of the span (跨度的长度)</param>
        /// <returns>The saturating length of the span (跨度的饱和长度).</returns>
        [MyCLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetSpanSaturatingLength(TSize length) {
            int len = length.SaturatingToInt32();
            return len;
        }

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER

        /// <summary>
        /// [Saturating] Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        /// <typeparam name="T">The type of the data items.</typeparam>
        /// <param name="reference">A reference to data.</param>
        /// <param name="length">The number of T elements that reference contains.</param>
        /// <returns>A read-only span.</returns>
        [MyCLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<T> CreateReadOnlySpanSaturating<T>(ref T reference, TSize length) {
            int len = GetSpanSaturatingLength(length);
            return MemoryMarshal.CreateReadOnlySpan(ref reference, len);
        }

        /// <summary>
        /// [Saturating] Creates a new span over a portion of a regular managed object.
        /// </summary>
        /// <typeparam name="T">The type of the data items.</typeparam>
        /// <param name="reference">A reference to data.</param>
        /// <param name="length">The number of T elements that reference contains.</param>
        /// <returns>A span.</returns>
        [MyCLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> CreateSpanSaturating<T>(ref T reference, TSize length) {
            int len = GetSpanSaturatingLength(length);
            return MemoryMarshal.CreateSpan(ref reference, len);
        }

#endif // NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER

    }
}
