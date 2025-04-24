using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Zyl.ExSpans {
    /// <summary>
    /// The interface of sizable range length (大范围长度的接口).
    /// </summary>
    [MyCLSCompliant(false)]
    public interface IExLength {

        /// <summary>
        /// The number of items in the sizable span (大范围跨度中的项数).
        /// </summary>
        [MyCLSCompliant(false)]
        public TSize Length {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
        }

    }
}
