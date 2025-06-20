using Xunit;

namespace Zyl.ExSpans.Tests.AExSpan {
    public static partial class ASequenceCompareTo {
        [Fact]
        public static void ZeroLengthSequenceCompareTo_Char() {
            var a = new char[3];

            var first = new ExSpan<char>(a, 1, 0);
            var second = new ReadOnlyExSpan<char>(a, 2, 0);
            int result = first.SequenceCompareTo<char>(second);
            Assert.Equal(0, result);
        }

        [Fact]
        public static void SameExSpanSequenceCompareTo_Char() {
            char[] a = { '4', '5', '6' };
            var span = new ExSpan<char>(a);
            int result = span.SequenceCompareTo<char>(span);
            Assert.Equal(0, result);
        }

        [Fact]
        public static void SequenceCompareToArrayImplicit_Char() {
            char[] a = { '4', '5', '6' };
            var first = new ExSpan<char>(a, 0, 3);
            int result = first.SequenceCompareTo<char>(a);
            Assert.Equal(0, result);
        }

        [Fact]
        public static void SequenceCompareToArraySegmentImplicit_Char() {
            char[] src = { '1', '2', '3' };
            char[] dst = { '5', '1', '2', '3', '9' };
            var segment = new ArraySegment<char>(dst, 1, 3);

            var first = new ExSpan<char>(src, 0, 3);
            int result = first.SequenceCompareTo<char>(segment);
            Assert.Equal(0, result);
        }

        [Fact]
        public static void LengthMismatchSequenceCompareTo_Char() {
            char[] a = { '4', '5', '6' };
            var first = new ExSpan<char>(a, 0, 2);
            var second = new ExSpan<char>(a, 0, 3);
            int result = first.SequenceCompareTo<char>(second);
            Assert.True(result < 0);

            result = second.SequenceCompareTo<char>(first);
            Assert.True(result > 0);

            // one sequence is empty
            first = new ExSpan<char>(a, 1, 0);

            result = first.SequenceCompareTo<char>(second);
            Assert.True(result < 0);

            result = second.SequenceCompareTo<char>(first);
            Assert.True(result > 0);
        }

        [Fact]
        public static void SequenceCompareToWithSingleMismatch_Char() {
            for (int length = 1; length < 32; length++) {
                for (int mismatchIndex = 0; mismatchIndex < length; mismatchIndex++) {
                    var first = new char[length];
                    var second = new char[length];
                    for (int i = 0; i < length; i++) {
                        first[i] = second[i] = (char)(i + 1);
                    }

                    second[mismatchIndex] = (char)(second[mismatchIndex] + 1);

                    var firstExSpan = new ExSpan<char>(first);
                    var secondExSpan = new ReadOnlyExSpan<char>(second);
                    int result = firstExSpan.SequenceCompareTo<char>(secondExSpan);
                    Assert.True(result < 0);

                    result = secondExSpan.SequenceCompareTo<char>(firstExSpan);
                    Assert.True(result > 0);
                }
            }
        }

        [Fact]
        public static void SequenceCompareToNoMatch_Char() {
            for (int length = 1; length < 32; length++) {
                var first = new char[length];
                var second = new char[length];

                for (int i = 0; i < length; i++) {
                    first[i] = (char)(i + 1);
                    second[i] = (char)(char.MaxValue - i);
                }

                var firstExSpan = new ExSpan<char>(first);
                var secondExSpan = new ReadOnlyExSpan<char>(second);
                int result = firstExSpan.SequenceCompareTo<char>(secondExSpan);
                Assert.True(result < 0);

                result = secondExSpan.SequenceCompareTo<char>(firstExSpan);
                Assert.True(result > 0);
            }
        }

        [Fact]
        public static void MakeSureNoSequenceCompareToChecksGoOutOfRange_Char() {
            for (int length = 0; length < 100; length++) {
                var first = new char[length + 2];
                first[0] = '8';
                first[length + 1] = '8';

                var second = new char[length + 2];
                second[0] = '9';
                second[length + 1] = '9';

                var span1 = new ExSpan<char>(first, 1, length);
                var span2 = new ReadOnlyExSpan<char>(second, 1, length);
                int result = span1.SequenceCompareTo<char>(span2);
                Assert.Equal(0, result);
            }
        }
    }
}
