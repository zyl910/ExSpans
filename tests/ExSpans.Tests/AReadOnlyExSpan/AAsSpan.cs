using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using Xunit;

namespace Zyl.ExSpans.Tests.AReadOnlyExSpan {
    public static partial class AAsSpan {
        [Fact]
        public static void StringAsSpanNullary() {
            string s = "Hello";
            ReadOnlyExSpan<char> span = s.AsExSpan();
            char[] expected = s.ToCharArray();
            span.Validate(expected);
        }

        [Fact]
        public static void StringAsSpanEmptyString() {
            string s = "";
            ReadOnlyExSpan<char> span = s.AsExSpan();
            span.ValidateNonNullEmpty();
        }

        [Fact]
        public static void StringAsSpanNullChecked() {
            string? s = null;
            ReadOnlyExSpan<char> span = s.AsExSpan();
            span.Validate();
            Assert.True(span == default);

            span = s.AsExSpan(0);
            span.Validate();
            Assert.True(span == default);

            span = s.AsExSpan(0, 0);
            span.Validate();
            Assert.True(span == default);
        }

        [Fact]
        public static void StringAsSpanNullNonZeroStartAndLength() {
            string? str = null;

            Assert.Throws<ArgumentOutOfRangeException>(() => str.AsExSpan(1).DontBox());
            Assert.Throws<ArgumentOutOfRangeException>(() => str.AsExSpan(-1).DontBox());

            Assert.Throws<ArgumentOutOfRangeException>(() => str.AsExSpan(0, 1).DontBox());
            Assert.Throws<ArgumentOutOfRangeException>(() => str.AsExSpan(1, 0).DontBox());
            Assert.Throws<ArgumentOutOfRangeException>(() => str.AsExSpan(1, 1).DontBox());
            Assert.Throws<ArgumentOutOfRangeException>(() => str.AsExSpan(-1, -1).DontBox());

#if NET8_0_OR_GREATER && TODO // [TODO why] NRange need System.Numerics.Tensors.dll
            Assert.Throws<ArgumentOutOfRangeException>(() => str.AsExSpan(new Index(1)).DontBox());
            Assert.Throws<ArgumentOutOfRangeException>(() => str.AsExSpan(new Index(0, fromEnd: true)).DontBox());

            Assert.Throws<ArgumentNullException>(() => str.AsExSpan(0..1).DontBox());
            Assert.Throws<ArgumentNullException>(() => str.AsExSpan(new Range(new Index(0), new Index(0, fromEnd: true))).DontBox());
            Assert.Throws<ArgumentNullException>(() => str.AsExSpan(new Range(new Index(0, fromEnd: true), new Index(0))).DontBox());
            Assert.Throws<ArgumentNullException>(() => str.AsExSpan(new Range(new Index(0, fromEnd: true), new Index(0, fromEnd: true))).DontBox());
#endif // NET8_0_OR_GREATER
        }

#if NOT_RELATED
        [Theory]
        [MemberData(nameof(TestHelpers.StringSliceTestData), MemberType = typeof(TestHelpers))]
        public static void AsSpan_StartAndLength(string text, int start, int length) {
            if (start == -1) {
                Validate(text, 0, text.Length, text.AsExSpan());
                Validate(text, 0, text.Length, text.AsExSpan(0));
#if NET8_0_OR_GREATER && TODO // [TODO why] NRange need System.Numerics.Tensors.dll
                Validate(text, 0, text.Length, text.AsExSpan(0..^0));
#endif // NET8_0_OR_GREATER
            } else if (length == -1) {
                Validate(text, start, text.Length - start, text.AsExSpan(start));
#if NET8_0_OR_GREATER && TODO // [TODO why] NRange need System.Numerics.Tensors.dll
                Validate(text, start, text.Length - start, text.AsExSpan(start..));
#endif // NET8_0_OR_GREATER
            } else {
                Validate(text, start, length, text.AsExSpan(start, length));
#if NET8_0_OR_GREATER && TODO // [TODO why] NRange need System.Numerics.Tensors.dll
                Validate(text, start, length, text.AsExSpan(start..(start + length)));
#endif // NET8_0_OR_GREATER
            }

            static unsafe void Validate(string text, int start, int length, ReadOnlyExSpan<char> span) {
                Assert.Equal(length, span.Length);
                fixed (char* pText = text) {
                    // Unsafe.AsPointer is safe here since it's pinned (since text and span should be the same string)
                    char* expected = pText + start;
                    void* actual = Unsafe.AsPointer(ref ExMemoryMarshal.GetReference(span));
                    Assert.Equal((IntPtr)expected, (IntPtr)actual);
                }
            }
        }
#endif // NOT_RELATED

#if NET8_0_OR_GREATER && TODO // [TODO why] NRange need System.Numerics.Tensors.dll
        [Theory]
        [MemberData(nameof(TestHelpers.StringSlice2ArgTestOutOfRangeData), MemberType = typeof(TestHelpers))]
        public static unsafe void AsSpan_2Arg_OutOfRange(string text, int start) {
            AssertExtensions.Throws<ArgumentOutOfRangeException>("start", () => text.AsExSpan(start).DontBox());
            if (start >= 0) {
                AssertExtensions.Throws<ArgumentOutOfRangeException>("startIndex", () => text.AsExSpan(new Index(start)).DontBox());
            }
        }

        [Theory]
        [MemberData(nameof(TestHelpers.StringSlice3ArgTestOutOfRangeData), MemberType = typeof(TestHelpers))]
        public static unsafe void AsSpan_3Arg_OutOfRange(string text, int start, int length) {
            AssertExtensions.Throws<ArgumentOutOfRangeException>("start", () => text.AsExSpan(start, length).DontBox());
            if (start >= 0 && length >= 0 && start + length >= 0) {
                AssertExtensions.Throws<ArgumentOutOfRangeException>("length", () => text.AsExSpan(start..(start + length)).DontBox());
            }
        }
#endif // NET8_0_OR_GREATER
    }
}
