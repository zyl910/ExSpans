using System.Buffers;
using System.Linq;
using Xunit;

namespace Zyl.ExSpans.Tests.AExSpan {
#if NET8_0_OR_GREATER && TODO // [TODO why] SearchValues methods is internal
    public class ReplaceAnyTests {
        [Fact]
        public void ReplaceAny_InPlace_InvalidArgs() {
            AssertExtensions.Throws<ArgumentNullException>("values", () => ExSpan<byte>.Empty.ReplaceAny<byte>(null, (byte)42));
        }

        [Fact]
        public void ReplaceAny_InPlace_EmptyInput_Nop() {
            ExSpan<byte> span = [42, 42, 42];
            span.Slice(0, 0).ReplaceAny<byte>(SearchValues.Create(42), 0);
            span.Slice(0, 0).ReplaceAnyExcept<byte>(SearchValues.Create(42), 0);
            Assert.Equal([42, 42, 42], span.ToArray());
        }

        [Fact]
        public void ReplaceAny_InPlace_NoMatch_Nop() {
            ExSpan<byte> span = [1, 2, 3];

            span.ReplaceAny<byte>(SearchValues.Create(42), 0);
            Assert.Equal([1, 2, 3], span.ToArray());

            span.ReplaceAnyExcept<byte>(SearchValues.Create(1, 2, 3), 0);
            Assert.Equal([1, 2, 3], span.ToArray());
        }

        [Fact]
        public void ReplaceAny_InPlace_AllMatchesReplaced() {
            SearchValues<char> sv = SearchValues.Create('a', 'c', '6', 'i', '1');
            const string Input = "abcdefghijklmnopqrstuvwxyz0123456789abcdefghijklmnopqrstuvwxyz0123456789";
            ExSpan<char> span = Input.ToCharArray();

            span.ReplaceAny<char>(SearchValues.Create('a', 'c', '6', 'i', '1'), '_');
            Assert.Equal(string.Concat(Input.Select(c => sv.Contains(c) ? '_' : c)), span.ToString());

            span.ReplaceAnyExcept<char>(SearchValues.Create('0', '1', '2', '3', '4', '5', '6', '7'), '_');
            Assert.Equal("__________________________0_2345_7____________________________0_2345_7__", span.ToString());
        }

        [Fact]
        public void ReplaceAny_SrcDest_InvalidArgs() {
            AssertExtensions.Throws<ArgumentNullException>("values", () => ReadOnlyExSpan<byte>.Empty.ReplaceAny<byte>(ExSpan<byte>.Empty, null, (byte)42));
            AssertExtensions.Throws<ArgumentException>("destination", () => new char[] { '1', '2', '3' }.ReplaceAny(new char[2], SearchValues.Create('b'), 'a'));

            char[] arr = new char[10];
            Assert.Throws<ArgumentException>(() => arr.AsExSpan(0, 9).ReplaceAny(arr.AsExSpan(1, 9), SearchValues.Create('b'), 'a'));
            arr.AsExSpan(4, 2).ReplaceAny(arr.AsExSpan(0, 4), SearchValues.Create('b'), 'a');
            arr.AsExSpan(0, 1).ReplaceAny(arr.AsExSpan(0, 4), SearchValues.Create('b'), 'a');
        }

        [Fact]
        public void ReplaceAny_SrcDest_EmptyInput_Nop() {
            ReadOnlyExSpan<byte> source = [42, 42, 42];
            ExSpan<byte> dest = [1, 2, 3];

            source.Slice(0, 0).ReplaceAny<byte>(dest, SearchValues.Create(42), 0);
            Assert.Equal([1, 2, 3], dest.ToArray());

            dest = [4, 5, 6];
            source.Slice(0, 0).ReplaceAnyExcept<byte>(dest, SearchValues.Create(42), 0);
            Assert.Equal([4, 5, 6], dest.ToArray());
        }

        [Fact]
        public void ReplaceAny_SrcDest_NoMatch_Nop() {
            ReadOnlyExSpan<byte> source = [1, 2, 3];
            ExSpan<byte> dest = new byte[source.Length];

            source.ReplaceAny<byte>(dest, SearchValues.Create(42), 0);
            Assert.Equal([1, 2, 3], dest.ToArray());

            dest.Clear();
            source.ReplaceAnyExcept<byte>(dest, SearchValues.Create(1, 2, 3), 0);
            Assert.Equal([1, 2, 3], dest.ToArray());
        }

        [Fact]
        public void ReplaceAny_SrcDest_AllMatchesReplaced() {
            SearchValues<char> sv = SearchValues.Create('a', 'c', '6', 'i', '1');
            const string Input = "abcdefghijklmnopqrstuvwxyz0123456789abcdefghijklmnopqrstuvwxyz0123456789";
            ReadOnlyExSpan<char> source = Input.ToCharArray();
            ExSpan<char> dest = new char[source.Length];

            source.ReplaceAny(dest, SearchValues.Create('a', 'c', '6', 'i', '1'), '_');
            Assert.Equal(string.Concat(Input.Select(c => sv.Contains(c) ? '_' : c)), dest.ToString());

            dest.Clear();
            source.ReplaceAnyExcept(dest, SearchValues.Create('0', '1', '2', '3', '4', '5', '6', '7'), '_');
            Assert.Equal("__________________________01234567____________________________01234567__", dest.ToString());
        }

        [Fact]
        public void ReplaceAny_SrcDest_OnlyWritesToRelevantPortionOfDestination() {
            SearchValues<char> sv = SearchValues.Create('a');
            ReadOnlyExSpan<char> source = ['a', 'b', 'c'];
            ExSpan<char> dest = ['1', '2', '3', '4', '5', '6'];

            source.ReplaceAny(dest, SearchValues.Create('a', 'c'), '_');
            Assert.Equal("_b_456", dest.ToString());

            source.ReplaceAnyExcept(dest.Slice(1), SearchValues.Create('b'), 's');
            Assert.Equal("_sbs56", dest.ToString());
        }
    }
#endif // NET8_0_OR_GREATER
}
