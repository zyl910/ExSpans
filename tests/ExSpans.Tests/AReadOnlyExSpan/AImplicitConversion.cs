using Xunit;

namespace Zyl.ExSpans.Tests.AReadOnlyExSpan {
    public static partial class AImplicitConversion {
        [Fact]
        public static void NullImplicitCast() {
            int[]? dst = null;
            ReadOnlyExSpan<int> srcSpan = dst;
            Assert.True(ReadOnlyExSpan<int>.Empty == srcSpan);
        }

        [Fact]
        public static void ArraySegmentDefaultImplicitCast() {
            ArraySegment<int> dst = default;
            ReadOnlyExSpan<int> srcSpan = dst;
            Assert.True(ReadOnlyExSpan<int>.Empty == srcSpan);
        }
    }
}
