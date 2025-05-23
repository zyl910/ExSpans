using Xunit;

namespace Zyl.ExSpans.Tests.AExSpan {
    public static partial class AImplicitConversion {
        [Fact]
        public static void NullImplicitCast() {
            int[] dst = null;
            ExSpan<int> srcExSpan = dst;
            Assert.True(ExSpan<int>.Empty == srcExSpan);
        }

        [Fact]
        public static void ArraySegmentDefaultImplicitCast() {
            ArraySegment<int> dst = default;
            ExSpan<int> srcExSpan = dst;
            Assert.True(ExSpan<int>.Empty == srcExSpan);
        }
    }
}
