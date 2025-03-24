using System;
using System.Collections.Generic;
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
        public static T[] Empty<T>() {
#if NETSTANDARD1_3_OR_GREATER || NETCOREAPP1_0_OR_GREATER || NET46_OR_GREATER
            return Array.Empty<T>();
#else
            return EmptyArray<T>.Value;
#endif
        }

    }
}
