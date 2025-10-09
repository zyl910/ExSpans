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

namespace Zyl.ExSpans.Buffers {

    /// <summary>
    /// Function for measure capacity (测量容量的函数).
    /// </summary>
    /// <param name="sender">The sender (发送者),</param>
    /// <param name="oldLength">Old length (旧的长度).</param>
    /// <param name="oldCapacity">Old capacity (旧的容量).</param>
    /// <param name="newLength">New length (新的长度).</param>
    /// <param name="newAlignment">New alignment value (in bytes) of the memory block. This must be a power of <c>2</c>. When it is 1, it means no alignment is required. When it is 0, use the previous value (新的内存块的对齐值（以字节为单位）. 这必须是 2的幂. 为 1时表示无需对齐.为0时使用上一次的值).</param>
    /// <param name="newCapacity">New capacity (新的容量).</param>
    /// <param name="suggestCapacity">Suggest capacity. It is valid when it is greater than 0 (建议容量. 它大于 0 时有效).</param>
    /// <returns>Returns the modified capacity. When not modified, The <paramref name="newCapacity"/> should be returned (返回修改后的容量. 不修改时, 应返回 newCapacity).</returns>
    /// <remarks>
    /// <para>If <paramref name="suggestCapacity"/> is greater than 0, it means only adjusting the capacity. It is recommended not to modify the capacity at this time, but to directly return <paramref name="newCapacity"/> (如果 suggestCapacity 大于 0, 表示仅调整容量. 建议此时不要修改容量, 而是直接返回 newCapacity)</para>
    /// </remarks>
    public delegate nint MeasureCapacityFunc(object sender, nint oldLength, nint oldCapacity, nint newLength, nint newAlignment, nint newCapacity, nint suggestCapacity);

    /// <summary>
    /// A abstract memory manager that supports automatic memory reallocation and alignment. When the length is less than <see cref="AbstractAllocExMemoryManager{T}.MaxArrayLength">MaxArrayLength</see>, it uses array Pooling; otherwise, it uses native memory
    /// (支持自动内存重新分配和对齐的抽象内存管理器. 当长度小于 <see cref="AbstractAllocExMemoryManager{T}.MaxArrayLength">MaxArrayLength</see> 时它使用数组池，否则它就使用原生内存).
    /// </summary>
    /// <typeparam name="T">The element type (元素的类型).</typeparam>
    public abstract class AbstractReallocExMemoryManager<T> : AbstractAllocExMemoryManager<T> where T : unmanaged {
        private readonly MeasureCapacityFunc? _onMeasureCapacity;

        /// <summary>
        /// Create AbstractReallocExMemoryManager. It contains parameters <paramref name="Pool"/>, <paramref name="length"/>, <paramref name="alignment"/>, <paramref name="flags"/>, <paramref name="maxArrayLength"/>, <paramref name="capacity"/>, <paramref name="onMeasureCapacity"/>.
        /// </summary>
        /// <param name="Pool">The <see cref="ArrayPool{T}"/> instance used to rent array. If it is null, only unmanaged memory will be used (用于租用数组的 <see cref="ArrayPool{T}"/> 实例. 若它为空, 则仅使用非托管内存).</param>
        /// <param name="length">Length of data (数据的长度).</param>
        /// <param name="alignment">The alignment value (in bytes) of the memory block. This must be a power of <c>2</c>. When it is 1, it means no alignment is required. When it is 0, use the previous value (内存块的对齐值（以字节为单位）. 这必须是 2的幂. 为 1时表示无需对齐.为0时使用上一次的值).</param>
        /// <param name="flags">Memory alloc flags (内存分配标志).</param>
        /// <param name="maxArrayLength">Maximum array length for array Pool allocation. Defaults to <see cref="ExSpansGlobal.PoolMaxArrayLength"/> if it is 0 (数组池分配时的最大数组长度. 它为0时默认为 <see cref="ExSpansGlobal.PoolMaxArrayLength"/>). </param>
        /// <param name="capacity">Initial capacity. It is valid when it is greater than <paramref name="length"/> (初始容量. 它大于 length 时有效).</param>
        /// <param name="onMeasureCapacity">Function for measure capacity (测量容量的函数).</param>
        /// <exception cref="ArgumentOutOfRangeException">The length parameter must be greater than or equal to 0. The length parameter out of array max length.</exception>
        public AbstractReallocExMemoryManager(ArrayPool<T>? Pool, TSize length, TSize alignment = 0, MemoryAllocFlags flags = default, TSize maxArrayLength = 0, TSize capacity = 0, MeasureCapacityFunc? onMeasureCapacity = null)
            : base(Pool, length, alignment, flags, maxArrayLength, capacity) {
            _onMeasureCapacity = onMeasureCapacity;
        }

