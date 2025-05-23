using Xunit;

namespace Zyl.ExSpans.Tests.AExSpan {
    public static partial class AStartsWith {
        [Fact]
        public static void ZeroLengthStartsWith_Byte() {
            byte[] a = new byte[3];

            ExSpan<byte> span = new ExSpan<byte>(a);
            ReadOnlyExSpan<byte> slice = new ReadOnlyExSpan<byte>(a, 2, 0);
            bool b = span.StartsWith<byte>(slice);
            Assert.True(b);
        }

        [Fact]
        public static void SameExSpanStartsWith_Byte() {
            byte[] a = { 4, 5, 6 };
            ExSpan<byte> span = new ExSpan<byte>(a);
            bool b = span.StartsWith<byte>(span);
            Assert.True(b);
        }

        [Fact]
        public static void LengthMismatchStartsWith_Byte() {
            byte[] a = { 4, 5, 6 };
            ExSpan<byte> span = new ExSpan<byte>(a, 0, 2);
            ReadOnlyExSpan<byte> slice = new ReadOnlyExSpan<byte>(a, 0, 3);
            bool b = span.StartsWith<byte>(slice);
            Assert.False(b);
        }

        [Fact]
        public static void StartsWithMatch_Byte() {
            byte[] a = { 4, 5, 6 };
            ExSpan<byte> span = new ExSpan<byte>(a, 0, 3);
            ReadOnlyExSpan<byte> slice = new ReadOnlyExSpan<byte>(a, 0, 2);
            bool b = span.StartsWith<byte>(slice);
            Assert.True(b);
        }

        [Fact]
        public static void StartsWithMatchDifferentExSpans_Byte() {
            byte[] a = { 4, 5, 6 };
            byte[] b = { 4, 5, 6 };
            ExSpan<byte> span = new ExSpan<byte>(a, 0, 3);
            ReadOnlyExSpan<byte> slice = new ReadOnlyExSpan<byte>(b, 0, 3);
            bool c = span.StartsWith<byte>(slice);
            Assert.True(c);
        }

        [Fact]
        public static void StartsWithNoMatch_Byte() {
            for (int length = 1; length < 32; length++) {
                for (int mismatchIndex = 0; mismatchIndex < length; mismatchIndex++) {
                    byte[] first = new byte[length];
                    byte[] second = new byte[length];
                    for (int i = 0; i < length; i++) {
                        first[i] = second[i] = (byte)(i + 1);
                    }

                    second[mismatchIndex] = (byte)(second[mismatchIndex] + 1);

                    ExSpan<byte> firstExSpan = new ExSpan<byte>(first);
                    ReadOnlyExSpan<byte> secondExSpan = new ReadOnlyExSpan<byte>(second);
                    bool b = firstExSpan.StartsWith<byte>(secondExSpan);
                    Assert.False(b);
                }
            }
        }

        [Fact]
        public static void MakeSureNoStartsWithChecksGoOutOfRange_Byte() {
            for (int length = 0; length < 100; length++) {
                byte[] first = new byte[length + 2];
                first[0] = 99;
                first[length + 1] = 99;
                byte[] second = new byte[length + 2];
                second[0] = 100;
                second[length + 1] = 100;
                ExSpan<byte> span1 = new ExSpan<byte>(first, 1, length);
                ReadOnlyExSpan<byte> span2 = new ReadOnlyExSpan<byte>(second, 1, length);
                bool b = span1.StartsWith<byte>(span2);
                Assert.True(b);
            }
        }
    }
}
