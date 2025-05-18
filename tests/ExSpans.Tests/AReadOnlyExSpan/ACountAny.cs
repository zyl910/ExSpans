#if NET9_0_OR_GREATER
#define PARAMS_COLLECTIONS // C# 13 - params collections. https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-13#params-collections
#endif // NET9_0_OR_GREATER

using System.Buffers;
using System.Collections.Generic;
using Xunit;

namespace Zyl.ExSpans.Tests.AReadOnlyExSpan {
    public static partial class ACountAny {
#if NET8_0_OR_GREATER && TODO // [TODO why] SearchValues methods is internal
        [Fact]
        public static void CountAny_InvalidArgs_Throws() {
            AssertExtensions.Throws<ArgumentNullException>("values", () => ReadOnlyExSpan<char>.Empty.CountAny((SearchValues<char>)null));
        }
#endif // NET8_0_OR_GREATER

        [Fact]
        public static void CountAny_EmptySpan_Returns0() {
            ReadOnlyExSpan<char> span = default;
#if NET8_0_OR_GREATER && TODO // [TODO why] SearchValues methods is internal
            Assert.Equal(0, span.CountAny(SearchValues.Create(['a', 'b', 'c'])));
#endif // NET8_0_OR_GREATER
#if PARAMS_COLLECTIONS
            Assert.Equal(0, span.CountAny('a', 'b', 'c'));
            //Assert.Equal(0, span.CountAny(['a', 'b', 'c'], EqualityComparer<char>.Default)); // CS9174	Cannot initialize type 'ReadOnlyExSpan<char>' with a collection expression because the type is not constructible.
#else
            Assert.Equal(0, span.CountAny((ReadOnlySpan<char>)['a', 'b', 'c']));
#endif // PARAMS_COLLECTIONS
            Assert.Equal(0, span.CountAny((ReadOnlySpan<char>)['a', 'b', 'c'], EqualityComparer<char>.Default));
        }

        [Fact]
        public static void CountAny_EmptyValues_Returns0() {
            ReadOnlyExSpan<char> span = default;
#if NET8_0_OR_GREATER && TODO // [TODO why] SearchValues methods is internal
            Assert.Equal(0, span.CountAny(SearchValues.Create(ReadOnlyExSpan<char>.Empty)));
#endif // NET8_0_OR_GREATER
#if PARAMS_COLLECTIONS
            Assert.Equal(0, span.CountAny());
            //Assert.Equal(0, span.CountAny([], EqualityComparer<char>.Default));
#else
            Assert.Equal(0, span.CountAny(default));
#endif // PARAMS_COLLECTIONS
            Assert.Equal(0, span.CountAny((ReadOnlySpan<char>)[], EqualityComparer<char>.Default));
        }

        [Fact]
        public static void CountAny_CountMatchesExpected() {
#if NET8_0_OR_GREATER && TODO // [TODO why] SearchValues methods is internal
            Assert.Equal(7, "abcdefgabcdefga".AsSpan().CountAny(SearchValues.Create(['a', 'b', 'c'])));
#endif // NET8_0_OR_GREATER
#if PARAMS_COLLECTIONS
            Assert.Equal(7, "abcdefgabcdefga".AsSpan().AsReadOnlyExSpan().CountAny('a', 'b', 'c'));
            //Assert.Equal(7, "abcdefgabcdefga".AsExSpan().CountAny(['a', 'b', 'c'], EqualityComparer<char>.Default));
#else
            Assert.Equal(7, "abcdefgabcdefga".AsSpan().AsReadOnlyExSpan().CountAny((ReadOnlySpan<char>)['a', 'b', 'c']));
#endif // PARAMS_COLLECTIONS
            Assert.Equal(7, "abcdefgabcdefga".AsExSpan().CountAny((ReadOnlySpan<char>)['a', 'b', 'c'], EqualityComparer<char>.Default));
#if NET8_0_OR_GREATER
#if PARAMS_COLLECTIONS
            //Assert.Equal(15, "abcdefgabcdefga".AsExSpan().CountAny(['a', 'b', 'c'], EqualityComparer<char>.Create((x, y) => true, x => 0)));
#else
#endif // PARAMS_COLLECTIONS
            Assert.Equal(15, "abcdefgabcdefga".AsExSpan().CountAny((ReadOnlySpan<char>)['a', 'b', 'c'], EqualityComparer<char>.Create((x, y) => true, x => 0)));
#endif // NET8_0_OR_GREATER
        }
    }
}
