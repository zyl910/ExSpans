using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Zyl.ExSpans {
    /// <summary>
    /// The provider interface of Ex span (扩展跨度的提供者接口).
    /// </summary>
    /// <typeparam name="T">The element type (元素的类型).</typeparam>
    interface IExSpanProvider<T> : IExSpanBase<T> {

        /// <summary>
        /// Create a <see cref="ExSpan{T}"/> using the specified type (使用指定类型来创建 <see cref="ExSpan{T}"/>).
        /// </summary>
        /// <typeparam name="TTo">The specified type (指定类型).</typeparam>
        /// <returns>a <see cref="ExSpan{T}"/>.</returns>
        public ExSpan<TTo> CreateExSpan<TTo>();

        /// <summary>
        /// Create a <see cref="ExSpan{T}"/> using the specified type with saturating (饱和的使用指定类型来创建 <see cref="ExSpan{T}"/>).
        /// </summary>
        /// <typeparam name="TTo">The specified type (指定类型).</typeparam>
        /// <returns>a <see cref="ExSpan{T}"/>.</returns>
        public ExSpan<TTo> CreateExSpanSaturating<TTo>();

    }
}
