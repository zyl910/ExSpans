global using TSize = System.UIntPtr;

using System;
using System.Collections.Generic;
using System.Linq;
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

    }
}
