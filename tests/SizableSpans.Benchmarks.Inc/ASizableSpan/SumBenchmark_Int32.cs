#undef BENCHMARKS_OFF

using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Zyl.SizableSpans.Extensions;

namespace Zyl.SizableSpans.Benchmarks.ASizableSpan {
#if BENCHMARKS_OFF
    using BenchmarkAttribute = FakeBenchmarkAttribute;
#else
#endif // BENCHMARKS_OFF

    // My type.
    using TMy = Int32;

    /// <summary>
    /// <see cref="SizableSpan{T}"/> sum benchmark - Int32.
    /// </summary>
#if NETCOREAPP3_0_OR_GREATER && DRY_JOB
    [DryJob]
#endif // NETCOREAPP3_0_OR_GREATER && DRY_JOB
    public class SumBenchmark_Int32 : AbstractSharedBenchmark_Int32 {

        /// <summary>
        /// Sum for array.
        /// </summary>
        /// <param name="src">Source array.</param>
        /// <param name="srcCount">Source count</param>
        /// <returns>Returns the sum.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TMy StaticSumForArray(TMy[] src, int srcCount) {
            TMy rt = 0; // Result.
            for (int i = 0; i < srcCount; ++i) {
                rt += src[i];
            }
            return rt;
        }

        [Benchmark(Baseline = true)]
        public void SumForArray() {
            dstTMy = StaticSumForArray(srcArray, srcArray.Length);
            if (CheckMode) {
                baselineTMy = dstTMy;
                BenchmarkUtil.WriteItem("# SumForArray", string.Format("{0}", baselineTMy));
            }
        }

        /// <summary>
        /// Sum for pointer.
        /// </summary>
        /// <param name="src">Source array.</param>
        /// <param name="srcCount">Source count</param>
        /// <returns>Returns the sum.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe TMy StaticSumForPtr(TMy[] src, int srcCount) {
            TMy rt = 0; // Result.
            fixed(TMy* p0 = &src[0]) {
                TMy* pEnd = p0 + srcCount;
                TMy* p = p0;
                for (; p < pEnd; ++p) {
                    rt += *p;
                }
            }
            return rt;
        }

        [Benchmark]
        public void SumForPtr() {
            dstTMy = StaticSumForPtr(srcArray, srcArray.Length);
            CheckResult("SumForPtr");
        }

        /// <summary>
        /// Sum for Span.
        /// </summary>
        /// <param name="src">Source array.</param>
        /// <param name="srcCount">Source count</param>
        /// <returns>Returns the sum.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TMy StaticSumForSpan(TMy[] src, int srcCount) {
            TMy rt = 0; // Result.
            Span<TMy> span = new Span<TMy>(src, 0, srcCount);
            for (int i = 0; i < srcCount; ++i) {
                rt += span[i];
            }
            return rt;
        }

        [Benchmark]
        public void SumForSpan() {
            if (BenchmarkUtil.IsLastRun) {
                Volatile.Write(ref dstTMy, 0);
                //Debugger.Break();
            }
            dstTMy = StaticSumForSpan(srcArray, srcArray.Length);
            CheckResult("SumForSpan");
        }

        /// <summary>
        /// Sum for SizableSpan.
        /// </summary>
        /// <param name="src">Source array.</param>
        /// <param name="srcCount">Source count</param>
        /// <returns>Returns the sum.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TMy StaticSumForSizableSpan(TMy[] src, int srcCount) {
            TMy rt = 0; // Result.
            nuint srcCountU = (nuint)srcCount;
            SizableSpan<TMy> span = new SizableSpan<TMy>(src, (nuint)0, srcCountU);
            for (nuint i = (nuint)0; i.LessThan(srcCountU); i+=1) {
                rt += span[i];
            }
            return rt;
        }

        [Benchmark]
        public void SumForSizableSpan() {
            if (BenchmarkUtil.IsLastRun) {
                Volatile.Write(ref dstTMy, 0);
                //Debugger.Break();
            }
            dstTMy = StaticSumForSizableSpan(srcArray, srcArray.Length);
            CheckResult("SumForSizableSpan");
        }

        /// <summary>
        /// Sum for SizableSpan by pointer constructor.
        /// </summary>
        /// <param name="src">Source array.</param>
        /// <param name="srcCount">Source count</param>
        /// <returns>Returns the sum.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe TMy StaticSumForSizableSpanByPtr(TMy[] src, int srcCount) {
            TMy rt = 0; // Result.
            nuint srcCountU = (nuint)srcCount;
            fixed (TMy* p0 = &src[0]) {
                SizableSpan<TMy> span = new SizableSpan<TMy>(p0, srcCountU);
                for (nuint i = (nuint)0; i.LessThan(srcCountU); i += 1) {
                    rt += span[i];
                }
            }
            return rt;
        }

        [Benchmark]
        public void SumForSizableSpanByPtr() {
            dstTMy = StaticSumForSizableSpanByPtr(srcArray, srcArray.Length);
            CheckResult("SumForSizableSpanByPtr");
        }

    }
}
