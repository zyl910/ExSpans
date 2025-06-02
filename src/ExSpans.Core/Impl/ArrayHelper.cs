using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Zyl.ExSpans.Reflection;

namespace Zyl.ExSpans.Impl {
    /// <summary>
    /// <see cref="Array"/> Helper.
    /// </summary>
    public static class ArrayHelper {

        /// <summary>
        /// This is the threshold where Introspective sort switches to Insertion sort.
        /// Empirically, 16 seems to speed up most cases without slowing down others, at least for integers.
        /// Large value types may benefit from a smaller number.
        /// </summary>
        internal const int IntrosortSizeThreshold = 16; // Array.IntrosortSizeThreshold

        /// <summary>
        /// Assigns the given value of type T to each element of the specified array.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the array.</typeparam>
        /// <param name="array">The array to be filled.</param>
        /// <param name="value">The value to assign to each array element.</param>
        public static void Fill<T>(T[] array, T value) {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_0_OR_GREATER
            Array.Fill<T>(array, value);
#else
            if (array == null) {
                throw new ArgumentNullException(nameof(array));
            }

            if (!TypeHelper.IsValueType<T>() && array.GetType() != typeof(T[])) {
#if NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_0_OR_GREATER || NET40_OR_GREATER_OR_GREATER
                if (array.LongLength >= int.MaxValue) {
                    for (nint i = 0; i < (nint)array.LongLength; i++) {
                        array[i] = value;
                    }
                    return;
                }
#endif // NETSTANDARD2_0_OR_GREATER || NETCOREAPP1_0_OR_GREATER || NET40_OR_GREATER_OR_GREATER
                for (int i = 0; i < array.Length; i++) {
                    array[i] = value;
                }
            } else {
                new Span<T>(array).Fill(value);
            }
#endif
        }

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

    }
}
