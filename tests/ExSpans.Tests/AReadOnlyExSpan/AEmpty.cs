using Xunit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Zyl.ExSpans.Tests.AReadOnlyExSpan {
    public static partial class AEmpty {
        [Fact]
        public static void Empty() {
            ReadOnlyExSpan<int> empty = ReadOnlyExSpan<int>.Empty;
            Assert.True(empty.IsEmpty);
            Assert.Equal(0, empty.Length);
            unsafe {
                ref int expected = ref Unsafe.AsRef<int>(null);
                ref int actual = ref Unsafe.AsRef(in MemoryMarshal.GetReference(empty));
                Assert.True(Unsafe.AreSame(ref expected, ref actual));
            }
        }
    }
}
