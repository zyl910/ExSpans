using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Zyl.ExSpans {
    /// <summary>
    /// Helper methods of <see cref="SafeBufferSpanProvider{TSafeBuffer}"/> (<see cref="SafeBufferSpanProvider{TSafeBuffer}"/> 的辅助方法).
    /// </summary>
    public static class SafeBufferSpanProviders {
        /// <summary>
        /// Create span provider (创建跨度提供者).
        /// </summary>
        /// <param name="source">The source <see cref="SafeBuffer"/> (源 <see cref="SafeBuffer"/>)</param>
        /// <returns>Returns span provider (返回跨度提供者).</returns>
        public static SafeBufferSpanProvider CreateSpanProvider<TSafeBuffer>(this TSafeBuffer source) where TSafeBuffer : SafeBuffer {
            return new SafeBufferSpanProvider(source);
        }
    }
}
