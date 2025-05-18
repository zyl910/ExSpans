using Xunit;

namespace Zyl.ExSpans.Tests.AReadOnlyExSpan {
    public static partial class ACastUp {
#if NET7_0_OR_GREATER
        [Fact]
        public static void CastUp() {
            ReadOnlyExSpan<string> strings = new string[] { "Hello", "World" };
            ReadOnlyExSpan<object> span = ReadOnlyExSpan<object>.CastUp(strings);
            span.ValidateReferenceType("Hello", "World");
        }
#endif // NET7_0_OR_GREATER
    }
}
