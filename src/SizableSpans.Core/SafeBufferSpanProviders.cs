using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Zyl.SizableSpans {
    /// <summary>
    /// Helper methods of <see cref="SafeBufferSpanProvider{TSafeBuffer}"/> (<see cref="SafeBufferSpanProvider{TSafeBuffer}"/> 的辅助方法).
    /// </summary>
    public static class SafeBufferSpanProviders {
        /// <summary>
        /// Create span provider (创建跨距提供者).
        /// </summary>
        /// <typeparam name="TSafeBuffer">The type of <see cref="SafeBuffer"/>(<see cref="SafeBuffer"/> 的类型).</typeparam>
        /// <param name="source"></param>
        /// <returns>Returns span provider (返回跨距提供者).</returns>
        public static SafeBufferSpanProvider<TSafeBuffer> CreateSpanProvider<TSafeBuffer>(this TSafeBuffer source) where TSafeBuffer : SafeBuffer {
            return new SafeBufferSpanProvider<TSafeBuffer>(source);
        }
    }
}
