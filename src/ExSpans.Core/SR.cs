using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Zyl.ExSpans {
    internal static class SR {

        internal static string Argument_DestinationTooShort => "Destination is too short.";
        internal static string Argument_OverlapAlignmentMismatch => "Overlapping spans have mismatching alignment.";
        internal static string Argument_InvalidTypeWithPointersNotSupported => "Cannot use type '{0}'. Only value types without pointers or references are supported.";

        internal static string NotSupported_CannotCallEqualsOnExSpan => "Calls to the Equals method are not supported.";
        internal static string NotSupported_CannotCallGetHashCodeOnExSpan => "Calls to the GetHashCode method are not supported.";

        internal static string sourceBytesToCopy => "sourceBytesToCopy is greater than destinationSizeInBytes.";

        internal static string Format(
#if NET7_0_OR_GREATER
                [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif // NET7_0_OR_GREATER
                string format, params object?[] args) {
            return string.Format(format, args);
        }
    }
}
