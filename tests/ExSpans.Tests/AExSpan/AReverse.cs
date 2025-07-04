using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Xunit;
using static Zyl.ExSpans.Tests.TestHelpers;

namespace Zyl.ExSpans.Tests.AExSpan {
    public static partial class AReverse {
        [Fact]
        public static void ReverseEmpty() {
            var span = ExSpan<byte>.Empty;
            span.Reverse();

            byte[] actual = { 1, 2, 3, 4 };
            byte[] expected = { 1, 2, 3, 4 };

            span = actual;
            span.Slice(2, 0).Reverse();

            Assert.Equal<byte>(expected, span.ToArray());
        }

        [Fact]
        public static void ReverseEmptyWithReference() {
            var span = ExSpan<string>.Empty;
            span.Reverse();

            string[] actual = { "a", "b", "c", "d" };
            string[] expected = { "a", "b", "c", "d" };

            span = actual;
            span.Slice(2, 0).Reverse();

            Assert.Equal(expected, span.ToArray());
        }

        [Fact]
        public static void ReverseByte() {
            for (int length = 0; length < byte.MaxValue; length++) {
                byte[] actual = new byte[length];
                byte[] expected = new byte[length];
                for (int i = 0; i < length; i++) {
                    actual[i] = expected[length - 1 - i] = (byte)i;
                }

                var span = new ExSpan<byte>(actual);
                span.Reverse();
                Assert.Equal<byte>(expected, actual);
            }
        }

        [Fact]
        public static void ReverseByteTwiceReturnsOriginal() {
            byte[] actual = { 1, 2, 3, 4, 5 };
            byte[] expected = { 1, 2, 3, 4, 5 };

            ExSpan<byte> span = actual;
            span.Reverse();
            span.Reverse();
            Assert.Equal<byte>(expected, actual);
        }

        public static IEnumerable<object[]> GetReverseByteUnalignedArguments() {
            const int offset = 1;

            yield return new object[] { offset, 4 };
            yield return new object[] { offset, 5 };

            // vectorized execution paths for AVX2
            int cnt = 32; // Vector256<byte>.Count;
            yield return new object[] { offset, offset + cnt * 2 }; // even
            yield return new object[] { offset, offset + cnt * 2 + 1 }; // odd
            // vectorized execution paths for SSE2
            cnt = 16; // Vector128<byte>.Count;
            yield return new object[] { offset, offset + cnt * 2 }; // even
            yield return new object[] { offset, offset + cnt * 2 + 1 }; // odd
        }

        [Theory, MemberData(nameof(GetReverseByteUnalignedArguments))]
        public static void ReverseByteUnaligned(int offset, int length) {
            var actualFull = new byte[length];
            for (int i = 0; i < length; i++) {
                actualFull[i] = (byte)i;
            }
            byte[] expectedFull = new byte[length];
            Array.Copy(actualFull, expectedFull, length);
            ArrayReverse(expectedFull, offset, length - offset - 1);

            var expectedExSpan = new ExSpan<byte>(expectedFull, offset, length - offset - 1);
            var actualExSpan = new ExSpan<byte>(actualFull, offset, length - offset - 1);
            actualExSpan.Reverse();

            byte[] actual = actualExSpan.ToArray();
            byte[] expected = expectedExSpan.ToArray();
            Assert.Equal<byte>(expected, actual);
            Assert.Equal(expectedFull[0], actualFull[0]);
            Assert.Equal(expectedFull[length - 1], actualFull[length - 1]);
        }

        [Theory]
        [InlineData(3)] // non-vectorized, odd
        [InlineData(4)] // non-vectorized, even
        [InlineData(127)] // vectorized, odd
        [InlineData(128)] // vectorized, even
        public static void ReverseChar(int length) {
            char[] actual = new char[length];
            char[] expected = new char[length];
            for (int i = 0; i < length; i++) {
                actual[i] = expected[length - 1 - i] = (char)i;
            }

            var span = new ExSpan<char>(actual);
            span.Reverse();
            Assert.Equal<char>(expected, actual);
        }

        [Fact]
        public static void ReverseCharTwiceReturnsOriginal() {
            char[] actual = { 'a', 'b', 'c', 'd' };
            char[] expected = { 'a', 'b', 'c', 'd' };

            ExSpan<char> span = actual;
            span.Reverse();
            span.Reverse();
            Assert.Equal<char>(expected, actual);
        }

        public static IEnumerable<object[]> GetReverseIntPtrOffsetArguments() {
            const int offset = 2;

            yield return new object[] { offset, 2 };

            // vectorized execution paths for AVX2
            int avx2VectorSize = 32 / IntPtr.Size; // IntPtr.Size == 4 ? Vector256<int>.Count : Vector256<long>.Count;
            yield return new object[] { offset, offset + avx2VectorSize * 2 }; // even
            yield return new object[] { offset, offset + avx2VectorSize * 2 + 1 }; // odd
            // vectorized execution paths for SSE
            int ss2VectorSize = 16 / IntPtr.Size; // IntPtr.Size == 4 ? Vector128<int>.Count : Vector128<long>.Count;
            yield return new object[] { offset, offset + ss2VectorSize * 2 }; // even
            yield return new object[] { offset, offset + ss2VectorSize * 2 + 1 }; // odd
        }

