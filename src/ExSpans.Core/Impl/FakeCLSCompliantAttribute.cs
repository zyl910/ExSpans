using System;
using System.Collections.Generic;
using System.Text;

namespace Zyl.ExSpans.Impl {
    /// <summary>
    /// Fake - Indicates whether a program element is compliant with the Common Language Specification (CLS). This class cannot be inherited.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = false)]
    public sealed class FakeCLSCompliantAttribute : Attribute {
        /// <summary>
        /// Initializes an instance of the System.CLSCompliantAttribute class with a Boolean value indicating whether the indicated program element is CLS-compliant.
        /// </summary>
        /// <param name="isCompliant">true if CLS-compliant; otherwise, false.</param>
        public FakeCLSCompliantAttribute(bool isCompliant) {
            IsCompliant = isCompliant;
        }

        /// <summary>
        /// Gets the Boolean value indicating whether the indicated program element is CLS-compliant.
        /// </summary>
        /// <returns>true if the program element is CLS-compliant; otherwise, false.</returns>
        public bool IsCompliant { get; }
    }
}
