#if DEBUG
#else
#define CALL_LARGE
#endif // DEBUG

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Zyl.ExSpans.Tests.Fake.Attributes;

namespace Zyl.ExSpans.Tests.AExSpan {
    public static class ACopyTo {
        [Fact]
        public static void TryCopyTo() {
            int[] src = { 1, 2, 3 };
            int[] dst = { 99, 100, 101 };

            ExSpan<int> srcSpan = new ExSpan<int>(src);
            bool success = srcSpan.TryCopyTo(dst);
            Assert.True(success);
            Assert.Equal(src, dst);
        }

        [Fact]
        public static void TryCopyToSingle() {
            int[] src = { 1 };
            int[] dst = { 99 };

            ExSpan<int> srcSpan = new ExSpan<int>(src);
            bool success = srcSpan.TryCopyTo(dst);
            Assert.True(success);
            Assert.Equal(src, dst);
        }

        [Fact]
        public static void TryCopyToArraySegmentImplicit() {
            int[] src = { 1, 2, 3 };
            int[] dst = { 5, 99, 100, 101, 10 };
            var segment = new ArraySegment<int>(dst, 1, 3);

            ExSpan<int> srcSpan = new ExSpan<int>(src);
            bool success = srcSpan.TryCopyTo(segment);
            Assert.True(success);
            /*
            Assert.Equal(src.AsExSpan(), segment);
            */
        }

        [Fact]
        public static void TryCopyToEmpty() {
            int[] src = { };
            int[] dst = { 99, 100, 101 };

            ExSpan<int> srcSpan = new ExSpan<int>(src);
            bool success = srcSpan.TryCopyTo(dst);
            Assert.True(success);
            int[] expected = { 99, 100, 101 };
            Assert.Equal(expected, dst);
        }

        [Fact]
        public static void TryCopyToLonger() {
            int[] src = { 1, 2, 3 };
            int[] dst = { 99, 100, 101, 102 };

            ExSpan<int> srcSpan = new ExSpan<int>(src);
            bool success = srcSpan.TryCopyTo(dst);
            Assert.True(success);
            int[] expected = { 1, 2, 3, 102 };
            Assert.Equal(expected, dst);
        }

        [Fact]
        public static void TryCopyToShorter() {
            int[] src = { 1, 2, 3 };
            int[] dst = { 99, 100 };

            ExSpan<int> srcSpan = new ExSpan<int>(src);
            bool success = srcSpan.TryCopyTo(dst);
            Assert.False(success);
            int[] expected = { 99, 100 };
            Assert.Equal(expected, dst);  // TryCopyTo() checks for sufficient space before doing any copying.
        }

        [Fact]
        public static void CopyToShorter() {
            int[] src = { 1, 2, 3 };
            int[] dst = { 99, 100 };

            ExSpan<int> srcSpan = new ExSpan<int>(src);
            TestHelpers.AssertThrows<ArgumentException, int>(srcSpan, (_srcSpan) => _srcSpan.CopyTo(dst));
            int[] expected = { 99, 100 };
            Assert.Equal(expected, dst);  // CopyTo() checks for sufficient space before doing any copying.
        }

        [Fact]
        public static void Overlapping1() {
            int[] a = { 90, 91, 92, 93, 94, 95, 96, 97 };

            ExSpan<int> src = new ExSpan<int>(a, (TSize)1, (TSize)6);
            ExSpan<int> dst = new ExSpan<int>(a, (TSize)2, (TSize)6);
            src.CopyTo(dst);

            int[] expected = { 90, 91, 91, 92, 93, 94, 95, 96 };
            Assert.Equal(expected, a);
        }

        [Fact]
        public static void Overlapping2() {
            int[] a = { 90, 91, 92, 93, 94, 95, 96, 97 };

            ExSpan<int> src = new ExSpan<int>(a, (TSize)2, (TSize)6);
            ExSpan<int> dst = new ExSpan<int>(a, (TSize)1, (TSize)6);
            src.CopyTo(dst);

            int[] expected = { 90, 92, 93, 94, 95, 96, 97, 97 };
            Assert.Equal(expected, a);
        }
        
        [Fact]
        public static void CopyToArray() {
            int[] src = { 1, 2, 3 };
            ExSpan<int> dst = new int[3] { 99, 100, 101 };

            src.CopyTo(dst);
            Assert.Equal(src, dst.ToArray());
        }

        [Fact]
        public static void CopyToSingleArray() {
            int[] src = { 1 };
            ExSpan<int> dst = new int[1] { 99 };

            src.CopyTo(dst);
            Assert.Equal(src, dst.ToArray());
        }

        [Fact]
        public static void CopyToEmptyArray() {
            int[] src = { };
            ExSpan<int> dst = new int[3] { 99, 100, 101 };

            src.CopyTo(dst);
            int[] expected = { 99, 100, 101 };
            Assert.Equal(expected, dst.ToArray());

            ExSpan<int> dstEmpty = new int[0] { };

            src.CopyTo(dstEmpty);
            int[] expectedEmpty = { };
            Assert.Equal(expectedEmpty, dstEmpty.ToArray());
        }

        [Fact]
        public static void CopyToLongerArray() {
            int[] src = { 1, 2, 3 };
            ExSpan<int> dst = new int[4] { 99, 100, 101, 102 };

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

#if !NET7_0_OR_GREATER
        [Fact]
        public static void CopyToCovariantArray() {
            string[] src = new string[] { "Hello" };
            ExSpan<object> dst = new object[] { "world" };

            src.CopyTo<object>(dst); // .NET 7.0+ - System.ArrayTypeMismatchException : Attempted to access an element as a type incompatible with the array.
            Assert.Equal("Hello", dst[(nint)0]);
        }
#endif // !NET7_0_OR_GREATER

        // This test case tests the ExSpan.CopyTo method for large buffers of size 4GB or more. In the fast path,
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
#if CALL_LARGE
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
                return;
            }
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
                        var spanFirst = new ExSpan<Guid>(memBlockFirst.ToPointer(), (nint)GuidCount);

                        ref Guid memorySecond = ref Unsafe.AsRef<Guid>(memBlockSecond.ToPointer());
                        var spanSecond = new ExSpan<Guid>(memBlockSecond.ToPointer(), (nint)GuidCount);

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
#else
            _ = bufferSize;
#endif // CALL_LARGE
        }

        [Fact]
        public static void CopyToVaryingSizes() {
            const int MaxLength = 2048;

            var rng = new Random();
            byte[] inputArray = new byte[MaxLength];
            ExSpan<byte> inputExSpan = inputArray;
            ExSpan<byte> outputExSpan = new byte[MaxLength];
            ExSpan<byte> allZerosExSpan = new byte[MaxLength];

            // Test all inputs from size 0 .. MaxLength (inclusive) to make sure we don't have
            // gaps in our Memmove logic.
            for (int i = 0; i <= MaxLength; i++) {
                // Arrange

                rng.NextBytes(inputArray);
                outputExSpan.Clear();

                // Act

                inputExSpan.Slice((TSize)0, (TSize)i).CopyTo(outputExSpan);

                // Assert

                Assert.True(inputExSpan.Slice((TSize)0, (TSize)i).SequenceEqual(outputExSpan.Slice((TSize)0, (TSize)i))); // src successfully copied to dst
                Assert.True(outputExSpan.Slice((TSize)i).SequenceEqual(allZerosExSpan.Slice((TSize)i))); // no other part of dst was overwritten
            }
        }

    }
}
