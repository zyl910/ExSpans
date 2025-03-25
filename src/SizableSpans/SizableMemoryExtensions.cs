using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Zyl.SizableSpans.Impl;

namespace Zyl.SizableSpans {
    /// <summary>
    /// Provides extension methods for the span-related types, such as <see cref="SizableSpan{T}"/> and <see cref="ReadOnlySizableSpan{T}"/> (提供跨度相关的类型的扩展方法，例如 <see cref="SizableSpan{T}"/> 和 <see cref="ReadOnlySizableSpan{T}"/>).
    /// </summary>
    public static partial class SizableMemoryExtensions {

        /// <summary>
        /// Creates a new span over the target array (在目标数组上创建新的跨度).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="array">The target array (目标数组).</param>
        /// <returns>The span representation of the array (数组的跨度表示形式).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SizableSpan<T> AsSizableSpan<T>(this T[]? array) {
            return new SizableSpan<T>(array);
        }

        /// <summary>
        /// Creates a new span over the portion of the target array beginning
        /// at 'start' index and ending at 'end' index (exclusive)
        /// (在目标数组中从“start”索引开始到“end”索引结束(不包括)的部分上创建一个新的跨度).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="array">The target array (目标数组).</param>
        /// <param name="start">The index at which to begin the span (开始跨度处的索引).</param>
        /// <param name="length">The number of items in the span (跨度中的项数).</param>
        /// <returns>The span representation of the array (数组的跨度表示形式).</returns>
        /// <remarks>Returns default when <paramref name="array"/> is null.</remarks>
        /// <exception cref="ArrayTypeMismatchException">Thrown when <paramref name="array"/> is covariant and array's type is not exactly T[].</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the specified <paramref name="start"/> or end index is not in the range (&lt;0 or &gt;Length).
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SizableSpan<T> AsSizableSpan<T>(this T[]? array, TSize start, TSize length) {
            return new SizableSpan<T>(array, start, length);
        }

        /// <summary>
        /// Creates a new span over the target array (从指定位置开始到数组的结尾，在目标数组的一部分上创建一个新的跨度).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="array">The target array (目标数组).</param>
        /// <param name="start">The index at which to begin the span (开始跨度处的索引).</param>
        /// <returns>The span representation of the array (数组的跨度表示形式).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SizableSpan<T> AsSizableSpan<T>(this T[]? array, TSize start) {
            if (array == null) {
                if (start != TSize.Zero)
                    ThrowHelper.ThrowArgumentOutOfRangeException();
                return default;
            }
            if (IntPtrs.GreaterThanOrEqual(start, array.NULength())) {
                ThrowHelper.ThrowArgumentOutOfRangeException();
            }
            return new SizableSpan<T>(array, start, IntPtrs.Subtract(array.NULength(), start));
        }

#if NETSTANDARD2_0_OR_GREATER || NETCOREAPP3_0_OR_GREATER
        /// <summary>
        /// Creates a new span over the portion of the target array defined by an <see cref="Index"/> value (在由 <see cref="Index"/> 值定义的目标数组部分上创建新的跨度).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="array">The target array (目标数组).</param>
        /// <param name="startIndex">The starting index (起始索引).</param>
        /// <returns>The span representation of the array (数组的跨度表示形式).</returns>
        /// <exception cref="ArgumentNullException">The array argument is null</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SizableSpan<T> AsSizableSpan<T>(this T[]? array, Index startIndex) {
            if (array == null) {
                if (!startIndex.Equals(Index.Start))
                    throw new ArgumentNullException(nameof(array), "The array argument is null!");

                return default;
            }
            int actualIndex = startIndex.GetOffset(array.Length);
            if (actualIndex < 0) {
                ThrowHelper.ThrowArgumentOutOfRangeException();
            }
            return new SizableSpan<T>(array, (TSize)actualIndex, IntPtrs.Subtract(array.NULength(), (TSize)actualIndex));
        }

        /// <summary>
        /// Creates a new span over a portion of a target array defined by a <see cref="Range"/> value (在由 <see cref="Range"/> 值定义的目标数组部分上创建新的跨度).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="array">The target array (目标数组).</param>
        /// <param name="range">The range of the array to convert (要转换的数组范围).</param>
        /// <returns>The span representation of the array (数组的跨度表示形式).</returns>
        /// <exception cref="ArgumentNullException">The array argument is null</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SizableSpan<T> AsSizableSpan<T>(this T[]? array, Range range) {
            if (array == null) {
                Index startIndex = range.Start;
                Index endIndex = range.End;
                if (!startIndex.Equals(Index.Start) || !endIndex.Equals(Index.Start))
                    throw new ArgumentNullException(nameof(array), "The array argument is null!");
                return default;
            }
            (int start, int length) = range.GetOffsetAndLength(array.Length);
            return new SizableSpan<T>(array, (TSize)start, (TSize)length);
        }

#endif // NETSTANDARD2_0_OR_GREATER || NETCOREAPP3_0_OR_GREATER

    }
}
