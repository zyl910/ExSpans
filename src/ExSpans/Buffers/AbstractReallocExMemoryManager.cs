#if NET6_0_OR_GREATER
#define NATIVE_MEMORY_ALIGNED // NativeMemory.AlignedAlloc Method. https://learn.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.nativememory.alignedalloc
#endif // NET6_0_OR_GREATER

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Zyl.ExSpans.Buffers {

    /// <summary>
    /// Function for measure capacity (测量容量的函数).
    /// </summary>
    /// <param name="sender">The sender (发送者),</param>
    /// <param name="oldLength">Old length (旧的长度).</param>
    /// <param name="oldCapacity">Old capacity (旧的容量).</param>
    /// <param name="newLength">New length (新的长度).</param>
    /// <param name="newCapacity">New capacity (新的容量).</param>
    /// <returns>Returns the modified capacity. When not modified, The <paramref name="newCapacity"/> should be returned (返回修改后的容量. 不修改时, 应返回 newCapacity).</returns>
    /// <remarks>
    /// <para>If <paramref name="oldLength"/> is the same as <paramref name="newLength"/>, it means only adjusting the capacity. It is recommended not to modify the capacity at this time, but to directly return <paramref name="newCapacity"/> (如果 oldLength 与 newLength 相同, 表示仅调整容量. 建议此时不要修改容量, 而是直接返回 newCapacity)</para>
    /// </remarks>
    public delegate nint MeasureCapacityFunc(object sender, nint oldLength, nint oldCapacity, nint newLength, nint newCapacity);

    /// <summary>
    /// A memory manager that supports automatic memory reallocation and alignment. When the length is less than <see cref="AbstractAllocExMemoryManager{T}.MaxArrayLength"/>, it uses array pooling; otherwise, it uses native memory
    /// (支持自动内存重新分配和对齐的内存管理器. 当长度小于 <see cref="AbstractAllocExMemoryManager{T}.MaxArrayLength"/> 时它使用数组池，否则它就使用原生内存).
    /// </summary>
    /// <typeparam name="T">The element type (元素的类型).</typeparam>
    public abstract class AbstractReallocExMemoryManager<T> : AbstractAllocExMemoryManager<T> {
        private readonly MeasureCapacityFunc? _onMeasureCapacity;

        /// <summary>
        /// Create AbstractReallocExMemoryManager. It contains parameters <paramref name="pool"/>, <paramref name="length"/>, <paramref name="alignment"/>, <paramref name="flags"/>, <paramref name="maxArrayLength"/>, <paramref name="capacity"/>, <paramref name="onMeasureCapacity"/>.
        /// </summary>
        /// <param name="pool">The <see cref="ArrayPool{T}"/> instance used to rent array. If it is null, only unmanaged memory will be used (用于租用数组的 <see cref="ArrayPool{T}"/> 实例. 若它为空, 则仅使用非托管内存).</param>
        /// <param name="length">Length of data (数据的长度).</param>
        /// <param name="alignment">The alignment value (in bytes) of the memory block. This must be a power of <c>2</c>. When it is 1, it means no alignment is required. When it is 0, use the previous value (内存块的对齐值（以字节为单位）. 这必须是 2的幂. 为 1时表示无需对齐.为0时使用上一次的值).</param>
        /// <param name="flags">Memory alloc flags (内存分配标志).</param>
        /// <param name="maxArrayLength">Maximum array length for array pool allocation. Defaults to <see cref="ExSpansGlobal.PoolMaxArrayLength"/> if it is 0 (数组池分配时的最大数组长度. 它为0时默认为 <see cref="ExSpansGlobal.PoolMaxArrayLength"/>). </param>
        /// <param name="capacity">Initial capacity. It is valid when it is greater than <paramref name="length"/> (初始容量. 它大于 length 时有效).</param>
        /// <param name="onMeasureCapacity">Function for measure capacity (测量容量的函数).</param>
        /// <exception cref="ArgumentOutOfRangeException">The length parameter must be greater than or equal to 0. The length parameter out of array max length.</exception>
        public AbstractReallocExMemoryManager(ArrayPool<T>? pool, TSize length, TSize alignment = 0, MemoryAllocFlags flags = default, TSize maxArrayLength = 0, TSize capacity = 0, MeasureCapacityFunc? onMeasureCapacity = null)
            : base(pool, length, alignment, flags, maxArrayLength, capacity) {
            _onMeasureCapacity = onMeasureCapacity;
        }

        /// <summary>
        /// [Dangerous] Reallocate memory based on the new length. It is a dangerous operation that will invalidate any <see cref="Span{T}"/> or <see cref="Memory{T}"/> previously created, so please recreate the Span or Memory type after the call (根据新的长度，重新分配内存. 它是危险操作, 会导致先前创建的 Span 或 Memory 均失效, 请在调用后重新创建 Span 或 Memory 等类型).
        /// </summary>
        /// <param name="capacity">New capacity (新的容量).</param>
        /// <exception cref="OutOfMemoryException">There is not enough memory available on the system</exception>
        public virtual void DangerousSetCapacity(TSize capacity) {
            Realloc(Length, 0, capacity);
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
        /// <param name="capacity">New capacity. It is valid when it is greater than <paramref name="length"/> (新的容量. 它大于 length 时有效).</param>
        /// <exception cref="OutOfMemoryException">There is not enough memory available on the system</exception>
        /// <exception cref="ArgumentOutOfRangeException">The length parameter must be greater than or equal to 0. The length parameter out of array max length.</exception>
        protected virtual void Realloc(TSize length, TSize alignment, TSize capacity) {
            // Check.
            if (length < 0) {
                throw new ArgumentOutOfRangeException(nameof(length), "The length parameter must be greater than or equal to 0.");
            }
            if (capacity < 0) {
                throw new ArgumentOutOfRangeException(nameof(capacity), "The capacity parameter must be greater than or equal to 0.");
            }
            PointerUtil.CheckAlignmentValidOrUnused(alignment);
            //bool alignmentUsed = PointerUtil.IsAlignmentUsed(alignment);
            // Do.
            TSize newCapacity = capacity;
            newCapacity = ReallocMeasure(length, alignment, newCapacity);
            ReallocProcess(length, alignment, newCapacity);
        }

        /// <summary>
        /// [Dangerous] Reallocate memory - Measure capacity (重新分配内存 - 测量容量).
        /// </summary>
        /// <param name="length">New length (新的长度). If old Length is the same as new <paramref name="length"/>, it means only adjusting the capacity (如果 旧 Length 与 新 length 相同, 表示仅调整容量).</param>
        /// <param name="alignment">New alignment value (in bytes) of the memory block. This must be a power of <c>2</c>. When it is 1, it means no alignment is required. When it is 0, use the previous value (新的内存块的对齐值（以字节为单位）. 这必须是 2的幂. 为 1时表示无需对齐.为0时使用上一次的值).</param>
        /// <param name="capacity">New capacity (新的容量).</param>
        /// <returns>Returns the modified capacity (返回修改后的容量).</returns>
        protected virtual nint ReallocMeasure(TSize length, TSize alignment, TSize capacity) {
            TSize newCapacity = ReallocMeasureBody(length, alignment, capacity);
            newCapacity = ReallocMeasureFire(length, alignment, newCapacity);
            return newCapacity;
        }

        /// <summary>
        /// [Dangerous] Reallocate memory - Measure capacity - Body (重新分配内存 - 测量容量 - 主体).
        /// </summary>
        /// <inheritdoc cref="ReallocMeasure(nint, nint, nint)"/>
        protected virtual nint ReallocMeasureBody(TSize length, TSize alignment, TSize capacity) {
            if (capacity > 0) {
                if (capacity < length) {
                    throw new ArgumentOutOfRangeException(nameof(capacity), string.Format("The capacity({0}) parameter must be greater than length({1}).", (long)capacity, (long)length));
                }
                return capacity;
            }
            if (this.Length == length) {
                if (capacity == 0 && length > 0) {
                    capacity = length;
                }
                return capacity;
            }
            if (this.Length < length) {
                // Grow.
                capacity = length + (length / 2); // + 50%
            } else {
                // Trim.
                if (Flags.HasFlag(MemoryAllocFlags.TrimOnHalf)) {
                    nint m = this.Capacity / 2; // 50%
                    if (length < m) {
                        capacity = length;
                    } else {
                        capacity = this.Capacity;
                    }
                } else {
                    capacity = this.Capacity;
                }
            }
            return capacity;
        }

        /// <summary>
        /// [Dangerous] Reallocate memory - Measure capacity - Fire notify (重新分配内存 - 测量容量 - 触发通知).
        /// </summary>
        /// <inheritdoc cref="ReallocMeasure(nint, nint, nint)"/>
        protected virtual nint ReallocMeasureFire(TSize length, TSize alignment, TSize capacity) {
            MeasureCapacityFunc? func = OnMeasureCapacity;
            if (func != null) {
                capacity = func(this, this.Length, this.Capacity, length, capacity);
            }
            return capacity;
        }

        /// <summary>
        /// [Dangerous] Reallocate memory - Process (重新分配内存 - 处理).
        /// </summary>
        /// <param name="length">New length (新的长度). If old Length is the same as new <paramref name="length"/>, it means only adjusting the capacity (如果 旧 Length 与 新 length 相同, 表示仅调整容量).</param>
        /// <param name="alignment">New alignment value (in bytes) of the memory block. This must be a power of <c>2</c>. When it is 1, it means no alignment is required. When it is 0, use the previous value (新的内存块的对齐值（以字节为单位）. 这必须是 2的幂. 为 1时表示无需对齐.为0时使用上一次的值).</param>
        /// <param name="capacity">New capacity (新的容量).</param>
        /// <exception cref="OutOfMemoryException">There is not enough memory available on the system</exception>
        protected virtual void ReallocProcess(TSize length, TSize alignment, TSize capacity) {
            if (capacity == this.Capacity) {
                if (ReallocProcessLength(length, alignment, capacity)) {
                    return;
                }
            }
            // Try capacity.
            try {
                ReallocProcessCapacity(length, alignment, capacity);
            } catch (Exception ex) {
                Debug.WriteLine(string.Format("The ReallocProcessCapacity run fail! The params is ({0}, {1}, {2}). {3}", (long)length, (long)alignment, (long)capacity, ex.Message));
                if (length >= capacity) {
                    throw;
                }
            }
            // Try length.
            try {
                ReallocProcessCapacity(length, alignment, length);
            } catch (Exception ex) {
                Debug.WriteLine(string.Format("The ReallocProcessCapacity run fail! The params is ({0}, {1}, {2}). {3}", (long)length, (long)alignment, (long)length, ex.Message));
                // try ReallocProcessLength.
                if (length != this.Length) {
                }
                throw;
            }
            // Try length.
            // Fallback.
            if (length > this.Capacity) {
                ReallocProcessCapacity(length, alignment, length);
            } else {
                ReallocProcessLength(length, alignment, this.Capacity);
            }
        }

        /// <summary>
        /// [Dangerous] Reallocate memory - Process capacity change (重新分配内存 - 处理容量变化).
        /// </summary>
        /// <inheritdoc cref="ReallocProcess(nint, nint, nint)"/>
        private void ReallocProcessCapacity(TSize length, TSize alignment, TSize capacity) {
        }

        /// <summary>
        /// [Dangerous] Reallocate memory - Process length change (重新分配内存 - 处理长度变化).
        /// </summary>
        /// <returns>Returns true if successful, false otherwise (成功时返回true, 否则为false).</returns>
        /// <inheritdoc cref="ReallocProcess(nint, nint, nint)"/>
        private bool ReallocProcessLength(TSize length, TSize alignment, TSize capacity) {
            nint oldLength = this.Length;
            if (alignment == 0 || alignment <= this.Alignment) {
                if (alignment > 0 && alignment < this.Alignment) {
#if NATIVE_MEMORY_ALIGNED
#else
#endif // NATIVE_MEMORY_ALIGNED
                }
                this.Length = length;
                if (alignment > 0) {
                    this.Alignment = alignment;
                }
                if (length > oldLength && Flags.HasFlag(MemoryAllocFlags.ClearAlloc)) {
                    ExSpan<T> span = GetExSpan().Slice(oldLength);
                    span.Clear();
                }
            } else {
                T[]? oldArray = this.DataArray;
                if (oldArray is not null) {
                    TSize itemsOfAlignment = PointerUtil.GetEnoughItemCount(alignment, Unsafe.SizeOf<T>());
                    if ((oldArray.Length - itemsOfAlignment) < length) {
                        return false;
                    }
                    // TODO.
                } else {
                    nint byteCountBody = checked(capacity * Unsafe.SizeOf<T>());
                    nint byteCount = byteCountBody;
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
