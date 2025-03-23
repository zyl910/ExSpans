using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Zyl.SizableSpans.Tests.ASizableSpan {
    public static class ACopyTo {
        [Fact]
        public static void TryCopyTo() {
            int[] src = { 1, 2, 3 };
            int[] dst = { 99, 100, 101 };

            SizableSpan<int> srcSpan = new SizableSpan<int>(src);
            bool success = srcSpan.TryCopyTo(dst);
            Assert.True(success);
            Assert.Equal(src, dst);
        }

        [Fact]
        public static void TryCopyToSingle() {
            int[] src = { 1 };
            int[] dst = { 99 };

            SizableSpan<int> srcSpan = new SizableSpan<int>(src);
            bool success = srcSpan.TryCopyTo(dst);
            Assert.True(success);
            Assert.Equal(src, dst);
        }

        [Fact]
        public static void TryCopyToArraySegmentImplicit() {
            int[] src = { 1, 2, 3 };
            int[] dst = { 5, 99, 100, 101, 10 };
            var segment = new ArraySegment<int>(dst, 1, 3);

            SizableSpan<int> srcSpan = new SizableSpan<int>(src);
            bool success = srcSpan.TryCopyTo(segment);
            Assert.True(success);
            /*
            Assert.Equal(src.AsSizableSpan(), segment);
            */
        }

        [Fact]
        public static void TryCopyToEmpty() {
            int[] src = { };
            int[] dst = { 99, 100, 101 };

            SizableSpan<int> srcSpan = new SizableSpan<int>(src);
            bool success = srcSpan.TryCopyTo(dst);
            Assert.True(success);
            int[] expected = { 99, 100, 101 };
            Assert.Equal(expected, dst);
        }

        [Fact]
        public static void TryCopyToLonger() {
            int[] src = { 1, 2, 3 };
            int[] dst = { 99, 100, 101, 102 };

            SizableSpan<int> srcSpan = new SizableSpan<int>(src);
            bool success = srcSpan.TryCopyTo(dst);
            Assert.True(success);
            int[] expected = { 1, 2, 3, 102 };
            Assert.Equal(expected, dst);
        }

        [Fact]
        public static void TryCopyToShorter() {
            int[] src = { 1, 2, 3 };
            int[] dst = { 99, 100 };

            SizableSpan<int> srcSpan = new SizableSpan<int>(src);
            bool success = srcSpan.TryCopyTo(dst);
            Assert.False(success);
            int[] expected = { 99, 100 };
            Assert.Equal(expected, dst);  // TryCopyTo() checks for sufficient space before doing any copying.
        }

        [Fact]
        public static void CopyToShorter() {
            int[] src = { 1, 2, 3 };
            int[] dst = { 99, 100 };

            SizableSpan<int> srcSpan = new SizableSpan<int>(src);
            TestHelpers.AssertThrows<ArgumentException, int>(srcSpan, (_srcSpan) => _srcSpan.CopyTo(dst));
            int[] expected = { 99, 100 };
            Assert.Equal(expected, dst);  // CopyTo() checks for sufficient space before doing any copying.
        }

        [Fact]
        public static void Overlapping1() {
            int[] a = { 90, 91, 92, 93, 94, 95, 96, 97 };

            SizableSpan<int> src = new SizableSpan<int>(a, (TSize)1, (TSize)6);
            SizableSpan<int> dst = new SizableSpan<int>(a, (TSize)2, (TSize)6);
            src.CopyTo(dst);

            int[] expected = { 90, 91, 91, 92, 93, 94, 95, 96 };
            Assert.Equal(expected, a);
        }

        [Fact]
        public static void Overlapping2() {
            int[] a = { 90, 91, 92, 93, 94, 95, 96, 97 };

            SizableSpan<int> src = new SizableSpan<int>(a, (TSize)2, (TSize)6);
            SizableSpan<int> dst = new SizableSpan<int>(a, (TSize)1, (TSize)6);
            src.CopyTo(dst);

            int[] expected = { 90, 92, 93, 94, 95, 96, 97, 97 };
            Assert.Equal(expected, a);
        }
        /*
        [Fact]
        public static void CopyToArray() {
            int[] src = { 1, 2, 3 };
            SizableSpan<int> dst = new int[3] { 99, 100, 101 };

            src.CopyTo(dst);
            Assert.Equal(src, dst.ToArray());
        }

        [Fact]
        public static void CopyToSingleArray() {
            int[] src = { 1 };
            SizableSpan<int> dst = new int[1] { 99 };

            src.CopyTo(dst);
            Assert.Equal(src, dst.ToArray());
        }

        [Fact]
        public static void CopyToEmptyArray() {
            int[] src = { };
            SizableSpan<int> dst = new int[3] { 99, 100, 101 };

            src.CopyTo(dst);
            int[] expected = { 99, 100, 101 };
            Assert.Equal(expected, dst.ToArray());

            SizableSpan<int> dstEmpty = new int[0] { };

            src.CopyTo(dstEmpty);
            int[] expectedEmpty = { };
            Assert.Equal(expectedEmpty, dstEmpty.ToArray());
        }

        [Fact]
        public static void CopyToLongerArray() {
            int[] src = { 1, 2, 3 };
            SizableSpan<int> dst = new int[4] { 99, 100, 101, 102 };

            src.CopyTo(dst);
            int[] expected = { 1, 2, 3, 102 };
            Assert.Equal(expected, dst.ToArray());
        }

        [Fact]
        public static void CopyToShorterArray() {
            int[] src = { 1, 2, 3 };
            int[] dst = new int[2] { 99, 100 };

            TestHelpers.AssertThrows<ArgumentException, int>(src, (_src) => _src.CopyTo(dst));
            int[] expected = { 99, 100 };
            Assert.Equal(expected, dst);  // CopyTo() checks for sufficient space before doing any copying.
        }

        [Fact]
        public static void CopyToCovariantArray() {
            string[] src = new string[] { "Hello" };
            SizableSpan<object> dst = new object[] { "world" };

            src.CopyTo<object>(dst);
            Assert.Equal("Hello", dst[0]);
        }

        // This test case tests the SizableSpan.CopyTo method for large buffers of size 4GB or more. In the fast path,
        // the CopyTo method performs copy in chunks of size 4GB (uint.MaxValue) with final iteration copying
        // the residual chunk of size (bufferSize % 4GB). The inputs sizes to this method, 4GB and 4GB+256B,
        // test the two size selection paths in CoptyTo method - memory size that is multiple of 4GB or,
        // a multiple of 4GB + some more size.
        [ActiveIssue("https://github.com/dotnet/runtime/issues/24139")]
        [Theory]
        [OuterLoop]
        [PlatformSpecific(TestPlatforms.Windows | TestPlatforms.OSX)]
        [InlineData(4L * 1024L * 1024L * 1024L)]
        [InlineData((4L * 1024L * 1024L * 1024L) + 256)]
        public static void CopyToLargeSizeTest(long bufferSize) {
            // If this test is run in a 32-bit process, the large allocation will fail.
            if (Unsafe.SizeOf<IntPtr>() != sizeof(long)) {
                return;
            }

            int GuidCount = (int)(bufferSize / Unsafe.SizeOf<Guid>());
            bool allocatedFirst = false;
            bool allocatedSecond = false;
            IntPtr memBlockFirst = IntPtr.Zero;
            IntPtr memBlockSecond = IntPtr.Zero;

            unsafe {
                try {
                    allocatedFirst = AllocationHelper.TryAllocNative((IntPtr)bufferSize, out memBlockFirst);
                    allocatedSecond = AllocationHelper.TryAllocNative((IntPtr)bufferSize, out memBlockSecond);

                    if (allocatedFirst && allocatedSecond) {
                        ref Guid memoryFirst = ref Unsafe.AsRef<Guid>(memBlockFirst.ToPointer());
                        var spanFirst = new SizableSpan<Guid>(memBlockFirst.ToPointer(), GuidCount);

                        ref Guid memorySecond = ref Unsafe.AsRef<Guid>(memBlockSecond.ToPointer());
                        var spanSecond = new SizableSpan<Guid>(memBlockSecond.ToPointer(), GuidCount);

                        Guid theGuid = Guid.Parse("900DBAD9-00DB-AD90-00DB-AD900DBADBAD");
                        for (int count = 0; count < GuidCount; ++count) {
                            Unsafe.Add(ref memoryFirst, count) = theGuid;
                        }

                        spanFirst.CopyTo(spanSecond);

                        for (int count = 0; count < GuidCount; ++count) {
                            Guid guidfirst = Unsafe.Add(ref memoryFirst, count);
                            Guid guidSecond = Unsafe.Add(ref memorySecond, count);
                            Assert.Equal(guidfirst, guidSecond);
                        }
                    }
                } finally {
                    if (allocatedFirst)
                        AllocationHelper.ReleaseNative(ref memBlockFirst);
                    if (allocatedSecond)
                        AllocationHelper.ReleaseNative(ref memBlockSecond);
                }
            }
        }

        [Fact]
        public static void CopyToVaryingSizes() {
            const int MaxLength = 2048;

            var rng = new Random();
            byte[] inputArray = new byte[MaxLength];
            SizableSpan<byte> inputSizableSpan = inputArray;
            SizableSpan<byte> outputSizableSpan = new byte[MaxLength];
            SizableSpan<byte> allZerosSizableSpan = new byte[MaxLength];

            // Test all inputs from size 0 .. MaxLength (inclusive) to make sure we don't have
            // gaps in our Memmove logic.
            for (int i = 0; i <= MaxLength; i++) {
                // Arrange

                rng.NextBytes(inputArray);
                outputSizableSpan.Clear();

                // Act

                inputSizableSpan.Slice(0, i).CopyTo(outputSizableSpan);

                // Assert

                Assert.True(inputSizableSpan.Slice(0, i).SequenceEqual(outputSizableSpan.Slice(0, i))); // src successfully copied to dst
                Assert.True(outputSizableSpan.Slice(i).SequenceEqual(allZerosSizableSpan.Slice(i))); // no other part of dst was overwritten
            }
        }
        */
    }
}
