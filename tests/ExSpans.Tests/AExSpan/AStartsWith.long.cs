using Xunit;

namespace Zyl.ExSpans.Tests.AExSpan {
    public static partial class AStartsWith {
        [Fact]
        public static void ZeroLengthStartsWith_Long() {
            var a = new long[3];

            var span = new ExSpan<long>(a);
            var slice = new ReadOnlyExSpan<long>(a, 2, 0);
            bool b = span.StartsWith<long>(slice);
            Assert.True(b);
        }

        [Fact]
        public static void SameExSpanStartsWith_Long() {
            long[] a = { 488238291, 52498989823, 619890289890 };
            var span = new ExSpan<long>(a);
            bool b = span.StartsWith<long>(span);
            Assert.True(b);
        }

        [Fact]
        public static void LengthMismatchStartsWith_Long() {
            long[] a = { 488238291, 52498989823, 619890289890 };
            var span = new ExSpan<long>(a, 0, 2);
            var slice = new ReadOnlyExSpan<long>(a, 0, 3);
            bool b = span.StartsWith<long>(slice);
            Assert.False(b);
        }

        [Fact]
        public static void StartsWithMatch_Long() {
            long[] a = { 488238291, 52498989823, 619890289890 };
            var span = new ExSpan<long>(a, 0, 3);
            var slice = new ReadOnlyExSpan<long>(a, 0, 2);
            bool b = span.StartsWith<long>(slice);
            Assert.True(b);
        }

        [Fact]
        public static void StartsWithMatchDifferentExSpans_Long() {
            long[] a = { 488238291, 52498989823, 619890289890 };
            long[] b = { 488238291, 52498989823, 619890289890 };
            var span = new ExSpan<long>(a, 0, 3);
            var slice = new ReadOnlyExSpan<long>(b, 0, 3);
            bool c = span.StartsWith<long>(slice);
            Assert.True(c);
        }

        [Fact]
        public static void StartsWithNoMatch_Long() {
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
                    bool b = firstExSpan.StartsWith<long>(secondExSpan);
                    Assert.False(b);
                }
            }
        }

        [Fact]
        public static void MakeSureNoStartsWithChecksGoOutOfRange_Long() {
            for (int length = 0; length < 100; length++) {
                var first = new long[length + 2];
                first[0] = 99;
                first[length + 1] = 99;
                var second = new long[length + 2];
                second[0] = 100;
                second[length + 1] = 100;
                var span1 = new ExSpan<long>(first, 1, length);
                var span2 = new ReadOnlyExSpan<long>(second, 1, length);
                bool b = span1.StartsWith<long>(span2);
                Assert.True(b);
            }
        }
    }
}
