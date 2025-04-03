using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Zyl.SizableSpans.Extensions;
using Zyl.SizableSpans.Impl;

namespace Zyl.SizableSpans {
    /// <summary>
    /// The span provider that manages the pointer acquire for <see cref="SafeBuffer"/> (管理 <see cref="SafeBuffer"/> 指针获取的跨度提供者). For example, it can provide span access for memory mapped files (例如它可以为内存映射文件提供跨度访问器).
    /// </summary>
    /// <typeparam name="TSafeBuffer">The type of <see cref="SafeBuffer"/>(<see cref="SafeBuffer"/> 的类型).</typeparam>
    /// <remarks>
    /// <para>Example:</para>
    /// <code>
    /// static void TestMemoryMappedFile(TextWriter writer) {
    ///     try {
    ///         const string MemoryMappedFileMapName = "SizableSpans.Sample.tmp";
    ///         const string MemoryMappedFilePath = MemoryMappedFileMapName;
    ///         const long MemoryMappedFileSize = 1 * 1024 * 1024; // 1MB
    ///         using MemoryMappedFile mappedFile = MemoryMappedFile.CreateFromFile(MemoryMappedFilePath, FileMode.Create, MemoryMappedFileMapName, MemoryMappedFileSize);
    ///         using MemoryMappedViewAccessor accessor = mappedFile.CreateViewAccessor();
    ///         using SafeBufferSpanProvider&lt;SafeMemoryMappedViewHandle&gt; spanProvider = accessor.SafeMemoryMappedViewHandle.CreateSpanProvider();
    ///         // Write.
    ///         SizableSpan&lt;int&gt; spanInt = spanProvider.CreateSizableSpan&lt;int&gt;();
    ///         spanInt.Fill(0x01020304);
    ///         spanInt[(nuint)0] = 0x12345678;
    ///         // Read.
    ///         writer.WriteLine(string.Format("Data[0]: {0} // 0x{0:X}", spanInt[(nuint)0]));
    ///         writer.WriteLine(string.Format("Data[1]: {0} // 0x{0:X}", spanInt[(nuint)1]));
    ///     } catch (Exception ex) {
    ///         writer.WriteLine(string.Format("Run TestMemoryMappedFile fail! {0}", ex.ToString()));
    ///     }
    ///     // Output:
    ///     // Data[0]: 305419896 // 0x12345678
    ///     // Data[1]: 16909060 // 0x1020304
    /// }
    /// </code>
    /// </remarks>
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
