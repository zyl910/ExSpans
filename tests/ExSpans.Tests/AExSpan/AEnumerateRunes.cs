using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Zyl.ExSpans.Tests.AExSpan {
    public static partial class AEnumerateRunes {
        [Fact]
        public static void EnumerateRunesEmpty() {
            Assert.False(MemoryExtensions.EnumerateRunes(ReadOnlyExSpan<char>.Empty).GetEnumerator().MoveNext());
            Assert.False(MemoryExtensions.EnumerateRunes(ExSpan<char>.Empty).GetEnumerator().MoveNext());
        }

        [Theory]
        [InlineData(new char[0], new int[0])] // empty
        [InlineData(new char[] { 'x', 'y', 'z' }, new int[] { 'x', 'y', 'z' })]
        [InlineData(new char[] { 'x', '\uD86D', '\uDF54', 'y' }, new int[] { 'x', 0x2B754, 'y' })] // valid surrogate pair
        [InlineData(new char[] { 'x', '\uD86D', 'y' }, new int[] { 'x', 0xFFFD, 'y' })] // standalone high surrogate
        [InlineData(new char[] { 'x', '\uDF54', 'y' }, new int[] { 'x', 0xFFFD, 'y' })] // standalone low surrogate
        [InlineData(new char[] { 'x', '\uD86D' }, new int[] { 'x', 0xFFFD })] // standalone high surrogate at end of string
        [InlineData(new char[] { 'x', '\uDF54' }, new int[] { 'x', 0xFFFD })] // standalone low surrogate at end of string
        [InlineData(new char[] { 'x', '\uD86D', '\uD86D', 'y' }, new int[] { 'x', 0xFFFD, 0xFFFD, 'y' })] // two high surrogates should be two replacement chars
        [InlineData(new char[] { 'x', '\uFFFD', 'y' }, new int[] { 'x', 0xFFFD, 'y' })] // literal U+FFFD
        public static void EnumerateRunes_Battery(char[] chars, int[] expected) {
            // Test data is smuggled as char[] instead of straight-up string since the test framework
            // doesn't like invalid UTF-16 literals.

            // first, test ExSpan<char>

            List<int> enumeratedValues = new List<int>();
            foreach (Rune rune in ((ExSpan<char>)chars).EnumerateRunes()) {
                enumeratedValues.Add(rune.Value);
            }
            Assert.Equal(expected, enumeratedValues.ToArray());

            // next, ROS<char>

            enumeratedValues.Clear();
            foreach (Rune rune in ((ReadOnlyExSpan<char>)chars).EnumerateRunes()) {
                enumeratedValues.Add(rune.Value);
            }
            Assert.Equal(expected, enumeratedValues.ToArray());
        }

        [Fact]
        public static void EnumerateRunes_DoesNotReadPastEndOfExSpan() {
            // As an optimization, reading scalars from a string *may* read past the end of the string
            // to the terminating null. This optimization is invalid for arbitrary spans, so this test
            // ensures that we're not performing this optimization here.

            ReadOnlyExSpan<char> span = "xy\U0002B754z".AsExSpan(1, 2); // well-formed string, but span splits surrogate pair

            List<int> enumeratedValues = new List<int>();
            foreach (Rune rune in span.EnumerateRunes()) {
                enumeratedValues.Add(rune.Value);
            }
            Assert.Equal(new int[] { 'y', '\uFFFD' }, enumeratedValues.ToArray());
        }
    }
}
