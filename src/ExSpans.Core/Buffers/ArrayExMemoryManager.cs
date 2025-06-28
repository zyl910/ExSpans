using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace Zyl.ExSpans.Buffers {
    /// <summary>
    /// Array pool based memory manager. It will rent the array from the array pool based on the length parameter, and return array to the array pool when when <see cref="IDisposable.Dispose"/>. Please note that the maximum length of an array is <see cref="Array.MaxLength"/> and generally cannot exceed 2GB
    /// (基于数组池的内存管理器. 它会根据长度参数, 从数组池中租借数组, 并在 Dispose 时归还数组到数组池. 请注意, 数组的最大长度为 <see cref="Array.MaxLength"/>, 一般不能超过 2GB).
    /// </summary>
    /// <typeparam name="T">The element type (元素的类型).</typeparam>
    public sealed class ArrayExMemoryManager<T> : AbstractArrayExMemoryManager<T>, IDisposable {

        /// <summary>
        /// Create ArrayExMemoryManager. It contains parameters <paramref name="length"/>, <paramref name="pool"/>, <paramref name="flags"/>.
        /// </summary>
        /// <param name="length">Length of unmanaged data (非托管数据的长度).</param>
        /// <param name="pool">The <see cref="ArrayPool{T}"/> instance used to rent array. Defaults to <see cref="ArrayPool{T}.Shared"/> if it is null (用于租用数组的 <see cref="ArrayPool{T}"/> 实例. 它为空时默认为 <see cref="ArrayPool{T}.Shared"/>).</param>
        /// <param name="flags">Memory alloc flags (内存分配标志). This class supports these flags: <see cref="MemoryAllocFlags.ClearAlloc"/>, <see cref="MemoryAllocFlags.ClearFree"/>.</param>
        /// <exception cref="ArgumentOutOfRangeException">The length parameter must be greater than or equal to 0. The length parameter out of array max length.</exception>
        public ArrayExMemoryManager(nint length, ArrayPool<T>? pool, MemoryAllocFlags flags = default) : base(length, pool, flags) {
        }

        /// <summary>
        /// Create ArrayExMemoryManager. It contains parameters <paramref name="length"/>.
        /// </summary>
        /// <param name="length">Length of unmanaged data (非托管数据的长度).</param>
        /// <exception cref="ArgumentOutOfRangeException">The length parameter must be greater than or equal to 0. The length parameter out of array max length.</exception>
        public ArrayExMemoryManager(nint length) : this(length, null) {
        }

    }
}
