using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Zyl.SizableSpans {
    /// <summary>
    /// The provider interface of sizable range span (大范围跨度的提供者接口).
    /// </summary>
    /// <typeparam name="T">The element type (元素的类型).</typeparam>
    interface ISizableSpanProvider<T> : ISizableSpanBase<T> {

        /// <summary>
        /// Create a <see cref="SizableSpan{T}"/> using the specified type (使用指定类型来创建 <see cref="SizableSpan{T}"/>).
        /// </summary>
        /// <typeparam name="TTo">The specified type (指定类型).</typeparam>
        /// <returns>a <see cref="SizableSpan{T}"/></returns>
        public SizableSpan<TTo> CreateSizableSpan<TTo>();

    }
}
