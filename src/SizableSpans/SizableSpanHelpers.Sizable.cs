using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Zyl.SizableSpans {
    partial class SizableSpanHelpers {

        /// <summary>
        /// The safe maximum length of <see cref="Span{T}"/> (<see cref="Span{T}"/> 安全的最大长度.).
        /// </summary>
        /// <seealso cref="Span{T}.Length"/>
        /// <seealso cref="Array.MaxLength"/>
        public static readonly TSize SpanMaxLengthSafe = (TSize)(1024 * 1024 * 1024); // 1G

        /// <summary>
        /// Gets a value that indicates whether the current process is a 64-bit process (获取一个值，该值指示当前进程是否为 64 位进程).
        /// </summary>
        /// <value>true if the process is 64-bit; otherwise, false (如果进程为 64 位进程，则为 true；否则为 false).</value>
        public static bool Is64BitProcess {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get {
#if NETSTANDARD1_0_OR_GREATER && !NETSTANDARD2_0_OR_GREATER
                return sizeof(long) == IntPtr.Size;
#else
                return Environment.Is64BitProcess;
#endif
            }
        }

    }
}
