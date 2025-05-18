using Xunit;

namespace Zyl.ExSpans.Tests.AReadOnlyExSpan {
    public static partial class AGetHashCode {
        [Fact]
        public static void CannotCallGetHashCodeOnSpan() {
            ReadOnlyExSpan<int> span = new ReadOnlyExSpan<int>(new int[0]);

            try {
#pragma warning disable 0618
                int result = span.GetHashCode();
#pragma warning restore 0618
                Assert.Fail();
            } catch (Exception ex) {
                Assert.True(ex is NotSupportedException);
            }
        }
    }
}
