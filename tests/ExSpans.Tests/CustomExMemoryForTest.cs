using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Zyl.ExSpans.Buffers;

namespace Zyl.ExSpans.Tests {
    public class CustomExMemoryForTest<T> : ExMemoryManager<T> {
        private bool _disposed;
        private int _referenceCount;
        private int _noReferencesCalledCount;
        private T[]? _array;
        private readonly int _offset;
        private readonly int _length;

        public CustomExMemoryForTest(T[] array) : this(array, 0, array.Length) {
        }

        public CustomExMemoryForTest(T[] array, int offset, int length) {
            if (array is null) throw new ArgumentNullException("array");
            _array = array;
            _offset = offset;
            _length = length;
        }

        public bool IsDisposed => _disposed;

        protected bool IsRetained => _referenceCount > 0;

        public override Span<T> GetSpan() {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(CustomExMemoryForTest<T>));
            return new Span<T>(_array, _offset, _length);
        }

        public override MemoryHandle Pin(int elementIndex = 0) {
            unsafe {
                if (IsDisposed)
                    throw new ObjectDisposedException(nameof(CustomExMemoryForTest<T>));
                if (_array is null) throw new ArgumentNullException("array");
                Interlocked.Increment(ref _referenceCount);

                try {
                    if ((uint)elementIndex > (uint)(_array.Length - _offset)) {
                        throw new ArgumentOutOfRangeException(nameof(elementIndex));
                    }

                    var handle = GCHandle.Alloc(_array, GCHandleType.Pinned);
                    return new MemoryHandle(Unsafe.Add<T>((void*)handle.AddrOfPinnedObject(), _offset + elementIndex), handle, this);
                } catch {
                    Unpin();
                    throw;
                }
            }
        }

        protected override bool TryGetArray(out ArraySegment<T> segment) {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(CustomExMemoryForTest<T>));
            if (_array is null) throw new ArgumentNullException("array");
            segment = new ArraySegment<T>(_array, _offset, _length);
            return true;
        }

        protected override void Dispose(bool disposing) {
            if (_disposed)
                return;

            if (disposing) {
                _array = null;
            }

            _disposed = true;

        }

        public override void Unpin() {
            int newRefCount = Interlocked.Decrement(ref _referenceCount);

            if (newRefCount < 0)
                throw new InvalidOperationException();

            if (newRefCount == 0) {
                _noReferencesCalledCount++;
            }
        }

        public ExMemory<T> CreateExMemoryForTest(int length) => CreateExMemory(length);

        public ExMemory<T> CreateExMemoryForTest(int start, int length) => CreateExMemory(start, length);

        public override ExSpan<T> GetExSpan() {
            return GetSpan().AsExSpan();
        }
    }
}
