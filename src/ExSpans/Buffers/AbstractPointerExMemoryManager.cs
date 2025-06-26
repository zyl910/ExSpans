using System;
using System.Buffers;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Zyl.ExSpans.Buffers {
    /// <summary>
    /// Pointer based abstract memory manager. It also supports the Owner parameter, which can support <see cref="SafeBufferSpanProvider"/> types
    /// (基于指针的抽象内存管理者. 它还支持 Owner 参数, 它可支持 SafeBufferSpanProvider 类型).
    /// </summary>
    /// <typeparam name="T">The element type (元素的类型).</typeparam>
    /// <typeparam name="TOwner">The owner type (所有者的类型).</typeparam>
    public unsafe abstract class AbstractPointerExMemoryManager<T, TOwner> : ExMemoryManager<T> where TOwner: IDisposable {
        private void* _pointer;
        private nint _length;
        private TOwner? _owner;
        private bool _needFree;
        private bool _isDisposed;

        /// <summary>
        /// Create AbstractPointerExMemoryManager.
        /// </summary>
        /// <param name="owner">The owner of pointer (指针的所有者).</param>
        /// <param name="pointer">Pointer of unmanaged data (非托管数据的指针).</param>
        /// <param name="length">Length of unmanaged data (非托管数据的长度).</param>
        /// <param name="needFree">Is it need to free pointer (是否需要释放指针).</param>
        [CLSCompliant(false)]
        protected AbstractPointerExMemoryManager(TOwner? owner, void* pointer, nint length, bool needFree) {
            Owner = owner;
            Pointer = pointer;
            Length = length;
            NeedFree = needFree;
        }

        /// <inheritdoc/>
        public override Span<T> GetSpan() {
            if (_length <= 0) {
                return Span<T>.Empty;
            }
            return new ExSpan<T>(Pointer, Length).AsSpan();
        }

        /// <inheritdoc/>
        public override ExSpan<T> GetExSpan() {
            if (_length <= 0) {
                return ExSpan<T>.Empty;
            }
            return new ExSpan<T>(Pointer, Length);
        }

        /// <inheritdoc cref="IPinnable.Pin(int)"/>
        public override MemoryHandle Pin(int elementIndex = 0) {
            _ = elementIndex;
            return new MemoryHandle(Pointer);
        }

        /// <inheritdoc cref="IPinnable.Unpin"/>
        public override void Unpin() {
            // No work.
        }

        /// <summary>
        /// Dispose.
        /// </summary>
        /// <param name="disposing">Is disposing.</param>
        protected override void Dispose(bool disposing) {
            if (_isDisposed) return;
            _isDisposed = true;
            if (NeedFree) {
                if (Owner is null) {
                    ExNativeMemory.Free(Pointer);
                } else {
                    Owner?.Dispose();
                }
            }
        }

        /// <summary>Pointer of unmanaged data (非托管数据的指针).</summary>
        [CLSCompliant(false)]
        public unsafe void* Pointer { get => _pointer; protected set => _pointer = value; }

        /// <summary>Length of unmanaged data (非托管数据的长度).</summary>
        public nint Length { get => _length; protected set => _length = value; }

        /// <summary>The owner of pointer. When Dispose occurs and NeedFree is true, if Owner is empty, its Dispose method will be called; otherwise, the <see cref="ExNativeMemory.Free(void*)"/> method will be called (指针的所有者. 在 Dispose 发生且 NeedFree 为 true 时, 若 Owner 为空会调用它的 Dispose 方法, 否则会调用 <see cref="ExNativeMemory.Free(void*)"/> 方法).</summary>
        public TOwner? Owner { get => _owner; protected set => _owner = value; }

        /// <summary>Is it need to free pointer. If it is true, Dispose will perform the free operation (是否需要释放指针. 若它为 true 时, Dispose 会执行释放操作).</summary>
        public bool NeedFree { get => _needFree; protected set => _needFree = value; }

        /// <summary>Is it Disposed (是否已处置).</summary>
        protected bool IsDisposed { get => _isDisposed; }
    }
}
