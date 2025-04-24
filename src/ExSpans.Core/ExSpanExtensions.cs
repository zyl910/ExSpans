using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Zyl.ExSpans.Extensions;

namespace Zyl.ExSpans {
    /// <summary>
    /// Provides commonly used extension methods for the span-related types, such as <see cref="ExSpan{T}"/> and <see cref="ReadOnlyExSpan{T}"/> (提供跨度相关的类型的常用的扩展方法，例如 <see cref="ExSpan{T}"/> 和 <see cref="ReadOnlyExSpan{T}"/>).
    /// </summary>
    public static partial class ExSpanExtensions {

        /// <summary>
        /// Creates a new span over the target array (在目标数组上创建新的跨度).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="array">The target array (目标数组).</param>
        /// <returns>The span representation of the array (数组的跨度表示形式).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ExSpan<T> AsExSpan<T>(this T[]? array) {
            return new ExSpan<T>(array);
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
        [MyCLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ExSpan<T> AsExSpan<T>(this T[]? array, TSize start, TSize length) {
            return new ExSpan<T>(array, start, length);
        }

        /// <summary>
        /// Creates a new span over the target array (从指定位置开始到数组的结尾，在目标数组的一部分上创建一个新的跨度).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="array">The target array (目标数组).</param>
        /// <param name="start">The index at which to begin the span (开始跨度处的索引).</param>
        /// <returns>The span representation of the array (数组的跨度表示形式).</returns>
        [MyCLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ExSpan<T> AsExSpan<T>(this T[]? array, TSize start) {
            if (array == null) {
                if (start != TSize.Zero)
                    ThrowHelper.ThrowArgumentOutOfRangeException();
                return default;
            }
            if (IntPtrExtensions.GreaterThanOrEqual(start, array.ExLength())) {
                ThrowHelper.ThrowArgumentOutOfRangeException();
            }
            return new ExSpan<T>(array, start, IntPtrExtensions.Subtract(array.ExLength(), start));
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
        public static ExSpan<T> AsExSpan<T>(this T[]? array, Index startIndex) {
            if (array == null) {
                if (!startIndex.Equals(Index.Start))
                    throw new ArgumentNullException(nameof(array), "The array argument is null!");

                return default;
            }
            int actualIndex = startIndex.GetOffset(array.Length);
            if (actualIndex < 0) {
                ThrowHelper.ThrowArgumentOutOfRangeException();
            }
            return new ExSpan<T>(array, (TSize)actualIndex, IntPtrExtensions.Subtract(array.ExLength(), (TSize)actualIndex));
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
        public static ExSpan<T> AsExSpan<T>(this T[]? array, Range range) {
            if (array == null) {
                Index startIndex = range.Start;
                Index endIndex = range.End;
                if (!startIndex.Equals(Index.Start) || !endIndex.Equals(Index.Start))
                    throw new ArgumentNullException(nameof(array), "The array argument is null!");
                return default;
            }
            (int start, int length) = range.GetOffsetAndLength(array.Length);
            return new ExSpan<T>(array, (TSize)start, (TSize)length);
        }

#endif // NETSTANDARD2_0_OR_GREATER || NETCOREAPP3_0_OR_GREATER

        /// <summary>
        /// Copies the contents of the array into the span. If the source
        /// and destinations overlap, this method behaves as if the original values in
        /// a temporary location before the destination is overwritten.
        /// (将数组的内容复制到跨度中. 如果源和目标重叠, 则此方法的行为就像覆盖目标之前临时位置中的原始值一样).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="source">The array to copy items from (要从中复制项的数组).</param>
        /// <param name="destination">The span to copy items into (要将项复制到的跨度).</param>
        /// <exception cref="ArgumentException">
        /// Thrown when the destination Span is shorter than the source array.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyTo<T>(this T[]? source, ExSpan<T> destination) {
            new ReadOnlyExSpan<T>(source).CopyTo(destination);
        }

    }
}
