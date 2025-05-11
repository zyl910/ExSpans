using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;


#if NETSTANDARD2_0_OR_GREATER || NETCOREAPP1_0_OR_GREATER || NET20
using System.Runtime.Serialization;
#endif // NETSTANDARD2_0_OR_GREATER || NETCOREAPP1_0_OR_GREATER || NET20
using System.Text;
using Zyl.ExSpans.Extensions;

namespace Zyl.ExSpans.Exceptions {
    /// <summary>
    /// ExSpan length too long exception (扩展跨度长度太长的异常)
    /// </summary>
    [Serializable]
    public class ExSpanTooLongException : ExSpanAbstractException {
        /// <summary>
        /// Create <see cref="ExSpanTooLongException"/>.
        /// </summary>
        public ExSpanTooLongException() : this("The ExSpan length is too long!") {
        }

        /// <summary>
        /// Create <see cref="ExSpanTooLongException"/>, with message params.
        /// </summary>
        /// <param name="message">The message.</param>
        public ExSpanTooLongException(string? message) : base(message) {
        }

        /// <summary>
        /// Create <see cref="ExSpanTooLongException"/>, with message/inner params.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner.</param>
        public ExSpanTooLongException(string? message, Exception? inner) : base(message, inner) {
        }

#if NETSTANDARD2_0_OR_GREATER || NETCOREAPP1_0_OR_GREATER || NET20
#pragma warning disable SYSLIB0051 // Type or member is obsolete
        /// <summary>
        /// Create <see cref="ExSpanTooLongException"/>, with info/context params.
        /// </summary>
        /// <param name="info">The info.</param>
        /// <param name="context">The context.</param>
        protected ExSpanTooLongException(SerializationInfo info, StreamingContext context) : base(info, context) {
        }
#pragma warning restore SYSLIB0051 // Type or member is obsolete
#endif // NETSTANDARD2_0_OR_GREATER || NETCOREAPP1_0_OR_GREATER || NET20

        /// <summary>
        /// Throw exceptions constructed from length (抛出根据长度构造的异常).
        /// </summary>
        /// <param name="length">The length.</param>
        /// <param name="memberName">The member name.</param>
        /// <exception cref="ExSpanTooLongException">Alway throws exception.</exception>
        [DoesNotReturn]
        public static void ThrowByLength(TSize length, [CallerMemberName] string? memberName = null) {
            string message = (memberName is null) ? $"The ExSpan length({length}) is too long!"
                : $"The ExSpan length({length}) is too long! The {memberName} method not support it!";
            throw new ExSpanTooLongException(message);
        }

        /// <summary>
        /// Throws an exception if the length is out of the range of <see cref="Int32"/> (当长度超出 <see cref="Int32"/> 的范围时抛出异常).
        /// </summary>
        /// <param name="length">The length.</param>
        /// <param name="memberName">The member name.</param>
        /// <exception cref="ExSpanTooLongException">Throws an exception if the length is out of the range of Int32.</exception>
        public static void ThrowIfOutInt32(TSize length, [CallerMemberName] string? memberName = null) {
            if (length.IsLengthInInt32()) return;
            ThrowByLength(length, memberName);
        }
    }
}
