﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Zyl.ExSpans.Tests.Fake.Attributes;
using static Zyl.ExSpans.Tests.TestHelpers;

namespace Zyl.ExSpans.Tests.AExSpan {
    public static class AClear {
        [Fact]
        public static void ClearEmpty() {
            var span = ExSpan<byte>.Empty;
            span.Clear();
        }

        [Fact]
        public static void ClearEmptyWithReference() {
            var span = ExSpan<string>.Empty;
            span.Clear();
        }

        [Fact]
        public static void ClearByteLonger() {
            const byte initial = 5;
            var actual = new byte[2048];
            for (int i = 0; i < actual.Length; i++) {
                actual[i] = initial;
            }
            var expected = new byte[actual.Length];

            var span = new ExSpan<byte>(actual);
            span.Clear();
            Assert.Equal<byte>(expected, actual);
        }

        [Fact]
        public static void ClearByteUnaligned() {
            const byte initial = 5;
            const int length = 32;
            var actualFull = new byte[length];
            for (int i = 0; i < length; i++) {
                actualFull[i] = initial;
            }
            var expectedFull = new byte[length];

            var start = 1;
            var expectedExSpan = new ExSpan<byte>(expectedFull, (TSize)start, (TSize)(length - start - 1));
            var actualExSpan = new ExSpan<byte>(actualFull, (TSize)start, (TSize)(length - start - 1));
            actualExSpan.Clear();

            byte[] actual = actualExSpan.ToArray();
            byte[] expected = expectedExSpan.ToArray();
            Assert.Equal<byte>(expected, actual);
            Assert.Equal(initial, actualFull[0]);
            Assert.Equal(initial, actualFull[length - 1]);
        }

        [Fact]
        public static unsafe void ClearByteUnalignedFixed() {
            const byte initial = 5;
            const int length = 32;
            var actualFull = new byte[length];
            for (int i = 0; i < length; i++) {
                actualFull[i] = initial;
            }
            var expectedFull = new byte[length];

            var start = 1;
            var expectedExSpan = new ExSpan<byte>(expectedFull, (TSize)start, (TSize)(length - start - 1));
            fixed (byte* p = actualFull) {
                var actualExSpan = new ExSpan<byte>(p + start, (TSize)(length - start - 1));
                actualExSpan.Clear();

                byte[] actual = actualExSpan.ToArray();
                byte[] expected = expectedExSpan.ToArray();
                Assert.Equal<byte>(expected, actual);
                Assert.Equal(initial, actualFull[0]);
                Assert.Equal(initial, actualFull[length - 1]);
            }
        }

        [Fact]
        public static void ClearIntPtrOffset() {
            IntPtr initial = IntPtr.Zero + 5;
            const int length = 32;
            var actualFull = new IntPtr[length];
            for (int i = 0; i < length; i++) {
                actualFull[i] = initial;
            }
            var expectedFull = new IntPtr[length];

            var start = 2;
            var expectedExSpan = new ExSpan<IntPtr>(expectedFull, (TSize)start, (TSize)(length - start - 1));
            var actualExSpan = new ExSpan<IntPtr>(actualFull, (TSize)start, (TSize)(length - start - 1));
            actualExSpan.Clear();

            IntPtr[] actual = actualExSpan.ToArray();
            IntPtr[] expected = expectedExSpan.ToArray();
            Assert.Equal<IntPtr>(expected, actual);
            Assert.Equal(initial, actualFull[0]);
            Assert.Equal(initial, actualFull[length - 1]);
        }

        [Fact]
        public static void ClearIntPtrLonger() {
            IntPtr initial = IntPtr.Zero + 5;
            var actual = new IntPtr[2048];
            for (int i = 0; i < actual.Length; i++) {
                actual[i] = initial;
            }
            var expected = new IntPtr[actual.Length];

            var span = new ExSpan<IntPtr>(actual);
            span.Clear();
            Assert.Equal<IntPtr>(expected, actual);
        }

        [Fact]
        public static void ClearValueTypeWithoutReferences() {
            int[] actual = { 1, 2, 3 };
            int[] expected = { 0, 0, 0 };

            var span = new ExSpan<int>(actual);
            span.Clear();
            Assert.Equal<int>(expected, actual);
        }

        [Fact]
        public static void ClearValueTypeWithoutReferencesLonger() {
            int[] actual = new int[2048];
            for (int i = 0; i < actual.Length; i++) {
                actual[i] = i + 1;
            }
            int[] expected = new int[actual.Length];

            var span = new ExSpan<int>(actual);
            span.Clear();
            Assert.Equal<int>(expected, actual);
        }

