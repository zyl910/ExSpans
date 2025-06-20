using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Zyl.ExSpans.Tests.AExSpan {
    // Adapted from IndexOf.T.cs
    public static partial class AContains // .Contains<T>
    {
        [Fact]
        public static void ZeroLengthContains() {
            ExSpan<int> span = new ExSpan<int>(ArrayHelper.Empty<int>());

            bool found = span.Contains(0);
            Assert.False(found);
        }

        [Fact]
        public static void TestContains() {
            for (int length = 0; length < 32; length++) {
                int[] a = new int[length];
                for (int i = 0; i < length; i++) {
                    a[i] = 10 * (i + 1);
                }
                ExSpan<int> span = new ExSpan<int>(a);

                for (int targetIndex = 0; targetIndex < length; targetIndex++) {
                    int target = a[targetIndex];
                    bool found = span.Contains(target);
                    Assert.True(found);
                }
            }
        }

        [Fact]
        public static void TestMultipleContains() {
            for (int length = 2; length < 32; length++) {
                int[] a = new int[length];
                for (int i = 0; i < length; i++) {
                    a[i] = 10 * (i + 1);
                }

                a[length - 1] = 5555;
                a[length - 2] = 5555;

                ExSpan<int> span = new ExSpan<int>(a);
                bool found = span.Contains(5555);
                Assert.True(found);
            }
        }

        [Fact]
        public static void OnNoMatchForContainsMakeSureEveryElementIsCompared() {
            for (int length = 0; length < 100; length++) {
                TIntLog log = new TIntLog();

                TInt[] a = new TInt[length];
                for (int i = 0; i < length; i++) {
                    a[i] = new TInt(10 * (i + 1), log);
                }
                ExSpan<TInt> span = new ExSpan<TInt>(a);
                bool found = span.Contains(new TInt(9999, log));
                Assert.False(found);

                // Since we asked for a non-existent value, make sure each element of the array was compared once.
                // (Strictly speaking, it would not be illegal for IndexOf to compare an element more than once but
                // that would be a non-optimal implementation and a red flag. So we'll stick with the stricter test.)
                Assert.Equal(a.Length, log.Count);
                foreach (TInt elem in a) {
                    int numCompares = log.CountCompares(elem.Value, 9999);
                    Assert.True(numCompares == 1, $"Expected {numCompares} == 1 for element {elem.Value}.");
                }
            }
        }

        [Fact]
        public static void MakeSureNoChecksForContainsGoOutOfRange() {
            const int GuardValue = 77777;
            const int GuardLength = 50;

            void checkForOutOfRangeAccess(int x, int y) {
                if (x == GuardValue || y == GuardValue)
                    throw new Exception("Detected out of range access in IndexOf()");
            }

            for (int length = 0; length < 100; length++) {
                TInt[] a = new TInt[GuardLength + length + GuardLength];
                for (int i = 0; i < a.Length; i++) {
                    a[i] = new TInt(GuardValue, checkForOutOfRangeAccess);
                }

                for (int i = 0; i < length; i++) {
                    a[GuardLength + i] = new TInt(10 * (i + 1), checkForOutOfRangeAccess);
                }

                ExSpan<TInt> span = new ExSpan<TInt>(a, GuardLength, length);
                bool found = span.Contains(new TInt(9999, checkForOutOfRangeAccess));
                Assert.False(found);
            }
        }

        [Fact]
        public static void ZeroLengthContains_String() {
            ExSpan<string> span = new ExSpan<string>(ArrayHelper.Empty<string>());
            bool found = span.Contains("a");
            Assert.False(found);
        }

        [Fact]
        public static void TestMatchContains_String() {
            for (int length = 0; length < 32; length++) {
                string[] a = new string[length];
                for (int i = 0; i < length; i++) {
                    a[i] = (10 * (i + 1)).ToString();
                }
                ExSpan<string> span = new ExSpan<string>(a);

                for (int targetIndex = 0; targetIndex < length; targetIndex++) {
                    string target = a[targetIndex];
                    bool found = span.Contains(target);
                    Assert.True(found);
                }
            }
        }

        [Fact]
        public static void TestNoMatchContains_String() {
            var rnd = new Random(42);
            for (int length = 0; length <= byte.MaxValue; length++) {
                string[] a = new string[length];
                string target = (rnd.Next(0, 256)).ToString();
                for (int i = 0; i < length; i++) {
                    string val = (i + 1).ToString();
                    a[i] = val == target ? (target + 1) : val;
                }
                ExSpan<string> span = new ExSpan<string>(a);

                bool found = span.Contains(target);
                Assert.False(found);
            }
        }

        [Fact]
        public static void TestMultipleMatchContains_String() {
            for (int length = 2; length < 32; length++) {
                string[] a = new string[length];
                for (int i = 0; i < length; i++) {
                    a[i] = (10 * (i + 1)).ToString();
                }

                a[length - 1] = "5555";
                a[length - 2] = "5555";

                ExSpan<string> span = new ExSpan<string>(a);
                bool found = span.Contains("5555");
                Assert.True(found);
            }
        }

        [Theory]
        [MemberData(nameof(TestHelpers.ContainsNullData), MemberType = typeof(TestHelpers))]
        public static void ContainsNull_String(string?[]? spanInput, bool expected) {
            ExSpan<string?> theStrings = spanInput;
            Assert.Equal(expected, theStrings.Contains(null));
        }

        [Theory]
        [InlineData(new int[] { 1, 2, 3, 4 }, 4, true)]
        [InlineData(new int[] { 1, 2, 3, 4 }, 5, false)]
        public static void Contains_Int32(int[] array, int value, bool expectedResult) {
            // Test with short ExSpan
            ExSpan<int> span = new ExSpan<int>(array);
            bool result = span.Contains(value);
            Assert.Equal(result, expectedResult);

            // Test with long ExSpan
            for (int i = 0; i < 10; i++)
                array = array.Concat(array).ToArray();
            span = new ExSpan<int>(array);
            result = span.Contains(value);
            Assert.Equal(result, expectedResult);
        }

        [Theory]
        [InlineData(new long[] { 1, 2, 3, 4 }, 4, true)]
        [InlineData(new long[] { 1, 2, 3, 4 }, 5, false)]
        public static void Contains_Int64(long[] array, long value, bool expectedResult) {
            // Test with short ExSpan
            ExSpan<long> span = new ExSpan<long>(array);
            bool result = span.Contains(value);
            Assert.Equal(result, expectedResult);

            // Test with long ExSpan
            for (int i = 0; i < 10; i++)
                array = array.Concat(array).ToArray();
            span = new ExSpan<long>(array);
            result = span.Contains(value);
            Assert.Equal(result, expectedResult);
        }

        [Theory]
        [InlineData(new byte[] { 1, 2, 3, 4 }, 4, true)]
        [InlineData(new byte[] { 1, 2, 3, 4 }, 5, false)]
        public static void Contains_Byte(byte[] array, byte value, bool expectedResult) {
            // Test with short ExSpan
            ExSpan<byte> span = new ExSpan<byte>(array);
            bool result = span.Contains(value);
            Assert.Equal(result, expectedResult);

            // Test with long ExSpan
            for (int i = 0; i < 10; i++)
                array = array.Concat(array).ToArray();
            span = new ExSpan<byte>(array);
            result = span.Contains(value);
            Assert.Equal(result, expectedResult);
        }

        [Theory]
        [InlineData(new char[] { 'a', 'b', 'c', 'd' }, 'd', true)]
        [InlineData(new char[] { 'a', 'b', 'c', 'd' }, 'e', false)]
        public static void Contains_Char(char[] array, char value, bool expectedResult) {
            // Test with short ExSpan
            ExSpan<char> span = new ExSpan<char>(array);
            bool result = span.Contains(value);
            Assert.Equal(result, expectedResult);

            // Test with long ExSpan
            for (int i = 0; i < 10; i++)
                array = array.Concat(array).ToArray();
            span = new ExSpan<char>(array);
            result = span.Contains(value);
            Assert.Equal(result, expectedResult);

        }

        [Theory]
        [InlineData(new float[] { 1, 2, 3, 4 }, 4, true)]
        [InlineData(new float[] { 1, 2, 3, 4 }, 5, false)]
        public static void Contains_Float(float[] array, float value, bool expectedResult) {
            // Test with short ExSpan
            ExSpan<float> span = new ExSpan<float>(array);
            bool result = span.Contains(value);
            Assert.Equal(result, expectedResult);

            // Test with long ExSpan
            for (int i = 0; i < 10; i++)
                array = array.Concat(array).ToArray();
            span = new ExSpan<float>(array);
            result = span.Contains(value);
            Assert.Equal(result, expectedResult);
        }

        [Theory]
        [InlineData(new double[] { 1, 2, 3, 4 }, 4, true)]
        [InlineData(new double[] { 1, 2, 3, 4 }, 5, false)]
        public static void Contains_Double(double[] array, double value, bool expectedResult) {
            // Test with short ExSpan
            ExSpan<double> span = new ExSpan<double>(array);
            bool result = span.Contains(value);
            Assert.Equal(result, expectedResult);

            // Test with long ExSpan
            for (int i = 0; i < 10; i++)
                array = array.Concat(array).ToArray();
            span = new ExSpan<double>(array);
            result = span.Contains(value);
            Assert.Equal(result, expectedResult);
        }
    }
}
