using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Zyl.ExSpans.Tests.AExSpan {
    public partial class ASequenceEqual {

        [Fact]
        public static void ZeroLengthSequenceEqual_Byte() {
            byte[] a = new byte[3];

            ExSpan<byte> first = new ExSpan<byte>(a, (TSize)1, (TSize)0);
            ExSpan<byte> second = new ExSpan<byte>(a, (TSize)2, (TSize)0);

            Assert.True(first.SequenceEqual<byte>(second));
            Assert.True(first.SequenceEqual<byte>(second, null));
            Assert.True(first.SequenceEqual<byte>(second, EqualityComparer<byte>.Default));
        }

        [Fact]
        public static void SameExSpanSequenceEqual_Byte() {
            byte[] a = { 4, 5, 6 };
            ExSpan<byte> span = new ExSpan<byte>(a);

            Assert.True(span.SequenceEqual<byte>(span));
            Assert.True(span.SequenceEqual<byte>(span, null));
            Assert.True(span.SequenceEqual<byte>(span, EqualityComparer<byte>.Default));
        }

        [Fact]
        public static void SequenceEqualArrayImplicit_Byte() {
            byte[] a = { 4, 5, 6 };
            ExSpan<byte> first = new ExSpan<byte>(a, (TSize)0, (TSize)3);

            Assert.True(first.SequenceEqual<byte>(a));
            Assert.True(first.SequenceEqual<byte>(a, null));
            Assert.True(first.SequenceEqual<byte>(a, EqualityComparer<byte>.Default));
        }

        [Fact]
        public static void SequenceEqualArraySegmentImplicit_Byte() {
            byte[] src = { 1, 2, 3 };
            byte[] dst = { 5, 1, 2, 3, 10 };
            var segment = new ArraySegment<byte>(dst, 1, 3);

            ExSpan<byte> first = new ExSpan<byte>(src, (TSize)0, (TSize)3);

            Assert.True(first.SequenceEqual<byte>(segment));
            Assert.True(first.SequenceEqual<byte>(segment, null));
            Assert.True(first.SequenceEqual<byte>(segment, EqualityComparer<byte>.Default));
        }

        [Fact]
        public static void LengthMismatchSequenceEqual_Byte() {
            byte[] a = { 4, 5, 6 };
            ExSpan<byte> first = new ExSpan<byte>(a, (TSize)0, (TSize)3);
            ExSpan<byte> second = new ExSpan<byte>(a, (TSize)0, (TSize)2);

            Assert.False(first.SequenceEqual<byte>(second));
            Assert.False(first.SequenceEqual<byte>(second, null));
            Assert.False(first.SequenceEqual<byte>(second, EqualityComparer<byte>.Default));
        }

        [Fact]
        public static void SequenceEqualNoMatch_Byte() {
            for (int length = 1; length < 32; length++) {
                for (int mismatchIndex = 0; mismatchIndex < length; mismatchIndex++) {
                    byte[] first = new byte[length];
                    byte[] second = new byte[length];
                    for (int i = 0; i < length; i++) {
                        first[i] = second[i] = (byte)(i + 1);
                    }

                    second[mismatchIndex] = (byte)(second[mismatchIndex] + 1);

                    ExSpan<byte> firstExSpan = new ExSpan<byte>(first);
                    ReadOnlyExSpan<byte> secondExSpan = new ReadOnlyExSpan<byte>(second);

                    Assert.False(firstExSpan.SequenceEqual<byte>(secondExSpan));
                    Assert.False(firstExSpan.SequenceEqual<byte>(secondExSpan, null));
                    Assert.False(firstExSpan.SequenceEqual<byte>(secondExSpan, EqualityComparer<byte>.Default));
                }
            }
        }

        [Fact]
        public static void MakeSureNoSequenceEqualChecksGoOutOfRange_Byte() {
            for (int length = 0; length < 100; length++) {
                byte[] first = new byte[length + 2];
                first[0] = 99;
                first[length + 1] = 99;
                byte[] second = new byte[length + 2];
                second[0] = 100;
                second[length + 1] = 100;
                ExSpan<byte> span1 = new ExSpan<byte>(first, (TSize)1, (TSize)length);
                ReadOnlyExSpan<byte> span2 = new ReadOnlyExSpan<byte>(second, (TSize)1, (TSize)length);

                Assert.True(span1.SequenceEqual<byte>(span2));
                Assert.True(span1.SequenceEqual<byte>(span2, null));
                Assert.True(span1.SequenceEqual<byte>(span2, EqualityComparer<byte>.Default));
            }
        }

    }
}
