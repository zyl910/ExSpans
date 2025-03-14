using System;

namespace System.Diagnostics.CodeAnalysis {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
#else

    /// <summary>
    /// Specifies that a method that will never return under any circumstance.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    internal sealed class DoesNotReturnAttribute : Attribute {
        /// <summary>
        /// Initializes a new instance of the this class.
        /// </summary>
        public DoesNotReturnAttribute() {
        }
    }

#endif
}