        /// <summary>
        /// [Dangerous] Reallocate memory based on the new length. It is a dangerous operation that will invalidate any <see cref="Span{T}"/> or <see cref="Memory{T}"/> previously created, so please recreate the Span or Memory type after the call (根据新的长度，重新分配内存. 它是危险操作, 会导致先前创建的 Span 或 Memory 均失效, 请在调用后重新创建 Span 或 Memory 等类型).
        /// </summary>
        /// <param name="suggestCapacity">Suggest capacity. It is valid when it is greater than 0 (建议容量. 它大于 0 时有效).</param>
        /// <exception cref="OutOfMemoryException">There is not enough memory available on the system</exception>
        public virtual void DangerousSetCapacity(TSize suggestCapacity) {
            Realloc(Length, 0, suggestCapacity);
        }

        /// <summary>
        /// [Dangerous] Set the capacity to the target value. It is a dangerous operation that will invalidate any <see cref="Span{T}"/> or <see cref="Memory{T}"/> previously created, so please recreate the Span or Memory type after the call (将容量设置为目标值. 它是危险操作, 会导致先前创建的 Span 或 Memory 均失效, 请在调用后重新创建 Span 或 Memory 等类型).
        /// </summary>
        /// <param name="length">New length (新的长度). If old Length is the same as new <paramref name="length"/>, it means only adjusting the capacity (如果 旧 Length 与 新 length 相同, 表示仅调整容量).</param>
        /// <param name="alignment">New alignment value (in bytes) of the memory block. This must be a power of <c>2</c>. When it is 1, it means no alignment is required. When it is 0, use the previous value (新的内存块的对齐值（以字节为单位）. 这必须是 2的幂. 为 1时表示无需对齐.为0时使用上一次的值).</param>
        /// <exception cref="OutOfMemoryException">There is not enough memory available on the system</exception>
        public virtual void DangerousRealloc(TSize length, TSize alignment = 0) {
            Realloc(length, alignment, 0);
        }

        /// <summary>
        /// [Dangerous] Reallocate memory based on the new length or capacity. (根据新的长度或容量，重新分配内存).
        /// </summary>
        /// <param name="length">New length (新的长度). If old Length is the same as new <paramref name="length"/>, it means only adjusting the capacity (如果 旧 Length 与 新 length 相同, 表示仅调整容量).</param>
        /// <param name="alignment">New alignment value (in bytes) of the memory block. This must be a power of <c>2</c>. When it is 1, it means no alignment is required. When it is 0, use the previous value (新的内存块的对齐值（以字节为单位）. 这必须是 2的幂. 为 1时表示无需对齐.为0时使用上一次的值).</param>
        /// <param name="suggestCapacity">Suggest capacity. It is valid when it is greater than 0 (建议容量. 它大于 0 时有效).</param>
        /// <exception cref="OutOfMemoryException">There is not enough memory available on the system</exception>
        /// <exception cref="ArgumentOutOfRangeException">The length parameter must be greater than or equal to 0. The length parameter out of array max length.</exception>
        protected virtual void Realloc(TSize length, TSize alignment, TSize suggestCapacity) {
            // Check.
            if (length < 0) {
                throw new ArgumentOutOfRangeException(nameof(length), "The length parameter must be greater than or equal to 0.");
            }
            if (suggestCapacity < 0) {
                throw new ArgumentOutOfRangeException(nameof(suggestCapacity), "The suggestCapacity parameter must be greater than or equal to 0.");
            }
            PointerUtil.CheckAlignmentValidOrUnused(alignment);
            //bool alignmentUsed = PointerUtil.IsAlignmentUsed(alignment);
            // Do.
            TSize newCapacity = ReallocMeasure(length, alignment, suggestCapacity);
            ReallocProcess(length, alignment, newCapacity, suggestCapacity);
        }

