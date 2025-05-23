using Xunit;

namespace Zyl.ExSpans.Tests.AExSpan {
    public static partial class ALastIndexOfSequence {
        [Fact]
        public static void LastIndexOfSequenceMatchAtStart() {
            ExSpan<int> span = new ExSpan<int>(new int[] { 5, 1, 77, 2, 3, 77, 77, 4, 5, 77, 77, 77, 88, 6, 6, 77, 77, 88, 9 });
            ExSpan<int> value = new ExSpan<int>(new int[] { 5, 1, 77 });
            int index = span.LastIndexOf(value);
            Assert.Equal(0, index);
        }

        [Fact]
        public static void LastIndexOfSequenceMultipleMatch() {
            ExSpan<int> span = new ExSpan<int>(new int[] { 1, 2, 3, 1, 2, 3, 1, 2, 3, 1 });
            ExSpan<int> value = new ExSpan<int>(new int[] { 2, 3 });
            int index = span.LastIndexOf(value);
            Assert.Equal(7, index);
        }

        [Fact]
        public static void LastIndexOfSequenceRestart() {
            ExSpan<int> span = new ExSpan<int>(new int[] { 0, 1, 77, 2, 3, 77, 77, 4, 5, 77, 77, 77, 88, 6, 6, 77, 77, 8, 9, 77, 0, 1 });
            ExSpan<int> value = new ExSpan<int>(new int[] { 77, 77, 88 });
            int index = span.LastIndexOf(value);
            Assert.Equal(10, index);
        }

        [Fact]
        public static void LastIndexOfSequenceNoMatch() {
            ExSpan<int> span = new ExSpan<int>(new int[] { 0, 1, 77, 2, 3, 77, 77, 4, 5, 77, 77, 77, 88, 6, 6, 77, 77, 88, 9 });
            ExSpan<int> value = new ExSpan<int>(new int[] { 77, 77, 88, 99 });
            int index = span.LastIndexOf(value);
            Assert.Equal(-1, index);
        }

        [Fact]
        public static void LastIndexOfSequenceNotEvenAHeadMatch() {
            ExSpan<int> span = new ExSpan<int>(new int[] { 0, 1, 77, 2, 3, 77, 77, 4, 5, 77, 77, 77, 88, 6, 6, 77, 77, 88, 9 });
            ExSpan<int> value = new ExSpan<int>(new int[] { 100, 77, 88, 99 });
            int index = span.LastIndexOf(value);
            Assert.Equal(-1, index);
        }

        [Fact]
        public static void LastIndexOfSequenceMatchAtVeryEnd() {
            ExSpan<int> span = new ExSpan<int>(new int[] { 0, 1, 2, 3, 4, 5 });
            ExSpan<int> value = new ExSpan<int>(new int[] { 3, 4, 5 });
            int index = span.LastIndexOf(value);
            Assert.Equal(3, index);
        }

        [Fact]
        public static void LastIndexOfSequenceJustPastVeryEnd() {
            ExSpan<int> span = new ExSpan<int>(new int[] { 0, 1, 2, 3, 4, 5 }, 0, 5);
            ExSpan<int> value = new ExSpan<int>(new int[] { 3, 4, 5 });
            int index = span.LastIndexOf(value);
            Assert.Equal(-1, index);
        }

        [Fact]
        public static void LastIndexOfSequenceZeroLengthValue() {
            // A zero-length value is always "found" at the end of the span.
            ExSpan<int> span = new ExSpan<int>(new int[] { 0, 1, 77, 2, 3, 77, 77, 4, 5, 77, 77, 77, 88, 6, 6, 77, 77, 88, 9 });
            ExSpan<int> value = new ExSpan<int>(ArrayHelper.Empty<int>());
            int index = span.LastIndexOf(value);
            Assert.Equal(span.Length, index);
        }

        [Fact]
        public static void LastIndexOfSequenceZeroLengthExSpan() {
            ExSpan<int> span = new ExSpan<int>(ArrayHelper.Empty<int>());
            ExSpan<int> value = new ExSpan<int>(new int[] { 1, 2, 3 });
            int index = span.LastIndexOf(value);
            Assert.Equal(-1, index);
        }

        [Fact]
        public static void LastIndexOfSequenceLengthOneValue() {
            ExSpan<int> span = new ExSpan<int>(new int[] { 0, 1, 2, 3, 4, 5 });
            ExSpan<int> value = new ExSpan<int>(new int[] { 2 });
            int index = span.LastIndexOf(value);
            Assert.Equal(2, index);
        }

        [Fact]
        public static void LastIndexOfSequenceLengthOneValueAtVeryEnd() {
            ExSpan<int> span = new ExSpan<int>(new int[] { 0, 1, 2, 3, 4, 5 });
            ExSpan<int> value = new ExSpan<int>(new int[] { 5 });
            int index = span.LastIndexOf(value);
            Assert.Equal(5, index);
        }

        [Fact]
        public static void LastIndexOfSequenceLengthOneValueMultipleTimes() {
            ExSpan<int> span = new ExSpan<int>(new int[] { 0, 1, 5, 3, 4, 5 });
            ExSpan<int> value = new ExSpan<int>(new int[] { 5 });
            int index = span.LastIndexOf(value);
            Assert.Equal(5, index);
        }

        [Fact]
        public static void LastIndexOfSequenceLengthOneValueJustPasttVeryEnd() {
            ExSpan<int> span = new ExSpan<int>(new int[] { 0, 1, 2, 3, 4, 5 }, 0, 5);
            ExSpan<int> value = new ExSpan<int>(new int[] { 5 });
            int index = span.LastIndexOf(value);
            Assert.Equal(-1, index);
        }

