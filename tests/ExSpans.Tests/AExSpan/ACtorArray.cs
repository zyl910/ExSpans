using Xunit;

namespace Zyl.ExSpans.Tests.AExSpan {
    //
    // Tests for ExSpan<T>.ctor(T[])
    //
    // These tests will also exercise the matching codepaths in ExSpan<T>.ctor(T[], int) and .ctor(T[], int, int). This makes it easier to ensure
    // that these parallel tests stay consistent, and avoid excess repetition in the files devoted to those specific overloads.
    //
    public static partial class ACtorArray {
        [Fact]
        public static void CtorArray1() {
            int[] a = { 91, 92, -93, 94 };
            ExSpan<int> span;

            span = new ExSpan<int>(a);
            span.Validate(91, 92, -93, 94);

            span = new ExSpan<int>(a, 0, a.Length);
            span.Validate(91, 92, -93, 94);
        }

        [Fact]
        public static void CtorArray2() {
            long[] a = { 91, -92, 93, 94, -95 };
            ExSpan<long> span;

            span = new ExSpan<long>(a);
            span.Validate(91, -92, 93, 94, -95);

            span = new ExSpan<long>(a, 0, a.Length);
            span.Validate(91, -92, 93, 94, -95);
        }

        [Fact]
        public static void CtorArray3() {
            object o1 = new object();
            object o2 = new object();
            object[] a = { o1, o2 };
            ExSpan<object> span;

            span = new ExSpan<object>(a);
            span.ValidateReferenceType(o1, o2);

            span = new ExSpan<object>(a, 0, a.Length);
            span.ValidateReferenceType(o1, o2);
        }

        [Fact]
        public static void CtorArrayZeroLength() {
            int[] empty = ArrayHelper.Empty<int>();
            ExSpan<int> span;

            span = new ExSpan<int>(empty);
            span.ValidateNonNullEmpty();

            span = new ExSpan<int>(empty, 0, empty.Length);
            span.ValidateNonNullEmpty();
        }

        [Fact]
        public static void CtorArrayNullArray() {
            var span = new ExSpan<int>(null);
            span.Validate();
            Assert.True(span == default);

            span = new ExSpan<int>(null, 0, 0);
            span.Validate();
            Assert.True(span == default);
        }

        [Fact]
        public static void CtorArrayNullArrayNonZeroStartAndLength() {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ExSpan<int>(null, 1, 0).DontBox());
            Assert.Throws<ArgumentOutOfRangeException>(() => new ExSpan<int>(null, 0, 1).DontBox());
            Assert.Throws<ArgumentOutOfRangeException>(() => new ExSpan<int>(null, 1, 1).DontBox());
            Assert.Throws<ArgumentOutOfRangeException>(() => new ExSpan<int>(null, -1, -1).DontBox());
        }

        [Fact]
        public static void CtorArrayWrongArrayType() {
            // Cannot pass variant array, if array type is not a valuetype.
            string[] a = { "Hello" };
            Assert.Throws<ArrayTypeMismatchException>(() => new ExSpan<object>(a).DontBox());
            Assert.Throws<ArrayTypeMismatchException>(() => new ExSpan<object>(a, 0, a.Length).DontBox());
        }

        [Fact]
        public static void CtorArrayWrongValueType() {
            // Can pass variant array, if array type is a valuetype.

            uint[] a = { 42u, 0xffffffffu };
            int[] aAsIntArray = (int[])(object)a;
            ExSpan<int> span;

            span = new ExSpan<int>(aAsIntArray);
            span.Validate(42, -1);

            span = new ExSpan<int>(aAsIntArray, 0, aAsIntArray.Length);
            span.Validate(42, -1);
        }
    }
}