        /// <summary>
        /// [Dangerous] Reallocate memory - Measure suggestCapacity (重新分配内存 - 测量容量).
        /// </summary>
        /// <param name="length">New length (新的长度). If old Length is the same as new <paramref name="length"/>, it means only adjusting the suggestCapacity (如果 旧 Length 与 新 length 相同, 表示仅调整容量).</param>
        /// <param name="alignment">New alignment value (in bytes) of the memory block. This must be a power of <c>2</c>. When it is 1, it means no alignment is required. When it is 0, use the previous value (新的内存块的对齐值（以字节为单位）. 这必须是 2的幂. 为 1时表示无需对齐.为0时使用上一次的值).</param>
        /// <param name="suggestCapacity">Suggest capacity. It is valid when it is greater than 0 (建议容量. 它大于 0 时有效).</param>
        /// <returns>Returns the modified suggestCapacity (返回修改后的容量).</returns>
        protected virtual nint ReallocMeasure(TSize length, TSize alignment, TSize suggestCapacity) {
            TSize newCapacity = ReallocMeasureBody(length, alignment, suggestCapacity);
            newCapacity = ReallocMeasureFire(length, alignment, newCapacity, suggestCapacity);
            return newCapacity;
        }

        /// <summary>
        /// [Dangerous] Reallocate memory - Measure suggestCapacity - Body (重新分配内存 - 测量容量 - 主体).
        /// </summary>
        /// <inheritdoc cref="ReallocMeasure(nint, nint, nint)"/>
        protected virtual nint ReallocMeasureBody(TSize length, TSize alignment, TSize suggestCapacity) {
            if (suggestCapacity > 0) {
                if (suggestCapacity < length) {
                    throw new ArgumentOutOfRangeException(nameof(suggestCapacity), string.Format("The suggestCapacity({0}) parameter must be greater than length({1}).", (long)suggestCapacity, (long)length));
                }
                return suggestCapacity;
            }
            TSize newCapacity = this.Capacity;
            if (this.Length == length) {
                // Keep old.
            } else if (this.Length < length) {
                // Grow.
                newCapacity = unchecked(this.Capacity * 2);
                if (newCapacity < 0 || newCapacity < length) {
                    newCapacity = length;
                }
            } else {
                // Trim.
                if (Flags.HasFlag(MemoryAllocFlags.TrimOnHalf)) {
                    nint m = this.Capacity / 2; // 50%
                    if (length <= m) {
                        newCapacity = length;
                    } else {
                        // Keep old.
                        //newCapacity = this.Capacity;
                    }
                } else {
                    // Keep old.
                }
            }
            return newCapacity;
        }

        /// <summary>
        /// [Dangerous] Reallocate memory - Measure suggestCapacity - Fire notify (重新分配内存 - 测量容量 - 触发通知).
        /// </summary>
        /// <inheritdoc cref="ReallocMeasure(nint, nint, nint)"/>
        protected virtual nint ReallocMeasureFire(TSize length, TSize alignment, TSize capacity, TSize suggestCapacity) {
            MeasureCapacityFunc? func = OnMeasureCapacity;
            if (func != null) {
                capacity = func(this, this.Length, this.Capacity, length, alignment, capacity, suggestCapacity);
            }
            return capacity;
        }

