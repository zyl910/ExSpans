using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace Zyl.ExSpans.Buffers {
    /// <summary>
    /// Manager of <see cref="ExMemory{T}"/> that provides the implementation.
    /// </summary>
    public abstract class ExMemoryManager<T> : MemoryManager<T>, IExMemoryOwner<T>, IPinnable {
        /// <summary>
        /// Returns a <see cref="ExMemory{T}"/>.
        /// </summary>
        public virtual ExMemory<T> ExMemory => new ExMemory<T>(this, GetExSpan().Length);

        /// <summary>
        /// Returns a <see cref="ExSpan{T}"/> wrapping the underlying memory.
        /// </summary>
        public abstract ExSpan<T> GetExSpan();

        /// <summary>
        /// Returns a <see cref="ExMemory{T}"/> for the current <see cref="ExMemoryManager{T}"/>.
        /// </summary>
        /// <param name="length">The element count in the memory, starting at offset 0.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected ExMemory<T> CreateExMemory(TSize length) => new ExMemory<T>(this, length);

        /// <summary>
        /// Returns a <see cref="ExMemory{T}"/> for the current <see cref="ExMemoryManager{T}"/>.
        /// </summary>
        /// <param name="start">The offset to the element which the returned memory starts at.</param>
        /// <param name="length">The element count in the memory, starting at element offset <paramref name="start"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected ExMemory<T> CreateExMemory(TSize start, TSize length) => new ExMemory<T>(this, start, length);

    }
}
