using System;
using System.Collections.Generic;
using System.Text;

namespace Zyl.ExSpans.Buffers {
    /// <summary>
    /// Pointer based memory manager. It also supports the Owner parameter, which can support <see cref="SafeBufferSpanProvider"/> types
    /// (基于指针的内存管理者. 它还支持 Owner 参数, 它可支持 <see cref="SafeBufferSpanProvider"/> 类型).
    /// </summary>
    /// <typeparam name="T">The element type (元素的类型).</typeparam>
    /// <typeparam name="TOwner">The owner type (所有者的类型).</typeparam>
    /// <remarks>
    /// <para>The <see cref="PointerExMemoryManager{T}"/> applies when Owner is a reference type. The <see cref="PointerExMemoryManager{T, TOwner}"/> applies when Owner is a value type
    /// (<see cref="PointerExMemoryManager{T}"/> 适用于Owner是引用类型时. <see cref="PointerExMemoryManager{T, TOwner}"/> 适用于Owner是值类型时).</para>
    /// <para><see cref="SafeBufferSpanProvider"/> is a lightweight, low overhead approach that only supports synchronous code. Although <see cref="PointerExMemoryManager{T, TOwner}"/> has a higher overhead, but it supports <see cref="Memory{T}"/> types, and supports not only synchronous code, but also asynchronous code. (SafeBufferSpanProvider是一种轻量级低开销的办法, 仅支持同步代码. 而 PointerExMemoryManager 虽然开销大一些, 但它支持 Memory 类型, 且不仅支持同步代码，还支持异步代码).</para>
    /// </remarks>
    public unsafe sealed class PointerExMemoryManager<T, TOwner> : AbstractPointerExMemoryManager<T, TOwner> where TOwner : IDisposable {

#pragma warning disable CA2015
        /// <summary>
        /// Finalizer of PointerExMemoryManager.
        /// </summary>
        /// <remarks>
        /// <para>CA2015: Adding a finalizer to a type derived from MemoryManager may permit memory to be freed while it is still in use by a Span.https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca2015</para>
        /// <para>To support weak references, it is necessary to break the CA2015 warning and use finalizer to free up memory. Please developers to handle the lifespan of Span (为了支持弱引用, 需要突破CA2015警告, 利用终结期来释放内存. 请开发者处理好 Span 的生存期).</para>
        /// </remarks>
        ~PointerExMemoryManager() {
            Dispose(false);
        }
#pragma warning restore CA2015

        /// <summary>
        /// Create PointerExMemoryManager. It contains parameters <paramref name="owner"/>, <paramref name="pointer"/>, <paramref name="length"/>, <paramref name="needFree"/>.
        /// </summary>
        /// <param name="owner">The owner of pointer (指针的所有者).</param>
        /// <param name="pointer">Pointer of unmanaged data (非托管数据的指针).</param>
        /// <param name="length">Length of unmanaged data (非托管数据的长度).</param>
        /// <param name="needFree">Is it need to free pointer. If it is true, Dispose will execute the free operation (是否需要释放指针. 若它为 true 时, Dispose 会执行释放操作).</param>
        /// <exception cref="ArgumentOutOfRangeException">The length parameter must be greater than or equal to 0.</exception>
        [CLSCompliant(false)]
        public PointerExMemoryManager(TOwner owner, void* pointer, nint length, bool needFree) : base(owner, pointer, length, needFree) {
        }

        /// <summary>
        /// Create PointerExMemoryManager. It contains parameters <paramref name="owner"/>, <paramref name="pointer"/>, <paramref name="length"/>. The needFree parameter defaults to <c>true</c>.
        /// </summary>
        /// <param name="owner">The owner of pointer (指针的所有者).</param>
        /// <param name="pointer">Pointer of unmanaged data (非托管数据的指针).</param>
        /// <param name="length">Length of unmanaged data (非托管数据的长度).</param>
        /// <exception cref="ArgumentOutOfRangeException">The length parameter must be greater than or equal to 0.</exception>
        [CLSCompliant(false)]
        public PointerExMemoryManager(TOwner owner, void* pointer, nint length) : this(owner, pointer, length, true) {
        }

    }
}
