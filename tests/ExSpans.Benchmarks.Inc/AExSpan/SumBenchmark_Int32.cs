#undef BENCHMARKS_OFF

using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Zyl.ExSpans.Extensions;

namespace Zyl.ExSpans.Benchmarks.AExSpan {
#if BENCHMARKS_OFF
    using BenchmarkAttribute = FakeBenchmarkAttribute;
#else
#endif // BENCHMARKS_OFF


    // My type.
    using TMy = Int32;

    /// <summary>
    /// <see cref="ExSpan{T}"/> sum benchmark - Int32.
    /// </summary>
#if NETCOREAPP3_0_OR_GREATER && DRY_JOB
    [DryJob]
#endif // NETCOREAPP3_0_OR_GREATER && DRY_JOB
    public class SumBenchmark_Int32 : AbstractSharedBenchmark_Int32 {

        /// <summary>
        /// Summation using index access to arrays (使用索引访问数组实现求和).
        /// </summary>
        /// <param name="src">Source array.</param>
        /// <param name="srcCount">Source count</param>
        /// <returns>Returns the sum.</returns>
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
        /// Summation using native pointer access to arrays (使用原生指针访问数组实现求和).
        /// Sum for pointer.
        /// </summary>
        /// <param name="src">Source array.</param>
        /// <param name="srcCount">Source count</param>
        /// <returns>Returns the sum.</returns>
        public static unsafe TMy StaticSumForPtr(TMy[] src, int srcCount) {
            TMy rt = 0; // Result.
            fixed (TMy* p0 = &src[0]) {
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
        /// Summation using index access to Span (使用索引访问 Span 实现求和).
        /// Sum for Span.
        /// </summary>
        /// <param name="src">Source array.</param>
        /// <param name="srcCount">Source count</param>
        /// <returns>Returns the sum.</returns>
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
        /// Summation using index access to ExSpan (使用索引访问 ExSpan 实现求和).
        /// </summary>
        /// <param name="src">Source array.</param>
        /// <param name="srcCount">Source count</param>
        /// <returns>Returns the sum.</returns>
        public static TMy StaticSumForExSpan(TMy[] src, int srcCount) {
            TMy rt = 0; // Result.
            nint srcCountCur = srcCount;
            ExSpan<TMy> span = new ExSpan<TMy>(src, (nint)0, srcCountCur);
            for (nint i = 0; i < srcCountCur; i += 1) {
                rt += span[i];
            }
            return rt;
        }

        [Benchmark]
        public void SumForExSpan() {
            if (BenchmarkUtil.IsLastRun) {
                Volatile.Write(ref dstTMy, 0);
                //Debugger.Break();
            }
            dstTMy = StaticSumForExSpan(srcArray, srcArray.Length);
            CheckResult("SumForExSpan");
        }

        /// <summary>
        /// Summation using index access to ExSpan created by pointer (使用索引访问 指针创建的ExSpan 实现求和).
        /// Sum for ExSpan by pointer constructor.
        /// </summary>
        /// <param name="src">Source array.</param>
        /// <param name="srcCount">Source count</param>
        /// <returns>Returns the sum.</returns>
        public static unsafe TMy StaticSumForExSpanByPtr(TMy[] src, int srcCount) {
            TMy rt = 0; // Result.
            nint srcCountCur = srcCount;
            fixed (TMy* p0 = &src[0]) {
                ExSpan<TMy> span = new ExSpan<TMy>(p0, srcCountCur);
                for (nint i = 0; i < srcCountCur; i += 1) {
                    rt += span[i];
                }
            }
            return rt;
        }

        [Benchmark]
        public void SumForExSpanByPtr() {
            dstTMy = StaticSumForExSpanByPtr(srcArray, srcArray.Length);
            CheckResult("SumForExSpanByPtr");
        }

        /// <summary>
        /// Summation using native pointer access to ExSpan (使用原生指针访问 ExSpan 实现求和).
        /// Sum for ExSpan use pointer.
        /// </summary>
        /// <param name="src">Source array.</param>
        /// <param name="srcCount">Source count</param>
        /// <returns>Returns the sum.</returns>
        public static unsafe TMy StaticSumForExSpanUsePtr(TMy[] src, int srcCount) {
            TMy rt = 0; // Result.
            nint srcCountCur = srcCount;
            ExSpan<TMy> span = new ExSpan<TMy>(src, (nint)0, srcCountCur);
            fixed (TMy* p0 = &span.GetPinnableReference()) {
                TMy* pEnd = p0 + srcCount;
                TMy* p = p0;
                for (; p < pEnd; ++p) {
                    rt += *p;
                }
            }
            return rt;
        }

        [Benchmark]
        public void SumForExSpanUsePtr() {
            dstTMy = StaticSumForExSpanUsePtr(srcArray, srcArray.Length);
            CheckResult("SumForExSpanUsePtr");
        }

        /// <summary>
        /// Summation using managed pointer(ref) access to ExSpan (使用 托管指针(ref) 访问 ExSpan 实现求和).
        /// </summary>
        /// <param name="src">Source array.</param>
        /// <param name="srcCount">Source count</param>
        /// <returns>Returns the sum.</returns>
        public static TMy StaticSumForExSpanUseRef(TMy[] src, int srcCount) {
            TMy rt = 0; // Result.
            nint srcCountCur = srcCount;
            ExSpan<TMy> span = new ExSpan<TMy>(src, (nint)0, srcCountCur);
            ref TMy p0 = ref span.GetPinnableReference(); // Or `ref TMy p0 = ref span[0]`.
            ref TMy pEnd = ref Unsafe.Add(ref p0, srcCount);
            ref TMy p = ref p0;
            for (; Unsafe.IsAddressLessThan(ref p, ref pEnd); p = ref Unsafe.Add(ref p, 1)) {
                rt += p;
            }
            return rt;
        }

        [Benchmark]
        public void SumForExSpanUseRef() {
            dstTMy = StaticSumForExSpanUseRef(srcArray, srcArray.Length);
            CheckResult("SumForExSpanUseRef");
        }

    }
}
