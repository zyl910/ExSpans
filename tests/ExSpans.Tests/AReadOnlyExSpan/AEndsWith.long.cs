using Xunit;

namespace Zyl.ExSpans.Tests.AReadOnlyExSpan {
    public static partial class AEndsWith {
        [Fact]
        public static void ZeroLengthEndsWith_Long() {
            var a = new long[3];

            var span = new ReadOnlyExSpan<long>(a);
            var slice = new ReadOnlyExSpan<long>(a, 2, 0);
            bool b = span.EndsWith<long>(slice);
            Assert.True(b);
        }

        [Fact]
        public static void SameSpanEndsWith_Long() {
            long[] a = { 488238291, 52498989823, 619890289890 };
            var span = new ReadOnlyExSpan<long>(a);
            bool b = span.EndsWith(span);
            Assert.True(b);
        }

        [Fact]
        public static void LengthMismatchEndsWith_Long() {
            long[] a = { 488238291, 52498989823, 619890289890 };
            var span = new ReadOnlyExSpan<long>(a, 0, 2);
            var slice = new ReadOnlyExSpan<long>(a, 0, 3);
            bool b = span.EndsWith(slice);
            Assert.False(b);
        }

        [Fact]
        public static void EndsWithMatch_Long() {
            long[] a = { 488238291, 52498989823, 619890289890 };
            var span = new ReadOnlyExSpan<long>(a, 0, 3);
            var slice = new ReadOnlyExSpan<long>(a, 1, 2);
            bool b = span.EndsWith(slice);
            Assert.True(b);
        }

        [Fact]
        public static void EndsWithMatchDifferentSpans_Long() {
            long[] a = { 488238291, 52498989823, 619890289890 };
            long[] b = { 488238291, 52498989823, 619890289890 };
            var span = new ReadOnlyExSpan<long>(a, 0, 3);
            var slice = new ReadOnlyExSpan<long>(b, 0, 3);
            bool c = span.EndsWith(slice);
            Assert.True(c);
        }

        [Fact]
        public static void EndsWithNoMatch_Long() {
            for (int length = 1; length < 32; length++) {
                for (int mismatchIndex = 0; mismatchIndex < length; mismatchIndex++) {
                    var first = new long[length];
                    var second = new long[length];
                    for (int i = 0; i < length; i++) {
                        first[i] = second[i] = (long)(i + 1);
                    }

                    second[mismatchIndex] = (long)(second[mismatchIndex] + 1);

                    var firstSpan = new ReadOnlyExSpan<long>(first);
                    var secondSpan = new ReadOnlyExSpan<long>(second);
                    bool b = firstSpan.EndsWith(secondSpan);
                    Assert.False(b);
                }
            }
        }

        [Fact]
        public static void MakeSureNoEndsWithChecksGoOutOfRange_Long() {
            for (int length = 0; length < 100; length++) {
                var first = new long[length + 2];
                first[0] = 99;
                first[length + 1] = 99;
                var second = new long[length + 2];
                second[0] = 100;
                second[length + 1] = 100;
                var span1 = new ReadOnlyExSpan<long>(first, 1, length);
                var span2 = new ReadOnlyExSpan<long>(second, 1, length);
                bool b = span1.EndsWith(span2);
                Assert.True(b);
            }
        }
    }
}
