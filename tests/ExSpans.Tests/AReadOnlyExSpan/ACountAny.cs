using System.Buffers;
using System.Collections.Generic;
using Xunit;

namespace Zyl.ExSpans.Tests.AReadOnlyExSpan {
    public static partial class ACountAny {
        [Fact]
        public static void CountAny_InvalidArgs_Throws() {
            AssertExtensions.Throws<ArgumentNullException>("values", () => ReadOnlyExSpan<char>.Empty.CountAny((SearchValues<char>)null));
        }

        [Fact]
        public static void CountAny_EmptySpan_Returns0() {
            ReadOnlyExSpan<char> span = default;
            Assert.Equal(0, span.CountAny(SearchValues.Create(['a', 'b', 'c'])));
            Assert.Equal(0, span.CountAny('a', 'b', 'c'));
            Assert.Equal(0, span.CountAny(['a', 'b', 'c'], EqualityComparer<char>.Default));
        }

        [Fact]
        public static void CountAny_EmptyValues_Returns0() {
            ReadOnlyExSpan<char> span = default;
            Assert.Equal(0, span.CountAny(SearchValues.Create(ReadOnlyExSpan<char>.Empty)));
            Assert.Equal(0, span.CountAny());
            Assert.Equal(0, span.CountAny([], EqualityComparer<char>.Default));
        }

        [Fact]
        public static void CountAny_CountMatchesExpected() {
            Assert.Equal(7, "abcdefgabcdefga".AsSpan().CountAny(SearchValues.Create(['a', 'b', 'c'])));
            Assert.Equal(7, "abcdefgabcdefga".AsSpan().CountAny('a', 'b', 'c'));
            Assert.Equal(7, "abcdefgabcdefga".AsSpan().CountAny(['a', 'b', 'c'], EqualityComparer<char>.Default));
            Assert.Equal(15, "abcdefgabcdefga".AsSpan().CountAny(['a', 'b', 'c'], EqualityComparer<char>.Create((x, y) => true, x => 0)));
        }
    }
}
