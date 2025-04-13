using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Zyl.SizableSpans {
    /// <summary>
    /// The provider interface of read only sizable range span (只读大范围跨度的提供者接口).
    /// </summary>
    /// <typeparam name="T">The element type (元素的类型).</typeparam>
    interface IReadOnlySizableSpanProvider<T> : IReadOnlySizableSpanBase<T> {

        /// <summary>
        /// Create a <see cref="ReadOnlySizableSpan{T}"/> using the specified type (使用指定类型来创建 <see cref="ReadOnlySizableSpan{T}"/>).
        /// </summary>
        /// <typeparam name="TTo">The specified type (指定类型).</typeparam>
        /// <returns>a <see cref="ReadOnlySizableSpan{T}"/></returns>
        public ReadOnlySizableSpan<TTo> CreateReadOnlySizableSpan<TTo>();

        /// <summary>
        /// Create a <see cref="ReadOnlySizableSpan{T}"/> using the specified type with saturating (饱和的使用指定类型来创建 <see cref="ReadOnlySizableSpan{T}"/>).
        /// </summary>
        /// <typeparam name="TTo">The specified type (指定类型).</typeparam>
        /// <returns>a <see cref="ReadOnlySizableSpan{T}"/></returns>
        public ReadOnlySizableSpan<TTo> CreateReadOnlySizableSpanSaturating<TTo>();

    }
}
