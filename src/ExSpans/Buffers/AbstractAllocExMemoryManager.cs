using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Zyl.ExSpans.Buffers {
    /// <summary>
    /// A memory manager that supports automatic memory allocation and alignment. When the length is less than <see cref="ExSpansGlobal.PoolMaxArrayLength"/>, it uses array pooling; otherwise, it uses native memory
    /// (支持自动内存分配和对齐的内存管理器. 当长度小于 PoolMaxArrayLength 时它使用数组池，否则它就使用原生内存).
    /// </summary>
    /// <typeparam name="T">The element type (元素的类型).</typeparam>
    public unsafe abstract class AbstractAllocExMemoryManager<T> : AbstractArrayExMemoryManager<T> {
        private nint _alignment = 0;
        private nint _capacityRaw = 0;
        private nint _offset = 0;
        private void* _pointerAligned = null;
        private GCHandle _arrayHandle = default;

        /// <summary>
        /// Create ArrayExMemoryManager. It contains parameters <paramref name="length"/>, <paramref name="pool"/>, <paramref name="flags"/>.
        /// </summary>
        /// <param name="length">Length of unmanaged data (非托管数据的长度).</param>
        /// <param name="pool">The <see cref="ArrayPool{T}"/> instance used to rent array. Defaults to <see cref="ArrayPool{T}.Shared"/> if it is null (用于租用数组的 <see cref="ArrayPool{T}"/> 实例. 它为空时默认为 <see cref="ArrayPool{T}.Shared"/>).</param>
        /// <param name="flags">Memory alloc flags (内存分配标志). This class supports these flags: <see cref="MemoryAllocFlags.ClearAlloc"/>, <see cref="MemoryAllocFlags.ClearFree"/>.</param>
        /// <exception cref="ArgumentOutOfRangeException">The length parameter must be greater than or equal to 0. The length parameter out of array max length.</exception>
        public AbstractAllocExMemoryManager(nint length, ArrayPool<T>? pool, MemoryAllocFlags flags = default) : base(length, pool, flags) {
            // TODO.
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing) {
            bool oldIsDisposed = IsDisposed;
            try {
                if (oldIsDisposed) {
                    // TODO.
                }
            } finally {
                base.Dispose(disposing);
            }
        }

        /// <summary>The alignment value (in bytes) of the memory block. This must be a power of <c>2</c>. When it is 1, it means no alignment is required. When it is 0, use the previous value (内存块的对齐值（以字节为单位）. 这必须是 2的幂. 为 1时表示无需对齐.为0时使用上一次的值).</summary>
        public nint Alignment {
            get => _alignment;
            protected set => _alignment = value;
        }

        /// <summary>Pinned array handle (钉住数组的句柄).</summary>
        protected GCHandle ArrayHandle {
            get => _arrayHandle;
            set => _arrayHandle = value;
        }

        /// <summary>The raw value of the capacity. This value is not affected by alignment (容量的原始值. 该值不受对齐的影响).</summary>
        protected nint CapacityRaw {
            get => _capacityRaw;
            set => _capacityRaw = value;
        }

        /// <summary>Offset of the aligned data (已对齐数据的偏移).</summary>
        protected nint Offset {
            get => _offset;
            set => _offset = value;
        }

        /// <summary>The aligned pointer to data. Subtract Offset to get the raw pointer (数据的已对齐指针. 减去 Offset 后可获得原始指针).</summary>
        [CLSCompliant(false)]
        protected void* PointerAligned {
            get => _pointerAligned;
            set => _pointerAligned = value;
        }

    }
}
