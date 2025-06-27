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
    /// Array pool based abstract memory manager. It will rent the array from the array pool based on the length parameter, and return array to the array pool when when Dispose
    /// (基于数组池的抽象内存管理者. 它会根据长度参数, 从数组池中租借数组, 并在 Dispose 时归还数组到数组池).
    /// </summary>
    /// <typeparam name="T">The element type (元素的类型).</typeparam>
    public abstract class AbstractArrayExMemoryManager<T> : ExMemoryManager<T> {
        private nint _length;
        private ArrayPool<T>? _pool;
        private T[]? _dataArray;
        private bool _isDisposed = false;

        /// <summary>
        /// Create AbstractPointerExMemoryManager.
        /// </summary>
        /// <param name="length">Length of unmanaged data (非托管数据的长度).</param>
        /// <param name="pool">The <see cref="ArrayPool{T}"/> instance used to rent array. Defaults to <see cref="ArrayPool{T}.Shared"/> if it is null (用于租用数组的 <see cref="ArrayPool{T}"/> 实例. 它为空时默认为 <see cref="ArrayPool{T}.Shared"/>).</param>
        /// <exception cref="ArgumentOutOfRangeException">The length parameter must be greater than or equal to 0. The length parameter out of array max length.</exception>
        [CLSCompliant(false)]
        protected AbstractArrayExMemoryManager(nint length, ArrayPool<T>? pool = null) {
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
            _dataArray = pool.Rent((int)length);
            _length = length;
            _pool = pool;
        }

        /// <summary>
        /// Dispose.
        /// </summary>
        /// <param name="disposing">Is disposing.</param>
        protected override void Dispose(bool disposing) {
            if (_isDisposed) return;
            _isDisposed = true;
            // free.
            if (_dataArray is not null && _pool is not null) {
                _pool.Return(_dataArray);
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
            if (_dataArray is null) {
                return default;
            } else {
                if (elementIndex>= _length) {
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

        /// <summary>Is it Disposed (是否已处置).</summary>
        protected bool IsDisposed { get => _isDisposed; }

        /// <summary>Length of data (数据的长度).</summary>
        public nint Length {
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
