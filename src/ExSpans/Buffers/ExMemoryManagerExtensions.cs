using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Zyl.ExSpans.Buffers {
    /// <summary>
    /// Provides extension methods for the <see cref="ExMemoryManager{T}"/> types (提供 <see cref="ExMemoryManager{T}"/>相关类型的扩展方法).
    /// </summary>
    public static class ExMemoryManagerExtensions {

        /// <summary>
        /// Create <see cref="PointerExMemoryManager{T, TOwner}"/> based on <see cref="SafeBufferSpanProvider"/> (根据 SafeBufferSpanProvider 创建 PointerExMemoryManager).
        /// </summary>
        /// <param name="source">The source (源).</param>
        /// <returns>Returns a memory manager (返回内存管理器).</returns>
        public static unsafe PointerExMemoryManager<byte, SafeBufferSpanProvider> CreatePointerExMemoryManager(this SafeBufferSpanProvider source) {
            void* pointer = source.Pointer;
            return new PointerExMemoryManager<byte, SafeBufferSpanProvider>(source, pointer, source.Length);
        }

        /// <summary>
        /// Create <see cref="PointerExMemoryManager{T, TOwner}"/> based on <see cref="SafeBuffer"/> (根据 SafeBuffer 创建 PointerExMemoryManager).
        /// </summary>
        /// <typeparam name="TSafeBuffer">The <see cref="SafeBuffer"/> type (SafeBuffer的类型).</typeparam>
        /// <param name="source">The source (源).</param>
        /// <returns>Returns a memory manager (返回内存管理器).</returns>
        public static unsafe PointerExMemoryManager<byte, SafeBufferSpanProvider> CreatePointerExMemoryManager<TSafeBuffer>(this TSafeBuffer source) where TSafeBuffer : SafeBuffer {
            SafeBufferSpanProvider spanProvider = source.CreateSpanProvider();
            return spanProvider.CreatePointerExMemoryManager();
        }

    }
}
