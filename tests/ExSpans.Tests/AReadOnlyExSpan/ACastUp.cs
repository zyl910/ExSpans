using Xunit;

namespace Zyl.ExSpans.Tests.AReadOnlyExSpan {
    public static partial class ACastUp {
        [Fact]
        public static void CastUp() {
            ReadOnlyExSpan<string> strings = new string[] { "Hello", "World" };
            ReadOnlyExSpan<object> span = ReadOnlyExSpan<object>.CastUp(strings);
            span.ValidateReferenceType("Hello", "World");
        }
    }
}
