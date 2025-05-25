using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Zyl.ExSpans.Tests.AExSpan {
#nullable disable
    public static partial class ASort {
        [Fact]
        public static void Sort_InvalidArguments_Throws() {
            AssertExtensions.Throws<ArgumentNullException>("comparison", () => ExMemoryExtensions.Sort(ExSpan<byte>.Empty, (Comparison<byte>)null));
            AssertExtensions.Throws<ArgumentNullException>("comparison", () => ExMemoryExtensions.Sort(ExSpan<byte>.Empty, ExSpan<byte>.Empty, (Comparison<byte>)null));

            Assert.Throws<ArgumentException>(() => ExMemoryExtensions.Sort((ExSpan<byte>)new byte[1], (ExSpan<byte>)new byte[2]));
            Assert.Throws<ArgumentException>(() => ExMemoryExtensions.Sort((ExSpan<byte>)new byte[2], (ExSpan<byte>)new byte[1]));
            Assert.Throws<ArgumentException>(() => ExMemoryExtensions.Sort((ExSpan<byte>)new byte[1], (ExSpan<byte>)new byte[2], Comparer<byte>.Default.Compare));
            Assert.Throws<ArgumentException>(() => ExMemoryExtensions.Sort((ExSpan<byte>)new byte[2], (ExSpan<byte>)new byte[1], Comparer<byte>.Default.Compare));
            Assert.Throws<ArgumentException>(() => ExMemoryExtensions.Sort((ExSpan<byte>)new byte[1], (ExSpan<byte>)new byte[2], Comparer<byte>.Default));
            Assert.Throws<ArgumentException>(() => ExMemoryExtensions.Sort((ExSpan<byte>)new byte[2], (ExSpan<byte>)new byte[1], Comparer<byte>.Default));

            Assert.Throws<InvalidOperationException>(() => ExMemoryExtensions.Sort((ExSpan<NotImcomparable>)new NotImcomparable[10]));
            Assert.Throws<InvalidOperationException>(() => ExMemoryExtensions.Sort((ExSpan<NotImcomparable>)new NotImcomparable[10], (ExSpan<byte>)new byte[10]));
        }

        private struct NotImcomparable { }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        public static void Sort_CovariantArraysAllowed(int overload) {
            ExSpan<object> actual = Enumerable.Range(0, 10).Select(i => (object)i.ToString()).Reverse().ToArray();

            object[] expected = actual.ToArray();
            Array.Sort(expected);

            switch (overload) {
                case 0:
                    ExMemoryExtensions.Sort(actual);
                    break;
                case 1:
                    ExMemoryExtensions.Sort(actual, StringComparer.CurrentCulture.Compare);
                    break;
                case 2:
                    ExMemoryExtensions.Sort(actual, (IComparer<object>)null);
                    break;
                case 3:
                    ExMemoryExtensions.Sort(actual, new byte[actual.Length].AsExSpan());
                    break;
                case 4:
                    ExMemoryExtensions.Sort(actual, new byte[actual.Length].AsExSpan(), StringComparer.CurrentCulture.Compare);
                    break;
                case 5:
                    ExMemoryExtensions.Sort(actual, new byte[actual.Length].AsExSpan(), (IComparer<object>)null);
                    break;
            }

            Assert.Equal(expected, actual.ToArray());
        }

        [Theory]
        [MemberData(nameof(Sort_MemberData))]
        public static void Sort<T>(T[] origKeys, IComparer<T> comparer) {
            T[] expectedKeys = origKeys.ToArray();
            Array.Sort(expectedKeys, comparer);

            byte[] origValues = new byte[origKeys.Length];
            new Random(42).NextBytes(origValues);
            byte[] expectedValues = origValues.ToArray();
            Array.Sort(origKeys.ToArray(), expectedValues, comparer);

            ExSpan<T> keys;
            ExSpan<byte> values;

            if (comparer == null) {
                keys = origKeys.ToArray();
                ExMemoryExtensions.Sort(keys);
                Assert.Equal(expectedKeys, keys.ToArray());
            }

            keys = origKeys.ToArray();
            ExMemoryExtensions.Sort(keys, comparer);
            Assert.Equal(expectedKeys, keys.ToArray());

            keys = origKeys.ToArray();
            ExMemoryExtensions.Sort(keys, comparer != null ? (Comparison<T>)comparer.Compare : Comparer<T>.Default.Compare);
            Assert.Equal(expectedKeys, keys.ToArray());

            if (comparer == null) {
                keys = origKeys.ToArray();
                values = origValues.ToArray();
                ExMemoryExtensions.Sort(keys, values);
                Assert.Equal(expectedKeys, keys.ToArray());
                Assert.Equal(expectedValues, values.ToArray());
            }

            keys = origKeys.ToArray();
            values = origValues.ToArray();
            ExMemoryExtensions.Sort(keys, values, comparer);
            Assert.Equal(expectedKeys, keys.ToArray());
            Assert.Equal(expectedValues, values.ToArray());

            keys = origKeys.ToArray();
            values = origValues.ToArray();
            ExMemoryExtensions.Sort(keys, values, comparer != null ? (Comparison<T>)comparer.Compare : Comparer<T>.Default.Compare);
            Assert.Equal(expectedKeys, keys.ToArray());
            Assert.Equal(expectedValues, values.ToArray());
        }

        public static IEnumerable<object[]> Sort_MemberData() {
            var rand = new Random(42);
            foreach (int length in new[] { 0, 1, 2, 3, 10 }) {
                yield return new object[] { CreateArray(i => i.ToString()), null };
                yield return new object[] { CreateArray(i => (byte)i), null };
                yield return new object[] { CreateArray(i => (sbyte)i), null };
                yield return new object[] { CreateArray(i => (ushort)i), null };
                yield return new object[] { CreateArray(i => (short)i), null };
                yield return new object[] { CreateArray(i => (uint)i), null };
                yield return new object[] { CreateArray(i => i), null };
                yield return new object[] { CreateArray(i => (ulong)i), null };
                yield return new object[] { CreateArray(i => (long)i), null };
                yield return new object[] { CreateArray(i => (char)i), null };
                yield return new object[] { CreateArray(i => i % 2 == 0), null };
                yield return new object[] { CreateArray(i => (float)i), null };
                yield return new object[] { CreateArray(i => (double)i), null };
                yield return new object[] { CreateArray(i => (IntPtr)i), Comparer<IntPtr>.Create((i, j) => i.ToInt64().CompareTo(j.ToInt64())) };
                yield return new object[] { CreateArray(i => (UIntPtr)i), Comparer<UIntPtr>.Create((i, j) => i.ToUInt64().CompareTo(j.ToUInt64())) };

                T[] CreateArray<T>(Func<int, T> getValue) => Enumerable.Range(0, length).Select(_ => getValue(rand.Next())).ToArray();
            }
        }
    }
#nullable restore
}