        /// <summary>
        /// [Dangerous] Reallocate memory - Process (重新分配内存 - 处理).
        /// </summary>
        /// <param name="length">New length (新的长度). If old Length is the same as new <paramref name="length"/>, it means only adjusting the capacity (如果 旧 Length 与 新 length 相同, 表示仅调整容量).</param>
        /// <param name="alignment">New alignment value (in bytes) of the memory block. This must be a power of <c>2</c>. When it is 1, it means no alignment is required. When it is 0, use the previous value (新的内存块的对齐值（以字节为单位）. 这必须是 2的幂. 为 1时表示无需对齐.为0时使用上一次的值).</param>
        /// <param name="capacity">New capacity (新的容量).</param>
        /// <param name="suggestCapacity">Suggest capacity. It is valid when it is greater than 0 (建议容量. 它大于 0 时有效).</param>
        /// <exception cref="OutOfMemoryException">There is not enough memory available on the system</exception>
        protected virtual void ReallocProcess(TSize length, TSize alignment, TSize capacity, TSize suggestCapacity) {
            bool triedLength = false;
            if (capacity == this.Capacity && 0 == suggestCapacity) {
                triedLength = true;
                if (ReallocProcessLength(length, alignment, capacity)) {
                    return;
                }
            }
            // Try capacity.
            try {
                ReallocProcessCapacity(length, alignment, capacity, suggestCapacity);
            } catch (Exception ex) {
                Debug.WriteLine(string.Format("The ReallocProcessCapacity run fail! The params is ({0}, {1}, {2}). {3}", (long)length, (long)alignment, (long)capacity, ex.Message));
                if (length >= capacity || suggestCapacity > 0) {
                    throw;
                }
            }
            // Try length.
            try {
                ReallocProcessCapacity(length, alignment, length, 0);
            } catch (Exception ex) {
                Debug.WriteLine(string.Format("The ReallocProcessCapacity run fail! The params is ({0}, {1}, {2}). {3}", (long)length, (long)alignment, (long)length, ex.Message));
                // try ReallocProcessLength.
                if (triedLength) {
                    throw;
                }
            }
            // Fallback.
            if (ReallocProcessLength(length, alignment, capacity)) {
                return;
            }
            throw new OutOfMemoryException($"There is not enough memory available on the system. length={(long)length}, alignment={(long)alignment}.");
        }

