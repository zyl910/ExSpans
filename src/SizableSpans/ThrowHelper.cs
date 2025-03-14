using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Zyl.SizableSpans {

#if NET6_0_OR_GREATER
    [StackTraceHidden]
#endif // NET6_0_OR_GREATER
    internal static class ThrowHelper {

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
        [DoesNotReturn]
#endif
        internal static void ThrowArgumentOutOfRangeException() {
            throw new ArgumentOutOfRangeException();
        }

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
        [DoesNotReturn]
#endif
        internal static void ThrowArrayTypeMismatchException() {
            throw new ArrayTypeMismatchException();
        }

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
        [DoesNotReturn]
#endif
        internal static void ThrowIndexOutOfRangeException() {
            throw new IndexOutOfRangeException();
        }

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
        [DoesNotReturn]
#endif
        internal static void ThrowInvalidTypeWithPointersNotSupported(Type targetType) {
            throw new ArgumentException(SR.Format(SR.Argument_InvalidTypeWithPointersNotSupported, targetType));
        }

    }
}
