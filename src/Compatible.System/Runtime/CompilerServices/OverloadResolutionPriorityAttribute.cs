﻿using System;
using System.Collections.Generic;
using System.Text;

namespace System.Runtime.CompilerServices {
#if NET9_0_OR_GREATER
#else

    /// <summary>
    /// Specifies the priority of a member in overload resolution. When unspecified, the default priority is 0.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
#if SYSTEM_PRIVATE_CORELIB
    public
#else
    internal
#endif
    sealed class OverloadResolutionPriorityAttribute : Attribute {
        /// <summary>
        /// Initializes a new instance of the <see cref="OverloadResolutionPriorityAttribute"/> class.
        /// </summary>
        /// <param name="priority">The priority of the attributed member. Higher numbers are prioritized, lower numbers are deprioritized. 0 is the default if no attribute is present.</param>
        public OverloadResolutionPriorityAttribute(int priority) {
            Priority = priority;
        }

        /// <summary>
        /// The priority of the member.
        /// </summary>
        public int Priority { get; }
    }

#endif // NET9_0_OR_GREATER
}
