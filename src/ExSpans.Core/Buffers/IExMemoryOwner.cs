using System;
using System.Buffers;

namespace Zyl.ExSpans.Buffers {
    /// <summary>
    /// Owner of <see cref="ExMemory{T}"/> that is responsible for disposing the underlying memory appropriately. It can be regarded as the <see cref="IMemoryOwner{T}"/> of <see cref="TSize"/> index range
    /// (负责适当地释放基础内存的 <see cref="ExMemory{T}"/> 所有者. 它可以被视为 <see cref="TSize"/> 索引范围的 <see cref="IMemoryOwner{T}"/>).
    /// </summary>
    /// <typeparam name="T">The element type (元素的类型).</typeparam>
    public interface IExMemoryOwner<T> : IMemoryOwner<T>, IDisposable {
        /// <summary>
        /// Returns a <see cref="ExMemory{T}"/>.
        /// </summary>
        ExMemory<T> ExMemory { get; }
    }
}
