using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Zyl.SizableSpans {
    /// <summary>
    /// The interface of sizable range length (大范围长度的接口).
    /// </summary>
    [CLSCompliant(false)]
    public interface ISizableLength {

        /// <summary>
        /// The number of items in the sizable span (大范围跨度中的项数).
        /// </summary>
        [CLSCompliant(false)]
        public TSize Length {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
        }

    }
}
