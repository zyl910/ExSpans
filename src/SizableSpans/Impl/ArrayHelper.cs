using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Zyl.SizableSpans.Impl {
    /// <summary>
    /// <see cref="Array"/> Helper.
    /// </summary>
    public static class ArrayHelper {

#if NETSTANDARD1_3_OR_GREATER || NETCOREAPP1_0_OR_GREATER || NET46_OR_GREATER
#else
        private static class EmptyArray<T> {
#pragma warning disable CA1825, IDE0300 // this is the implementation of Array.Empty<T>()
            internal static readonly T[] Value = new T[0];
#pragma warning restore CA1825, IDE0300
        }
#endif

        /// <summary>
        /// Returns an empty array (返回一个空数组).
        /// </summary>
        /// <typeparam name="T">The type of the elements of the array (数组元素的类型).</typeparam>
        /// <returns>An empty array (一个空数组).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] Empty<T>() {
#if NETSTANDARD1_3_OR_GREATER || NETCOREAPP1_0_OR_GREATER || NET46_OR_GREATER
            return Array.Empty<T>();
#else
            return EmptyArray<T>.Value;
#endif
        }

        /// <summary>
        /// Gets a native unsigned integer that represents the total number of elements in all the dimensions of the array (获取一个本机无符号整数，该整数表示数组所有维度中的元素总数).
        /// </summary>
        /// <typeparam name="T">The type of the elements of the array (数组元素的类型).</typeparam>
        /// <param name="source">Source array (源数组).</param>
        /// <returns>A native unsigned integer that represents the total number of elements in all the dimensions of the array (一个本机无符号整数，表示数组所有维度中的元素总数)</returns>
        public static TSize NULength<T>(this T[] source) {
#if NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_0_OR_GREATER || NET20_OR_GREATER
            return (TSize)source.LongLength;
#else
            return (TSize)source.Length;
#endif
        }

    }
}
