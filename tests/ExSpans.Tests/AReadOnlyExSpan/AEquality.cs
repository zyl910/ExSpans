using Xunit;

namespace Zyl.ExSpans.Tests.AReadOnlyExSpan {
    public static partial class AEquality {
        [Fact]
        public static void EqualityTrue() {
            int[] a = { 91, 92, 93, 94, 95 };
            ReadOnlyExSpan<int> left = new ReadOnlyExSpan<int>(a, 2, 3);
            ReadOnlyExSpan<int> right = new ReadOnlyExSpan<int>(a, 2, 3);

            Assert.True(left == right);
            Assert.True(!(left != right));
        }

        [Fact]
        public static void EqualityReflexivity() {
            int[] a = { 91, 92, 93, 94, 95 };
            ReadOnlyExSpan<int> left = new ReadOnlyExSpan<int>(a, 2, 3);

#pragma warning disable 1718 // Comparison made to same variable; did you mean to compare something else?
            Assert.True(left == left);
            Assert.True(!(left != left));
#pragma warning restore 1718
        }

        [Fact]
        public static void EqualityIncludesLength() {
            int[] a = { 91, 92, 93, 94, 95 };
            ReadOnlyExSpan<int> left = new ReadOnlyExSpan<int>(a, 2, 1);
            ReadOnlyExSpan<int> right = new ReadOnlyExSpan<int>(a, 2, 3);

            Assert.False(left == right);
            Assert.False(!(left != right));
        }

        [Fact]
        public static void EqualityIncludesBase() {
            int[] a = { 91, 92, 93, 94, 95 };
            ReadOnlyExSpan<int> left = new ReadOnlyExSpan<int>(a, 1, 3);
            ReadOnlyExSpan<int> right = new ReadOnlyExSpan<int>(a, 2, 3);

            Assert.False(left == right);
            Assert.False(!(left != right));
        }

        [Fact]
        public static void EqualityBasedOnLocationNotConstructor() {
            unsafe {
                int[] a = { 91, 92, 93, 94, 95 };
                fixed (int* pa = a) {
                    ReadOnlyExSpan<int> left = new ReadOnlyExSpan<int>(a, 2, 3);
                    ReadOnlyExSpan<int> right = new ReadOnlyExSpan<int>(pa + 2, 3);

                    Assert.True(left == right);
                    Assert.True(!(left != right));
                }
            }
        }

        [Fact]
        public static void EqualityComparesRangeNotContent() {
            ReadOnlyExSpan<int> left = new ReadOnlyExSpan<int>(new int[] { 0, 1, 2 }, 1, 1);
            ReadOnlyExSpan<int> right = new ReadOnlyExSpan<int>(new int[] { 0, 1, 2 }, 1, 1);

            Assert.False(left == right);
            Assert.False(!(left != right));
        }

        [Fact]
        public static void EmptySpansNotUnified() {
            ReadOnlyExSpan<int> left = new ReadOnlyExSpan<int>(new int[0]);
            ReadOnlyExSpan<int> right = new ReadOnlyExSpan<int>(new int[0]);

            Assert.False(left == right);
            Assert.False(!(left != right));
        }

        [Fact]
        public static void CannotCallEqualsOnSpan() {
            ReadOnlyExSpan<int> left = new ReadOnlyExSpan<int>(new int[0]);
            try {
#pragma warning disable 0618
                bool result = left.Equals(new object());
#pragma warning restore 0618
                Assert.Fail();
            } catch (Exception ex) {
                Assert.True(ex is NotSupportedException);
            }
        }
    }
}
