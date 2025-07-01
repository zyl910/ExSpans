#if NET6_0_OR_GREATER
#define NATIVE_MEMORY_ALIGNED // NativeMemory.AlignedAlloc Method. https://learn.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.nativememory.alignedalloc
#endif // NET6_0_OR_GREATER

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Zyl.ExSpans.Impl;

namespace Zyl.ExSpans.Buffers {
    /// <summary>
    /// A memory manager that supports automatic memory allocation and alignment. When the length is less than <see cref="ExSpansGlobal.PoolMaxArrayLength"/>, it uses array pooling; otherwise, it uses native memory
    /// (支持自动内存分配和对齐的内存管理器. 当长度小于 PoolMaxArrayLength 时它使用数组池，否则它就使用原生内存).
    /// </summary>
    /// <typeparam name="T">The element type (元素的类型).</typeparam>
    public unsafe abstract class AbstractAllocExMemoryManager<T> : AbstractArrayExMemoryManager<T> {
        private TSize _alignment = 0;
        private TSize _capacity = 0;
        private TSize _offset = 0;
        private void* _pointerAligned = null;
        private GCHandle _arrayHandle = default;

        /// <summary>
        /// Create ArrayExMemoryManager. It contains parameters <paramref name="length"/>, <paramref name="pool"/>, <paramref name="flags"/>.
        /// </summary>
        /// <param name="pool">The <see cref="ArrayPool{T}"/> instance used to rent array. If it is null, only unmanaged memory will be used (用于租用数组的 <see cref="ArrayPool{T}"/> 实例. 若它为空, 则仅使用非托管内存).</param>
        /// <param name="length">Length of unmanaged data (非托管数据的长度).</param>
        /// <param name="alignment">The alignment value (in bytes) of the memory block. This must be a power of <c>2</c>. When it is 1, it means no alignment is required. When it is 0, use the previous value (内存块的对齐值（以字节为单位）. 这必须是 2的幂. 为 1时表示无需对齐.为0时使用上一次的值).</param>
        /// <param name="flags">Memory alloc flags (内存分配标志). This class supports these flags: <see cref="MemoryAllocFlags.ClearAlloc"/>, <see cref="MemoryAllocFlags.ClearFree"/>, <see cref="MemoryAllocFlags.NoPressure"/>.</param>
        /// <exception cref="ArgumentOutOfRangeException">The length parameter must be greater than or equal to 0. The length parameter out of array max length.</exception>
        public AbstractAllocExMemoryManager(ArrayPool<T>? pool, TSize length, TSize alignment = 0, MemoryAllocFlags flags = default) : base(flags) {
            // Check.
            if (length < 0) {
                throw new ArgumentOutOfRangeException(nameof(length), "The length parameter must be greater than or equal to 0.");
            }
            PointerUtil.CheckAlignmentValidOrUnused(alignment);
            bool alignmentUsed = PointerUtil.IsAlignmentUsed(alignment);
            Pool = pool;
            Length = length;
            Alignment = alignment;
            if (length <= 0) {
                return;
            }
            // Try array.
            TSize capacity = length; // TODO: T 不是字节时, 计算结果不对. 拟改为 byteCountRaw.
            if (alignmentUsed) {
                capacity += alignment;
            }
            Offset = 0;
            PointerAligned = null;
            if (pool is not null && PointerUtil.IsArrayLengthValidInPool(capacity)) {
                try {
                    DataArray = pool.Rent((int)capacity);
                } catch (Exception ex) {
                    Debug.WriteLine(string.Format("Array pool rent array fail! The length is {0}. {1}", length, ex.Message));
                }
                if (DataArray is not null) {
                    try {
                        if (Flags.HasFlag(MemoryAllocFlags.ClearAlloc)) {
                            DataArray.AsExSpan().Clear();
                        }
                        if (alignmentUsed) {
                            ArrayHandle = GCHandle.Alloc(DataArray, GCHandleType.Pinned);
#if NETSTANDARD2_0_OR_GREATER || NETCOREAPP1_0_OR_GREATER || NET40_OR_GREATER
                            System.Threading.Thread.MemoryBarrier();
#endif // NETSTANDARD2_0_OR_GREATER || NETCOREAPP1_0_OR_GREATER || NET40_OR_GREATER
                            void* pointer = Unsafe.AsPointer(ref DataArray[0]);
                            Offset = PointerUtil.GetAlignOffset(pointer, Alignment);
                            PointerAligned = (byte*)pointer + Offset;
                        }
                        // Done.
                        Capacity = capacity;
                        return;
                    } catch (Exception ex) {
                        Debug.WriteLine(string.Format("Array pool config array fail! The length is {0}. {1}", length, ex.Message));
                        try {
                            pool.Return(DataArray);
                            DataArray = null;
                        } catch (Exception ex1) {
                            Debug.WriteLine(string.Format("Array pool return array fail! The length is {0}. {1}", length, ex1.Message));
                        }
                    }
                }
            }
            // Try native memory.
            try {
                nint byteCount = checked(length * Unsafe.SizeOf<T>());
                nint byteCountRaw = byteCount;
                if (alignmentUsed) {
#if NATIVE_MEMORY_ALIGNED
                    Capacity = length;
                    //Offset = 0;
                    PointerAligned = NativeMemory.AlignedAlloc((nuint)byteCountRaw, (nuint)alignment);
                    if (!Flags.HasFlag(MemoryAllocFlags.NoPressure)) {
                        GC.AddMemoryPressure(byteCountRaw);
                    }
                    if (Flags.HasFlag(MemoryAllocFlags.ClearAlloc)) {
                        ExMemoryMarshal.ClearWithoutReferences(ref Unsafe.AsRef<byte>(PointerAligned), (nuint)byteCountRaw);
                    }
                    return;
#else
                    byteCountRaw = checked(byteCount + alignment);
#endif // NATIVE_MEMORY_ALIGNED
                } else {
                    // byteCountRaw = byteCount;
                }
                void* pointer = ExNativeMemory.Alloc((nuint)byteCountRaw);
                if (!Flags.HasFlag(MemoryAllocFlags.NoPressure)) {
                    GC.AddMemoryPressure(byteCountRaw);
                }
                if (Flags.HasFlag(MemoryAllocFlags.ClearAlloc)) {
                    ExMemoryMarshal.ClearWithoutReferences(ref Unsafe.AsRef<byte>(pointer), (nuint)byteCountRaw);
                }
                if (alignmentUsed) {
                    Offset = PointerUtil.GetAlignOffset(pointer, Alignment);
                } else {
                    //Offset = 0;
                }
                PointerAligned = (byte*)pointer + Offset;
                // Done.
                Capacity = capacity;
            } catch (Exception ex) {
                Debug.WriteLine(string.Format("Alloc native memory fail! The length is {0}. {1}", length, ex.Message));
                throw;
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing) {
            bool oldIsDisposed = IsDisposed;
            try {
                if (!oldIsDisposed && Length > 0) {
                    bool alignmentUsed = PointerUtil.IsAlignmentUsed(Alignment);
                    T[]? array = DataArray;
                    if (array is not null) {
                        if (alignmentUsed) {
                            ArrayHandle.Free();
                            ArrayHandle = default;
                        }
                        // base free DataArray.
                    } else if (null != PointerAligned) {
#if NATIVE_MEMORY_ALIGNED
                        NativeMemory.Free(PointerAligned);
#else
                        void* pointer = (byte*)PointerAligned - Offset;
                        ExNativeMemory.Free(pointer);
#endif // NATIVE_MEMORY_ALIGNED
                        PointerAligned = null;
                    }
                }
            } finally {
                base.Dispose(disposing);
            }
        }

        /// <inheritdoc/>
        public override ExSpan<T> GetExSpan() {
            if (Length <= 0) {
                return ExSpan<T>.Empty;
            }
            bool alignmentUsed = PointerUtil.IsAlignmentUsed(Alignment);
            if (DataArray is not null && !alignmentUsed) {
                return new ExSpan<T>(DataArray, 0, Length);
            }
            return new ExSpan<T>(PointerAligned, Length);
        }

        /// <inheritdoc/>
        public override Span<T> GetSpan() {
            return GetExSpan().AsSpan();
        }

        /// <inheritdoc cref="IPinnable.Pin(int)"/>
        public unsafe override MemoryHandle Pin(int elementIndex = 0) {
            if (Length <= 0) {
                return default;
            }
            if (elementIndex >= Length) {
                throw new ArgumentOutOfRangeException(nameof(elementIndex), string.Format("The elementIndex({0}) parameter out of length({1}).", (long)elementIndex, (long)Length));
            }
            bool alignmentUsed = PointerUtil.IsAlignmentUsed(Alignment);
            void* pointer;
            if (DataArray is not null) {
                if (!alignmentUsed) {
                    GCHandle handle = GCHandle.Alloc(DataArray, GCHandleType.Pinned);
#if NETSTANDARD2_0_OR_GREATER || NETCOREAPP1_0_OR_GREATER || NET40_OR_GREATER
                    System.Threading.Thread.MemoryBarrier();
#endif // NETSTANDARD2_0_OR_GREATER || NETCOREAPP1_0_OR_GREATER || NET40_OR_GREATER
                    pointer = Unsafe.AsPointer(ref DataArray[elementIndex]);
                    return new MemoryHandle(pointer, handle);
                }
            }
            pointer = (byte*)PointerAligned + ((nint)Unsafe.SizeOf<T>() * elementIndex);
            return new MemoryHandle(pointer);
        }

        /// <summary>The alignment value (in bytes) of the memory block. This must be a power of <c>2</c>. When it is 1, it means no alignment is required. When it is 0, use the previous value (内存块的对齐值（以字节为单位）. 这必须是 2的幂. 为 1时表示无需对齐.为0时使用上一次的值).</summary>
        public TSize Alignment {
            get => _alignment;
            protected set => _alignment = value;
        }

        /// <summary>Pinned array handle (钉住数组的句柄).</summary>
        protected GCHandle ArrayHandle {
            get => _arrayHandle;
            set => _arrayHandle = value;
        }

        /// <summary>The value of the capacity. This value is not affected by alignment (容量值).</summary>
        protected TSize Capacity {
            get => _capacity;
            set => _capacity = value;
        }

        /// <summary>Offset of the aligned data (in bytes) (已对齐数据的偏移, 以字节为单位).</summary>
        protected TSize Offset {
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
