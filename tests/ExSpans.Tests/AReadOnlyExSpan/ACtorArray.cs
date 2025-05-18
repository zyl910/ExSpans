using Xunit;
using static Zyl.ExSpans.Tests.TestHelpers;

namespace Zyl.ExSpans.Tests.AReadOnlyExSpan {
    //
    // Tests for Span<T>.ctor(T[])
    //
    // These tests will also exercise the matching codepaths in Span<T>.ctor(T[], int) and .ctor(T[], int, int). This makes it easier to ensure
    // that these parallel tests stay consistent, and avoid excess repetition in the files devoted to those specific overloads.
    //
    public static partial class ACtorArray {
        [Fact]
        public static void CtorArray1() {
            int[] a = { 91, 92, -93, 94 };
            ReadOnlyExSpan<int> span;

            span = new ReadOnlyExSpan<int>(a);
            span.Validate(91, 92, -93, 94);

            span = new ReadOnlyExSpan<int>(a, 0, a.Length);
            span.Validate(91, 92, -93, 94);
        }

        [Fact]
        public static void CtorArray2() {
            long[] a = { 91, -92, 93, 94, -95 };
            ReadOnlyExSpan<long> span;

            span = new ReadOnlyExSpan<long>(a);
            span.Validate(91, -92, 93, 94, -95);

            span = new ReadOnlyExSpan<long>(a, 0, a.Length);
            span.Validate(91, -92, 93, 94, -95);
        }

        [Fact]
        public static void CtorArray3() {
            object o1 = new object();
            object o2 = new object();
            object[] a = { o1, o2 };
            ReadOnlyExSpan<object> span;

            span = new ReadOnlyExSpan<object>(a);
            span.ValidateReferenceType(o1, o2);

            span = new ReadOnlyExSpan<object>(a, 0, a.Length);
            span.ValidateReferenceType(o1, o2);
        }

        [Fact]
        public static void CtorArrayZeroLength() {
            int[] empty = Array.Empty<int>();
            ReadOnlyExSpan<int> span;

            span = new ReadOnlyExSpan<int>(empty);
            span.ValidateNonNullEmpty();

            span = new ReadOnlyExSpan<int>(empty, 0, empty.Length);
            span.ValidateNonNullEmpty();
        }

        [Fact]
        public static void CtorArrayNullArray() {
            var span = new ReadOnlyExSpan<int>(null);
            span.Validate();
            Assert.True(span == default);

            span = new ReadOnlyExSpan<int>(null, 0, 0);
            span.Validate();
            Assert.True(span == default);
        }

        [Fact]
        public static void CtorArrayNullArrayNonZeroStartAndLength() {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ReadOnlyExSpan<int>(null, 1, 0).DontBox());
            Assert.Throws<ArgumentOutOfRangeException>(() => new ReadOnlyExSpan<int>(null, 0, 1).DontBox());
            Assert.Throws<ArgumentOutOfRangeException>(() => new ReadOnlyExSpan<int>(null, 1, 1).DontBox());
            Assert.Throws<ArgumentOutOfRangeException>(() => new ReadOnlyExSpan<int>(null, -1, -1).DontBox());
        }

        [Fact]
        public static void CtorArrayWrongValueType() {
            // Can pass variant array, if array type is a valuetype.

            uint[] a = { 42u, 0xffffffffu };
            int[] aAsIntArray = (int[])(object)a;
            ReadOnlyExSpan<int> span;

            span = new ReadOnlyExSpan<int>(aAsIntArray);
            span.Validate(42, -1);

            span = new ReadOnlyExSpan<int>(aAsIntArray, 0, aAsIntArray.Length);
            span.Validate(42, -1);
        }

        [Fact]
        public static void CtorVariantArrayType() {
            // For ReadOnlyExSpan<T>, variant arrays are allowed for string to object
            // and reference type to object.

            ReadOnlyExSpan<object> span;

            string[] strArray = { "Hello" };
            span = new ReadOnlyExSpan<object>(strArray);
            span.ValidateReferenceType("Hello");
            span = new ReadOnlyExSpan<object>(strArray, 0, strArray.Length);
            span.ValidateReferenceType("Hello");

            TestClass c1 = new TestClass();
            TestClass c2 = new TestClass();
            TestClass[] clsArray = { c1, c2 };
            span = new ReadOnlyExSpan<object>(clsArray);
            span.ValidateReferenceType(c1, c2);
            span = new ReadOnlyExSpan<object>(clsArray, 0, clsArray.Length);
            span.ValidateReferenceType(c1, c2);
        }
    }
}
