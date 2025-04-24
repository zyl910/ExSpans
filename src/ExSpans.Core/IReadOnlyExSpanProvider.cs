using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Zyl.ExSpans {
    /// <summary>
    /// The provider interface of read only Ex span (只读扩展跨度的提供者接口).
    /// </summary>
    /// <typeparam name="T">The element type (元素的类型).</typeparam>
    interface IReadOnlyExSpanProvider<T> : IReadOnlyExSpanBase<T> {

        /// <summary>
        /// Create a <see cref="ReadOnlyExSpan{T}"/> using the specified type (使用指定类型来创建 <see cref="ReadOnlyExSpan{T}"/>).
        /// </summary>
        /// <typeparam name="TTo">The specified type (指定类型).</typeparam>
        /// <returns>a <see cref="ReadOnlyExSpan{T}"/>.</returns>
        public ReadOnlyExSpan<TTo> CreateReadOnlyExSpan<TTo>();

        /// <summary>
        /// Create a <see cref="ReadOnlyExSpan{T}"/> using the specified type with saturating (饱和的使用指定类型来创建 <see cref="ReadOnlyExSpan{T}"/>).
        /// </summary>
        /// <typeparam name="TTo">The specified type (指定类型).</typeparam>
        /// <returns>a <see cref="ReadOnlyExSpan{T}"/>.</returns>
        public ReadOnlyExSpan<TTo> CreateReadOnlyExSpanSaturating<TTo>();

    }
}
