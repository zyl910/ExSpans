using Xunit;

namespace Zyl.ExSpans.Tests.AExSpan {
    public static partial class AIndexOfSequence {
        [Fact]
        public static void IndexOfSequenceMatchAtStart() {
            ExSpan<int> span = new ExSpan<int>(new int[] { 5, 1, 77, 2, 3, 77, 77, 4, 5, 77, 77, 77, 88, 6, 6, 77, 77, 88, 9 });
            ExSpan<int> value = new ExSpan<int>(new int[] { 5, 1, 77 });
            TSize index = span.IndexOf(value);
            Assert.Equal(0, index);
        }

        [Fact]
        public static void IndexOfSequenceMultipleMatch() {
            ExSpan<int> span = new ExSpan<int>(new int[] { 1, 2, 3, 1, 2, 3, 1, 2, 3 });
            ExSpan<int> value = new ExSpan<int>(new int[] { 2, 3 });
            TSize index = span.IndexOf(value);
            Assert.Equal(1, index);
        }

        [Fact]
        public static void IndexOfSequenceRestart() {
            ExSpan<int> span = new ExSpan<int>(new int[] { 0, 1, 77, 2, 3, 77, 77, 4, 5, 77, 77, 77, 88, 6, 6, 77, 77, 88, 9 });
            ExSpan<int> value = new ExSpan<int>(new int[] { 77, 77, 88 });
            TSize index = span.IndexOf(value);
            Assert.Equal(10, index);
        }

        [Fact]
        public static void IndexOfSequenceNoMatch() {
            ExSpan<int> span = new ExSpan<int>(new int[] { 0, 1, 77, 2, 3, 77, 77, 4, 5, 77, 77, 77, 88, 6, 6, 77, 77, 88, 9 });
            ExSpan<int> value = new ExSpan<int>(new int[] { 77, 77, 88, 99 });
            TSize index = span.IndexOf(value);
            Assert.Equal(-1, index);
        }

        [Fact]
        public static void IndexOfSequenceNotEvenAHeadMatch() {
            ExSpan<int> span = new ExSpan<int>(new int[] { 0, 1, 77, 2, 3, 77, 77, 4, 5, 77, 77, 77, 88, 6, 6, 77, 77, 88, 9 });
            ExSpan<int> value = new ExSpan<int>(new int[] { 100, 77, 88, 99 });
            TSize index = span.IndexOf(value);
            Assert.Equal(-1, index);
        }

        [Fact]
        public static void IndexOfSequenceMatchAtVeryEnd() {
            ExSpan<int> span = new ExSpan<int>(new int[] { 0, 1, 2, 3, 4, 5 });
            ExSpan<int> value = new ExSpan<int>(new int[] { 3, 4, 5 });
            TSize index = span.IndexOf(value);
            Assert.Equal(3, index);
        }

        [Fact]
        public static void IndexOfSequenceJustPastVeryEnd() {
            ExSpan<int> span = new ExSpan<int>(new int[] { 0, 1, 2, 3, 4, 5 }, 0, 5);
            ExSpan<int> value = new ExSpan<int>(new int[] { 3, 4, 5 });
            TSize index = span.IndexOf(value);
            Assert.Equal(-1, index);
        }

        [Fact]
        public static void IndexOfSequenceZeroLengthValue() {
            // A zero-length value is always "found" at the start of the span.
            ExSpan<int> span = new ExSpan<int>(new int[] { 0, 1, 77, 2, 3, 77, 77, 4, 5, 77, 77, 77, 88, 6, 6, 77, 77, 88, 9 });
            ExSpan<int> value = new ExSpan<int>(ArrayHelper.Empty<int>());
            TSize index = span.IndexOf(value);
            Assert.Equal(0, index);
        }

        [Fact]
        public static void IndexOfSequenceZeroLengthExSpan() {
            ExSpan<int> span = new ExSpan<int>(ArrayHelper.Empty<int>());
            ExSpan<int> value = new ExSpan<int>(new int[] { 1, 2, 3 });
            TSize index = span.IndexOf(value);
            Assert.Equal(-1, index);
        }

        [Fact]
        public static void IndexOfSequenceLengthOneValue() {
            ExSpan<int> span = new ExSpan<int>(new int[] { 0, 1, 2, 3, 4, 5 });
            ExSpan<int> value = new ExSpan<int>(new int[] { 2 });
            TSize index = span.IndexOf(value);
            Assert.Equal(2, index);
        }

        [Fact]
        public static void IndexOfSequenceLengthOneValueAtVeryEnd() {
            ExSpan<int> span = new ExSpan<int>(new int[] { 0, 1, 2, 3, 4, 5 });
            ExSpan<int> value = new ExSpan<int>(new int[] { 5 });
            TSize index = span.IndexOf(value);
            Assert.Equal(5, index);
        }

        [Fact]
        public static void IndexOfSequenceLengthOneValueJustPasttVeryEnd() {
            ExSpan<int> span = new ExSpan<int>(new int[] { 0, 1, 2, 3, 4, 5 }, 0, 5);
            ExSpan<int> value = new ExSpan<int>(new int[] { 5 });
            TSize index = span.IndexOf(value);
            Assert.Equal(-1, index);
        }

