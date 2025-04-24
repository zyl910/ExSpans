using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Zyl.ExSpans.Impl {
    /// <summary>
    /// Helper methods of <see cref="Debug"/> (<see cref="Debug"/> 的帮助方法).
    /// </summary>
    public static class DebugHelper {

        /// <summary>
        /// Emits the specified error message.
        /// </summary>
        /// <param name="message">A message to emit.</param>
        [Conditional("DEBUG")]
        //[DoesNotReturn]
        public static void Fail(string? message) {
#if NETSTANDARD1_3_OR_GREATER || NETCOREAPP1_0_OR_GREATER || NET20_OR_GREATER
            Debug.Fail(message);
#else
            Debug.WriteLine(message);
#endif
        }

        /// <summary>
        /// Emits an error message and a detailed error message.
        /// </summary>
        /// <param name="message">A message to emit.</param>
        /// <param name="detailMessage">A detailed message to emit.</param>
        [Conditional("DEBUG")]
        //[DoesNotReturn]
        public static void Fail(string? message, string? detailMessage) {
#if NETSTANDARD1_3_OR_GREATER || NETCOREAPP1_0_OR_GREATER || NET20_OR_GREATER
            Debug.Fail(message, detailMessage);
#else
            Debug.WriteLine(message);
            Debug.WriteLine(detailMessage);
#endif
        }

    }
}
