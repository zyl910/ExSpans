using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Zyl.ExSpans.Impl {
    /// <summary>
    /// <see cref="Vector"/> Helper.
    /// </summary>
    public static class VectorHelper {

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
        public static Vector<T> LoadUnsafe<T>(ref readonly T source, nuint elementOffset) where T : struct {
            return Unsafe.As<T, Vector<T>>(ref ExUnsafe.Add(ref Unsafe.AsRef(in source), elementOffset));
        }

        /// <summary>
        /// Loads a vector from the given source.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="source">The source from which the vector will be loaded.</param>
        /// <returns>The vector loaded from <paramref name="source"/>.</returns>
        /// <exception cref="System.NotSupportedException">The type of <paramref name="source"/> (<typeparamref name="T"/>) is not supported.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector<T> LoadUnsafe<T>(ref readonly T source) where T : struct {
            return Unsafe.As<T, Vector<T>>(ref Unsafe.AsRef(in source));
        }

    }
}
