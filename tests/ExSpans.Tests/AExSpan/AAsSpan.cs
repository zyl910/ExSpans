using Xunit;

namespace Zyl.ExSpans.Tests.AExSpan {
    public static partial class AAsSpan {
        [Fact]
        public static void IntArrayAsExSpan() {
            int[] a = { 91, 92, -93, 94 };
            ExSpan<int> spanInt = a.AsExSpan();
            spanInt.Validate(91, 92, -93, 94);
        }

        [Fact]
        public static void LongArrayAsExSpan() {
            long[] b = { 91, -92, 93, 94, -95 };
            ExSpan<long> spanLong = b.AsExSpan();
            spanLong.Validate(91, -92, 93, 94, -95);
        }

        [Fact]
        public static void ObjectArrayAsExSpan() {
            object o1 = new object();
            object o2 = new object();
            object[] c = { o1, o2 };
            ExSpan<object> spanObject = c.AsExSpan();
            spanObject.ValidateReferenceType(o1, o2);
        }

        [Fact]
        public static void NullArrayAsExSpan() {
            int[] a = null;
            ExSpan<int> span = a.AsExSpan();
            span.Validate();
            Assert.True(span == default);
        }

        [Fact]
        public static void EmptyArrayAsExSpan() {
            int[] empty = ArrayHelper.Empty<int>();
            ExSpan<int> span = empty.AsExSpan();
            span.ValidateNonNullEmpty();
        }

        [Fact]
        public static void IntArraySegmentAsExSpan() {
            int[] a = { 91, 92, -93, 94 };
            ArraySegment<int> segmentInt = new ArraySegment<int>(a, 1, 2);
            ExSpan<int> spanInt = segmentInt.AsExSpan();
            spanInt.Validate(92, -93);
        }

        [Fact]
        public static void LongArraySegmentAsExSpan() {
            long[] b = { 91, -92, 93, 94, -95 };
            ArraySegment<long> segmentLong = new ArraySegment<long>(b, 1, 3);
            ExSpan<long> spanLong = segmentLong.AsExSpan();
            spanLong.Validate(-92, 93, 94);
        }

        [Fact]
        public static void ObjectArraySegmentAsExSpan() {
            object o1 = new object();
            object o2 = new object();
            object o3 = new object();
            object o4 = new object();
            object[] c = { o1, o2, o3, o4 };
            ArraySegment<object> segmentObject = new ArraySegment<object>(c, 1, 2);
            ExSpan<object> spanObject = segmentObject.AsExSpan();
            spanObject.ValidateReferenceType(o2, o3);
        }

        [Fact]
        public static void ZeroLengthArraySegmentAsExSpan() {
            int[] empty = ArrayHelper.Empty<int>();
            ArraySegment<int> segmentEmpty = new ArraySegment<int>(empty);
            ExSpan<int> spanEmpty = segmentEmpty.AsExSpan();
            spanEmpty.ValidateNonNullEmpty();

            int[] a = { 91, 92, -93, 94 };
            ArraySegment<int> segmentInt = new ArraySegment<int>(a, 0, 0);
            ExSpan<int> spanInt = segmentInt.AsExSpan();
            spanInt.ValidateNonNullEmpty();
        }

        [Fact]
        public static void CovariantAsExSpanNotSupported() {
            object[] a = new string[10];
            Assert.Throws<ArrayTypeMismatchException>(() => a.AsExSpan());
            Assert.Throws<ArrayTypeMismatchException>(() => a.AsExSpan(0, a.Length));
        }

        [Fact]
        public static void GuidArrayAsExSpanWithStartAndLength() {
            var arr = new Guid[20];

            ExSpan<Guid> slice = arr.AsExSpan(2, 2);
            Guid guid = Guid.NewGuid();
            slice[1] = guid;

            Assert.Equal(guid, arr[3]);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(3, 0)]
        [InlineData(3, 1)]
        [InlineData(3, 2)]
        [InlineData(3, 3)]
        [InlineData(10, 0)]
        [InlineData(10, 3)]
        [InlineData(10, 10)]
        public static void ArrayAsExSpanWithStart(int length, int start) {
            int[] a = new int[length];
            ExSpan<int> s = a.AsExSpan(start);
            Assert.Equal(length - start, s.Length);
            if (start != length) {
                s[0] = 42;
                Assert.Equal(42, a[start]);
            }
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(3, 0)]
        [InlineData(3, 1)]
        [InlineData(3, 2)]
        [InlineData(3, 3)]
        [InlineData(10, 0)]
        [InlineData(10, 3)]
        [InlineData(10, 10)]
        public static void ArraySegmentAsExSpanWithStart(int length, int start) {
            const int segmentOffset = 5;

            int[] a = new int[length + segmentOffset];
            ArraySegment<int> segment = new ArraySegment<int>(a, 5, length);
            ExSpan<int> s = segment.AsExSpan(start);
            Assert.Equal(length - start, s.Length);
            if (s.Length != 0) {
                s[0] = 42;
                Assert.Equal(42, a[segmentOffset + start]);
            }
        }

        [Theory]
        [InlineData(0, 0, 0)]
        [InlineData(3, 0, 3)]
        [InlineData(3, 1, 2)]
        [InlineData(3, 2, 1)]
        [InlineData(3, 3, 0)]
        [InlineData(10, 0, 5)]
        [InlineData(10, 3, 2)]
        public static void ArrayAsExSpanWithStartAndLength(int length, int start, int subLength) {
            int[] a = new int[length];
            ExSpan<int> s = a.AsExSpan(start, subLength);
            Assert.Equal(subLength, s.Length);
            if (subLength != 0) {
                s[0] = 42;
                Assert.Equal(42, a[start]);
            }
        }

        [Theory]
        [InlineData(0, 0, 0)]
        [InlineData(3, 0, 3)]
        [InlineData(3, 1, 2)]
        [InlineData(3, 2, 1)]
        [InlineData(3, 3, 0)]
        [InlineData(10, 0, 5)]
        [InlineData(10, 3, 2)]
        public static void ArraySegmentAsExSpanWithStartAndLength(int length, int start, int subLength) {
            const int segmentOffset = 5;

            int[] a = new int[length + segmentOffset];
            ArraySegment<int> segment = new ArraySegment<int>(a, segmentOffset, length);
            ExSpan<int> s = segment.AsExSpan(start, subLength);
            Assert.Equal(subLength, s.Length);
            if (subLength != 0) {
                s[0] = 42;
                Assert.Equal(42, a[segmentOffset + start]);
            }
        }

        [Theory]
        [InlineData(0, -1)]
        [InlineData(0, 1)]
        [InlineData(5, 6)]
        public static void ArrayAsExSpanWithStartNegative(int length, int start) {
            int[] a = new int[length];
            Assert.Throws<ArgumentOutOfRangeException>(() => a.AsExSpan(start));
        }

        [Theory]
        [InlineData(0, -1, 0)]
        [InlineData(0, 1, 0)]
        [InlineData(0, 0, -1)]
        [InlineData(0, 0, 1)]
        [InlineData(5, 6, 0)]
        [InlineData(5, 3, 3)]
        public static void ArrayAsExSpanWithStartAndLengthNegative(int length, int start, int subLength) {
            int[] a = new int[length];
            Assert.Throws<ArgumentOutOfRangeException>(() => a.AsExSpan(start, subLength));
        }
    }
}
