using Xunit;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Zyl.ExSpans.Tests.AExSpan {
    public static partial class AIndexOfSequence {
        [Fact]
        public static void IndexOfSequenceMatchAtStart_Byte() {
            ExSpan<byte> span = new ExSpan<byte>(new byte[] { 5, 1, 77, 2, 3, 77, 77, 4, 5, 77, 77, 77, 88, 6, 6, 77, 77, 88, 9 });
            ExSpan<byte> value = new ExSpan<byte>(new byte[] { 5, 1, 77 });
            TSize index = span.IndexOf(value);
            Assert.Equal(0, index);
        }

        [Fact]
        public static void IndexOfSequenceMultipleMatch_Byte() {
            ExSpan<byte> span = new ExSpan<byte>(new byte[] { 1, 2, 3, 1, 2, 3, 1, 2, 3 });
            ExSpan<byte> value = new ExSpan<byte>(new byte[] { 2, 3 });
            TSize index = span.IndexOf(value);
            Assert.Equal(1, index);
        }

        [Fact]
        public static void IndexOfSequenceRestart_Byte() {
            ExSpan<byte> span = new ExSpan<byte>(new byte[] { 0, 1, 77, 2, 3, 77, 77, 4, 5, 77, 77, 77, 88, 6, 6, 77, 77, 88, 9 });
            ExSpan<byte> value = new ExSpan<byte>(new byte[] { 77, 77, 88 });
            TSize index = span.IndexOf(value);
            Assert.Equal(10, index);
        }

        [Fact]
        public static void IndexOfSequenceNoMatch_Byte() {
            ExSpan<byte> span = new ExSpan<byte>(new byte[] { 0, 1, 77, 2, 3, 77, 77, 4, 5, 77, 77, 77, 88, 6, 6, 77, 77, 88, 9 });
            ExSpan<byte> value = new ExSpan<byte>(new byte[] { 77, 77, 88, 99 });
            TSize index = span.IndexOf(value);
            Assert.Equal(-1, index);
        }

        [Fact]
        public static void IndexOfSequenceNotEvenAHeadMatch_Byte() {
            ExSpan<byte> span = new ExSpan<byte>(new byte[] { 0, 1, 77, 2, 3, 77, 77, 4, 5, 77, 77, 77, 88, 6, 6, 77, 77, 88, 9 });
            ExSpan<byte> value = new ExSpan<byte>(new byte[] { 100, 77, 88, 99 });
            TSize index = span.IndexOf(value);
            Assert.Equal(-1, index);
        }

        [Fact]
        public static void IndexOfSequenceMatchAtVeryEnd_Byte() {
            ExSpan<byte> span = new ExSpan<byte>(new byte[] { 0, 1, 2, 3, 4, 5 });
            ExSpan<byte> value = new ExSpan<byte>(new byte[] { 3, 4, 5 });
            TSize index = span.IndexOf(value);
            Assert.Equal(3, index);
        }

        [Fact]
        public static void IndexOfSequenceJustPastVeryEnd_Byte() {
            ExSpan<byte> span = new ExSpan<byte>(new byte[] { 0, 1, 2, 3, 4, 5 }, 0, 5);
            ExSpan<byte> value = new ExSpan<byte>(new byte[] { 3, 4, 5 });
            TSize index = span.IndexOf(value);
            Assert.Equal(-1, index);
        }

        [Fact]
        public static void IndexOfSequenceZeroLengthValue_Byte() {
            // A zero-length value is always "found" at the start of the span.
            ExSpan<byte> span = new ExSpan<byte>(new byte[] { 0, 1, 77, 2, 3, 77, 77, 4, 5, 77, 77, 77, 88, 6, 6, 77, 77, 88, 9 });
            ExSpan<byte> value = new ExSpan<byte>(ArrayHelper.Empty<byte>());
            TSize index = span.IndexOf(value);
            Assert.Equal(0, index);
        }

        [Fact]
        public static void IndexOfSequenceZeroLengthExSpan_Byte() {
            ExSpan<byte> span = new ExSpan<byte>(ArrayHelper.Empty<byte>());
            ExSpan<byte> value = new ExSpan<byte>(new byte[] { 1, 2, 3 });
            TSize index = span.IndexOf(value);
            Assert.Equal(-1, index);
        }

        [Fact]
        public static void IndexOfSequenceLengthOneValue_Byte() {
            ExSpan<byte> span = new ExSpan<byte>(new byte[] { 0, 1, 2, 3, 4, 5 });
            ExSpan<byte> value = new ExSpan<byte>(new byte[] { 2 });
            TSize index = span.IndexOf(value);
            Assert.Equal(2, index);
        }

        [Fact]
        public static void IndexOfSequenceLengthOneValueAtVeryEnd_Byte() {
            ExSpan<byte> span = new ExSpan<byte>(new byte[] { 0, 1, 2, 3, 4, 5 });
            ExSpan<byte> value = new ExSpan<byte>(new byte[] { 5 });
            TSize index = span.IndexOf(value);
            Assert.Equal(5, index);
        }

        [Fact]
        public static void IndexOfSequenceLengthOneValueJustPasttVeryEnd_Byte() {
            ExSpan<byte> span = new ExSpan<byte>(new byte[] { 0, 1, 2, 3, 4, 5 }, 0, 5);
            ExSpan<byte> value = new ExSpan<byte>(new byte[] { 5 });
            TSize index = span.IndexOf(value);
            Assert.Equal(-1, index);
        }

        public static IEnumerable<object[]> IndexOfSubSeqData_Byte() {
            // searchSpace, value, expected IndexOf value, expected LastIndexOf value
            yield return new object[] { new byte[] { 0, 0, 0, 0, 0 }, new byte[] { 0, 0, 0 }, 0, 2 };
            yield return new object[] { new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, new byte[] { 0, 71, 0 }, -1, -1 };
            yield return new object[] { new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, new byte[] { 0, 0, 0 }, 0, 7 };
            yield return new object[] { new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 71, 0, 1, 0, 0, 0 }, new byte[] { 0, 71, 0, 1, 0 }, 10, 10 };
            yield return new object[] { new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 71, 0, 1, 0, 0, 0 }, new byte[] { 0, 0, 0, 1, 0 }, -1, -1 };
            yield return new object[] { new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 71, 0, 1, 0, 0, 0, 0 }, new byte[] { 0, 0, 0, 1, 0 }, -1, -1 };
            yield return new object[] { new byte[] { 0, 0, 0, 0, 0, 71, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, new byte[] { 0, 0, 0, 1, 0 }, -1, -1 };
            yield return new object[] { new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 71, 0, 1, 0, 0, 0 }, new byte[] { 0, 71, 1, 0, 0 }, -1, -1 };
            yield return new object[] { new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 71, 0, 1, 0, 0, 0 }, new byte[] { 0, 0, 1, 0, 0 }, -1, -1 };
            yield return new object[] { new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 71, 0, 1, 0, 0, 0, 0 }, new byte[] { 0, 0, 1, 0, 0 }, -1, -1 };
            yield return new object[] { new byte[] { 0, 0, 0, 0, 0, 71, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, new byte[] { 0, 0, 1, 0, 0 }, -1, -1 };
            yield return new object[] { new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 71, 0, 1, 0, 0, 0 }, new byte[] { 0, 1, 0, 0, 0 }, 12, 12 };
            yield return new object[] { new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 71, 0, 1, 0, 0, 0, 0 }, new byte[] { 0, 1, 0, 0, 0 }, 11, 11 };
            yield return new object[] { new byte[] { 0, 0, 0, 0, 0, 71, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, new byte[] { 0, 1, 0, 0, 0 }, 6, 6 };
            yield return new object[] { new byte[] { 0, 0, 0, 0, 71, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 71, 0, 1 }, new byte[] { 0, 0, 0, 1, 0 }, -1, -1 };
            yield return new object[] { new byte[] { 0, 0, 0, 0, 71, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 71, 0, 1 }, new byte[] { 0, 0, 0, 1, 0 }, -1, -1 };
            yield return new object[] { new byte[] { 0, 0, 0, 0, 71, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 71, 0, 1 }, new byte[] { 0, 0, 0, 0, 1, 0 }, -1, -1 };
            yield return new object[] { new byte[] { 0, 0, 0, 0, 71, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 71, 0, 1 }, new byte[] { 0, 0, 0, 0, 0, 1, 0 }, -1, -1 };
            yield return new object[] { new byte[] { 0, 0, 0, 0, 71, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 71, 0, 1 }, new byte[] { 0, 0, 0, 0, 0, 1, 0 }, -1, -1 };
            yield return new object[] { new byte[] { 0, 0, 0, 0, 71, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 71, 0, 1 }, new byte[] { 0, 0, 0, 0, 0, 1, 0 }, -1, -1 };
            yield return new object[] { new byte[] { 0, 0, 0, 0, 71, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 71, 0, 1 }, new byte[] { 0, 1, 0, 0, 1, 0, 0 }, -1, -1 };
            yield return new object[] { new byte[] { 0, 0, 0, 0, 71, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 71, 0, 1 }, new byte[] { 0, 1, 0, 0, 0, 0, 0 }, 5, 5 };
            yield return new object[] { new byte[] { 0, 0, 0, 0, 71, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 71, 0, 1 }, new byte[] { 0, 1, 0, 0, 0, 0, 0 }, 5, 5 };
            yield return new object[] { new byte[] { 0, 0, 0, 0, 71, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 71, 0, 1 }, new byte[] { 0, 1, 0, 0, 0, 0, 0 }, 5, 5 };
            yield return new object[] { new byte[] { 0, 0, 0, 0, 71, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 71, 0, 1, 0, 0, 0, 0, 1, 1, 0, 2, 0, 1, 1, 0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 0, 1, 0 }, new byte[] { 0, 0, 0 }, 0, 44 };
            yield return new object[] { new byte[] { 0, 0, 0, 0, 71, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 71, 0, 1, 0, 0, 0, 0, 1, 1, 0, 2, 0, 1, 1, 0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 0, 1, 0 }, new byte[] { 0, 0, 0, 0 }, 0, 43 };
            yield return new object[] { new byte[] { 0, 0, 0, 0, 71, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 71, 0, 1, 0, 0, 0, 0, 1, 1, 0, 2, 0, 1, 1, 0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 0, 1, 0 }, new byte[] { 0, 0, 0, 0, 0 }, 7, 42 };
            yield return new object[] { new byte[] { 0, 0, 0, 0, 71, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 71, 0, 1, 0, 0, 0, 0, 1, 1, 0, 2, 0, 1, 1, 0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 0, 1, 0 }, new byte[] { 0, 0, 0, 0, 0, 0 }, 7, 41 };
            yield return new object[] { new byte[] { 0, 0, 0, 0, 71, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 71, 0, 1, 0, 0, 0, 0, 1, 1, 0, 2, 0, 1, 1, 0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 0, 1, 0 }, new byte[] { 0, 0, 0, 0, 0, 0, 0 }, 7, 11 };
            yield return new object[] { new byte[] { 0, 0, 0, 0, 71, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 71, 0, 1, 0, 0, 0, 0, 1, 1, 0, 2, 0, 1, 1, 0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 0, 1, 0 }, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 }, 7, 10 };
            yield return new object[] { new byte[] { 0, 0, 0, 0, 71, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 71, 0, 1, 0, 0, 0, 0, 1, 1, 0, 2, 0, 1, 1, 0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 0, 1, 0 }, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 7, 9 };
            yield return new object[] { new byte[] { 0, 0, 0, 0, 71, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 71, 0, 1, 0, 0, 0, 0, 1, 1, 0, 2, 0, 1, 1, 0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 0, 1, 0 }, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 7, 7 };
            yield return new object[] { new byte[] { 0, 0, 0, 0, 71, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 71, 0, 1, 0, 0, 0, 0, 1, 1, 0, 2, 0, 1, 1, 0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 0, 1, 0 }, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, -1, -1 };
            yield return new object[] { new byte[] { 0, 0, 0, 0, 71, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 71, 0, 1, 0, 0, 0, 0, 1, 1, 0, 2, 0, 1, 1, 0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 0, 1, 0 }, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, -1, -1 };
            yield return new object[] { new byte[] { 0, 0, 0, 0, 71, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 71, 0, 1, 0, 0, 0, 0, 1, 1, 0, 2, 0, 1, 1, 0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 0, 1, 0 }, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, -1, -1 };
            yield return new object[] { new byte[] { 0, 0, 0, 0, 71, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 71, 0, 1, 0, 0, 0, 0, 1, 1, 0, 2, 0, 1, 1, 0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 0, 1, 0 }, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, -1, -1 };
            yield return new object[] { new byte[] { 0, 0, 0, 0, 71, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 71, 0, 1, 0, 0, 0, 0, 1, 1, 0, 2, 0, 1, 1, 0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 0, 1, 0 }, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, -1, -1 };
            yield return new object[] { new byte[] { 0, 0, 0, 0, 71, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 71, 0, 1, 0, 0, 0, 0, 1, 1, 0, 2, 0, 1, 1, 0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 0, 1, 0 }, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, -1, -1 };
            yield return new object[] { new byte[] { 0, 0, 0, 0, 71, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 71, 0, 1, 0, 0, 0, 0, 1, 1, 0, 2, 0, 1, 1, 0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 0, 1, 0 }, new byte[] { 0, 1, 0, 0 }, 5, 48 };
            yield return new object[] { new byte[] { 0, 0, 0, 0, 71, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 71, 0, 1, 0, 0, 0, 0, 1, 1, 0, 2, 0, 1, 1, 0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 0, 1, 0 }, new byte[] { 0, 0, 0, 1, 0 }, 44, 44 };
            yield return new object[] { new byte[] { 0, 0, 0, 0, 71, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 71, 0, 1, 0, 0, 0, 0, 1, 1, 0, 2, 0, 1, 1, 0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 0, 1, 0 }, new byte[] { 0, 1, 0, 0, 0, 0 }, 5, 19 };
            yield return new object[] { new byte[] { 0, 0, 0, 0, 71, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 71, 0, 1, 0, 0, 0, 0, 1, 1, 0, 2, 0, 1, 1, 0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 0, 1, 0 }, new byte[] { 0, 1, 0, 0, 0, 1, 0, 0 }, -1, -1 };
            yield return new object[] { new byte[] { 0, 0, 0, 0, 71, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 71, 0, 1, 0, 0, 0, 0, 1, 1, 0, 2, 0, 1, 1, 0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 0, 1, 0 }, new byte[] { 0, 0, 0, 0, 0, 0, 0 }, 7, 11 };
            yield return new object[] { new byte[] { 0, 0, 0, 0, 71, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 71, 0, 1, 0, 0, 0, 0, 1, 1, 0, 2, 0, 1, 1, 0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 0, 1, 0 }, new byte[] { 0, 0, 1, 0, 0, 1, 0, 0, 0, 0 }, -1, -1 };
            yield return new object[] { new byte[] { 0, 0, 0, 0, 71, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 71, 0, 1, 0, 0, 0, 0, 1, 1, 0, 2, 0, 1, 1, 0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 0, 1, 0 }, new byte[] { 0, 0, 0, 0, 1, 0, 0, 0, 0, 0 }, -1, -1 };
            yield return new object[] { new byte[] { 0, 0, 0, 0, 71, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 71, 0, 1, 0, 0, 0, 0, 1, 1, 0, 2, 0, 1, 1, 0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 0, 1, 0 }, new byte[] { 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0 }, -1, -1 };
            yield return new object[] { new byte[] { 0, 0, 0, 0, 71, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 71, 0, 1, 0, 0, 0, 0, 1, 1, 0, 2, 0, 1, 1, 0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 0, 1, 0 }, new byte[] { 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, -1, -1 };
            yield return new object[] { new byte[] { 0, 0, 0, 0, 71, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 71, 0, 1, 0, 0, 0, 0, 1, 1, 0, 2, 0, 1, 1, 0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 0, 1, 0 }, new byte[] { 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0 }, -1, -1 };
            yield return new object[] { new byte[] { 0, 0, 0, 0, 71, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 71, 0, 1, 0, 0, 0, 0, 1, 1, 0, 2, 0, 1, 1, 0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 0, 1, 0 }, new byte[] { 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0 }, -1, -1 };
            yield return new object[] { new byte[] { 0, 0, 0, 0, 71, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 71, 0, 1, 0, 0, 0, 0, 1, 1, 0, 2, 0, 1, 1, 0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 0, 1, 0 }, new byte[] { 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, -1, -1 };
            yield return new object[] { new byte[] { 0, 0, 0, 0, 71, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 71, 0, 1, 0, 0, 0, 0, 1, 1, 0, 2, 0, 1, 1, 0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 0, 1, 0 }, new byte[] { 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0 }, -1, -1 };
            yield return new object[] { new byte[] { 159, 133, 159, 133, 159, 133, 159, 133, 159, 133, 159, 133, 159, 133, 159, 133, 159, 133, 159, 133, 159, 133, 159, 133, 159, 133, 159, 133 }, new byte[] { 159, 133, 159, 133, 159, 133 }, 0, 22 };
            yield return new object[] { new byte[] { 159, 133, 159, 133, 159, 133, 159, 133, 159, 133, 159, 133, 159, 133, 159, 133, 159, 133, 159, 133, 159, 133, 159, 133, 159, 133, 159, 133, 159, 133, 159, 133, 159, 133, 159, 133, 159, 133, 159, 133, 159, 133, 159, 133, 159, 133, 159, 133, 159, 133, 159, 133, 159, 133, 159, 133 }, new byte[] { 159, 133, 255, 159, 133 }, -1, -1 };
            yield return new object[] { new byte[] { 159, 133, 159, 133, 159, 133, 159, 133, 159, 133, 159, 127, 159, 127, 159, 127, 159, 127, 159, 127, 159, 127, 159, 127, 159, 127, 159, 127, 159, 127, 159, 127, 159, 127, 159, 127, 159, 127, 159, 127, 160, 86, 160, 86, 160, 86, 160, 86, 160, 80 }, new byte[] { 160, 86, 160, 86, 160, 86, 160, 86, 160, 80 }, 40, 40 };
            yield return new object[] { new byte[] { 159, 133, 159, 133, 159, 133, 159, 133, 159, 133, 159, 127, 159, 127, 159, 127, 159, 127, 159, 127, 159, 127, 159, 127, 159, 127, 159, 127, 159, 127, 159, 127, 159, 127, 159, 127, 159, 127, 159, 127, 160, 86, 160, 86, 160, 86, 160, 86, 160, 80, 160, 80, 160, 80, 160, 80, 160, 80, 160, 80, 160, 80, 160, 86, 160, 86, 160, 86, 160, 86 }, new byte[] { 160, 86, 160, 86, 160, 86, 160, 86 }, 40, 62 };
            yield return new object[] { new byte[] { 159, 133, 159, 133, 159, 133, 159, 133, 159, 133, 159, 133, 159, 133, 159, 133, 159, 133, 159, 133, 159, 133, 159, 133, 159, 133, 159, 133, 159, 133, 159, 133, 159, 133, 159, 133, 159, 133, 159, 133, 159, 133, 159, 133, 159, 133, 159, 133, 159, 133, 159, 133, 159, 133, 159, 133 }, new byte[] { 0, 0, 0, 1 }, -1, -1 };
            yield return new object[] { new byte[] { 255, 160, 82, 159, 134, 159, 127, 255, 159, 141, 160, 85, 160, 82, 160, 88, 255, 159, 141, 159, 127, 159, 134, 255, 160, 88, 160, 85, 160, 82, 159, 141, 159, 127, 159, 134, 160, 88, 160, 85, 160, 82, 159, 141, 255, 159, 127, 159, 134, 160, 88, 160, 85, 160, 82, 159, 141, 159, 127, 159, 134, 160, 88, 159, 141, 160, 85, 255, 160, 82, 159, 134, 159, 141, 159, 134, 160, 85, 160, 82, 159, 141, 159, 127, 159, 134, 160, 82, 159, 141, 160, 85, 159, 134, 255, 160, 88, 159, 127, 160, 82, 160, 85, 159, 134, 255, 159, 141, 159, 127, 159, 134, 160, 85, 159, 141 }, new byte[] { 255, 159, 141, 159, 127, 159, 134, 255 }, 16, 16 };
            yield return new object[] { new byte[] { 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 49, 50 }, new byte[] { 49, 49 }, 29, 29 };
            yield return new object[] { new byte[] { 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 49 }, new byte[] { 49, 49 }, 29, 29 };
            yield return new object[] { new byte[] { 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 49, 50 }, new byte[] { 49, 49 }, 29, 29 };
            yield return new object[] { new byte[] { 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 50 }, new byte[] { 49, 49 }, -1, -1 };
            yield return new object[] { new byte[] { 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 49, 49 }, new byte[] { 49, 49, 49 }, 29, 29 };
            yield return new object[] { new byte[] { 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 49, 49, 50 }, new byte[] { 49, 49, 49 }, 29, 29 };
            yield return new object[] { new byte[] { 49, 49, 49, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 50 }, new byte[] { 49, 49, 49 }, 0, 1 };
            yield return new object[] { new byte[] { 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 49, 50 }, new byte[] { 48, 48 }, -1, -1 };
            yield return new object[] { new byte[] { 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 49 }, new byte[] { 48, 48 }, -1, -1 };
            yield return new object[] { new byte[] { 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 49, 50 }, new byte[] { 48, 48 }, -1, -1 };
            yield return new object[] { new byte[] { 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 50 }, new byte[] { 48, 48 }, -1, -1 };
            yield return new object[] { new byte[] { 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 49, 49 }, new byte[] { 48, 48, 48 }, -1, -1 };
            yield return new object[] { new byte[] { 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 49, 49, 50 }, new byte[] { 48, 48, 48 }, -1, -1 };
            yield return new object[] { new byte[] { 49, 49, 49, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 50 }, new byte[] { 48, 48, 48 }, -1, -1 };
            yield return new object[] { new byte[] { 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 49, 50 }, new byte[] { 48, 49, 48, 48 }, -1, -1 };
            yield return new object[] { new byte[] { 49, 48, 49, 49, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 50 }, new byte[] { 49, 48, 49, 49 }, 0, 0 };
            yield return new object[] { new byte[] { 49, 48, 49, 49, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 48, 49, 50 }, new byte[] { 160, 80, 48, 160, 80, 160, 80 }, -1, -1 };
            yield return new object[] { new byte[] { 49, 48, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49 }, new byte[] { 49, 48, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49 }, 0, 0 };
            yield return new object[] { new byte[] { 49, 48, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 48, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49 }, new byte[] { 49, 48, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49 }, 0, 15 };
            yield return new object[] { new byte[] { 49, 48, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 48, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49 }, new byte[] { 49, 48, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49 }, 0, 17 };
            yield return new object[] { new byte[] { 49, 48, 49, 49, 49, 49, 49, 49, 71, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 48, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 48, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49 }, new byte[] { 49, 48, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49 }, 18, 32 };
            yield return new object[] { new byte[] { 49, 48, 49, 49, 49, 49, 49, 49, 71, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 48, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 48, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49 }, new byte[] { 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49 }, 20, 20 };
            yield return new object[] { new byte[] { 49, 48, 49, 49, 49, 49, 49, 49, 71, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 48, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 48, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49 }, new byte[] { 49, 48, 49, 49, 49, 49, 49, 49, 71, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 48, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 48, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49 }, 0, 0 };
            yield return new object[] { new byte[] { 49, 48, 49, 49, 49, 49, 49, 49, 71, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 48, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 48, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49 }, new byte[] { 49, 48, 49, 49, 49, 49, 49, 49, 71, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 48, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 48, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49 }, 0, 0 };
            yield return new object[] { new byte[] { 49, 48, 49, 49, 49, 49, 49, 49, 71, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 48, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 48, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49 }, new byte[] { 49, 48, 49, 49, 49, 49, 49, 49, 71, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 48, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 48, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49 }, -1, -1 };
            yield return new object[] { new byte[] { 49, 48, 49, 49, 49, 49, 49, 49, 71, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 48, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 48, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49 }, new byte[] { 49, 48, 49, 49, 49, 49, 49, 49, 71, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 48, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 48, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49 }, 0, 0 };
            yield return new object[] { new byte[] { 49, 48, 49, 49, 49, 49, 49, 49, 71, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 48, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 48, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49 }, new byte[] { 49, 48, 49, 49, 49, 49, 49, 49, 71, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 48, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 48, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49 }, 0, 0 };
            yield return new object[] { new byte[] { 71, 71, 71, 71, 71, 71, 71, 71, 71, 71, 71, 71, 71, 71, 49, 48, 49, 49, 49, 49, 49, 49, 71, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 48, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 48, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 71, 71, 71, 71, 71, 71, 71, 71, 71, 71, 71, 71, 71, 71, 71 }, new byte[] { 71, 71, 71, 71, 71, 71, 71, 71, 71, 71, 71, 71, 71, 71, 71 }, 60, 60 };
            yield return new object[] { new byte[] { 71, 71, 71, 71, 71, 71, 71, 71, 71, 71, 71, 71, 71, 71, 71, 49, 48, 49, 49, 49, 49, 49, 49, 71, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 48, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 48, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 71, 71, 71, 71, 71, 71, 71, 71, 71, 71, 71, 71, 71, 71 }, new byte[] { 71, 71, 71, 71, 71, 71, 71, 71, 71, 71, 71, 71, 71, 71, 71 }, 0, 0 };
        }

        [Theory]
        [MemberData(nameof(IndexOfSubSeqData_Byte))]
        public static void ValueStartsAndEndsWithTheSameBytes(byte[] searchSpace, byte[] value, int expectedIndexOfValue, int expectedLastIndexOfValue) {
            Assert.Equal(expectedIndexOfValue, searchSpace.AsExSpan().IndexOf(value));
            Assert.Equal(expectedLastIndexOfValue, searchSpace.AsExSpan().LastIndexOf(value));
        }
    }
}
