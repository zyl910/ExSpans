using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Zyl.ExSpans {

#if NET6_0_OR_GREATER
    [StackTraceHidden]
#endif // NET6_0_OR_GREATER
    internal static class ThrowHelper {

        [DoesNotReturn]
        internal static void ThrowArgumentException_DestinationTooShort() {
            throw new ArgumentException(SR.Argument_DestinationTooShort, "destination");
        }

        [DoesNotReturn]
        internal static void ThrowArgumentException_OverlapAlignmentMismatch() {
            throw new ArgumentException(SR.Argument_OverlapAlignmentMismatch);
        }

        [DoesNotReturn]
        internal static void ThrowArgumentOutOfRangeException() {
            throw new ArgumentOutOfRangeException();
        }

        [DoesNotReturn]
        internal static void ThrowArrayTypeMismatchException() {
            throw new ArrayTypeMismatchException();
        }

        [DoesNotReturn]
        internal static void ThrowIndexOutOfRangeException() {
            throw new IndexOutOfRangeException();
        }

        [DoesNotReturn]
        internal static void ThrowInvalidTypeWithPointersNotSupported(Type targetType) {
            throw new ArgumentException(SR.Format(SR.Argument_InvalidTypeWithPointersNotSupported, targetType));
        }

    }
}