        [Theory, MemberData(nameof(GetReverseIntPtrOffsetArguments))]
        public static void ReverseIntPtrOffset(int offset, int length) {
            var actualFull = new IntPtr[length];
            for (int i = 0; i < length; i++) {
                actualFull[i] = IntPtr.Zero + i;
            }
            IntPtr[] expectedFull = new IntPtr[length];
            Array.Copy(actualFull, expectedFull, length);
            ArrayReverse(expectedFull, offset, length - offset);

            var expectedExSpan = new ExSpan<IntPtr>(expectedFull, offset, length - offset);
            var actualExSpan = new ExSpan<IntPtr>(actualFull, offset, length - offset);
            actualExSpan.Reverse();

            IntPtr[] actual = actualExSpan.ToArray();
            IntPtr[] expected = expectedExSpan.ToArray();
            Assert.Equal<IntPtr>(expected, actual);
            Assert.Equal(expectedFull[0], actualFull[0]);
            Assert.Equal(expectedFull[1], actualFull[1]);
        }

        [Theory]
        [InlineData(2048)] // even
        [InlineData(2049)] // odd
        public static void ReverseValueTypeWithoutReferencesFourBytesSize(int length) {
            (short, short)[] actual = new (short, short)[length];
            (short, short)[] expected = new (short, short)[length];
            for (int i = 0; i < length; i++) {
                actual[i] = expected[length - 1 - i] = ((short)i, (short)(i >> 16));
            }

            var span = new ExSpan<(short, short)>(actual);
            span.Reverse();
            Assert.Equal<(short, short)>(expected, actual);
        }

        [Theory]
        [InlineData(16)] // even
        [InlineData(15)] // odd
        public static void ReverseValueTypeWithoutReferencesEightByteSize(int length) {
            (int, int)[] actual = new (int, int)[length];
            (int, int)[] expected = new (int, int)[length];
            for (int i = 0; i < length; i++) {
                actual[i] = expected[length - 1 - i] = (-i, i);
            }

            var span = new ExSpan<(int, int)>(actual);
            span.Reverse();
            Assert.Equal<(int, int)>(expected, actual);
        }

        [Fact]
        public static void ReverseReferenceType() {
            const int length = 2048;
            string[] actual = new string[length];
            string[] expected = new string[length];
            for (int i = 0; i < length; i++) {
                actual[i] = expected[length - 1 - i] = i.ToString();
            }

            var span = new ExSpan<string>(actual);
            span.Reverse();
            Assert.Equal(expected, actual);
        }

        [Fact]
        public static void ReverseReferenceTwiceReturnsOriginal() {
            string[] actual = { "a1", "b2", "c3" };
            string[] expected = { "a1", "b2", "c3" };

            var span = new ExSpan<string>(actual);
            span.Reverse();
            span.Reverse();
            Assert.Equal(expected, actual);
        }

        [Fact]
        public static void ReverseEnumType() {
            TestEnum[] actual = { TestEnum.E0, TestEnum.E1, TestEnum.E2 };
            TestEnum[] expected = { TestEnum.E2, TestEnum.E1, TestEnum.E0 };

            var span = new ExSpan<TestEnum>(actual);
            span.Reverse();
            Assert.Equal<TestEnum>(expected, actual);
        }

        [Fact]
        public static void ReverseValueTypeWithReferences() {
            TestValueTypeWithReference[] actual = {
                new TestValueTypeWithReference() { I = 1, S = "a" },
                new TestValueTypeWithReference() { I = 2, S = "b" },
                new TestValueTypeWithReference() { I = 3, S = "c" } };
            TestValueTypeWithReference[] expected = {
                new TestValueTypeWithReference() { I = 3, S = "c" },
                new TestValueTypeWithReference() { I = 2, S = "b" },
                new TestValueTypeWithReference() { I = 1, S = "a" } };

            var span = new ExSpan<TestValueTypeWithReference>(actual);
            span.Reverse();
            Assert.Equal<TestValueTypeWithReference>(expected, actual);
        }

        // this copy of the old Array.Reverse implementation allows us to test new,
        // vectorized logic used by both Array.Reverse and ExSpan.Reverse
        static void ArrayReverse<T>(T[] array, int index, int length) {
            if (length <= 1) {
                return;
            }

            int firstIndex = index;
            int lastIndex = firstIndex + length - 1;
            do {
                (array[firstIndex], array[lastIndex]) = (array[lastIndex], array[firstIndex]);
                firstIndex++;
                lastIndex--;
            } while (firstIndex < lastIndex);
        }
    }
}
