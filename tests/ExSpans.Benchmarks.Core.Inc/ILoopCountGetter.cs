using System;
using System.Collections.Generic;
using System.Text;

namespace Zyl.SizableSpans.Benchmarks {
    /// <summary>
    /// Getter of LoopCount.
    /// </summary>
    internal interface ILoopCountGetter {
        /// <summary>
        /// Property LoopCount.
        /// </summary>
        int LoopCount { get; }
    }
}
