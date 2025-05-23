using Xunit;

namespace Zyl.ExSpans.Tests.AExSpan {
    public static partial class ASequenceCompareTo {
        [Fact]
        public static void ZeroLengthSequenceCompareTo_Long() {
            var a = new long[3];

            var first = new ExSpan<long>(a, 1, 0);
            var second = new ReadOnlyExSpan<long>(a, 2, 0);
            int result = first.SequenceCompareTo<long>(second);
            Assert.Equal(0, result);
        }

        [Fact]
        public static void SameExSpanSequenceCompareTo_Long() {
            long[] a = { 488238291, 52498989823, 619890289890 };
            var span = new ExSpan<long>(a);
            int result = span.SequenceCompareTo<long>(span);
            Assert.Equal(0, result);
        }

        [Fact]
        public static void SequenceCompareToArrayImplicit_Long() {
            long[] a = { 488238291, 52498989823, 619890289890 };
            var first = new ExSpan<long>(a, 0, 3);
            int result = first.SequenceCompareTo<long>(a);
            Assert.Equal(0, result);
        }

        [Fact]
        public static void SequenceCompareToArraySegmentImplicit_Long() {
            long[] src = { 1989089123, 234523454235, 3123213231 };
            long[] dst = { 5, 1989089123, 234523454235, 3123213231, 10 };
            var segment = new ArraySegment<long>(dst, 1, 3);

            var first = new ExSpan<long>(src, 0, 3);
            int result = first.SequenceCompareTo<long>(segment);
            Assert.Equal(0, result);
        }

        [Fact]
        public static void LengthMismatchSequenceCompareTo_Long() {
            long[] a = { 488238291, 52498989823, 619890289890 };
            var first = new ExSpan<long>(a, 0, 2);
            var second = new ExSpan<long>(a, 0, 3);
            int result = first.SequenceCompareTo<long>(second);
            Assert.True(result < 0);

            result = second.SequenceCompareTo<long>(first);
            Assert.True(result > 0);

            // one sequence is empty
            first = new ExSpan<long>(a, 1, 0);

            result = first.SequenceCompareTo<long>(second);
            Assert.True(result < 0);

            result = second.SequenceCompareTo<long>(first);
            Assert.True(result > 0);
        }

        [Fact]
        public static void SequenceCompareToWithSingleMismatch_Long() {
            for (int length = 1; length < 32; length++) {
                for (int mismatchIndex = 0; mismatchIndex < length; mismatchIndex++) {
                    var first = new long[length];
                    var second = new long[length];
                    for (int i = 0; i < length; i++) {
                        first[i] = second[i] = (long)(i + 1);
                    }

                    second[mismatchIndex] = (long)(second[mismatchIndex] + 1);

                    var firstExSpan = new ExSpan<long>(first);
                    var secondExSpan = new ReadOnlyExSpan<long>(second);
                    int result = firstExSpan.SequenceCompareTo<long>(secondExSpan);
                    Assert.True(result < 0);

                    result = secondExSpan.SequenceCompareTo<long>(firstExSpan);
                    Assert.True(result > 0);
                }
            }
        }

        [Fact]
        public static void SequenceCompareToNoMatch_Long() {
            for (int length = 1; length < 32; length++) {
                var first = new long[length];
                var second = new long[length];

                for (int i = 0; i < length; i++) {
                    first[i] = (long)(i + 1);
                    second[i] = (long)(long.MaxValue - i);
                }

                var firstExSpan = new ExSpan<long>(first);
                var secondExSpan = new ReadOnlyExSpan<long>(second);
                int result = firstExSpan.SequenceCompareTo<long>(secondExSpan);
                Assert.True(result < 0);

                result = secondExSpan.SequenceCompareTo<long>(firstExSpan);
                Assert.True(result > 0);
            }
        }

        [Fact]
        public static void MakeSureNoSequenceCompareToChecksGoOutOfRange_Long() {
            for (int length = 0; length < 100; length++) {
                var first = new long[length + 2];
                first[0] = 99;
                first[length + 1] = 99;

                var second = new long[length + 2];
                second[0] = 100;
                second[length + 1] = 100;

                var span1 = new ExSpan<long>(first, 1, length);
                var span2 = new ReadOnlyExSpan<long>(second, 1, length);
                int result = span1.SequenceCompareTo<long>(span2);
                Assert.Equal(0, result);
            }
        }
    }
}
