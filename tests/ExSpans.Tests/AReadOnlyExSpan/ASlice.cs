#if NET7_0_OR_GREATER
#define STRUCT_REF_FIELD // C# 11 - ref fields and ref scoped variables. https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/ref-struct#ref-fields
#endif // NET7_0_OR_GREATER

using Xunit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Zyl.ExSpans.Tests.AReadOnlyExSpan {
    public static partial class ASlice {
        [Fact]
        public static void SliceInt() {
            int[] a = { 90, 91, 92, 93, 94, 95, 96, 97, 98, 99 };
            ReadOnlyExSpan<int> span = new ReadOnlyExSpan<int>(a).Slice(6);
            Assert.Equal(4, span.Length);
            Assert.True(Unsafe.AreSame(ref a[6], ref Unsafe.AsRef(in ExMemoryMarshal.GetReference(span))));
        }

        [Fact]
        public static void SliceIntPastEnd() {
            int[] a = { 90, 91, 92, 93, 94, 95, 96, 97, 98, 99 };
            ReadOnlyExSpan<int> span = new ReadOnlyExSpan<int>(a).Slice(a.Length);
            Assert.Equal(0, span.Length);
#if STRUCT_REF_FIELD
            Assert.True(Unsafe.AreSame(ref a[a.Length - 1], ref Unsafe.Subtract<int>(ref Unsafe.AsRef(in ExMemoryMarshal.GetReference(span)), 1)));
#endif
        }

        [Fact]
        public static void SliceIntInt() {
            int[] a = { 90, 91, 92, 93, 94, 95, 96, 97, 98, 99 };
            ReadOnlyExSpan<int> span = new ReadOnlyExSpan<int>(a).Slice(3, 5);
            Assert.Equal(5, span.Length);
            Assert.True(Unsafe.AreSame(ref a[3], ref Unsafe.AsRef(in ExMemoryMarshal.GetReference(span))));
        }

        [Fact]
        public static void SliceIntIntUpToEnd() {
            int[] a = { 90, 91, 92, 93, 94, 95, 96, 97, 98, 99 };
            ReadOnlyExSpan<int> span = new ReadOnlyExSpan<int>(a).Slice(4, 6);
            Assert.Equal(6, span.Length);
            Assert.True(Unsafe.AreSame(ref a[4], ref Unsafe.AsRef(in ExMemoryMarshal.GetReference(span))));
        }

        [Fact]
        public static void SliceIntIntPastEnd() {
            int[] a = { 90, 91, 92, 93, 94, 95, 96, 97, 98, 99 };
            ReadOnlyExSpan<int> span = new ReadOnlyExSpan<int>(a).Slice(a.Length, 0);
            Assert.Equal(0, span.Length);
#if STRUCT_REF_FIELD
            Assert.True(Unsafe.AreSame(ref a[a.Length - 1], ref Unsafe.Subtract<int>(ref Unsafe.AsRef(in ExMemoryMarshal.GetReference(span)), 1)));
#endif
        }

        [Fact]
        public static void SliceIntRangeChecksd() {
            int[] a = { 90, 91, 92, 93, 94, 95, 96, 97, 98, 99 };
            Assert.Throws<ArgumentOutOfRangeException>(() => new ReadOnlyExSpan<int>(a).Slice(-1).DontBox());
            Assert.Throws<ArgumentOutOfRangeException>(() => new ReadOnlyExSpan<int>(a).Slice(a.Length + 1).DontBox());
            Assert.Throws<ArgumentOutOfRangeException>(() => new ReadOnlyExSpan<int>(a).Slice(-1, 0).DontBox());
            Assert.Throws<ArgumentOutOfRangeException>(() => new ReadOnlyExSpan<int>(a).Slice(0, a.Length + 1).DontBox());
            Assert.Throws<ArgumentOutOfRangeException>(() => new ReadOnlyExSpan<int>(a).Slice(2, a.Length + 1 - 2).DontBox());
            Assert.Throws<ArgumentOutOfRangeException>(() => new ReadOnlyExSpan<int>(a).Slice(a.Length + 1, 0).DontBox());
            Assert.Throws<ArgumentOutOfRangeException>(() => new ReadOnlyExSpan<int>(a).Slice(a.Length, 1).DontBox());
        }
    }
}
