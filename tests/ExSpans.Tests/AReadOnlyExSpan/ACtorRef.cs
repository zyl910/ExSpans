using Xunit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Zyl.ExSpans.Tests.AReadOnlyExSpan {
    public static partial class ACtorRef {
        [Fact]
        public static void CtorRef() {
            int value = 1;
            var s = new ReadOnlyExSpan<int>(in value);

            Assert.Equal(1, s.Length);
            Assert.Equal(1, s[0]);

            value = 2;
            Assert.Equal(2, s[0]);
        }
    }
}
