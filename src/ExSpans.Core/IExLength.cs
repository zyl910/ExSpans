using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Zyl.ExSpans {
    /// <summary>
    /// The interface of Ex length (扩展长度的接口).
    /// </summary>
    [MyCLSCompliant(false)]
    public interface IExLength {

        /// <summary>
        /// The number of items in the Ex span (扩展跨度中的项数).
        /// </summary>
        [MyCLSCompliant(false)]
        public TSize Length {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
        }

    }
}
