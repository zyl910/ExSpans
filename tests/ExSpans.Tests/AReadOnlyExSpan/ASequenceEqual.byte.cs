using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Zyl.SizableSpans.Tests.Fake.Attributes;

namespace Zyl.SizableSpans.Tests.AReadOnlySizableSpan {
    public static partial class ASequenceEqual {

        [Fact]
        public static void ZeroLengthSequenceEqual_Byte() {
            byte[] a = new byte[3];

            ReadOnlySizableSpan<byte> first = new ReadOnlySizableSpan<byte>(a, (TSize)1, (TSize)0);
            ReadOnlySizableSpan<byte> second = new ReadOnlySizableSpan<byte>(a, (TSize)2, (TSize)0);

            Assert.True(first.SequenceEqual<byte>(second));
            Assert.True(first.SequenceEqual<byte>(second, null));
            Assert.True(first.SequenceEqual<byte>(second, EqualityComparer<byte>.Default));
        }

        [Fact]
        public static void SameSizableSpanSequenceEqual_Byte() {
            byte[] a = { 4, 5, 6 };
            ReadOnlySizableSpan<byte> span = new ReadOnlySizableSpan<byte>(a);

            Assert.True(span.SequenceEqual<byte>(span));
            Assert.True(span.SequenceEqual<byte>(span, null));
            Assert.True(span.SequenceEqual<byte>(span, EqualityComparer<byte>.Default));
        }

        [Fact]
        public static void SequenceEqualArrayImplicit_Byte() {
            byte[] a = { 4, 5, 6 };
            ReadOnlySizableSpan<byte> first = new ReadOnlySizableSpan<byte>(a, (TSize)0, (TSize)3);

            Assert.True(first.SequenceEqual<byte>(a));
            Assert.True(first.SequenceEqual<byte>(a, null));
            Assert.True(first.SequenceEqual<byte>(a, EqualityComparer<byte>.Default));
        }

        [Fact]
        public static void SequenceEqualArraySegmentImplicit_Byte() {
            byte[] src = { 1, 2, 3 };
            byte[] dst = { 5, 1, 2, 3, 10 };
            var segment = new ArraySegment<byte>(dst, 1, 3);

            ReadOnlySizableSpan<byte> first = new ReadOnlySizableSpan<byte>(src, (TSize)0, (TSize)3);

            Assert.True(first.SequenceEqual<byte>(segment));
            Assert.True(first.SequenceEqual<byte>(segment, null));
            Assert.True(first.SequenceEqual<byte>(segment, EqualityComparer<byte>.Default));
        }

        [Fact]
        public static void LengthMismatchSequenceEqual_Byte() {
            byte[] a = { 4, 5, 6 };
            ReadOnlySizableSpan<byte> first = new ReadOnlySizableSpan<byte>(a, (TSize)0, (TSize)3);
            ReadOnlySizableSpan<byte> second = new ReadOnlySizableSpan<byte>(a, (TSize)0, (TSize)2);

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

                    ReadOnlySizableSpan<byte> firstSizableSpan = new ReadOnlySizableSpan<byte>(first);
                    ReadOnlySizableSpan<byte> secondSizableSpan = new ReadOnlySizableSpan<byte>(second);

                    Assert.False(firstSizableSpan.SequenceEqual<byte>(secondSizableSpan));
                    Assert.False(firstSizableSpan.SequenceEqual<byte>(secondSizableSpan, null));
                    Assert.False(firstSizableSpan.SequenceEqual<byte>(secondSizableSpan, EqualityComparer<byte>.Default));
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
                ReadOnlySizableSpan<byte> span1 = new ReadOnlySizableSpan<byte>(first, (TSize)1, (TSize)length);
                ReadOnlySizableSpan<byte> span2 = new ReadOnlySizableSpan<byte>(second, (TSize)1, (TSize)length);

                Assert.True(span1.SequenceEqual<byte>(span2));
                Assert.True(span1.SequenceEqual<byte>(span2, null));
                Assert.True(span1.SequenceEqual<byte>(span2, EqualityComparer<byte>.Default));
            }
        }

    }
}
