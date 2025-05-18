using System.Collections.Generic;
using Xunit;

namespace Zyl.ExSpans.Tests.AReadOnlyExSpan {
    using static AComparers;

    // Adapted from IndexOf.T.cs
    public static partial class AContains // .Contains<T>
    {
        [Fact]
        public static void ZeroLengthContains() {
            ReadOnlyExSpan<int> span = new ReadOnlyExSpan<int>(ArrayHelper.Empty<int>());

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

                for (int targetIndex = 0; targetIndex < length; targetIndex++) {
                    int target = a[targetIndex];
                    Assert.True(new ReadOnlyExSpan<int>(a).Contains(target));
                    Assert.All(GetDefaultEqualityComparers<int>(), comparer => Assert.True(new ReadOnlyExSpan<int>(a).Contains(target, comparer)));
#if NET8_0_OR_GREATER
                    Assert.False(new ReadOnlyExSpan<int>(a).Contains(target, GetFalseEqualityComparer<int>()));
#endif // NET8_0_OR_GREATER
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

                Assert.True(new ReadOnlyExSpan<int>(a).Contains(5555));
                Assert.All(GetDefaultEqualityComparers<int>(), comparer => Assert.True(new ReadOnlyExSpan<int>(a).Contains(5555, comparer)));
#if NET8_0_OR_GREATER
                Assert.False(new ReadOnlyExSpan<int>(a).Contains(5555, GetFalseEqualityComparer<int>()));
#endif // NET8_0_OR_GREATER
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
                ReadOnlyExSpan<TInt> span = new ReadOnlyExSpan<TInt>(a);
                Assert.False(span.Contains(new TInt(9999, log)));

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

                ReadOnlyExSpan<TInt> span = new ReadOnlyExSpan<TInt>(a, GuardLength, length);
                Assert.False(span.Contains(new TInt(9999, checkForOutOfRangeAccess)));
            }
        }

        [Fact]
        public static void ZeroLengthContains_String() {
            ReadOnlyExSpan<string> span = new ReadOnlyExSpan<string>(ArrayHelper.Empty<string>());
            Assert.False(span.Contains("a"));
            Assert.All(GetDefaultEqualityComparers<string>(), comparer => Assert.False(new ReadOnlyExSpan<string>(ArrayHelper.Empty<string>()).Contains("a", comparer)));
            Assert.False(span.Contains("a", null));
            Assert.False(span.Contains("a", EqualityComparer<string>.Default));
#if NET8_0_OR_GREATER
            Assert.False(span.Contains("a", EqualityComparer<string>.Create((i, j) => i == j)));
#endif // NET8_0_OR_GREATER
        }

        [Fact]
        public static void TestMatchContains_String() {
            for (int length = 0; length < 32; length++) {
                string[] a = new string[length];
                for (int i = 0; i < length; i++) {
                    a[i] = (10 * (i + 1)).ToString();
                }
                ReadOnlyExSpan<string> span = new ReadOnlyExSpan<string>(a);

                for (int targetIndex = 0; targetIndex < length; targetIndex++) {
                    string target = a[targetIndex];
                    Assert.True(span.Contains(target));
                    Assert.All(GetDefaultEqualityComparers<string>(), comparer => Assert.True(new ReadOnlyExSpan<string>(a).Contains(target, comparer)));
#if NET8_0_OR_GREATER
                    Assert.False(span.Contains(target, GetFalseEqualityComparer<string>()));
#endif // NET8_0_OR_GREATER
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
                ReadOnlyExSpan<string> span = new ReadOnlyExSpan<string>(a);

                Assert.False(span.Contains(target));
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

                ReadOnlyExSpan<string> span = new ReadOnlyExSpan<string>(a);
                Assert.True(span.Contains("5555"));
            }
        }

        [Theory]
        [MemberData(nameof(TestHelpers.ContainsNullData), MemberType = typeof(TestHelpers))]
        public static void ContainsNull_String(string?[] spanInput, bool expected) {
            ReadOnlyExSpan<string?> theStrings = spanInput;
            Assert.Equal(expected, theStrings!.Contains(null));
            Assert.All(GetDefaultEqualityComparers<string>(), comparer => Assert.Equal(expected, new ReadOnlyExSpan<string?>(spanInput).Contains(null, comparer!)));
        }
    }
}
