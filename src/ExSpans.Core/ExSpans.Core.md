# ExSpans.Core

ExSpans.Core: Extended spans of nint index range - Core type (nint 索引范围的扩展跨度 - 核心类型).

Commonly used types of `ExSpans.Core.dll`:

- `ExMemoryMarshal`: Provides a collection of methods for interoperating with ExSpan, and ExReadOnlySpan. It can be regarded as the MemoryMarshal of nint index range (提供与 ExSpan 和 ExReadOnlySpan 互操作的方法. 它可以被视为 nint 索引范围的 MemoryMarshal).
- `ExSpan<T>`: Provides a type-safe and memory-safe representation of a contiguous region of arbitrary memory. It can be regarded as the `Span<T>` of nint index range (提供任意内存的连续区域的类型安全和内存安全表示形式. 它可以被视为 nint 索引范围的 `Span<T>`).
- `ReadOnlyExSpan<T>`: Provides a type-safe and memory-safe read-only representation of a contiguous region of arbitrary memory. It can be regarded as the `ReadOnlySpan<T>` of nint index range (提供任意内存连续区域的类型安全且内存安全的只读表示形式. 它可以被视为 nint 索引范围的 `ReadOnlySpan<T>`).
- `SafeBufferSpanProvider`: The span provider that manages the pointer acquire for SafeBuffer (管理 SafeBuffer 指针获取的跨度提供者). For example, it can provide span access for memory mapped files (例如它可以为内存映射文件提供跨度访问器).

## Documentation

- Online document: https://zyl910.github.io/ExSpans_doc/
- DocFX: Run `docfx_serve.bat`. Then browse http://localhost:8080/ .
