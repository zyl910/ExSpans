using Xunit;

namespace Zyl.ExSpans.Tests.AReadOnlyExSpan {
    public static partial class AEndsWith {
        [Fact]
        public static void ZeroLengthEndsWith_Byte() {
            byte[] a = new byte[3];

            ReadOnlyExSpan<byte> span = new ReadOnlyExSpan<byte>(a);
            ReadOnlyExSpan<byte> slice = new ReadOnlyExSpan<byte>(a, 2, 0);
            bool b = span.EndsWith(slice);
            Assert.True(b);
        }

        [Fact]
        public static void SameSpanEndsWith_Byte() {
            byte[] a = { 4, 5, 6 };
            ReadOnlyExSpan<byte> span = new ReadOnlyExSpan<byte>(a);
            bool b = span.EndsWith(span);
            Assert.True(b);
        }

        [Fact]
        public static void LengthMismatchEndsWith_Byte() {
            byte[] a = { 4, 5, 6 };
            ReadOnlyExSpan<byte> span = new ReadOnlyExSpan<byte>(a, 0, 2);
            ReadOnlyExSpan<byte> slice = new ReadOnlyExSpan<byte>(a, 0, 3);
            bool b = span.EndsWith(slice);
            Assert.False(b);
        }

        [Fact]
        public static void EndsWithMatch_Byte() {
            byte[] a = { 4, 5, 6 };
            ReadOnlyExSpan<byte> span = new ReadOnlyExSpan<byte>(a, 0, 3);
            ReadOnlyExSpan<byte> slice = new ReadOnlyExSpan<byte>(a, 1, 2);
            bool b = span.EndsWith(slice);
            Assert.True(b);
        }

        [Fact]
        public static void EndsWithMatchDifferentSpans_Byte() {
            byte[] a = { 4, 5, 6 };
            byte[] b = { 4, 5, 6 };
            ReadOnlyExSpan<byte> span = new ReadOnlyExSpan<byte>(a, 0, 3);
            ReadOnlyExSpan<byte> slice = new ReadOnlyExSpan<byte>(b, 0, 3);
            bool c = span.EndsWith(slice);
            Assert.True(c);
        }

        [Fact]
        public static void EndsWithNoMatch_Byte() {
            for (int length = 1; length < 32; length++) {
                for (int mismatchIndex = 0; mismatchIndex < length; mismatchIndex++) {
                    byte[] first = new byte[length];
                    byte[] second = new byte[length];
                    for (int i = 0; i < length; i++) {
                        first[i] = second[i] = (byte)(i + 1);
                    }

                    second[mismatchIndex] = (byte)(second[mismatchIndex] + 1);

                    ReadOnlyExSpan<byte> firstSpan = new ReadOnlyExSpan<byte>(first);
                    ReadOnlyExSpan<byte> secondSpan = new ReadOnlyExSpan<byte>(second);
                    bool b = firstSpan.EndsWith(secondSpan);
                    Assert.False(b);
                }
            }
        }

        [Fact]
        public static void MakeSureNoEndsWithChecksGoOutOfRange_Byte() {
            for (int length = 0; length < 100; length++) {
                byte[] first = new byte[length + 2];
                first[0] = 99;
                first[length + 1] = 99;
                byte[] second = new byte[length + 2];
                second[0] = 100;
                second[length + 1] = 100;
                ReadOnlyExSpan<byte> span1 = new ReadOnlyExSpan<byte>(first, 1, length);
                ReadOnlyExSpan<byte> span2 = new ReadOnlyExSpan<byte>(second, 1, length);
                bool b = span1.EndsWith(span2);
                Assert.True(b);
            }
        }
    }
}
