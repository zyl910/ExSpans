using System.Numerics;
using Xunit;

namespace Zyl.ExSpans.Tests.AExSpan {
    public static partial class AIndexer {
        [Fact]
        public static void IndexerWithIndexTest() {
            ReadOnlyExSpan<char> span = "Hello".AsExSpan();
            Assert.Equal('H', span[new Index(0, fromEnd: false)]);
            Assert.Equal('o', span[new Index(1, fromEnd: true)]);
            Assert.Equal(span[new Index(2, fromEnd: false)], span[new Index(span.Length - 2, fromEnd: true)]);

            Assert.Throws<IndexOutOfRangeException>(() => "Hello".AsExSpan()[new Index(0, fromEnd: true)]);

            ExSpan<char> span1 = new ExSpan<char>(new char[] { 'H', 'e', 'l', 'l', 'o' });
            Assert.Equal('e', span1[new Index(1, fromEnd: false)]);
            Assert.Equal('l', span1[new Index(2, fromEnd: true)]);
            Assert.Equal(span1[new Index(2, fromEnd: false)], span1[new Index(span.Length - 2, fromEnd: true)]);

            Assert.Throws<IndexOutOfRangeException>(() =>
                new ExSpan<char>(new char[] { 'H', 'e', 'l', 'l', 'o' })[new Index(0, fromEnd: true)]);
        }

        [Fact]
        public static void IndexerWithRangeTest() {
            ReadOnlyExSpan<char> span = "Hello".AsExSpan();
            ReadOnlyExSpan<char> sliced = span[new Range(new Index(1, fromEnd: false), new Index(1, fromEnd: true))];
            Assert.True(span.Slice(1, 3) == sliced);

            Assert.Throws<ArgumentOutOfRangeException>(() => { ReadOnlyExSpan<char> s = "Hello".AsExSpan()[new Range(new Index(1, fromEnd: true), new Index(1, fromEnd: false))]; });

            ExSpan<char> span1 = new ExSpan<char>(new char[] { 'H', 'e', 'l', 'l', 'o' });
            ExSpan<char> sliced1 = span1[new Range(new Index(2, fromEnd: false), new Index(1, fromEnd: true))];
            Assert.True(span1.Slice(2, 2) == sliced1);

            Assert.Throws<ArgumentOutOfRangeException>(() => { ExSpan<char> s = new ExSpan<char>(new char[] { 'H', 'i' })[new Range(new Index(0, fromEnd: true), new Index(1, fromEnd: false))]; });
        }

        [Fact]
        public static void SlicingUsingIndexAndRangeTest() {
            Range range;
            string s = "0123456789ABCDEF";
            ReadOnlyExSpan<char> roExSpan = s.AsExSpan();
            ExSpan<char> span = new ExSpan<char>(s.ToCharArray());

            for (int i = 0; i < span.Length; i++) {
                Assert.True(span.Slice(i) == span[i..]);
                Assert.True(span.Slice(span.Length - i - 1) == span[^(i + 1)..]);

                Assert.True(roExSpan.Slice(i) == roExSpan[i..]);
                Assert.True(roExSpan.Slice(roExSpan.Length - i - 1) == roExSpan[^(i + 1)..]);

                range = new Range(Index.FromStart(i), Index.FromEnd(0));
                Assert.True(span.Slice(i, span.Length - i) == span[range]);
                Assert.True(roExSpan.Slice(i, roExSpan.Length - i) == roExSpan[range]);
            }

            range = new Range(Index.FromStart(0), Index.FromStart(span.Length + 1));

            Assert.Throws<ArgumentOutOfRangeException>(delegate () { var spp = new ExSpan<char>(s.ToCharArray())[range]; });
            Assert.Throws<ArgumentOutOfRangeException>(delegate () { var spp = s.AsExSpan()[range]; });
        }
    }
}
