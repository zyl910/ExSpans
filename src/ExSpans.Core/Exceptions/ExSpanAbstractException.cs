using System;
using System.Collections.Generic;
#if NETSTANDARD2_0_OR_GREATER || NETCOREAPP1_0_OR_GREATER || NET40_OR_GREATER
using System.Runtime.Serialization;
#endif // NETSTANDARD2_0_OR_GREATER || NETCOREAPP1_0_OR_GREATER || NET40_OR_GREATER
using System.Text;

namespace Zyl.ExSpans.Exceptions {
    /// <summary>
    /// Abstract class with ExSpan (扩展跨度的异常抽象类).
    /// </summary>
    [Serializable]
    public abstract class ExSpanAbstractException : Exception {
        /// <summary>
        /// Create <see cref="ExSpanAbstractException"/>.
        /// </summary>
        protected ExSpanAbstractException() : base() {
        }

        /// <summary>
        /// Create <see cref="ExSpanAbstractException"/>, with message params.
        /// </summary>
        /// <param name="message">The message.</param>
        protected ExSpanAbstractException(string? message) : base(message) {
        }

        /// <summary>
        /// Create <see cref="ExSpanAbstractException"/>, with message/inner params.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner.</param>
        protected ExSpanAbstractException(string? message, Exception? inner) : base(message, inner) {
        }

#if NETSTANDARD2_0_OR_GREATER || NETCOREAPP1_0_OR_GREATER || NET40_OR_GREATER
#pragma warning disable SYSLIB0051 // Type or member is obsolete
        /// <summary>
        /// Create <see cref="ExSpanAbstractException"/>, with info/context params.
        /// </summary>
        /// <param name="info">The info.</param>
        /// <param name="context">The context.</param>
        protected ExSpanAbstractException(SerializationInfo info, StreamingContext context) : base(info, context) {
        }
#pragma warning restore SYSLIB0051 // Type or member is obsolete
#endif // NETSTANDARD2_0_OR_GREATER || NETCOREAPP1_0_OR_GREATER || NET40_OR_GREATER

    }
}
