using Xunit;
using System.Linq;
using System.Collections.Generic;

namespace Zyl.ExSpans.Tests.AExSpan {
    public static partial class AGetEnumerator {
        public static IEnumerable<object[]> IntegerArrays() {
            yield return new object[] { new int[0] };
            yield return new object[] { new int[] { 42 } };
            yield return new object[] { new int[] { 42, 43, 44, 45 } };
        }

        [Theory]
        [MemberData(nameof(IntegerArrays))]
        public static void GetEnumerator_ForEach_AllValuesReturnedCorrectly(int[] array) {
            ExSpan<int> span = array;

            int sum = 0;
            foreach (int i in span) {
                sum += i;
            }

            Assert.Equal(Enumerable.Sum(array), sum);
        }

        [Theory]
        [MemberData(nameof(IntegerArrays))]
        public static void GetEnumerator_Manual_AllValuesReturnedCorrectly(int[] array) {
            ExSpan<int> span = array;

            int sum = 0;
            ExSpan<int>.Enumerator e = span.GetEnumerator();
            while (e.MoveNext()) {
                ref int i = ref e.Current;
                sum += i;
                Assert.Equal(e.Current, e.Current);
            }
            Assert.False(e.MoveNext());

            Assert.Equal(Enumerable.Sum(array), sum);
        }

        [Fact]
        public static void GetEnumerator_RefCurrentChangesAreStoredInExSpan() {
            ExSpan<ValueWrapper<int>> values = new ValueWrapper<int>[10];
            ExSpan<ValueWrapper<int>>.Enumerator e = values.GetEnumerator();

            int index = 0;
            while (e.MoveNext()) {
                e.Current.Value = index++;
            }

            for (int i = 0; i < values.Length; i++) {
                Assert.Equal(i, values[i].Value);
            }
        }

        struct ValueWrapper<T> {
            public T Value;
        }

        [Fact]
        public static void GetEnumerator_MoveNextOnDefault_ReturnsFalse() {
            Assert.False(default(ExSpan<int>.Enumerator).MoveNext());
            Assert.ThrowsAny<Exception>(() => default(ExSpan<int>.Enumerator).Current);
        }
    }
}
