using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Zyl.ExSpans.Extensions {
    /// <summary>
    /// Extensions of ExLength methods (ExLength 方法的扩展)
    /// </summary>
    public static class ExLengthExtensions {

        /// <summary>
        /// Gets a native unsigned integer that represents the total number of elements in all the dimensions of the array (获取一个本机整数，该整数表示数组所有维度中的元素总数).
        /// </summary>
        /// <typeparam name="T">The type of the elements of the array (数组元素的类型).</typeparam>
        /// <param name="source">Source array (源数组).</param>
        /// <returns>A native unsigned integer that represents the total number of elements in all the dimensions of the array (一个本机整数，表示数组所有维度中的元素总数)</returns>
        [MyCLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSize ExLength<T>(this T[] source) {
#if NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_0_OR_GREATER || NET40_OR_GREATER_OR_GREATER
            return (TSize)source.LongLength;
#else
            return (TSize)source.Length;
#endif
        }

        /// <summary>
        /// Gets a native unsigned integer that represents the total number of elements in <see cref="ArraySegment{T}"/> (获取一个本机整数，该整数表示 <see cref="ArraySegment{T}"/> 的元素总数).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="source">Source array (源数组).</param>
        /// <returns>A native unsigned integer that represents the total number of elements in <see cref="ArraySegment{T}"/> (一个本机整数，表示 <see cref="ArraySegment{T}"/> 的元素总数)</returns>
        [MyCLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSize ExLength<T>(this ArraySegment<T> source) {
            return (TSize)source.Count;
        }

        /// <summary>
        /// Gets a native unsigned integer that represents the total number of elements in <see cref="ReadOnlySpan{T}"/> (获取一个本机整数，该整数表示 <see cref="ReadOnlySpan{T}"/> 的元素总数).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="source">Source array (源数组).</param>
        /// <returns>A native unsigned integer that represents the total number of elements in <see cref="ReadOnlySpan{T}"/> (一个本机整数，表示 <see cref="ReadOnlySpan{T}"/> 的元素总数)</returns>
        [MyCLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSize ExLength<T>(this ReadOnlySpan<T> source) {
            return (TSize)source.Length;
        }

        /// <summary>
        /// Gets a native unsigned integer that represents the total number of elements in <see cref="Span{T}"/> (获取一个本机整数，该整数表示 <see cref="Span{T}"/> 的元素总数).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="source">Source array (源数组).</param>
        /// <returns>A native unsigned integer that represents the total number of elements in <see cref="Span{T}"/> (一个本机整数，表示 <see cref="Span{T}"/> 的元素总数)</returns>
        [MyCLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSize ExLength<T>(this Span<T> source) {
            return (TSize)source.Length;
        }

        /// <summary>
        /// Gets a native unsigned integer that represents the total number of elements in String (获取一个本机整数，该整数表示 String 的元素总数).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="source">Source array (源数组).</param>
        /// <returns>A native unsigned integer that represents the total number of elements in String (一个本机整数，表示 String 的元素总数)</returns>
        [MyCLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSize ExLength<T>(this string source) {
            return (TSize)source.Length;
        }

    }
}
