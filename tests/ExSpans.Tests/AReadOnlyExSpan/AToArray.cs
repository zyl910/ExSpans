using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Zyl.ExSpans.Tests.AReadOnlyExSpan {
    public static class AToArray {
        [Fact]
        public static void ToArray1() {
            int[] a = { 91, 92, 93 };
            ReadOnlyExSpan<int> span = new ReadOnlyExSpan<int>(a);
            int[] copy = span.ToArray();
            Assert.Equal<int>(a, copy);
            Assert.NotSame(a, copy);
        }

        [Fact]
        public static void ToArrayWithIndex() {
            int[] a = { 91, 92, 93, 94, 95 };
            var span = new ExSpan<int>(a);
            int[] copy = span.Slice((TSize)2).ToArray();

            Assert.Equal<int>(new int[] { 93, 94, 95 }, copy);
        }

        [Fact]
        public static void ToArrayWithIndexAndLength() {
            int[] a = { 91, 92, 93 };
            var span = new ExSpan<int>(a, (TSize)1, (TSize)1);
            int[] copy = span.ToArray();
            Assert.Equal<int>(new int[] { 92 }, copy);
        }

        [Fact]
        public static void ToArrayEmpty() {
            ReadOnlyExSpan<int> span = ReadOnlyExSpan<int>.Empty;
            int[] copy = span.ToArray();
#pragma warning disable xUnit2013 // Do not use equality check to check for collection size.
            Assert.Equal(0, copy.Length);
#pragma warning restore xUnit2013 // Do not use equality check to check for collection size.
        }
    }
}
