using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace System.Runtime.CompilerServices {
#if NET5_0_OR_GREATER
#else
    /// <summary>
    /// Reserved to be used by the compiler for tracking metadata.
    /// This class should not be used by developers in source code.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
#if SYSTEM_PRIVATE_CORELIB
    public
#else
    internal
#endif
    static class IsExternalInit {
    }
#endif // NET5_0_OR_GREATER
}
