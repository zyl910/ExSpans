using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Zyl.ExSpans.Extensions;

namespace Zyl.ExSpans {
    /// <summary>
    /// The span provider that manages the pointer acquire for <see cref="SafeBuffer"/> (管理 <see cref="SafeBuffer"/> 指针获取的跨度提供者). For example, it can provide span access for memory mapped files (例如它可以为内存映射文件提供跨度访问器).
    /// </summary>
    /// <remarks>
    /// <example>
    /// For example:
    /// <code>
    /// static void TestMemoryMappedFile(TextWriter writer) {
    ///     try {
    ///         const string MemoryMappedFilePath = "ExSpans.Sample.tmp";
    ///         const string? MemoryMappedFileMapName = null; // If it is not null, MacOS will throw an exception. System.PlatformNotSupportedException: Named maps are not supported.
    ///         const long MemoryMappedFileSize = 1 * 1024 * 1024; // 1MB
    ///         using MemoryMappedFile mappedFile = MemoryMappedFile.CreateFromFile(MemoryMappedFilePath, FileMode.Create, MemoryMappedFileMapName, MemoryMappedFileSize);
    ///         using MemoryMappedViewAccessor accessor = mappedFile.CreateViewAccessor();
    ///         using SafeBufferSpanProvider spanProvider = accessor.SafeMemoryMappedViewHandle.CreateSpanProvider();
    ///         // Write.
    ///         ExSpan&lt;int&gt; spanInt = spanProvider.CreateExSpan&lt;int&gt;();
    ///         spanInt.Fill(0x01020304);
    ///         spanInt[0] = 0x12345678;
    ///         // Read.
    ///         writer.WriteLine(string.Format("Data[0]: {0} // 0x{0:X}", spanInt[0]));
    ///         writer.WriteLine(string.Format("Data[1]: {0} // 0x{0:X}", spanInt[1]));
    ///     } catch (Exception ex) {
    ///         writer.WriteLine(string.Format("Run TestMemoryMappedFile fail! {0}", ex.ToString()));
    ///     }
    ///     // Output:
    ///     // Data[0]: 305419896 // 0x12345678
    ///     // Data[1]: 16909060 // 0x1020304
    /// }
    /// </code>
    /// </example>
    /// <para><see cref="SafeBufferSpanProvider"/> is a lightweight, low overhead approach that only supports synchronous code. Although PointerExMemoryManager has a higher overhead, but it supports <see cref="Memory{T}"/> types, and supports not only synchronous code, but also asynchronous code. (SafeBufferSpanProvider是一种轻量级低开销的办法, 仅支持同步代码. 而 PointerExMemoryManager 虽然开销大一些, 但它支持 Memory 类型, 且不仅支持同步代码，还支持异步代码).</para>
    /// </remarks>
    public unsafe readonly struct SafeBufferSpanProvider : IDisposable, IExLength, IReadOnlyExSpanProvider<byte>, IExSpanProvider<byte> {
        private readonly SafeBuffer? _source;
        private readonly byte* _pointer;

        /// <summary>
        /// Create SafeBufferSpanProvider (创建 SafeBufferSpanProvider).
        /// </summary>
        /// <param name="source">The source (源对象).</param>
        public SafeBufferSpanProvider(SafeBuffer source) {
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
        public ReadOnlyExSpan<TTo> CreateReadOnlyExSpan<TTo>() {
            if (null == _source) return ExSpan<TTo>.Empty;
            TSize len = checked((TSize)(_source.ByteLength / (ulong)Unsafe.SizeOf<TTo>()));
            return new ReadOnlyExSpan<TTo>(_pointer, len);
        }

        /// <inheritdoc/>
        public ReadOnlyExSpan<TTo> CreateReadOnlyExSpanSaturating<TTo>() {
            if (null == _source) return ExSpan<TTo>.Empty;
            TSize len = (_source.ByteLength / (ulong)Unsafe.SizeOf<TTo>()).SaturatingToTSize();
            return new ReadOnlyExSpan<TTo>(_pointer, len);
        }

        /// <inheritdoc/>
        public ExSpan<TTo> CreateExSpan<TTo>() {
            if (null == _source) return ExSpan<TTo>.Empty;
            TSize len = checked((TSize)(_source.ByteLength / (ulong)Unsafe.SizeOf<TTo>()));
            return new ExSpan<TTo>(_pointer, len);
        }

        /// <inheritdoc/>
        public ExSpan<TTo> CreateExSpanSaturating<TTo>() {
            if (null == _source) return ExSpan<TTo>.Empty;
            TSize len = (_source.ByteLength / (ulong)Unsafe.SizeOf<TTo>()).SaturatingToTSize();
            return new ExSpan<TTo>(_pointer, len);
        }

        /// <summary>
        /// Get source <see cref="SafeBuffer"/> (取得源 <see cref="SafeBuffer"/>).
        /// </summary>
        public SafeBuffer? Source { get { return _source; } }

        /// <inheritdoc/>
        [MyCLSCompliant(false)]
        public TSize Length {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (_source?.ByteLength ?? 0).SaturatingToTSize();
        }

    }
}
