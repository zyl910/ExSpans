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
    /// <see cref="Vector256"/> Helper.
    /// </summary>
    public static class Vector256Helper {

        /// <summary>
        /// Compares two vectors to determine if they are equal on a per-element basis.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="left">The vector to compare with right.</param>
        /// <param name="right">The vector to compare with left.</param>
        /// <returns>A vector whose elements are all-bits-set or zero, depending on if the corresponding elements in left and right were equal.</returns>
        /// <exception cref="System.NotSupportedException">The type of left and right (<typeparamref name="T"/>) is not supported.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector256<T> Equals<T>(Vector256<T> left, Vector256<T> right)
#if VECTOR_WHERE_STRUCT
                where T : struct
#endif // VECTOR_WHERE_STRUCT
                {
#if NET7_0_OR_GREATER
            return Vector256.Equals(left, right);
#else
            switch (Unsafe.SizeOf<T>()) {
                case 8:
                    return Vector256s.Equals(left.AsInt64(), right.AsInt64()).As<Int64, T>();
                case 4:
                    return Vector256s.Equals(left.AsInt32(), right.AsInt32()).As<Int32, T>();
                case 2:
                    return Vector256s.Equals(left.AsInt16(), right.AsInt16()).As<Int16, T>();
                default:
                    return Vector256s.Equals(left.AsByte(), right.AsByte()).As<Byte, T>();
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
        public static bool EqualsAll<T>(Vector256<T> left, Vector256<T> right)
#if VECTOR_WHERE_STRUCT
                where T : struct
#endif // VECTOR_WHERE_STRUCT
                {
#if NET7_0_OR_GREATER
            return Vector256.EqualsAll(left, right);
#else
            switch (Unsafe.SizeOf<T>()) {
                case 8:
                    return Vector256s.EqualsAll(left.AsInt64(), right.AsInt64());
                case 4:
                    return Vector256s.EqualsAll(left.AsInt32(), right.AsInt32());
                case 2:
                    return Vector256s.EqualsAll(left.AsInt16(), right.AsInt16());
                default:
                    return Vector256s.EqualsAll(left.AsByte(), right.AsByte());
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
        public static bool EqualsAny<T>(Vector256<T> left, Vector256<T> right)
#if VECTOR_WHERE_STRUCT
                where T : struct
#endif // VECTOR_WHERE_STRUCT
                {
#if NET7_0_OR_GREATER
            return Vector256.EqualsAny(left, right);
#else
            switch (Unsafe.SizeOf<T>()) {
                case 8:
                    return Vector256s.EqualsAny(left.AsInt64(), right.AsInt64());
                case 4:
                    return Vector256s.EqualsAny(left.AsInt32(), right.AsInt32());
                case 2:
                    return Vector256s.EqualsAny(left.AsInt16(), right.AsInt16());
                default:
                    return Vector256s.EqualsAny(left.AsByte(), right.AsByte());
            }

#endif // NET7_0_OR_GREATER
        }

        /// <summary>
        /// Extracts the most significant bit from each element in a vector.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="vector">The vector whose elements should have their most significant bit extracted.</param>
        /// <returns>The packed most significant bits extracted from the elements in vector.</returns>
        /// <exception cref="System.NotSupportedException">The type of vector (<typeparamref name="T"/>) is not supported.</exception>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ExtractMostSignificantBits<T>(Vector256<T> vector)
#if VECTOR_WHERE_STRUCT
                where T : struct
#endif // VECTOR_WHERE_STRUCT
                {
#if NET7_0_OR_GREATER
            return Vector256.ExtractMostSignificantBits(vector);
#else
            switch (Unsafe.SizeOf<T>()) {
                case 8:
                    return Vector256s.ExtractMostSignificantBits(vector.AsInt64());
                case 4:
                    return Vector256s.ExtractMostSignificantBits(vector.AsInt32());
                case 2:
                    return Vector256s.ExtractMostSignificantBits(vector.AsInt16());
                default:
                    return Vector256s.ExtractMostSignificantBits(vector.AsByte());
            }

#endif // NET7_0_OR_GREATER
        }

        /// <summary>Determines the index of the last element in a vector that is equal to a given value.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="vector">The vector whose elements are being checked.</param>
        /// <param name="value">The value to check for in <paramref name="vector" /></param>
        /// <returns>The index into <paramref name="vector" /> representing the last element that was equal to <paramref name="value" />; otherwise, <c>-1</c> if no such element exists.</returns>
        /// <exception cref="NotSupportedException">The type of <paramref name="vector" /> and <paramref name="value" /> (<typeparamref name="T" />) is not supported.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int LastIndexOf<T>(Vector256<T> vector, T value) where T : struct {
#if NET10_0_OR_GREATER
            return Vector256.LastIndexOf(vector, value);
#else
            return 31 - BitOperations.LeadingZeroCount(ExtractMostSignificantBits(Equals(vector, Vector256s.Create<T>(value))));
#endif // NET10_0_OR_GREATER
        }

        /// <summary>Determines the index of the last element in a vector that has all bits set.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="vector">The vector whose elements are being checked.</param>
        /// <returns>The index into <paramref name="vector" /> representing the last element that had all bits set; otherwise, <c>-1</c> if no such element exists.</returns>
        /// <exception cref="NotSupportedException">The type of <paramref name="vector" /> (<typeparamref name="T" />) is not supported.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int LastIndexOfWhereAllBitsSet<T>(Vector256<T> vector) where T : struct {
#if NET10_0_OR_GREATER
            return Vector256.LastIndexOf(vector, value);
#else
            if (typeof(T) == typeof(float)) {
                return LastIndexOf(vector.AsInt32(), -1);
            } else if (typeof(T) == typeof(double)) {
                return LastIndexOf(vector.AsInt64(), -1);
            } else {
                return LastIndexOf(vector, Scalars<T>.AllBitsSet);
            }
#endif // NET10_0_OR_GREATER
        }
    }
}

#endif // NETCOREAPP3_0_OR_GREATER
