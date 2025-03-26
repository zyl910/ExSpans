using System;
using System.Collections.Generic;
using System.Text;

namespace Zyl.SizableSpans.Extensions {
    /// <summary>
    /// Extensions of NULength methods (NULength 方法的扩展)
    /// </summary>
    public static class NULengthExtensions {

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
