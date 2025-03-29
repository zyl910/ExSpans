﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Zyl.VectorTraits;

namespace Zyl.SizableSpans.Benchmarks {

    // My type.
    using TMy = Byte;

    /// <summary>
    /// Abstract shared array benchmark - Byte.
    /// </summary>
    public class AbstractSharedBenchmark_Byte : AbstractSharedBenchmark {
        // -- TMy ref --
        protected static ref TMy dstTMy => ref dstByte;
        protected static ref TMy baselineTMy => ref baselineByte;
        protected static TMy[] srcArray => srcArrayByte;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void CheckResult(string name) {
            CheckResultByte(name);
        }


        // -- Params --
        public override int ShiftAmountMin {
            get {
                //return 1;
                return -1;
            }
        }
        public override int ShiftAmountMax {
            get {
                //return 1;
                return Scalars<TMy>.BitSize + 1;
            }
        }

    }
}
