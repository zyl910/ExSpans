using Xunit;

namespace Zyl.ExSpans.Tests.AReadOnlyExSpan {
    public static partial class ALastIndexOfSequence {
        [Fact]
        public static void LastIndexOfSequenceMatchAtStart_Byte() {
            ReadOnlyExSpan<byte> span = new ReadOnlyExSpan<byte>(new byte[] { 5, 1, 77, 2, 3, 77, 77, 4, 5, 77, 77, 77, 88, 6, 6, 77, 77, 88, 9 });
            ReadOnlyExSpan<byte> value = new ReadOnlyExSpan<byte>(new byte[] { 5, 1, 77 });
            int index = span.LastIndexOf(value);
            Assert.Equal(0, index);
        }

        [Fact]
        public static void LastIndexOfSequenceMultipleMatch_Byte() {
            ReadOnlyExSpan<byte> span = new ReadOnlyExSpan<byte>(new byte[] { 1, 2, 3, 1, 2, 3, 1, 2, 3, 1 });
            ReadOnlyExSpan<byte> value = new ReadOnlyExSpan<byte>(new byte[] { 2, 3 });
            int index = span.LastIndexOf(value);
            Assert.Equal(7, index);
        }

        [Fact]
        public static void LastIndexOfSequenceRestart_Byte() {
            ReadOnlyExSpan<byte> span = new ReadOnlyExSpan<byte>(new byte[] { 0, 1, 77, 2, 3, 77, 77, 4, 5, 77, 77, 77, 88, 6, 6, 77, 77, 8, 9, 77, 0, 1 });
            ReadOnlyExSpan<byte> value = new ReadOnlyExSpan<byte>(new byte[] { 77, 77, 88 });
            int index = span.LastIndexOf(value);
            Assert.Equal(10, index);
        }

        [Fact]
        public static void LastIndexOfSequenceNoMatch_Byte() {
            ReadOnlyExSpan<byte> span = new ReadOnlyExSpan<byte>(new byte[] { 0, 1, 77, 2, 3, 77, 77, 4, 5, 77, 77, 77, 88, 6, 6, 77, 77, 88, 9 });
            ReadOnlyExSpan<byte> value = new ReadOnlyExSpan<byte>(new byte[] { 77, 77, 88, 99 });
            int index = span.LastIndexOf(value);
            Assert.Equal(-1, index);
        }

        [Fact]
        public static void LastIndexOfSequenceNotEvenAHeadMatch_Byte() {
            ReadOnlyExSpan<byte> span = new ReadOnlyExSpan<byte>(new byte[] { 0, 1, 77, 2, 3, 77, 77, 4, 5, 77, 77, 77, 88, 6, 6, 77, 77, 88, 9 });
            ReadOnlyExSpan<byte> value = new ReadOnlyExSpan<byte>(new byte[] { 100, 77, 88, 99 });
            int index = span.LastIndexOf(value);
            Assert.Equal(-1, index);
        }

        [Fact]
        public static void LastIndexOfSequenceMatchAtVeryEnd_Byte() {
            ReadOnlyExSpan<byte> span = new ReadOnlyExSpan<byte>(new byte[] { 0, 1, 2, 3, 4, 5 });
            ReadOnlyExSpan<byte> value = new ReadOnlyExSpan<byte>(new byte[] { 3, 4, 5 });
            int index = span.LastIndexOf(value);
            Assert.Equal(3, index);
        }

        [Fact]
        public static void LastIndexOfSequenceJustPastVeryEnd_Byte() {
            ReadOnlyExSpan<byte> span = new ReadOnlyExSpan<byte>(new byte[] { 0, 1, 2, 3, 4, 5 }, 0, 5);
            ReadOnlyExSpan<byte> value = new ReadOnlyExSpan<byte>(new byte[] { 3, 4, 5 });
            int index = span.LastIndexOf(value);
            Assert.Equal(-1, index);
        }

        [Fact]
        public static void LastIndexOfSequenceZeroLengthValue_Byte() {
            // A zero-length value is always "found" at the end of the span.
            ReadOnlyExSpan<byte> span = new ReadOnlyExSpan<byte>(new byte[] { 0, 1, 77, 2, 3, 77, 77, 4, 5, 77, 77, 77, 88, 6, 6, 77, 77, 88, 9 });
            ReadOnlyExSpan<byte> value = new ReadOnlyExSpan<byte>(Array.Empty<byte>());
            int index = span.LastIndexOf(value);
            Assert.Equal(span.Length, index);
        }

        [Fact]
        public static void LastIndexOfSequenceZeroLengthSpan_Byte() {
            ReadOnlyExSpan<byte> span = new ReadOnlyExSpan<byte>(Array.Empty<byte>());
            ReadOnlyExSpan<byte> value = new ReadOnlyExSpan<byte>(new byte[] { 1, 2, 3 });
            int index = span.LastIndexOf(value);
            Assert.Equal(-1, index);
        }

        [Fact]
        public static void LastIndexOfSequenceLengthOneValue_Byte() {
            ReadOnlyExSpan<byte> span = new ReadOnlyExSpan<byte>(new byte[] { 0, 1, 2, 3, 4, 5 });
            ReadOnlyExSpan<byte> value = new ReadOnlyExSpan<byte>(new byte[] { 2 });
            int index = span.LastIndexOf(value);
            Assert.Equal(2, index);
        }

        [Fact]
        public static void LastIndexOfSequenceLengthOneValueAtVeryEnd_Byte() {
            ReadOnlyExSpan<byte> span = new ReadOnlyExSpan<byte>(new byte[] { 0, 1, 2, 3, 4, 5 });
            ReadOnlyExSpan<byte> value = new ReadOnlyExSpan<byte>(new byte[] { 5 });
            int index = span.LastIndexOf(value);
            Assert.Equal(5, index);
        }

        [Fact]
        public static void LastIndexOfSequenceLengthOneValueMultipleTimes_Byte() {
            ReadOnlyExSpan<byte> span = new ReadOnlyExSpan<byte>(new byte[] { 0, 1, 5, 3, 4, 5 });
            ReadOnlyExSpan<byte> value = new ReadOnlyExSpan<byte>(new byte[] { 5 });
            int index = span.LastIndexOf(value);
            Assert.Equal(5, index);
        }

        [Fact]
        public static void LastIndexOfSequenceLengthOneValueJustPasttVeryEnd_Byte() {
            ReadOnlyExSpan<byte> span = new ReadOnlyExSpan<byte>(new byte[] { 0, 1, 2, 3, 4, 5 }, 0, 5);
            ReadOnlyExSpan<byte> value = new ReadOnlyExSpan<byte>(new byte[] { 5 });
            int index = span.LastIndexOf(value);
            Assert.Equal(-1, index);
        }
    }
}
