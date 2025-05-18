using Xunit;

namespace Zyl.ExSpans.Tests.AReadOnlyExSpan {
    public static partial class AIndexOfSequence {
        [Fact]
        public static void IndexOfSequenceMatchAtStart_Byte() {
            ReadOnlyExSpan<byte> span = new ReadOnlyExSpan<byte>(new byte[] { 5, 1, 77, 2, 3, 77, 77, 4, 5, 77, 77, 77, 88, 6, 6, 77, 77, 88, 9 });
            ReadOnlyExSpan<byte> value = new ReadOnlyExSpan<byte>(new byte[] { 5, 1, 77 });
            int index = span.IndexOf(value);
            Assert.Equal(0, index);
        }

        [Fact]
        public static void IndexOfSequenceMultipleMatch_Byte() {
            ReadOnlyExSpan<byte> span = new ReadOnlyExSpan<byte>(new byte[] { 1, 2, 3, 1, 2, 3, 1, 2, 3 });
            ReadOnlyExSpan<byte> value = new ReadOnlyExSpan<byte>(new byte[] { 2, 3 });
            int index = span.IndexOf(value);
            Assert.Equal(1, index);
        }

        [Fact]
        public static void IndexOfSequenceRestart_Byte() {
            ReadOnlyExSpan<byte> span = new ReadOnlyExSpan<byte>(new byte[] { 0, 1, 77, 2, 3, 77, 77, 4, 5, 77, 77, 77, 88, 6, 6, 77, 77, 88, 9 });
            ReadOnlyExSpan<byte> value = new ReadOnlyExSpan<byte>(new byte[] { 77, 77, 88 });
            int index = span.IndexOf(value);
            Assert.Equal(10, index);
        }

        [Fact]
        public static void IndexOfSequenceNoMatch_Byte() {
            ReadOnlyExSpan<byte> span = new ReadOnlyExSpan<byte>(new byte[] { 0, 1, 77, 2, 3, 77, 77, 4, 5, 77, 77, 77, 88, 6, 6, 77, 77, 88, 9 });
            ReadOnlyExSpan<byte> value = new ReadOnlyExSpan<byte>(new byte[] { 77, 77, 88, 99 });
            int index = span.IndexOf(value);
            Assert.Equal(-1, index);
        }

        [Fact]
        public static void IndexOfSequenceNotEvenAHeadMatch_Byte() {
            ReadOnlyExSpan<byte> span = new ReadOnlyExSpan<byte>(new byte[] { 0, 1, 77, 2, 3, 77, 77, 4, 5, 77, 77, 77, 88, 6, 6, 77, 77, 88, 9 });
            ReadOnlyExSpan<byte> value = new ReadOnlyExSpan<byte>(new byte[] { 100, 77, 88, 99 });
            int index = span.IndexOf(value);
            Assert.Equal(-1, index);
        }

        [Fact]
        public static void IndexOfSequenceMatchAtVeryEnd_Byte() {
            ReadOnlyExSpan<byte> span = new ReadOnlyExSpan<byte>(new byte[] { 0, 1, 2, 3, 4, 5 });
            ReadOnlyExSpan<byte> value = new ReadOnlyExSpan<byte>(new byte[] { 3, 4, 5 });
            int index = span.IndexOf(value);
            Assert.Equal(3, index);
        }

        [Fact]
        public static void IndexOfSequenceJustPastVeryEnd_Byte() {
            ReadOnlyExSpan<byte> span = new ReadOnlyExSpan<byte>(new byte[] { 0, 1, 2, 3, 4, 5 }, 0, 5);
            ReadOnlyExSpan<byte> value = new ReadOnlyExSpan<byte>(new byte[] { 3, 4, 5 });
            int index = span.IndexOf(value);
            Assert.Equal(-1, index);
        }

        [Fact]
        public static void IndexOfSequenceZeroLengthValue_Byte() {
            // A zero-length value is always "found" at the start of the span.
            ReadOnlyExSpan<byte> span = new ReadOnlyExSpan<byte>(new byte[] { 0, 1, 77, 2, 3, 77, 77, 4, 5, 77, 77, 77, 88, 6, 6, 77, 77, 88, 9 });
            ReadOnlyExSpan<byte> value = new ReadOnlyExSpan<byte>(Array.Empty<byte>());
            int index = span.IndexOf(value);
            Assert.Equal(0, index);
        }

        [Fact]
        public static void IndexOfSequenceZeroLengthSpan_Byte() {
            ReadOnlyExSpan<byte> span = new ReadOnlyExSpan<byte>(Array.Empty<byte>());
            ReadOnlyExSpan<byte> value = new ReadOnlyExSpan<byte>(new byte[] { 1, 2, 3 });
            int index = span.IndexOf(value);
            Assert.Equal(-1, index);
        }

        [Fact]
        public static void IndexOfSequenceLengthOneValue_Byte() {
            ReadOnlyExSpan<byte> span = new ReadOnlyExSpan<byte>(new byte[] { 0, 1, 2, 3, 4, 5 });
            ReadOnlyExSpan<byte> value = new ReadOnlyExSpan<byte>(new byte[] { 2 });
            int index = span.IndexOf(value);
            Assert.Equal(2, index);
        }

        [Fact]
        public static void IndexOfSequenceLengthOneValueAtVeryEnd_Byte() {
            ReadOnlyExSpan<byte> span = new ReadOnlyExSpan<byte>(new byte[] { 0, 1, 2, 3, 4, 5 });
            ReadOnlyExSpan<byte> value = new ReadOnlyExSpan<byte>(new byte[] { 5 });
            int index = span.IndexOf(value);
            Assert.Equal(5, index);
        }

        [Fact]
        public static void IndexOfSequenceLengthOneValueJustPasttVeryEnd_Byte() {
            ReadOnlyExSpan<byte> span = new ReadOnlyExSpan<byte>(new byte[] { 0, 1, 2, 3, 4, 5 }, 0, 5);
            ReadOnlyExSpan<byte> value = new ReadOnlyExSpan<byte>(new byte[] { 5 });
            int index = span.IndexOf(value);
            Assert.Equal(-1, index);
        }
    }
}