        /// <summary>
        /// [Dangerous] Reallocate memory - Process capacity change (重新分配内存 - 处理容量变化).
        /// </summary>
        /// <inheritdoc cref="ReallocProcess(nint, nint, nint, nint)"/>
        protected unsafe virtual void ReallocProcessCapacity(TSize length, TSize alignment, TSize capacity, TSize suggestCapacity) {
            if (capacity < length) {
                throw new ArgumentOutOfRangeException(nameof(capacity), string.Format("The capacity({0}) parameter must be greater than length({1}).", (long)capacity, (long)length));
            }
            bool isClearAlloc = Flags.HasFlag(MemoryAllocFlags.ClearAlloc);
            bool isClearFree = Flags.HasFlag(MemoryAllocFlags.ClearFree);
            bool isKeepRealloc = Flags.HasFlag(MemoryAllocFlags.KeepRealloc);
            bool isNoPressure = Flags.HasFlag(MemoryAllocFlags.NoPressure);
            bool needFreeOld = false;
            T[]? arrayOld = DataArray;
            bool arrayOldIsPin = arrayOld is not null && null != PointerAligned;
            if (capacity <= 0) {
                // TrimExcess all.
                if (Capacity > 0) {
                    if (arrayOld is not null) {
                        if (arrayOldIsPin) {
                            ArrayHandle.Free();
                        }
                        // Free DataArray.
                        if (Pool is not null) {
                            Pool.Return(arrayOld, isClearFree);
                        } else {
                            if (isClearFree) {
                                GetExSpan().Clear();
                            }
                        }
                    } else if (null != PointerAligned && ByteCount > 0) {
                        if (isClearFree) {
                            GetExSpan().Clear();
                        }
#if NATIVE_MEMORY_ALIGNED
                        NativeMemory.Free(PointerAligned);
#else
                        void* pointer = (byte*)PointerAligned - Offset;
                        ExNativeMemory.Free(pointer);
#endif // NATIVE_MEMORY_ALIGNED
                        if (!isNoPressure) {
                            GC.RemoveMemoryPressure(ByteCount);
                        }
                    }
                    Length = length;
                    Alignment = alignment;
                    Capacity = capacity;
                    DataArray = null;
                    ArrayHandle = default;
                    ByteCount = 0;
                    Offset = 0;
                    PointerAligned = null;
                }
                return;
            }
            // == On (0 != capacity) ==
            T[]? array = null;
            GCHandle arrayHandle = default;
            TSize byteCount = 0;
            TSize offset = 0;
            void* pointerAligned = null;
            if (0 == alignment) alignment = Alignment;
            bool alignmentUsed = PointerUtil.IsAlignmentUsed(alignment);
            // Try array.
            TSize itemsOfAlignment = 0;
            if (alignmentUsed) {
                itemsOfAlignment = PointerUtil.GetEnoughItemCount(alignment, Unsafe.SizeOf<T>());
            }
            TSize capacityFull = capacity + itemsOfAlignment;
            if (Pool is not null && PointerUtil.IsArrayLengthValidInPool(capacityFull, MaxArrayLength)) {
                try {
                    array = Pool.Rent((int)capacityFull);
                } catch (Exception ex) {
                    Debug.WriteLine(string.Format("Array Pool rent array fail! The length is {0}. {1}", (long)capacity, ex.Message));
                }
                if (array is not null) {
                    try {
                        if (alignmentUsed) {
                            arrayHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
#if NETSTANDARD2_0_OR_GREATER || NETCOREAPP1_0_OR_GREATER || NET40_OR_GREATER
                            System.Threading.Thread.MemoryBarrier();
#endif // NETSTANDARD2_0_OR_GREATER || NETCOREAPP1_0_OR_GREATER || NET40_OR_GREATER
                            void* pointer = Unsafe.AsPointer(ref array[0]);
                            offset = PointerUtil.GetAlignOffset(pointer, alignment);
                            pointerAligned = (byte*)pointer + offset;
                        }
                        capacity = array.Length - itemsOfAlignment;
                        needFreeOld = true;
                    } catch (Exception ex) {
                        Debug.WriteLine(string.Format("Array Pool config array fail! The length is {0}. {1}", (long)length, ex.Message));
                        try {
                            Pool.Return(array);
                        } catch (Exception ex1) {
                            Debug.WriteLine(string.Format("Array Pool return array fail! The length is {0}. {1}", (long)length, ex1.Message));
                        }
                        array = null;
                        arrayHandle = default;
                        pointerAligned = null;
                        offset = 0;
                    }
                }
            }
            // Try native memory.
            if (null == array) {
                bool alignmentUsedOld = PointerUtil.IsAlignmentUsed(alignment);
                void* pointer = null;
                TSize byteCountBody = checked(capacity * Unsafe.SizeOf<T>());
                byteCount = byteCountBody;
                if (alignmentUsed) {
#if NATIVE_MEMORY_ALIGNED
#else
                    byteCount = checked(byteCountBody + alignment);
#endif // NATIVE_MEMORY_ALIGNED
                }
                // Try realloc.
                if (!isKeepRealloc && null == arrayOld && null != PointerAligned && ByteCount > 0) {
                    try {
                        bool allocRealloc = false;
                        if (alignmentUsedOld) {
                            if (alignmentUsed) {
#if NATIVE_MEMORY_ALIGNED
                                pointer = NativeMemory.AlignedRealloc(PointerAligned, (nuint)byteCount, (nuint)alignment);
#else
                                // Not support Realloc.
#endif // NATIVE_MEMORY_ALIGNED
                            } else {
                                allocRealloc = false; // AligndAlloc may not necessarily support Realloc.
                            }
                        } else {
                            allocRealloc = true;
                        }
                        if (allocRealloc) {
                            pointer = ExNativeMemory.Realloc((byte*)PointerAligned - Offset, (nuint)byteCount);
                        }
                    } catch (Exception ex) {
                        Debug.WriteLine(string.Format("Native memory realloc fail! The length is {0}. {1}", (long)capacity, ex.Message));
                    }
                }
                if (null != pointer) {
                    if (!isNoPressure) {
                        GC.RemoveMemoryPressure(this.ByteCount);
                        GC.AddMemoryPressure(byteCount);
                    }
                    if (alignmentUsed) {
#if NATIVE_MEMORY_ALIGNED
#else
                        offset = PointerUtil.GetAlignOffset(pointer, alignment);
#endif // NATIVE_MEMORY_ALIGNED
                    }
                    pointerAligned = (byte*)pointer + offset;
                } else {
                    // Try alloc.
                    needFreeOld = true;
                    if (alignmentUsed) {
#if NATIVE_MEMORY_ALIGNED
                        pointer = NativeMemory.AlignedAlloc((nuint)byteCount, (nuint)alignment);
#else
                        // Use `if (null == pointer)`.
#endif // NATIVE_MEMORY_ALIGNED
                    }
                    if (null == pointer) {
                        pointer = ExNativeMemory.Alloc((nuint)byteCount);
                    }
                    if (!isNoPressure) {
                        GC.AddMemoryPressure(byteCount);
                    }
                    if (alignmentUsed) {
#if NATIVE_MEMORY_ALIGNED
#else
                        offset = PointerUtil.GetAlignOffset(pointer, alignment);
#endif // NATIVE_MEMORY_ALIGNED
                    }
                    pointerAligned = (byte*)pointer + offset;
                }
            }
            // Clear and more.
            ExSpan<T> spanNew = (null == pointerAligned) ? new ExSpan<T>(array, 0, capacity) : new ExSpan<T>(pointerAligned, capacity);
            if (isKeepRealloc && this.Length > 0) {
                ExSpan<T> spanOld = this.GetExSpan();
                if (spanNew.Length <= spanOld.Length) {
                    spanOld.Slice(0, spanNew.Length).CopyTo(spanNew);
                } else {
                    spanOld.CopyTo(spanNew);
                    if (isClearAlloc) {
                        spanNew.Slice(spanOld.Length).Clear();
                    }
                }
            } else {
                if (isClearAlloc) {
                    spanNew.Clear();
                }
            }
            // Free old.
            if (needFreeOld) {
                if (arrayOld is not null) {
                    if (arrayOldIsPin) {
                        ArrayHandle.Free();
                    }
                    // Free DataArray.
                    if (arrayOld is not null && Pool is not null) {
                        Pool.Return(arrayOld, isClearFree);
                    }
                } else if (null != PointerAligned && ByteCount > 0) {
                    if (isClearFree) {
                        GetExSpan().Clear();
                    }
#if NATIVE_MEMORY_ALIGNED
                    NativeMemory.Free(PointerAligned);
#else
                    void* pointer = (byte*)PointerAligned - Offset;
                    ExNativeMemory.Free(pointer);
#endif // NATIVE_MEMORY_ALIGNED
                    if (!isNoPressure) {
                        GC.RemoveMemoryPressure(ByteCount);
                    }
                }
            }
            // == Done ==
            Length = length;
            Alignment = alignment;
            Capacity = capacity;
            DataArray = array;
            ArrayHandle = arrayHandle;
            ByteCount = byteCount;
            Offset = offset;
            PointerAligned = pointerAligned;
        }

