﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Zyl.ExSpans.Tests.AExSpan {
    partial class ASequenceEqual {

        [Fact]
        public static void ZeroLengthSequenceEqual_Long() {
            long[] a = new long[3];

            ExSpan<long> first = new ExSpan<long>(a, (TSize)1, (TSize)0);
            ExSpan<long> second = new ExSpan<long>(a, (TSize)2, (TSize)0);

            Assert.True(first.SequenceEqual<long>(second));
            Assert.True(first.SequenceEqual<long>(second, null));
            Assert.True(first.SequenceEqual<long>(second, EqualityComparer<long>.Default));
        }

        [Fact]
        public static void SameExSpanSequenceEqual_Long() {
            long[] a = { 488238291, 52498989823, 619890289890 };
            ExSpan<long> span = new ExSpan<long>(a);

            Assert.True(span.SequenceEqual<long>(span));
            Assert.True(span.SequenceEqual<long>(span), null);
            Assert.True(span.SequenceEqual<long>(span, EqualityComparer<long>.Default));
        }

        [Fact]
        public static void SequenceEqualArrayImplicit_Long() {
            long[] a = { 488238291, 52498989823, 619890289890 };
            ExSpan<long> first = new ExSpan<long>(a, (TSize)0, (TSize)3);

            Assert.True(first.SequenceEqual<long>(a));
            Assert.True(first.SequenceEqual<long>(a, null));
            Assert.True(first.SequenceEqual<long>(a, EqualityComparer<long>.Default));
        }

        [Fact]
        public static void SequenceEqualArraySegmentImplicit_Long() {
            long[] src = { 1989089123, 234523454235, 3123213231 };
            long[] dst = { 5, 1989089123, 234523454235, 3123213231, 10 };
            ArraySegment<long> segment = new ArraySegment<long>(dst, 1, 3);

            ExSpan<long> first = new ExSpan<long>(src, (TSize)0, (TSize)3);

            Assert.True(first.SequenceEqual<long>(segment));
            Assert.True(first.SequenceEqual<long>(segment, null));
            Assert.True(first.SequenceEqual<long>(segment, EqualityComparer<long>.Default));
        }

        [Fact]
        public static void LengthMismatchSequenceEqual_Long() {
            long[] a = { 488238291, 52498989823, 619890289890 };
            ExSpan<long> first = new ExSpan<long>(a, (TSize)0, (TSize)3);
            ExSpan<long> second = new ExSpan<long>(a, (TSize)0, (TSize)2);

            Assert.False(first.SequenceEqual<long>(second));
            Assert.False(first.SequenceEqual<long>(second, null));
            Assert.False(first.SequenceEqual<long>(second, EqualityComparer<long>.Default));
        }

        [Fact]
        public static void SequenceEqualNoMatch_Long() {
            for (int length = 1; length < 32; length++) {
                for (int mismatchIndex = 0; mismatchIndex < length; mismatchIndex++) {
                    long[] first = new long[length];
                    long[] second = new long[length];
                    for (int i = 0; i < length; i++) {
                        first[i] = second[i] = (byte)(i + 1);
                    }

                    second[mismatchIndex] = (byte)(second[mismatchIndex] + 1);

                    ExSpan<long> firstExSpan = new ExSpan<long>(first);
                    ReadOnlyExSpan<long> secondExSpan = new ReadOnlyExSpan<long>(second);

                    Assert.False(firstExSpan.SequenceEqual<long>(secondExSpan));
                    Assert.False(firstExSpan.SequenceEqual<long>(secondExSpan, null));
                    Assert.False(firstExSpan.SequenceEqual<long>(secondExSpan, EqualityComparer<long>.Default));
                }
            }
        }

        [Fact]
        public static void MakeSureNoSequenceEqualChecksGoOutOfRange_Long() {
            for (int length = 0; length < 100; length++) {
                long[] first = new long[length + 2];
                first[0] = 99;
                first[length + 1] = 99;
                long[] second = new long[length + 2];
                second[0] = 100;
                second[length + 1] = 100;
                ExSpan<long> span1 = new ExSpan<long>(first, (TSize)1, (TSize)length);
                ReadOnlyExSpan<long> span2 = new ReadOnlyExSpan<long>(second, (TSize)1, (TSize)length);

                Assert.True(span1.SequenceEqual<long>(span2));
                Assert.True(span1.SequenceEqual<long>(span2, null));
                Assert.True(span1.SequenceEqual<long>(span2, EqualityComparer<long>.Default));
            }
        }

    }
}
