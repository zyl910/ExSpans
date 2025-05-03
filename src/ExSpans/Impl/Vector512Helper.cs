#if NET8_0_OR_GREATER
#else
#define VECTOR_WHERE_STRUCT// Since .NET8, Vector type not have `where T : struct`.
#endif // NET8_0_OR_GREATER

#if NET8_0_OR_GREATER

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Text;
using Zyl.VectorTraits;

namespace Zyl.ExSpans.Impl {
    /// <summary>
    /// <see cref="Vector512"/> Helper.
    /// </summary>
    public static class Vector512Helper {

        /// <summary>Determines the index of the last element in a vector that is equal to a given value.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="vector">The vector whose elements are being checked.</param>
        /// <param name="value">The value to check for in <paramref name="vector" /></param>
        /// <returns>The index into <paramref name="vector" /> representing the last element that was equal to <paramref name="value" />; otherwise, <c>-1</c> if no such element exists.</returns>
        /// <exception cref="NotSupportedException">The type of <paramref name="vector" /> and <paramref name="value" /> (<typeparamref name="T" />) is not supported.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int LastIndexOf<T>(Vector512<T> vector, T value) where T : struct {
#if NET10_0_OR_GREATER
            return Vector512.LastIndexOf(vector, value);
#else
            return 63 - BitOperations.LeadingZeroCount(Vector512.ExtractMostSignificantBits(Vector512.Equals(vector, Vector512s.Create<T>(value))));
#endif // NET10_0_OR_GREATER
        }

        /// <summary>Determines the index of the last element in a vector that has all bits set.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="vector">The vector whose elements are being checked.</param>
        /// <returns>The index into <paramref name="vector" /> representing the last element that had all bits set; otherwise, <c>-1</c> if no such element exists.</returns>
        /// <exception cref="NotSupportedException">The type of <paramref name="vector" /> (<typeparamref name="T" />) is not supported.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int LastIndexOfWhereAllBitsSet<T>(Vector512<T> vector) where T : struct {
#if NET10_0_OR_GREATER
            return Vector512.LastIndexOf(vector, value);
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

#endif // NET8_0_OR_GREATER
