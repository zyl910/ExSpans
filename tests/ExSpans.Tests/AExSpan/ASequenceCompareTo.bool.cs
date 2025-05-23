using Xunit;

namespace Zyl.ExSpans.Tests.AExSpan {
    public static partial class ASequenceCompareTo {
        [Fact]
        public static void ZeroLengthSequenceCompareTo_Bool() {
            var a = new bool[3];

            var first = new ExSpan<bool>(a, 1, 0);
            var second = new ReadOnlyExSpan<bool>(a, 2, 0);
            int result = first.SequenceCompareTo<bool>(second);
            Assert.Equal(0, result);
        }

        [Fact]
        public static void SameExSpanSequenceCompareTo_Bool() {
            bool[] a = { true, true, false };
            var span = new ExSpan<bool>(a);
            int result = span.SequenceCompareTo<bool>(span);
            Assert.Equal(0, result);
        }

        [Fact]
        public static void SequenceCompareToArrayImplicit_Bool() {
            bool[] a = { true, true, false };
            var first = new ExSpan<bool>(a, 0, 3);
            int result = first.SequenceCompareTo<bool>(a);
            Assert.Equal(0, result);
        }

        [Fact]
        public static void SequenceCompareToArraySegmentImplicit_Bool() {
            bool[] src = { true, true, true };
            bool[] dst = { false, true, true, true, false };
            var segment = new ArraySegment<bool>(dst, 1, 3);

            var first = new ExSpan<bool>(src, 0, 3);
            int result = first.SequenceCompareTo<bool>(segment);
            Assert.Equal(0, result);
        }

        [Fact]
        public static void LengthMismatchSequenceCompareTo_Bool() {
            bool[] a = { true, true, false };
            var first = new ExSpan<bool>(a, 0, 2);
            var second = new ExSpan<bool>(a, 0, 3);
            int result = first.SequenceCompareTo<bool>(second);
            Assert.True(result < 0);

            result = second.SequenceCompareTo<bool>(first);
            Assert.True(result > 0);

            // one sequence is empty
            first = new ExSpan<bool>(a, 1, 0);

            result = first.SequenceCompareTo<bool>(second);
            Assert.True(result < 0);

            result = second.SequenceCompareTo<bool>(first);
            Assert.True(result > 0);
        }

        [Fact]
        public static void SequenceCompareToWithSingleMismatch_Bool() {
            for (int length = 1; length < 32; length++) {
                for (int mismatchIndex = 0; mismatchIndex < length; mismatchIndex++) {
                    var first = new bool[length];
                    var second = new bool[length];
                    for (int i = 0; i < length; i++) {
                        first[i] = second[i] = true;
                    }

                    second[mismatchIndex] = !second[mismatchIndex];

                    var firstExSpan = new ExSpan<bool>(first);
                    var secondExSpan = new ReadOnlyExSpan<bool>(second);
                    int result = firstExSpan.SequenceCompareTo<bool>(secondExSpan);
                    Assert.True(result > 0);

                    result = secondExSpan.SequenceCompareTo<bool>(firstExSpan);
                    Assert.True(result < 0);
                }
            }
        }

        [Fact]
        public static void SequenceCompareToNoMatch_Bool() {
            for (int length = 1; length < 32; length++) {
                var first = new bool[length];
                var second = new bool[length];

                for (int i = 0; i < length; i++) {
                    first[i] = (i % 2 != 0);
                    second[i] = (i % 2 == 0);
                }

                var firstExSpan = new ExSpan<bool>(first);
                var secondExSpan = new ReadOnlyExSpan<bool>(second);
                int result = firstExSpan.SequenceCompareTo<bool>(secondExSpan);
                Assert.True(result < 0);

                result = secondExSpan.SequenceCompareTo<bool>(firstExSpan);
                Assert.True(result > 0);
            }
        }

        [Fact]
        public static void MakeSureNoSequenceCompareToChecksGoOutOfRange_Bool() {
            for (int length = 0; length < 100; length++) {
                var first = new bool[length + 2];
                first[0] = true;
                for (int k = 1; k <= length; k++)
                    first[k] = false;
                first[length + 1] = true;

                var second = new bool[length + 2];
                second[0] = false;
                for (int k = 1; k <= length; k++)
                    second[k] = false;
                second[length + 1] = false;

                var span1 = new ExSpan<bool>(first, 1, length);
                var span2 = new ReadOnlyExSpan<bool>(second, 1, length);
                int result = span1.SequenceCompareTo<bool>(span2);
                Assert.Equal(0, result);
            }
        }
    }
}
