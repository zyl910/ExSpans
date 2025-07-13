using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace Zyl.ExSpans.Buffers {

    /// <summary>
    /// A memory manager that supports automatic memory allocation and alignment. When the length is less than <see cref="MaxArrayLength"/>, it uses array pooling; otherwise, it uses native memory
    /// (支持自动内存分配和对齐的内存管理器. 当长度小于 <see cref="MaxArrayLength"/> 时它使用数组池，否则它就使用原生内存).
    /// </summary>
    /// <typeparam name="T">The element type (元素的类型).</typeparam>
    public sealed class AllocExMemoryManager<T> : AbstractAllocExMemoryManager<T>, IDisposable where T : unmanaged {

#pragma warning disable CA2015
        /// <summary>
        /// Finalizer of AllocExMemoryManager.
        /// </summary>
        /// <remarks>
        /// <para>CA2015: Adding a finalizer to a type derived from MemoryManager may permit memory to be freed while it is still in use by a Span.https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca2015</para>
        /// <para>To support weak references, it is necessary to break the CA2015 warning and use finalizer to free up memory. Please developers to handle the lifespan of Span (为了支持弱引用, 需要突破CA2015警告, 利用终结期来释放内存. 请开发者处理好 Span 的生存期).</para>
        /// </remarks>
        ~AllocExMemoryManager() {
            Dispose(false);
        }
#pragma warning restore CA2015

        /// <summary>
        /// Create AllocExMemoryManager. It contains parameters <paramref name="pool"/>, <paramref name="length"/>, <paramref name="alignment"/>, <paramref name="flags"/>, <paramref name="maxArrayLength"/>.
        /// </summary>
        /// <param name="pool">The <see cref="ArrayPool{T}"/> instance used to rent array. If it is null, only unmanaged memory will be used (用于租用数组的 <see cref="ArrayPool{T}"/> 实例. 若它为空, 则仅使用非托管内存).</param>
        /// <param name="length">Length of data (数据的长度).</param>
        /// <param name="alignment">The alignment value (in bytes) of the memory block. This must be a power of <c>2</c>. When it is 1, it means no alignment is required. When it is 0, use the previous value (内存块的对齐值（以字节为单位）. 这必须是 2的幂. 为 1时表示无需对齐.为0时使用上一次的值).</param>
        /// <param name="flags">Memory alloc flags (内存分配标志). This class supports these flags: <see cref="MemoryAllocFlags.ClearAlloc"/>, <see cref="MemoryAllocFlags.ClearFree"/>, <see cref="MemoryAllocFlags.NoPressure"/>.</param>
        /// <param name="maxArrayLength">Maximum array length for array pool allocation. Defaults to <see cref="ExSpansGlobal.PoolMaxArrayLength"/> if it is 0 (数组池分配时的最大数组长度. 它为0时默认为 <see cref="ExSpansGlobal.PoolMaxArrayLength"/>). </param>
        /// <exception cref="ArgumentOutOfRangeException">The length parameter must be greater than or equal to 0. The length parameter out of array max length.</exception>
        public AllocExMemoryManager(ArrayPool<T>? pool, TSize length, TSize alignment = 0, MemoryAllocFlags flags = default, TSize maxArrayLength = 0)
            : base(pool, length, alignment, flags, maxArrayLength) {
        }

        /// <summary>
        /// Create AllocExMemoryManager. It contains parameters <paramref name="length"/>, <paramref name="alignment"/>, <paramref name="flags"/>, <paramref name="maxArrayLength"/>. The pool use <see cref="ArrayPool{T}.Shared"/>.
        /// </summary>
        /// <param name="length">Length of data (数据的长度).</param>
        /// <param name="alignment">The alignment value (in bytes) of the memory block. This must be a power of <c>2</c>. When it is 1, it means no alignment is required. When it is 0, use the previous value (内存块的对齐值（以字节为单位）. 这必须是 2的幂. 为 1时表示无需对齐.为0时使用上一次的值).</param>
        /// <param name="flags">Memory alloc flags (内存分配标志). This class supports these flags: <see cref="MemoryAllocFlags.ClearAlloc"/>, <see cref="MemoryAllocFlags.ClearFree"/>, <see cref="MemoryAllocFlags.NoPressure"/>.</param>
        /// <param name="maxArrayLength">Maximum array length for array pool allocation. Defaults to <see cref="ExSpansGlobal.PoolMaxArrayLength"/> if it is 0 (数组池分配时的最大数组长度. 它为0时默认为 <see cref="ExSpansGlobal.PoolMaxArrayLength"/>). </param>
        /// <exception cref="ArgumentOutOfRangeException">The length parameter must be greater than or equal to 0. The length parameter out of array max length.</exception>
        public AllocExMemoryManager(TSize length, TSize alignment = 0, MemoryAllocFlags flags = default, TSize maxArrayLength = 0)
            : base(ArrayPool<T>.Shared, length, alignment, flags, maxArrayLength) {
        }

        /// <summary>
        /// Create AllocExMemoryManager. It contains parameters <paramref name="length"/>. The pool use <see cref="ArrayPool{T}.Shared"/>.
        /// </summary>
        /// <param name="length">Length of data (数据的长度).</param>
        /// <exception cref="ArgumentOutOfRangeException">The length parameter must be greater than or equal to 0. The length parameter out of array max length.</exception>
        public AllocExMemoryManager(TSize length)
            : this(length, 0) {
        }

    }

}
