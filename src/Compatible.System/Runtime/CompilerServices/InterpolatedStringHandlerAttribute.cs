﻿using System;
using System.Collections.Generic;
using System.Text;

namespace System.Runtime.CompilerServices {
#if NET6_0_OR_GREATER
#else

    /// <summary>Indicates the attributed type is to be used as an interpolated string handler.</summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public sealed class InterpolatedStringHandlerAttribute : Attribute {
        /// <summary>Initializes the <see cref="InterpolatedStringHandlerAttribute"/>.</summary>
        public InterpolatedStringHandlerAttribute() { }
    }

#endif // NET6_0_OR_GREATER
}
