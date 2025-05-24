using Xunit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Zyl.ExSpans.Tests.AExSpan {
    public static partial class ACtorRef {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        [Fact]
        public static void CtorRef() {
            int value = 1;
            var s = new ExSpan<int>(ref value);

            Assert.Equal(1, s.Length);
            Assert.Equal(1, s[0]);

            s[0] = 2;
            Assert.Equal(2, value);

            value = 3;
            Assert.Equal(3, s[0]);
        }
#endif // NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
    }
}