        [Fact]
        public static void IndexOfSequenceMatchAtStart_String() {
            ExSpan<string> span = new ExSpan<string>(new string[] { "5", "1", "77", "2", "3", "77", "77", "4", "5", "77", "77", "77", "88", "6", "6", "77", "77", "88", "9" });
            ExSpan<string> value = new ExSpan<string>(new string[] { "5", "1", "77" });
            TSize index = span.IndexOf(value);
            Assert.Equal(0, index);
        }

        [Fact]
        public static void IndexOfSequenceMultipleMatch_String() {
            ExSpan<string> span = new ExSpan<string>(new string[] { "1", "2", "3", "1", "2", "3", "1", "2", "3" });
            ExSpan<string> value = new ExSpan<string>(new string[] { "2", "3" });
            TSize index = span.IndexOf(value);
            Assert.Equal(1, index);
        }

        [Fact]
        public static void IndexOfSequenceRestart_String() {
            ExSpan<string> span = new ExSpan<string>(new string[] { "0", "1", "77", "2", "3", "77", "77", "4", "5", "77", "77", "77", "88", "6", "6", "77", "77", "88", "9" });
            ExSpan<string> value = new ExSpan<string>(new string[] { "77", "77", "88" });
            TSize index = span.IndexOf(value);
            Assert.Equal(10, index);
        }

        [Fact]
        public static void IndexOfSequenceNoMatch_String() {
            ExSpan<string> span = new ExSpan<string>(new string[] { "0", "1", "77", "2", "3", "77", "77", "4", "5", "77", "77", "77", "88", "6", "6", "77", "77", "88", "9" });
            ExSpan<string> value = new ExSpan<string>(new string[] { "77", "77", "88", "99" });
            TSize index = span.IndexOf(value);
            Assert.Equal(-1, index);
        }

        [Fact]
        public static void IndexOfSequenceNotEvenAHeadMatch_String() {
            ExSpan<string> span = new ExSpan<string>(new string[] { "0", "1", "77", "2", "3", "77", "77", "4", "5", "77", "77", "77", "88", "6", "6", "77", "77", "88", "9" });
            ExSpan<string> value = new ExSpan<string>(new string[] { "100", "77", "88", "99" });
            TSize index = span.IndexOf(value);
            Assert.Equal(-1, index);
        }

        [Fact]
        public static void IndexOfSequenceMatchAtVeryEnd_String() {
            ExSpan<string> span = new ExSpan<string>(new string[] { "0", "1", "2", "3", "4", "5" });
            ExSpan<string> value = new ExSpan<string>(new string[] { "3", "4", "5" });
            TSize index = span.IndexOf(value);
            Assert.Equal(3, index);
        }

        [Fact]
        public static void IndexOfSequenceJustPastVeryEnd_String() {
            ExSpan<string> span = new ExSpan<string>(new string[] { "0", "1", "2", "3", "4", "5" }, 0, 5);
            ExSpan<string> value = new ExSpan<string>(new string[] { "3", "4", "5" });
            TSize index = span.IndexOf(value);
            Assert.Equal(-1, index);
        }

        [Fact]
        public static void IndexOfSequenceZeroLengthValue_String() {
            // A zero-length value is always "found" at the start of the span.
            ExSpan<string> span = new ExSpan<string>(new string[] { "0", "1", "77", "2", "3", "77", "77", "4", "5", "77", "77", "77", "88", "6", "6", "77", "77", "88", "9" });
            ExSpan<string> value = new ExSpan<string>(ArrayHelper.Empty<string>());
            TSize index = span.IndexOf(value);
            Assert.Equal(0, index);
        }

        [Fact]
        public static void IndexOfSequenceZeroLengthExSpan_String() {
            ExSpan<string> span = new ExSpan<string>(ArrayHelper.Empty<string>());
            ExSpan<string> value = new ExSpan<string>(new string[] { "1", "2", "3" });
            TSize index = span.IndexOf(value);
            Assert.Equal(-1, index);
        }

        [Fact]
        public static void IndexOfSequenceLengthOneValue_String() {
            ExSpan<string> span = new ExSpan<string>(new string[] { "0", "1", "2", "3", "4", "5" });
            ExSpan<string> value = new ExSpan<string>(new string[] { "2" });
            TSize index = span.IndexOf(value);
            Assert.Equal(2, index);
        }

        [Fact]
        public static void IndexOfSequenceLengthOneValueAtVeryEnd_String() {
            ExSpan<string> span = new ExSpan<string>(new string[] { "0", "1", "2", "3", "4", "5" });
            ExSpan<string> value = new ExSpan<string>(new string[] { "5" });
            TSize index = span.IndexOf(value);
            Assert.Equal(5, index);
        }

        [Fact]
        public static void IndexOfSequenceLengthOneValueJustPasttVeryEnd_String() {
            ExSpan<string> span = new ExSpan<string>(new string[] { "0", "1", "2", "3", "4", "5" }, 0, 5);
            ExSpan<string> value = new ExSpan<string>(new string[] { "5" });
            TSize index = span.IndexOf(value);
            Assert.Equal(-1, index);
        }

        [Theory]
        [MemberData(nameof(TestHelpers.IndexOfNullSequenceData), MemberType = typeof(TestHelpers))]
        public static void IndexOfNullSequence_String(string[] spanInput, string[] searchInput, int expected) {
            ExSpan<string> theStrings = spanInput;
            Assert.Equal(expected, theStrings.IndexOf(searchInput));
            Assert.Equal(expected, theStrings.IndexOf((ReadOnlyExSpan<string>)searchInput));
        }
    }
}
