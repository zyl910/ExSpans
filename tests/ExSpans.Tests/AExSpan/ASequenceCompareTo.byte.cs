using Xunit;

namespace Zyl.ExSpans.Tests.AExSpan {
    public static partial class ASequenceCompareTo {
        [Fact]
        public static void ZeroLengthSequenceCompareTo_Byte() {
            byte[] a = new byte[3];

            ExSpan<byte> first = new ExSpan<byte>(a, 1, 0);
            ExSpan<byte> second = new ExSpan<byte>(a, 2, 0);
            int result = first.SequenceCompareTo<byte>(second);
            Assert.Equal(0, result);
        }

        [Fact]
        public static void SameExSpanSequenceCompareTo_Byte() {
            byte[] a = { 4, 5, 6 };
            ExSpan<byte> span = new ExSpan<byte>(a);
            int result = span.SequenceCompareTo<byte>(span);
            Assert.Equal(0, result);
        }

        [Fact]
        public static void SequenceCompareToArrayImplicit_Byte() {
            byte[] a = { 4, 5, 6 };
            ExSpan<byte> first = new ExSpan<byte>(a, 0, 3);
            int result = first.SequenceCompareTo<byte>(a);
            Assert.Equal(0, result);
        }

        [Fact]
        public static void SequenceCompareToArraySegmentImplicit_Byte() {
            byte[] src = { 1, 2, 3 };
            byte[] dst = { 5, 1, 2, 3, 10 };
            var segment = new ArraySegment<byte>(dst, 1, 3);

            ExSpan<byte> first = new ExSpan<byte>(src, 0, 3);
            int result = first.SequenceCompareTo<byte>(segment);
            Assert.Equal(0, result);
        }

        [Fact]
        public static void LengthMismatchSequenceCompareTo_Byte() {
            byte[] a = { 4, 5, 6 };
            ExSpan<byte> first = new ExSpan<byte>(a, 0, 2);
            ExSpan<byte> second = new ExSpan<byte>(a, 0, 3);
            int result = first.SequenceCompareTo<byte>(second);
            Assert.True(result < 0);

            result = second.SequenceCompareTo<byte>(first);
            Assert.True(result > 0);

            // one sequence is empty
            first = new ExSpan<byte>(a, 1, 0);

            result = first.SequenceCompareTo<byte>(second);
            Assert.True(result < 0);

            result = second.SequenceCompareTo<byte>(first);
            Assert.True(result > 0);
        }

        [Fact]
        public static void SequenceCompareToEqual_Byte() {
            for (int length = 1; length < 128; length++) {
                var first = new byte[length];
                var second = new byte[length];
                for (int i = 0; i < length; i++) {
                    first[i] = second[i] = (byte)(i + 1);
                }

                var firstExSpan = new ExSpan<byte>(first);
                var secondExSpan = new ExSpan<byte>(second);

                Assert.Equal(0, firstExSpan.SequenceCompareTo<byte>(secondExSpan));
                Assert.Equal(0, secondExSpan.SequenceCompareTo<byte>(firstExSpan));
            }
        }

        [Fact]
        public static void SequenceCompareToWithSingleMismatch_Byte() {
            for (int length = 1; length < 128; length++) {
                for (int mismatchIndex = 0; mismatchIndex < length; mismatchIndex++) {
                    byte[] first = new byte[length];
                    byte[] second = new byte[length];
                    for (int i = 0; i < length; i++) {
                        first[i] = second[i] = (byte)(i + 1);
                    }

                    second[mismatchIndex] = (byte)(second[mismatchIndex] + 1);

                    ExSpan<byte> firstExSpan = new ExSpan<byte>(first);
                    ReadOnlyExSpan<byte> secondExSpan = new ReadOnlyExSpan<byte>(second);
                    int result = firstExSpan.SequenceCompareTo<byte>(secondExSpan);
                    Assert.True(result < 0);

                    result = secondExSpan.SequenceCompareTo<byte>(firstExSpan);
                    Assert.True(result > 0);
                }
            }
        }

        [Fact]
        public static void SequenceCompareToNoMatch_Byte() {
            for (int length = 1; length < 128; length++) {
                byte[] first = new byte[length];
                byte[] second = new byte[length];

                for (int i = 0; i < length; i++) {
                    first[i] = (byte)(i + 1);
                    second[i] = (byte)(byte.MaxValue - i);
                }

                ExSpan<byte> firstExSpan = new ExSpan<byte>(first);
                ReadOnlyExSpan<byte> secondExSpan = new ReadOnlyExSpan<byte>(second);
                int result = firstExSpan.SequenceCompareTo<byte>(secondExSpan);
                Assert.True(result < 0);

                result = secondExSpan.SequenceCompareTo<byte>(firstExSpan);
                Assert.True(result > 0);
            }
        }

        [Fact]
        public static void MakeSureNoSequenceCompareToChecksGoOutOfRange_Byte() {
            for (int length = 0; length < 100; length++) {
                byte[] first = new byte[length + 2];
                first[0] = 99;
                first[length + 1] = 99;
                byte[] second = new byte[length + 2];
                second[0] = 100;
                second[length + 1] = 100;
                ExSpan<byte> span1 = new ExSpan<byte>(first, 1, length);
                ReadOnlyExSpan<byte> span2 = new ReadOnlyExSpan<byte>(second, 1, length);
                int result = span1.SequenceCompareTo<byte>(span2);
                Assert.Equal(0, result);
            }
        }
    }
}