        /// <summary>
        /// [Dangerous] Reallocate memory - Process length change (重新分配内存 - 处理长度变化).
        /// </summary>
        /// <param name="length">New length (新的长度). If old Length is the same as new <paramref name="length"/>, it means only adjusting the capacity (如果 旧 Length 与 新 length 相同, 表示仅调整容量).</param>
        /// <param name="alignment">New alignment value (in bytes) of the memory block. This must be a power of <c>2</c>. When it is 1, it means no alignment is required. When it is 0, use the previous value (新的内存块的对齐值（以字节为单位）. 这必须是 2的幂. 为 1时表示无需对齐.为0时使用上一次的值).</param>
        /// <param name="capacity">New capacity (新的容量).</param>
        /// <returns>Returns true if successful, false otherwise (成功时返回true, 否则为false).</returns>
        protected unsafe virtual bool ReallocProcessLength(TSize length, TSize alignment, TSize capacity) {
            _ = capacity;
            TSize oldLength = this.Length;
            T[]? oldArray = this.DataArray;
            TSize alignmentActual = alignment;
            if (0 == alignmentActual) alignmentActual = this.Alignment;
            bool alignmentUsedNew = PointerUtil.IsAlignmentUsed(alignmentActual);
            bool alignmentUsedUnusedAlway = (alignment <= 1 && this.Alignment <= 1);
            if (oldLength == length && alignmentActual == this.Alignment) {
                // No change.
                if (alignment > 0) {
                    this.Alignment = alignment;
                }
                return true;
            }
            if (length > this.Capacity) {
                return false;
            }
            if ((alignmentActual != this.Alignment && !alignmentUsedUnusedAlway) && oldArray is null) {
#if NATIVE_MEMORY_ALIGNEDs
                // Changed alignment.
                return false;
#else
                // Next.
#endif // NATIVE_MEMORY_ALIGNED
            }
            // Check body.
            if (alignment == 0 || alignment <= this.Alignment || alignmentUsedUnusedAlway) {
                // OK.
            } else {
                // -- When alignment > this.Alignment
                if (Flags.HasFlag(MemoryAllocFlags.KeepRealloc)) {
                    return false;
                }
                if (oldArray is not null) {
                    TSize itemsOfAlignment = PointerUtil.GetEnoughItemCount(alignment, Unsafe.SizeOf<T>());
                    if ((oldArray.Length - itemsOfAlignment) < length) {
                        return false;
                    }
                    // Pin.
                    if (alignmentUsedNew && null == PointerAligned) {
                    }
                    // Set PointerAligned.
                    ArrayHandle = GCHandle.Alloc(oldArray, GCHandleType.Pinned);
#if NETSTANDARD2_0_OR_GREATER || NETCOREAPP1_0_OR_GREATER || NET40_OR_GREATER
                    System.Threading.Thread.MemoryBarrier();
#endif // NETSTANDARD2_0_OR_GREATER || NETCOREAPP1_0_OR_GREATER || NET40_OR_GREATER
                    void* pointer = Unsafe.AsPointer(ref oldArray[0]);
                    Offset = PointerUtil.GetAlignOffset(pointer, Alignment);
                    PointerAligned = (byte*)pointer + Offset;
                } else {
#if NATIVE_MEMORY_ALIGNEDs
                    return false;
#else
                    TSize byteCountBody = checked(capacity * Unsafe.SizeOf<T>());
                    TSize byteCount = byteCountBody + alignment;
                    if (byteCount > this.ByteCount) {
                        return false;
                    }
                    // Set PointerAligned.
                    void* pointer = (byte*)PointerAligned - Offset;
                    Offset = PointerUtil.GetAlignOffset(pointer, alignment);
                    PointerAligned = (byte*)pointer + Offset;
                    Capacity = (this.ByteCount - alignment) / Unsafe.SizeOf<T>();
#endif // NATIVE_MEMORY_ALIGNED
                }
            }
            // Done.
            this.Length = length;
            if (alignment > 0) {
                this.Alignment = alignment;
            }
            if (Flags.HasFlag(MemoryAllocFlags.ClearAlloc)) {
                ExSpan<T> span = GetExSpan();
                if (Flags.HasFlag(MemoryAllocFlags.KeepRealloc)) {
                    if (length > oldLength) {
                        span = span.Slice(oldLength);
                        span.Clear();
                    } else {
                        // No.
                    }
                } else {
                    span.Clear();
                }
            }
            return true;
        }

        /// <summary>
        /// [Dangerous] Sets the capacity to the actual length. It can be used to free up excess memory. It is a dangerous operation that will invalidate any <see cref="Span{T}"/> or <see cref="Memory{T}"/> previously created, so please recreate the Span or Memory type after the call (将容量设置为实际长度. 可用它释放多余的内存. 它是危险操作, 会导致先前创建的 Span 或 Memory 均失效, 请在调用后重新创建 Span 或 Memory 等类型).
        /// </summary>
        public virtual void TrimExcess() {
            DangerousSetCapacity(Length);
        }

        /// <summary>Function for measure capacity (测量容量的函数).</summary>
        public MeasureCapacityFunc? OnMeasureCapacity {
            get => _onMeasureCapacity;
        }

    }
}
