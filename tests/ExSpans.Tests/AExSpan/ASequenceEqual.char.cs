using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Zyl.ExSpans.Tests.AExSpan {
    partial class ASequenceEqual {

        [Fact]
        public static void ZeroLengthSequenceEqual_Char() {
            char[] a = new char[3];

            ExSpan<char> first = new ExSpan<char>(a, (TSize)1, (TSize)0);
            ExSpan<char> second = new ExSpan<char>(a, (TSize)2, (TSize)0);

            Assert.True(first.SequenceEqual(second));
            Assert.True(first.SequenceEqual(second, null));
            Assert.True(first.SequenceEqual(second, EqualityComparer<char>.Default));
        }

        [Fact]
        public static void SameExSpanSequenceEqual_Char() {
            char[] a = { '4', '5', '6' };
            ExSpan<char> span = new ExSpan<char>(a);

            Assert.True(span.SequenceEqual(span));
            Assert.True(span.SequenceEqual(span, null));
            Assert.True(span.SequenceEqual(span, EqualityComparer<char>.Default));
        }

        [Fact]
        public static void LengthMismatchSequenceEqual_Char() {
            char[] a = { '4', '5', '6' };
            ExSpan<char> first = new ExSpan<char>(a, (TSize)0, (TSize)3);
            ExSpan<char> second = new ExSpan<char>(a, (TSize)0, (TSize)2);

            Assert.False(first.SequenceEqual(second));
            Assert.False(first.SequenceEqual(second, null));
            Assert.False(first.SequenceEqual(second, EqualityComparer<char>.Default));
        }

        [Fact]
        public static void SequenceEqualNoMatch_Char() {
            for (int length = 1; length < 32; length++) {
                for (int mismatchIndex = 0; mismatchIndex < length; mismatchIndex++) {
                    char[] first = new char[length];
                    char[] second = new char[length];
                    for (int i = 0; i < length; i++) {
                        first[i] = second[i] = (char)(i + 1);
                    }

                    second[mismatchIndex] = (char)(second[mismatchIndex] + 1);

                    ExSpan<char> firstExSpan = new ExSpan<char>(first);
                    ReadOnlyExSpan<char> secondExSpan = new ReadOnlyExSpan<char>(second);

                    Assert.False(firstExSpan.SequenceEqual(secondExSpan));
                    Assert.False(firstExSpan.SequenceEqual(secondExSpan, null));
                    Assert.False(firstExSpan.SequenceEqual(secondExSpan, EqualityComparer<char>.Default));
                }
            }
        }

        [Fact]
        public static void MakeSureNoSequenceEqualChecksGoOutOfRange_Char() {
            for (int length = 0; length < 100; length++) {
                char[] first = new char[length + 2];
                first[0] = '9';
                first[length + 1] = '9';
                char[] second = new char[length + 2];
                second[0] = 'a';
                second[length + 1] = 'a';
                ExSpan<char> span1 = new ExSpan<char>(first, (TSize)1, (TSize)length);
                ReadOnlyExSpan<char> span2 = new ReadOnlyExSpan<char>(second, (TSize)1, (TSize)length);

                Assert.True(span1.SequenceEqual(span2));
                Assert.True(span1.SequenceEqual(span2, null));
                Assert.True(span1.SequenceEqual(span2, EqualityComparer<char>.Default));
            }
        }

    }
}