        [Fact]
        public static void LastIndexOfSequenceMatchAtStart_String() {
            ExSpan<string> span = new ExSpan<string>(new string[] { "5", "1", "77", "2", "3", "77", "77", "4", "5", "77", "77", "77", "88", "6", "6", "77", "77", "88", "9" });
            ExSpan<string> value = new ExSpan<string>(new string[] { "5", "1", "77" });
            int index = span.LastIndexOf(value);
            Assert.Equal(0, index);
        }

        [Fact]
        public static void LastIndexOfSequenceMultipleMatch_String() {
            ExSpan<string> span = new ExSpan<string>(new string[] { "1", "2", "3", "1", "2", "3", "1", "2", "3" });
            ExSpan<string> value = new ExSpan<string>(new string[] { "2", "3" });
            int index = span.LastIndexOf(value);
            Assert.Equal(7, index);
        }

        [Fact]
        public static void LastIndexOfSequenceRestart_String() {
            ExSpan<string> span = new ExSpan<string>(new string[] { "0", "1", "77", "2", "3", "77", "77", "4", "5", "77", "77", "77", "88", "6", "6", "77", "77", "8", "9", "77", "0", "1" });
            ExSpan<string> value = new ExSpan<string>(new string[] { "77", "77", "88" });
            int index = span.LastIndexOf(value);
            Assert.Equal(10, index);
        }

        [Fact]
        public static void LastIndexOfSequenceNoMatch_String() {
            ExSpan<string> span = new ExSpan<string>(new string[] { "0", "1", "77", "2", "3", "77", "77", "4", "5", "77", "77", "77", "88", "6", "6", "77", "77", "88", "9" });
            ExSpan<string> value = new ExSpan<string>(new string[] { "77", "77", "88", "99" });
            int index = span.LastIndexOf(value);
            Assert.Equal(-1, index);
        }

        [Fact]
        public static void LastIndexOfSequenceNotEvenAHeadMatch_String() {
            ExSpan<string> span = new ExSpan<string>(new string[] { "0", "1", "77", "2", "3", "77", "77", "4", "5", "77", "77", "77", "88", "6", "6", "77", "77", "88", "9" });
            ExSpan<string> value = new ExSpan<string>(new string[] { "100", "77", "88", "99" });
            int index = span.LastIndexOf(value);
            Assert.Equal(-1, index);
        }

        [Fact]
        public static void LastIndexOfSequenceMatchAtVeryEnd_String() {
            ExSpan<string> span = new ExSpan<string>(new string[] { "0", "1", "2", "3", "4", "5" });
            ExSpan<string> value = new ExSpan<string>(new string[] { "3", "4", "5" });
            int index = span.LastIndexOf(value);
            Assert.Equal(3, index);
        }

        [Fact]
        public static void LastIndexOfSequenceJustPastVeryEnd_String() {
            ExSpan<string> span = new ExSpan<string>(new string[] { "0", "1", "2", "3", "4", "5" }, 0, 5);
            ExSpan<string> value = new ExSpan<string>(new string[] { "3", "4", "5" });
            int index = span.LastIndexOf(value);
            Assert.Equal(-1, index);
        }

        [Fact]
        public static void LastIndexOfSequenceZeroLengthValue_String() {
            // A zero-length value is always "found" at the end of the span.
            ExSpan<string> span = new ExSpan<string>(new string[] { "0", "1", "77", "2", "3", "77", "77", "4", "5", "77", "77", "77", "88", "6", "6", "77", "77", "88", "9" });
            ExSpan<string> value = new ExSpan<string>(ArrayHelper.Empty<string>());
            int index = span.LastIndexOf(value);
            Assert.Equal(span.Length, index);
        }

        [Fact]
        public static void LastIndexOfSequenceZeroLengthExSpan_String() {
            ExSpan<string> span = new ExSpan<string>(ArrayHelper.Empty<string>());
            ExSpan<string> value = new ExSpan<string>(new string[] { "1", "2", "3" });
            int index = span.LastIndexOf(value);
            Assert.Equal(-1, index);
        }

        [Fact]
        public static void LastIndexOfSequenceLengthOneValue_String() {
            ExSpan<string> span = new ExSpan<string>(new string[] { "0", "1", "2", "3", "4", "5" });
            ExSpan<string> value = new ExSpan<string>(new string[] { "2" });
            int index = span.LastIndexOf(value);
            Assert.Equal(2, index);
        }

        [Fact]
        public static void LastIndexOfSequenceLengthOneValueAtVeryEnd_String() {
            ExSpan<string> span = new ExSpan<string>(new string[] { "0", "1", "2", "3", "4", "5" });
            ExSpan<string> value = new ExSpan<string>(new string[] { "5" });
            int index = span.LastIndexOf(value);
            Assert.Equal(5, index);
        }

        [Fact]
        public static void LastIndexOfSequenceLengthOneValueJustPasttVeryEnd_String() {
            ExSpan<string> span = new ExSpan<string>(new string[] { "0", "1", "2", "3", "4", "5" }, 0, 5);
            ExSpan<string> value = new ExSpan<string>(new string[] { "5" });
            int index = span.LastIndexOf(value);
            Assert.Equal(-1, index);
        }

        [Theory]
        [MemberData(nameof(TestHelpers.LastIndexOfNullSequenceData), MemberType = typeof(TestHelpers))]
        public static void LastIndexOfNullSequence_String(string[] spanInput, string[] searchInput, int expected) {
            ExSpan<string> theStrings = spanInput;
            Assert.Equal(expected, theStrings.LastIndexOf(searchInput));
            Assert.Equal(expected, theStrings.LastIndexOf((ReadOnlyExSpan<string>)searchInput));
        }
    }
}
