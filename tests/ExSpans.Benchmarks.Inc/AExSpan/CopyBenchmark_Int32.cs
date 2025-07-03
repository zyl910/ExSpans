#undef BENCHMARKS_OFF

using BenchmarkDotNet.Attributes;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Zyl.ExSpans.Impl;

#nullable enable
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
    public class CopyBenchmark_Int32 : AbstractSharedBenchmark_Int32 {
        protected static TMy[]? dstArray = null;

        protected override void ArraySetup() {
            base.ArraySetup();
            dstArray = new TMy[N];
        }

        /// <summary>Copy by <see cref="Span{T}.CopyTo"/> (用 <see cref="Span{T}.CopyTo"/> 复制).
        /// </summary>
        /// <param name="dst">Destination array.</param>
        /// <param name="src">Source array.</param>
        /// <param name="srcCount">Source count</param>
        /// <returns>Returns the check flag.</returns>
        public static TMy StaticCopySpan(TMy[] dst, TMy[] src, int srcCount) {
            src.AsSpan().CopyTo(dst.AsSpan());
            return dst[0];
        }

        [Benchmark(Baseline = true)]
        public void CopySpan() {
            dstTMy = StaticCopySpan(dstArray!, srcArray, srcArray.Length);
            if (CheckMode) {
                baselineTMy = dstTMy;
                BenchmarkUtil.WriteItem("# CopySpan", string.Format("{0}", baselineTMy));
            }
        }

        /// <summary>Copy by <see cref="ExSpan{T}.CopyTo"/> (用 <see cref="ExSpan{T}.CopyTo"/> 复制).</summary>
        /// <inheritdoc cref="StaticCopySpan"/>
        public static TMy StaticCopyExSpan(TMy[] dst, TMy[] src, int srcCount) {
            src.AsExSpan().CopyTo(dst.AsExSpan());
            return dst[0];
        }

        [Benchmark]
        public void CopyExSpan() {
            dstTMy = StaticCopyExSpan(dstArray!, srcArray, srcArray.Length);
            CheckResult("CopyExSpan");
        }

        /// <summary>Copy by <see cref="Buffer.MemoryCopy"/> (用 <see cref="Buffer.MemoryCopy"/> 复制).</summary>
        /// <inheritdoc cref="StaticCopySpan"/>
        public unsafe static TMy StaticCopyBuffer(TMy[] dst, TMy[] src, int srcCount) {
            fixed (TMy* pDst = &dst[0], pSrc = &src[0]) {
                nint sizeInBytes = (nint)srcCount * Unsafe.SizeOf<TMy>();
                Buffer.MemoryCopy(pSrc, pDst, sizeInBytes, sizeInBytes);
            }
            return dst[0];
        }

        [Benchmark]
        public void CopyBuffer() {
            dstTMy = StaticCopyBuffer(dstArray!, srcArray, srcArray.Length);
            CheckResult("CopyBuffer");
        }

        /// <summary>Copy by <see cref="BufferHelper.MemoryCopy"/> (用 <see cref="BufferHelper.MemoryCopy"/> 复制).</summary>
        /// <inheritdoc cref="StaticCopySpan"/>
        public unsafe static TMy StaticCopyBufferHelper(TMy[] dst, TMy[] src, int srcCount) {
            fixed (TMy* pDst = &dst[0], pSrc = &src[0]) {
                nint sizeInBytes = (nint)srcCount * Unsafe.SizeOf<TMy>();
                BufferHelper.MemoryCopy(pSrc, pDst, sizeInBytes, sizeInBytes);
            }
            return dst[0];
        }

        [Benchmark]
        public void CopyBufferHelper() {
            dstTMy = StaticCopyBufferHelper(dstArray!, srcArray, srcArray.Length);
            CheckResult("CopyBufferHelper");
        }

        /// <summary>Copy by <see cref="BufferHelper.MemoryCopy"/> (用 <see cref="BufferHelper.MemoryCopy"/> 复制).</summary>
        /// <inheritdoc cref="StaticCopySpan"/>
        public static TMy StaticCopyBufferHelperCore(TMy[] dst, TMy[] src, int srcCount) {
            BufferHelper.Memmove(ref dst[0], ref src[0], (uint)srcCount);
            return dst[0];
        }

        [Benchmark]
        public void CopyBufferHelperCore() {
            dstTMy = StaticCopyBufferHelperCore(dstArray!, srcArray, srcArray.Length);
            CheckResult("CopyBufferHelperCore");
        }

        [DllImport("ntdll.dll", EntryPoint = "RtlMoveMemory", SetLastError = false)]
        private static extern void RtlMoveMemory(IntPtr dest, IntPtr src, nuint size);

        /// <summary>Copy by WinAPI - RtlMoveMemory (用 WinAPI - RtlMoveMemory 复制).</summary>
        /// <inheritdoc cref="StaticCopySpan"/>
        public unsafe static TMy StaticCopyWinApi(TMy[] dst, TMy[] src, int srcCount) {
            fixed (TMy* pDst = &dst[0], pSrc = &src[0]) {
                nint sizeInBytes = (nint)srcCount * Unsafe.SizeOf<TMy>();
                RtlMoveMemory((nint)pDst, (nint)pSrc, (nuint)sizeInBytes);
            }
            return dst[0];
        }

        [Benchmark]
        public void CopyWinApi() {
            dstTMy = StaticCopyWinApi(dstArray!, srcArray, srcArray.Length);
            CheckResult("CopyWinApi");
        }

    }
}
#nullable restore
