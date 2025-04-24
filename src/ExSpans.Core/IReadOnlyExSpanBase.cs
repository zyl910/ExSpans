using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace Zyl.ExSpans {
    /// <summary>
    /// The base interface of read only Ex span (只读扩展跨度的基本接口).
    /// </summary>
    /// <typeparam name="T">The element type (元素的类型).</typeparam>
    [MyCLSCompliant(false)]
    public interface IReadOnlyExSpanBase<T> : IExLength {

        /// <summary>
        /// Returns a read only reference to the 0th element of the span. If the span is empty, returns null reference.
        /// It can be used for pinning and is required to support the use of span within a fixed statement (返回对只读跨度的第0个元素的引用。如果跨度为空，则返回null引用. 它可用于固定，并且需要支持在 fixed 语句中使用跨度).
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref readonly T GetPinnableReadOnlyReference();

    }
}
