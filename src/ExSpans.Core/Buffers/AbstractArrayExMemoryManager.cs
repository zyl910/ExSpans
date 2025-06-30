using System;
using System.Buffers;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Zyl.ExSpans.Buffers {

    /// <summary>
    /// Memory alloc flags (内存分配标志).
    /// </summary>
    [Flags]
    public enum MemoryAllocFlags {
        /// <summary>None (无).</summary>
        None = 0,
        /// <summary>Zero the data when alloc. This flag will come with a small amount of overhead (分配时将数据清零. 该标志会来带少量开销).</summary>
        ClearAlloc = 1,
        /// <summary>Zero the data when free. This flag will come with a small amount of overhead (释放时将数据清零. 该标志会来带少量开销).</summary>
        ClearFree = 2,
        /// <summary>Keep old data when realloc. This flag will come with a large amount of overhead (重新分配时保留旧数据. 该标志会来带大量开销).</summary>
        KeepRealloc = 4,
        /// <summary>No set memory pressure. Without this flag, call <see cref="GC.AddMemoryPressure(long)"/> method when unmanaged memory allocated, and call <see cref="GC.RemoveMemoryPressure(long)"/> method when unmanaged memory freed. (不设置内存压力. 没有此标志时, 分配非托管内存时会调用 AddMemoryPressure 方法, 释放非托管内存时会调用 RemoveMemoryPressure 方法).</summary>
        NoPressure = 8,
        /// <summary>Manual trim. When the length decreases, the TrimExcess method is generally not automatically called and needs to be manually called (手动修剪. 当长度变小时, 一般不会自动调用 TrimExcess 方法 , 需要手动调用该方法).</summary>
        TrimManual = 0x10,
    }

    /// <summary>
    /// Array pool based abstract memory manager. It will rent the array from the array pool based on the length parameter, and return array to the array pool when when <see cref="IDisposable.Dispose"/>. Please note that the maximum length of an array is <see cref="Array.MaxLength"/> and generally cannot exceed 2GB
    /// (基于数组池的抽象内存管理器. 它会根据长度参数, 从数组池中租借数组, 并在 Dispose 时归还数组到数组池. 请注意, 数组的最大长度为 <see cref="Array.MaxLength"/>, 一般不能超过 2GB).
    /// </summary>
    /// <typeparam name="T">The element type (元素的类型).</typeparam>
    public abstract class AbstractArrayExMemoryManager<T> : ExMemoryManager<T> {
        private TSize _length;
        private ArrayPool<T>? _pool;
        private readonly MemoryAllocFlags _flags;
        private T[]? _dataArray;
        private bool _isDisposed = false;

        /// <summary>
        /// Create AbstractArrayExMemoryManager without create array.
        /// </summary>
        /// <param name="flags">Memory alloc flags (内存分配标志).</param>
        protected AbstractArrayExMemoryManager(MemoryAllocFlags flags = default) {
            _pool = null;
            _length = 0;
            _flags = flags;
            _dataArray = null;
        }

        /// <summary>
        /// Create AbstractArrayExMemoryManager.
        /// </summary>
        /// <param name="pool">The <see cref="ArrayPool{T}"/> instance used to rent array. Defaults to <see cref="ArrayPool{T}.Shared"/> if it is null (用于租用数组的 <see cref="ArrayPool{T}"/> 实例. 它为空时默认为 <see cref="ArrayPool{T}.Shared"/>).</param>
        /// <param name="length">Length of unmanaged data (非托管数据的长度).</param>
        /// <param name="flags">Memory alloc flags (内存分配标志). This class supports these flags: <see cref="MemoryAllocFlags.ClearAlloc"/>, <see cref="MemoryAllocFlags.ClearFree"/>.</param>
        /// <exception cref="ArgumentOutOfRangeException">The length parameter must be greater than or equal to 0. The length parameter out of array max length.</exception>
        protected AbstractArrayExMemoryManager(ArrayPool<T>? pool, TSize length, MemoryAllocFlags flags = default) {
            // Check.
            if (length < 0) {
                throw new ArgumentOutOfRangeException(nameof(length), "The length parameter must be greater than or equal to 0.");
            }
            if ((long)length > (long)int.MaxValue) {
                throw new ArgumentOutOfRangeException(nameof(length), string.Format("The length({0}) parameter out of array max length.", (long)length));
            }
#if NET6_0_OR_GREATER
            if ((long)length > (long)Array.MaxLength) {
                throw new ArgumentOutOfRangeException(nameof(length), string.Format("The length({0}) parameter out of array max length.", (long)length));
            }
#endif // NET6_0_OR_GREATER
            // Alloc.
            if (pool is null) {
                pool = ArrayPool<T>.Shared;
            }
            _dataArray = (length > 0) ? pool.Rent((int)length) : null;
            _length = length;
            _pool = pool;
            _flags = flags;
            if (_length > 0 && Flags.HasFlag(MemoryAllocFlags.ClearAlloc)) {
                GetExSpan().Clear();
            }
        }

        /// <summary>
        /// Clean up of any leftover managed and unmanaged resources.
        /// </summary>
        /// <param name="disposing">Is disposing.</param>
        protected override void Dispose(bool disposing) {
            if (_isDisposed) return;
            _isDisposed = true;
            // free.
            if (_dataArray is not null && _pool is not null) {
                bool clearArray = Flags.HasFlag(MemoryAllocFlags.ClearFree);
                _pool.Return(_dataArray, clearArray);
                _dataArray = null;
            }
        }

        /// <summary>
        /// Gets an array instance wrapping the underlying array in use (获取正在使用的封装底层数组的数组实例).
        /// </summary>
        /// <returns>An array instance wrapping the underlying array in use (正在使用的封装底层数组的数组实例).</returns>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public T[]? DangerousGetArray() {
            return _dataArray;
        }

        /// <inheritdoc/>
        public override ExSpan<T> GetExSpan() {
            if (_length <= 0 || _dataArray is null) {
                return ExSpan<T>.Empty;
            }
            return new ExSpan<T>(_dataArray, 0, _length);
        }

        /// <inheritdoc/>
        public override Span<T> GetSpan() {
            if (_length <= 0 || _dataArray is null) {
                return Span<T>.Empty;
            }
            return new ExSpan<T>(_dataArray, 0, _length).AsSpan();
        }

        /// <inheritdoc cref="IPinnable.Pin(int)"/>
        public unsafe override MemoryHandle Pin(int elementIndex = 0) {
            if (_dataArray is null || Length <= 0) {
                return default;
            } else {
                if (elementIndex >= _length) {
                    throw new ArgumentOutOfRangeException(nameof(elementIndex), string.Format("The elementIndex({0}) parameter out of length({1}).", (long)elementIndex, (long)_length));
                }
                GCHandle handle = GCHandle.Alloc(_dataArray, GCHandleType.Pinned);
#if NETSTANDARD2_0_OR_GREATER || NETCOREAPP1_0_OR_GREATER || NET40_OR_GREATER
                System.Threading.Thread.MemoryBarrier();
#endif // NETSTANDARD2_0_OR_GREATER || NETCOREAPP1_0_OR_GREATER || NET40_OR_GREATER
                void* pointer = Unsafe.AsPointer(ref _dataArray[elementIndex]);
                return new MemoryHandle(pointer, handle);
            }
        }

        /// <inheritdoc cref="IPinnable.Unpin"/>
        public override void Unpin() {
            // No work required.
        }

        /// <summary>Array of data (数据的数组).</summary>
        protected T[]? DataArray {
            get => _dataArray;
            set => _dataArray = value;
        }
        /// <summary>Memory alloc flags (内存分配标志).</summary>
        public MemoryAllocFlags Flags {
            get => _flags;
        }

        /// <summary>Is it Disposed (是否已处置).</summary>
        protected bool IsDisposed { get => _isDisposed; }

        /// <summary>Length of data (数据的长度).</summary>
        public TSize Length {
            get => _length;
            protected set => _length = value;
        }

        /// <summary>The <see cref="ArrayPool{T}"/> instance used to rent array (用于租用数组的 <see cref="ArrayPool{T}"/> 实例).</summary>
        public ArrayPool<T>? Pool {
            get => _pool;
            protected set => _pool = value;
        }
    }
}
