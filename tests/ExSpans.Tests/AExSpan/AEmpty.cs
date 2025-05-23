using Xunit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Zyl.ExSpans.Tests.AExSpan {
    public static partial class AEmpty {
        [Fact]
        public static void Empty() {
            ExSpan<int> empty = ExSpan<int>.Empty;
            Assert.True(empty.IsEmpty);
            Assert.Equal(0, empty.Length);
            unsafe {
                ref int expected = ref Unsafe.AsRef<int>(null);
                ref int actual = ref ExMemoryMarshal.GetReference(empty);
                Assert.True(Unsafe.AreSame(ref expected, ref actual));
            }
        }
    }
}
