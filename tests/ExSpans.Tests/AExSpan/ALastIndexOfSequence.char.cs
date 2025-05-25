using Xunit;

namespace Zyl.ExSpans.Tests.AExSpan {
    public static partial class ALastIndexOfSequence {
        [Fact]
        public static void LastIndexOfSequenceMatchAtStart_Char() {
            ExSpan<char> span = new ExSpan<char>(new char[] { '5', '1', '7', '2', '3', '7', '7', '4', '5', '7', '7', '7', '8', '6', '6', '7', '7', '8', '9' });
            ExSpan<char> value = new ExSpan<char>(new char[] { '5', '1', '7' });
            TSize index = span.LastIndexOf(value);
            Assert.Equal(0, index);
        }

        [Fact]
        public static void LastIndexOfSequenceMultipleMatch_Char() {
            ExSpan<char> span = new ExSpan<char>(new char[] { '1', '2', '3', '1', '2', '3', '1', '2', '3', '1' });
            ExSpan<char> value = new ExSpan<char>(new char[] { '2', '3' });
            TSize index = span.LastIndexOf(value);
            Assert.Equal(7, index);
        }

        [Fact]
        public static void LastIndexOfSequenceRestart_Char() {
            ExSpan<char> span = new ExSpan<char>(new char[] { '5', '1', '7', '2', '3', '7', '7', '4', '5', '7', '7', '7', '8', '6', '6', '7', '7', '6', '9', '7', '0', '1' });
            ExSpan<char> value = new ExSpan<char>(new char[] { '7', '7', '8' });
            TSize index = span.LastIndexOf(value);
            Assert.Equal(10, index);
        }

        [Fact]
        public static void LastIndexOfSequenceNoMatch_Char() {
            ExSpan<char> span = new ExSpan<char>(new char[] { '0', '1', '7', '2', '3', '7', '7', '4', '5', '7', '7', '7', '8', '6', '6', '7', '7', '8', '9' });
            ExSpan<char> value = new ExSpan<char>(new char[] { '7', '7', '8', 'X' });
            TSize index = span.LastIndexOf(value);
            Assert.Equal(-1, index);
        }

        [Fact]
        public static void LastIndexOfSequenceNotEvenAHeadMatch_Char() {
            ExSpan<char> span = new ExSpan<char>(new char[] { '0', '1', '7', '2', '3', '7', '7', '4', '5', '7', '7', '7', '8', '6', '6', '7', '7', '8', '9' });
            ExSpan<char> value = new ExSpan<char>(new char[] { 'X', '7', '8', '9' });
            TSize index = span.LastIndexOf(value);
            Assert.Equal(-1, index);
        }

        [Fact]
        public static void LastIndexOfSequenceMatchAtVeryEnd_Char() {
            ExSpan<char> span = new ExSpan<char>(new char[] { '0', '1', '2', '3', '4', '5' });
            ExSpan<char> value = new ExSpan<char>(new char[] { '3', '4', '5' });
            TSize index = span.LastIndexOf(value);
            Assert.Equal(3, index);
        }

        [Fact]
        public static void LastIndexOfSequenceJustPastVeryEnd_Char() {
            ExSpan<char> span = new ExSpan<char>(new char[] { '0', '1', '2', '3', '4', '5' }, 0, 5);
            ExSpan<char> value = new ExSpan<char>(new char[] { '3', '4', '5' });
            TSize index = span.LastIndexOf(value);
            Assert.Equal(-1, index);
        }

        [Fact]
        public static void LastIndexOfSequenceZeroLengthValue_Char() {
            // A zero-length value is always "found" at the end of the span.
            ExSpan<char> span = new ExSpan<char>(new char[] { '0', '1', '7', '2', '3', '7', '7', '4', '5', '7', '7', '7', '8', '6', '6', '7', '7', '8', '9' });
            ExSpan<char> value = new ExSpan<char>(ArrayHelper.Empty<char>());
            TSize index = span.LastIndexOf(value);
            Assert.Equal(span.Length, index);
        }

        [Fact]
        public static void LastIndexOfSequenceZeroLengthExSpan_Char() {
            ExSpan<char> span = new ExSpan<char>(ArrayHelper.Empty<char>());
            ExSpan<char> value = new ExSpan<char>(new char[] { '1', '2', '3' });
            TSize index = span.LastIndexOf(value);
            Assert.Equal(-1, index);
        }

        [Fact]
        public static void LastIndexOfSequenceLengthOneValue_Char() {
            ExSpan<char> span = new ExSpan<char>(new char[] { '0', '1', '2', '3', '4', '5' });
            ExSpan<char> value = new ExSpan<char>(new char[] { '2' });
            TSize index = span.LastIndexOf(value);
            Assert.Equal(2, index);
        }

        [Fact]
        public static void LastIndexOfSequenceLengthOneValueAtVeryEnd_Char() {
            ExSpan<char> span = new ExSpan<char>(new char[] { '0', '1', '2', '3', '4', '5' });
            ExSpan<char> value = new ExSpan<char>(new char[] { '5' });
            TSize index = span.LastIndexOf(value);
            Assert.Equal(5, index);
        }

        [Fact]
        public static void LastIndexOfSequenceLengthOneValueMultipleTimes_Char() {
            ExSpan<char> span = new ExSpan<char>(new char[] { '0', '1', '5', '3', '4', '5' });
            ExSpan<char> value = new ExSpan<char>(new char[] { '5' });
            TSize index = span.LastIndexOf(value);
            Assert.Equal(5, index);
        }

        [Fact]
        public static void LastIndexOfSequenceLengthOneValueJustPasttVeryEnd_Char() {
            ExSpan<char> span = new ExSpan<char>(new char[] { '0', '1', '2', '3', '4', '5' }, 0, 5);
            ExSpan<char> value = new ExSpan<char>(new char[] { '5' });
            TSize index = span.LastIndexOf(value);
            Assert.Equal(-1, index);
        }
    }
}
