#if NET8_0_OR_GREATER
#else
#define VECTOR_WHERE_STRUCT// Since .NET8, Vector type not have `where T : struct`.
#endif // NET8_0_OR_GREATER

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using Zyl.VectorTraits;
using Zyl.VectorTraits.Extensions.SameW;

namespace Zyl.ExSpans.Impl {
    /// <summary>
    /// <see cref="Vector"/> Helper.
    /// </summary>
    public static class VectorHelper {

        /// <summary>
        /// Extracts the most significant bit from each element in a vector.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="vector">The vector whose elements should have their most significant bit extracted.</param>
        /// <returns>The packed most significant bits extracted from the elements in vector.</returns>
        /// <exception cref="System.NotSupportedException">The type of vector (<typeparamref name="T"/>) is not supported.</exception>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong ExtractMostSignificantBits<T>(this Vector<T> vector) where T : struct {
#if NETX_0_OR_GREATER
            return Vector.ExtractMostSignificantBits(vector);
#else
            switch (Unsafe.SizeOf<T>()) {
                case 8:
                    return Vectors.ExtractMostSignificantBits(vector.AsInt64());
                case 4:
                    return Vectors.ExtractMostSignificantBits(vector.AsInt32());
                case 2:
                    return Vectors.ExtractMostSignificantBits(vector.AsInt16());
                default:
                    return Vectors.ExtractMostSignificantBits(vector.AsByte());
            }

#endif // NETX_0_OR_GREATER
        }

        /// <summary>
        /// Loads a vector from the given source and element offset.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="source">The source to which elementOffset will be added before loading the vector.</param>
        /// <param name="elementOffset">The element offset from source from which the vector will be loaded.</param>
        /// <returns>The vector loaded from <paramref name="source"/> plus <paramref name="elementOffset"/>.</returns>
        /// <exception cref="System.NotSupportedException">The type of <paramref name="source"/> (<typeparamref name="T"/>) is not supported.</exception>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector<T> LoadUnsafe<T>(ref readonly T source, nuint elementOffset)
#if VECTOR_WHERE_STRUCT
                where T : struct
#endif // VECTOR_WHERE_STRUCT
                {
#if NET8_0_OR_GREATER
            return Vector.LoadUnsafe(in source, elementOffset);
#else
            return Unsafe.As<T, Vector<T>>(ref ExUnsafe.Add(ref Unsafe.AsRef(in source), elementOffset));
#endif // NET8_0_OR_GREATER
        }

        /// <summary>
        /// Loads a vector from the given source.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="source">The source from which the vector will be loaded.</param>
        /// <returns>The vector loaded from <paramref name="source"/>.</returns>
        /// <exception cref="System.NotSupportedException">The type of <paramref name="source"/> (<typeparamref name="T"/>) is not supported.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector<T> LoadUnsafe<T>(ref readonly T source)
#if VECTOR_WHERE_STRUCT
                where T : struct
#endif // VECTOR_WHERE_STRUCT
                {
#if NET8_0_OR_GREATER
            return Vector.LoadUnsafe(in source);
#else
            return Unsafe.As<T, Vector<T>>(ref Unsafe.AsRef(in source));
#endif // NET8_0_OR_GREATER
        }

    }
}
