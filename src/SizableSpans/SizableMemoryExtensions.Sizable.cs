using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Zyl.SizableSpans.Extensions;

namespace Zyl.SizableSpans {
    partial class SizableMemoryExtensions {

        /// <summary>
        /// Fills the contents of this span with the given value (用指定的值填充此跨度的内容).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="span">Target span (目标跨度).</param>
        /// <param name="value">The given value (指定的值).</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Fill<T>(this SizableSpan<T> span, T value) {
            SizableSpanHelpers.Fill(ref span.GetPinnableReference(), span.Length.ToUIntPtr(), value);
        }

    }
}