        [Fact]
        public static void ClearValueTypeWithoutReferencesPointerSize() {
            long[] actual = new long[15];
            for (int i = 0; i < actual.Length; i++) {
                actual[i] = i + 1;
            }
            long[] expected = new long[actual.Length];

            var span = new ExSpan<long>(actual);
            span.Clear();
            Assert.Equal<long>(expected, actual);
        }

        [Fact]
        public static void ClearReferenceType() {
            string[] actual = { "a", "b", "c" };
            string?[] expected = { null, null, null };

            var span = new ExSpan<string>(actual);
            span.Clear();
            Assert.Equal(expected, actual);
        }

        [Fact]
        public static void ClearReferenceTypeLonger() {
            string[] actual = new string[2048];
            for (int i = 0; i < actual.Length; i++) {
                actual[i] = (i + 1).ToString();
            }
            string[] expected = new string[actual.Length];

            var span = new ExSpan<string>(actual);
            span.Clear();
            Assert.Equal(expected, actual);
        }

        [Fact]
        public static void ClearReferenceTypeSlice() {
            // A string array [ ""1", ..., "20" ]
            string[] baseline = Enumerable.Range(1, 20).Select(i => i.ToString()).ToArray();

            for (int i = 0; i < 16; i++) {
                // Going to clear array.Slice(1, i) manually,
                // then compare it against array.Slice(1, i).Clear().
                // Test is written this way to allow detecting overrunning bounds.

                string?[] expected = (string[])baseline.Clone();
                for (int j = 1; j <= i; j++) {
                    expected[j] = null;
                }

                string[] actual = (string[])baseline.Clone();
                actual.AsExSpan((TSize)1, (TSize)i).Clear();

                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public static void ClearEnumType() {
            TestEnum[] actual = { TestEnum.E0, TestEnum.E1, TestEnum.E2 };
            TestEnum[] expected = { default, default, default };

            var span = new ExSpan<TestEnum>(actual);
            span.Clear();
            Assert.Equal<TestEnum>(expected, actual);
        }

        [Fact]
        public static void ClearValueTypeWithReferences() {
            TestValueTypeWithReference[] actual = {
                new TestValueTypeWithReference() { I = 1, S = "a" },
                new TestValueTypeWithReference() { I = 2, S = "b" },
                new TestValueTypeWithReference() { I = 3, S = "c" } };
            TestValueTypeWithReference[] expected = {
                default,
                default,
                default };

            var span = new ExSpan<TestValueTypeWithReference>(actual);
            span.Clear();
            Assert.Equal<TestValueTypeWithReference>(expected, actual);
        }
        
        // NOTE: ClearLongerThanUintMaxValueBytes test is constrained to run on Windows and MacOSX because it causes
        //       problems on Linux due to the way deferred memory allocation works. On Linux, the allocation can
        //       succeed even if there is not enough memory but then the test may get killed by the OOM killer at the
        //       time the memory is accessed which triggers the full memory allocation.
        [Fact]
        [OuterLoop]
        [PlatformSpecific(TestPlatforms.Windows | TestPlatforms.OSX)]
        static unsafe void ClearLongerThanUintMaxValueBytes() {
#if CALL_LARGE
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
                return;
            }
            if (sizeof(IntPtr) == sizeof(long)) {
                // Arrange
                nint bytes = unchecked((nint)(((long)int.MaxValue) * sizeof(int)));
                int length = (int)(((long)bytes) / sizeof(int));

                if (!AllocationHelper.TryAllocNative(bytes, out IntPtr memory)) {
                    Console.WriteLine($"Span.Clear test {nameof(ClearLongerThanUintMaxValueBytes)} skipped (could not alloc memory).");
                    return;
                }

                try {
                    ExSpan<int> span = new ExSpan<int>(memory.ToPointer(), (nint)length);
                    span.Fill(5);

                    // Act
                    span.Clear();

                    // Assert using custom code for perf and to avoid allocating extra memory
                    ref int data = ref Unsafe.AsRef<int>(memory.ToPointer());
                    for (int i = 0; i < length; i++) {
                        var actual = Unsafe.Add(ref data, i);
                        if (actual != 0) {
                            Assert.Equal(0, actual);
                        }
                    }
                } finally {
                    AllocationHelper.ReleaseNative(ref memory);
                }
            }
#else
#endif // CALL_LARGE
        }

    }
}
