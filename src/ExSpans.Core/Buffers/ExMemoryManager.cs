using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace Zyl.ExSpans.Buffers {
    /// <summary>
    /// Manager of <see cref="ExMemory{T}"/> that provides the implementation. It can be regarded as the <see cref="MemoryManager{T}"/> of <see cref="TSize"/> index range
    /// (用于实现的 <see cref="ExMemory{T}"/> 管理器. 它可以被视为 <see cref="TSize"/> 索引范围的 <see cref="MemoryManager{T}"/>).
    /// </summary>
    /// <typeparam name="T">The element type (元素的类型).</typeparam>
    public abstract class ExMemoryManager<T> : MemoryManager<T>, IExMemoryOwner<T>, IDisposable, IPinnable {
        /// <summary>
        /// Returns a <see cref="ExMemory{T}"/> (返回 <see cref="ExMemory{T}"/>).
        /// </summary>
        public virtual ExMemory<T> ExMemory => new ExMemory<T>(this, GetExSpan().Length);

        /// <summary>
        /// Returns a <see cref="ExSpan{T}"/> wrapping the underlying memory (返回包装了底层内存的 <see cref="ExSpan{T}"/>).
        /// </summary>
        /// <returns>Returns a span (返回一个跨度).</returns>
        public abstract ExSpan<T> GetExSpan();

        /// <inheritdoc/>
        public override Span<T> GetSpan() {
            return GetExSpan().AsSpan();
        }

        /// <summary>
        /// Returns a memory buffer consisting of a specified number of elements from the memory managed by the current memory manager (返回一个内存缓冲区，它由当前内存管理器管理的内存中指定数量的元素构成).
        /// </summary>
        /// <param name="length">The element count in the memory, starting at offset 0 (内存缓冲区内的元素数，偏移量从 0 开始).</param>
        /// <returns>Returns a memory buffer (返回一个内存缓冲区).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected ExMemory<T> CreateExMemory(TSize length) => new ExMemory<T>(this, length);

        /// <summary>
        /// Returns a memory buffer consisting of a specified number of elements starting at a specified offset from the memory managed by the current memory manager (返回一个内存缓冲区，它由当前内存管理器管理的内存中指定开始与数量的元素构成).
        /// </summary>
        /// <param name="start">The offset to the element which the returned memory starts at (元素的偏移量，返回的内存缓冲区将其作为起始偏移量).</param>
        /// <param name="length">The element count in the memory, starting at element offset <paramref name="start"/> (要包括在返回的内存缓冲区中的元素数).</param>
        /// <returns>Returns a memory buffer (返回一个内存缓冲区).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected ExMemory<T> CreateExMemory(TSize start, TSize length) => new ExMemory<T>(this, start, length);

    }
}
