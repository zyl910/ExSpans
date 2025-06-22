using System;
using System.Collections.Generic;
using System.Text;

namespace Zyl.ExSpans.Impl {
    /// <summary>
    /// <see cref="HashCode"/> Helper.
    /// </summary>
    public static class HashCodeHelper {

        /// <summary>
        /// Combines one values into a hash code.
        /// </summary>
        /// <typeparam name="T1">The type of the first value to combine into the hash code.</typeparam>
        /// <param name="value1">The first value to combine into the hash code.</param>
        /// <returns>The hash code that represents the one values.</returns>
        public static int Combine<T1>(T1 value1) {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
            return HashCode.Combine(value1);
#else
            return value1?.GetHashCode() ?? 0;
#endif // NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        }

        /// <summary>
        /// Combines two values into a hash code.
        /// </summary>
        /// <typeparam name="T1">The type of the first value to combine into the hash code.</typeparam>
        /// <typeparam name="T2">The type of the second value to combine into the hash code.</typeparam>
        /// <param name="value1">The first value to combine into the hash code.</param>
        /// <param name="value2">The second value to combine into the hash code.</param>
        /// <returns>The hash code that represents the two values.</returns>
        public static int Combine<T1, T2>(T1 value1, T2 value2) {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
            return HashCode.Combine(value1, value2);
#else
            uint hc1 = (uint)(value1?.GetHashCode() ?? 0);
            uint hc2 = (uint)(value2?.GetHashCode() ?? 0);
            uint hash = hc1 ^ hc2;
            return (int)hash;
#endif // NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        }

        /// <summary>
        /// Combines three values into a hash code.
        /// </summary>
        /// <typeparam name="T1">The type of the first value to combine into the hash code.</typeparam>
        /// <typeparam name="T2">The type of the second value to combine into the hash code.</typeparam>
        /// <typeparam name="T3">The type of the third value to combine into the hash code.</typeparam>
        /// <param name="value1">The first value to combine into the hash code.</param>
        /// <param name="value2">The second value to combine into the hash code.</param>
        /// <param name="value3">The third value to combine into the hash code.</param>
        /// <returns>The hash code that represents the three values.</returns>
        public static int Combine<T1, T2, T3>(T1 value1, T2 value2, T3 value3) {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
            return HashCode.Combine(value1, value2, value3);
#else
            uint hc1 = (uint)(value1?.GetHashCode() ?? 0);
            uint hc2 = (uint)(value2?.GetHashCode() ?? 0);
            uint hc3 = (uint)(value3?.GetHashCode() ?? 0);
            uint hash = hc1 ^ hc2 ^ hc3;
            return (int)hash;
#endif // NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        }

    }
}
