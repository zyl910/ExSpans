using Xunit;

namespace Zyl.ExSpans.Tests.AExSpan {
    //
    // Tests for ExSpan<T>.ctor(T[], int, int). If the test is not specific to this overload, consider putting it in CtorArray.cs instread.
    //
    public static partial class ACtorArrayIntInt {
        [Fact]
        public static void CtorArrayIntInt1() {
            int[] a = { 90, 91, 92, 93, 94, 95, 96, 97, 98 };
            ExSpan<int> span = new ExSpan<int>(a, 3, 2);
            span.Validate(93, 94);
        }

        [Fact]
        public static void CtorArrayIntInt2() {
            long[] a = { 90, 91, 92, 93, 94, 95, 96, 97, 98 };
            ExSpan<long> span = new ExSpan<long>(a, 4, 3);
            span.Validate(94, 95, 96);
        }

        [Fact]
        public static void CtorArrayIntIntRangeExtendsToEndOfArray() {
            long[] a = { 90, 91, 92, 93, 94, 95, 96, 97, 98 };
            ExSpan<long> span = new ExSpan<long>(a, 4, 5);
            span.Validate(94, 95, 96, 97, 98);
        }

        [Fact]
        public static void CtorArrayIntIntNegativeStart() {
            int[] a = new int[3];
            Assert.Throws<ArgumentOutOfRangeException>(() => new ExSpan<int>(a, -1, 0).DontBox());
        }

        [Fact]
        public static void CtorArrayIntIntStartTooLarge() {
            int[] a = new int[3];
            Assert.Throws<ArgumentOutOfRangeException>(() => new ExSpan<int>(a, 4, 0).DontBox());
        }

        [Fact]
        public static void CtorArrayIntIntNegativeLength() {
            int[] a = new int[3];
            Assert.Throws<ArgumentOutOfRangeException>(() => new ExSpan<int>(a, 0, -1).DontBox());
        }

        [Fact]
        public static void CtorArrayIntIntStartAndLengthTooLarge() {
            int[] a = new int[3];
            Assert.Throws<ArgumentOutOfRangeException>(() => new ExSpan<int>(a, 3, 1).DontBox());
            Assert.Throws<ArgumentOutOfRangeException>(() => new ExSpan<int>(a, 2, 2).DontBox());
            Assert.Throws<ArgumentOutOfRangeException>(() => new ExSpan<int>(a, 1, 3).DontBox());
            Assert.Throws<ArgumentOutOfRangeException>(() => new ExSpan<int>(a, 0, 4).DontBox());
            Assert.Throws<ArgumentOutOfRangeException>(() => new ExSpan<int>(a, int.MaxValue, int.MaxValue).DontBox());
        }

        [Fact]
        public static void CtorArrayIntIntStartEqualsLength() {
            // Valid for start to equal the array length. This returns an empty span that starts "just past the array."
            int[] a = { 91, 92, 93 };
            ExSpan<int> span = new ExSpan<int>(a, 3, 0);
            span.ValidateNonNullEmpty();
        }
    }
}
