﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Zyl.VectorTraits;

namespace Zyl.SizableSpans.Benchmarks {

    // My type.
    using TMy = SByte;

    /// <summary>
    /// Abstract shared array benchmark - SByte.
    /// </summary>
    public class AbstractSharedBenchmark_SByte : AbstractSharedBenchmark {
        // -- TMy ref --
        protected static ref TMy dstTMy => ref dstSByte;
        protected static ref TMy baselineTMy => ref baselineSByte;
        protected static TMy[] srcArray => srcArraySByte;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void CheckResult(string name) {
            CheckResultSByte(name);
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
