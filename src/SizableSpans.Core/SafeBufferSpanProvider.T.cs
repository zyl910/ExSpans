using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Zyl.SizableSpans.Extensions;
using Zyl.SizableSpans.Impl;

namespace Zyl.SizableSpans {
    /// <summary>
    /// The span provider that manages the pointer acquire for <see cref="SafeBuffer"/> (管理 <see cref="SafeBuffer"/> 的指针获取的跨距提供者). For example, it can provide span access for memory mapped files (例如它可以为内存映射文件提供跨度访问器).
    /// </summary>
    /// <typeparam name="TSafeBuffer">The type of <see cref="SafeBuffer"/>(<see cref="SafeBuffer"/> 的类型).</typeparam>
    public unsafe readonly ref struct SafeBufferSpanProvider<TSafeBuffer> : IDisposable, ISizableLength, IReadOnlySizableSpanProvider<byte>, ISizableSpanProvider<byte>
                where TSafeBuffer : SafeBuffer {
        private readonly TSafeBuffer _source;
        private readonly byte* _pointer;

        /// <summary>
        /// Create SafeBufferSpanProvider (创建 SafeBufferSpanProvider).
        /// </summary>
        /// <param name="source"></param>
        public SafeBufferSpanProvider(TSafeBuffer source) {
            _source = source;
            if (null != source) {
                source.AcquirePointer(ref _pointer);
            } else {
                _pointer = default;
            }
        }

        /// <inheritdoc/>
        public void Dispose() {
            _source?.ReleasePointer();
        }

        /// <inheritdoc/>
        public ref readonly byte GetPinnableReadOnlyReference() {
            return ref GetPinnableReference();
        }

        /// <inheritdoc/>
        public ref byte GetPinnableReference() {
            return ref Unsafe.AsRef<byte>(_pointer);
        }

        /// <inheritdoc/>
        public ReadOnlySizableSpan<TTo> CreateReadOnlySizableSpan<TTo>() {
            nuint len = (_source.ByteLength / (ulong)Unsafe.SizeOf<TTo>()).SaturatingToUIntPtr();
            return new ReadOnlySizableSpan<TTo>(_pointer, len);
        }

        /// <inheritdoc/>
        public SizableSpan<TTo> CreateSizableSpan<TTo>() {
            if (null == _source) return SizableSpan<TTo>.Empty;
            nuint len = (_source.ByteLength / (ulong)Unsafe.SizeOf<TTo>()).SaturatingToUIntPtr();
            return new SizableSpan<TTo>(_pointer, len);
        }

        /// <summary>
        /// Get source SafeBuffer.
        /// </summary>
        public TSafeBuffer Source { get { return _source; } }

        /// <inheritdoc/>
        public TSize Length {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (_source?.ByteLength ?? 0).SaturatingToUIntPtr();
        }

    }
}
