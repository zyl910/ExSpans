#if NET8_0_OR_GREATER
#else
#define VECTOR_WHERE_STRUCT// Since .NET8, Vector type not have `where T : struct`.
#endif // NET8_0_OR_GREATER

#if NETCOREAPP3_0_OR_GREATER

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Text;
using Zyl.VectorTraits;

namespace Zyl.ExSpans.Impl {
    /// <summary>
    /// <see cref="Vector128"/> Helper.
    /// </summary>
    public static class Vector128Helper {

        /// <summary>
        /// Compares two vectors to determine if they are equal on a per-element basis.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="left">The vector to compare with right.</param>
        /// <param name="right">The vector to compare with left.</param>
        /// <returns>A vector whose elements are all-bits-set or zero, depending on if the corresponding elements in left and right were equal.</returns>
        /// <exception cref="System.NotSupportedException">The type of left and right (<typeparamref name="T"/>) is not supported.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<T> Equals<T>(Vector128<T> left, Vector128<T> right)
#if VECTOR_WHERE_STRUCT
                where T : struct
#endif // VECTOR_WHERE_STRUCT
                {
#if NET7_0_OR_GREATER
            return Vector128.Equals(left, right);
#else
            switch (Unsafe.SizeOf<T>()) {
                case 8:
                    return Vector128s.Equals(left.AsInt64(), right.AsInt64()).As<Int64, T>();
                case 4:
                    return Vector128s.Equals(left.AsInt32(), right.AsInt32()).As<Int32, T>();
                case 2:
                    return Vector128s.Equals(left.AsInt16(), right.AsInt16()).As<Int16, T>();
                default:
                    return Vector128s.Equals(left.AsByte(), right.AsByte()).As<Byte, T>();
            }

#endif // NET7_0_OR_GREATER
        }

        /// <summary>
        /// Compares two vectors to determine if all elements are equal.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="left">The vector to compare with right.</param>
        /// <param name="right">The vector to compare with left.</param>
        /// <returns>true if all elements in left was equal to the corresponding element in right.</returns>
        /// <exception cref="System.NotSupportedException">The type of left and right (<typeparamref name="T"/>) is not supported.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool EqualsAll<T>(Vector128<T> left, Vector128<T> right)
#if VECTOR_WHERE_STRUCT
                where T : struct
#endif // VECTOR_WHERE_STRUCT
                {
#if NET7_0_OR_GREATER
            return Vector128.EqualsAll(left, right);
#else
            switch (Unsafe.SizeOf<T>()) {
                case 8:
                    return Vector128s.EqualsAll(left.AsInt64(), right.AsInt64());
                case 4:
                    return Vector128s.EqualsAll(left.AsInt32(), right.AsInt32());
                case 2:
                    return Vector128s.EqualsAll(left.AsInt16(), right.AsInt16());
                default:
                    return Vector128s.EqualsAll(left.AsByte(), right.AsByte());
            }

#endif // NET7_0_OR_GREATER
        }

        /// <summary>
        /// Compares two vectors to determine if any elements are equal.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="left">The vector to compare with right.</param>
        /// <param name="right">The vector to compare with left.</param>
        /// <returns>true if any elements in left was equal to the corresponding element in right.</returns>
        /// <exception cref="System.NotSupportedException">The type of left and right (<typeparamref name="T"/>) is not supported.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool EqualsAny<T>(Vector128<T> left, Vector128<T> right)
#if VECTOR_WHERE_STRUCT
                where T : struct
#endif // VECTOR_WHERE_STRUCT
                {
#if NET7_0_OR_GREATER
            return Vector128.EqualsAny(left, right);
#else
            switch (Unsafe.SizeOf<T>()) {
                case 8:
                    return Vector128s.EqualsAny(left.AsInt64(), right.AsInt64());
                case 4:
                    return Vector128s.EqualsAny(left.AsInt32(), right.AsInt32());
                case 2:
                    return Vector128s.EqualsAny(left.AsInt16(), right.AsInt16());
                default:
                    return Vector128s.EqualsAny(left.AsByte(), right.AsByte());
            }

#endif // NET7_0_OR_GREATER
        }

    }
}

#endif // NETCOREAPP3_0_OR_GREATER
