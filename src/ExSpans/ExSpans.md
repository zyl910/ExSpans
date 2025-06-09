# ExSpans

ExSpans: Extended spans of nint index range (nint 索引范围的扩展跨度).

Commonly used types of `ExSpans.Core.dll`:

- `ExMemoryMarshal`: Provides a collection of methods for interoperating with ExSpan, and ExReadOnlySpan. It can be regarded as the MemoryMarshal of nint index range (提供与 ExSpan 和 ExReadOnlySpan 互操作的方法. 它可以被视为 nint 索引范围的 MemoryMarshal).
- `ExSpan<T>`: Provides a type-safe and memory-safe representation of a contiguous region of arbitrary memory. It can be regarded as the `Span<T>` of nint index range (提供任意内存的连续区域的类型安全和内存安全表示形式. 它可以被视为 nint 索引范围的 `Span<T>`).
- `ReadOnlyExSpan<T>`: Provides a type-safe and memory-safe read-only representation of a contiguous region of arbitrary memory. It can be regarded as the `ReadOnlySpan<T>` of nint index range (提供任意内存连续区域的类型安全且内存安全的只读表示形式. 它可以被视为 nint 索引范围的 `ReadOnlySpan<T>`).
- `SafeBufferSpanProvider`: The span provider that manages the pointer acquire for SafeBuffer (管理 SafeBuffer 指针获取的跨度提供者). For example, it can provide span access for memory mapped files (例如它可以为内存映射文件提供跨度访问器).

Commonly used types of `ExSpans.dll`:

- `ExMemoryExtensions`: Provides extension methods for the span-related types, such as ExSpan<T> and ReadOnlyExSpan<T>. It can be regarded as the MemoryExtensions of nint index range (提供跨度相关类型的扩展方法，例如 ExSpan<T> 和 ReadOnlyExSpan<T>. 它可以被视为 nint 索引范围的 MemoryExtensions).
- `ExNativeMemory`: This class contains methods that are mainly used to manage native memory. It can enable early versions NET can also use the method of <see cref="NativeMemory"/>, which will revert back to the implementation of <see cref="Marshal.AllocHGlobal(IntPtr)"/> (此类包含了主要用于管理本机内存的方法. 它能使早期版本的 .NET 也能使用 <see cref="NativeMemory"/> 的方法, 此时会回退为 <see cref="Marshal.AllocHGlobal(IntPtr)"/> 的实现).

## Documentation

- Online document: https://zyl910.github.io/ExSpans_doc/
- DocFX: Run `docfx_serve.bat`. Then browse http://localhost:8080/ .
