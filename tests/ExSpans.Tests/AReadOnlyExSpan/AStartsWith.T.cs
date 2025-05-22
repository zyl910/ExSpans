using Xunit;

namespace Zyl.ExSpans.Tests.AReadOnlyExSpan {
    using static AComparers;

    public static partial class AStartsWith {
        [Fact]
        public static void ZeroLengthStartsWith() {
            int[] a = new int[3];

            Assert.True(new ReadOnlyExSpan<int>(a, 1, 0).StartsWith(new ReadOnlyExSpan<int>(a, 2, 0)));
            Assert.All(GetDefaultEqualityComparers<int>(), comparer => Assert.True(new ReadOnlyExSpan<int>(a, 1, 0).StartsWith(new ReadOnlyExSpan<int>(a, 2, 0), comparer)));
#if NET8_0_OR_GREATER
            Assert.True(new ReadOnlyExSpan<int>(a, 1, 0).StartsWith(new ReadOnlyExSpan<int>(a, 2, 0), GetFalseEqualityComparer<int>()));
#endif // NET8_0_OR_GREATER
        }

        [Fact]
        public static void SameSpanStartsWith() {
            int[] a = { 4, 5, 6 };
            Assert.True(new ReadOnlyExSpan<int>(a).StartsWith(a));
            Assert.All(GetDefaultEqualityComparers<int>(), comparer => Assert.True(new ReadOnlyExSpan<int>(a).StartsWith(a, comparer)));
#if NET8_0_OR_GREATER
            Assert.False(new ReadOnlyExSpan<int>(a).StartsWith(a, GetFalseEqualityComparer<int>()));
#endif // NET8_0_OR_GREATER
        }

        [Fact]
        public static void LengthMismatchStartsWith() {
            int[] a = { 4, 5, 6 };

            Assert.False(new ReadOnlyExSpan<int>(a, 0, 2).StartsWith(new ReadOnlyExSpan<int>(a, 0, 3)));
            Assert.All(GetDefaultEqualityComparers<int>(), comparer => Assert.False(new ReadOnlyExSpan<int>(a, 0, 2).StartsWith(new ReadOnlyExSpan<int>(a, 0, 3), comparer)));
        }

        [Fact]
        public static void StartsWithMatch() {
            int[] a = { 4, 5, 6 };

            Assert.True(new ReadOnlyExSpan<int>(a, 0, 3).StartsWith(new ReadOnlyExSpan<int>(a, 0, 2)));
            Assert.All(GetDefaultEqualityComparers<int>(), comparer => Assert.True(new ReadOnlyExSpan<int>(a, 0, 3).StartsWith(new ReadOnlyExSpan<int>(a, 0, 2), comparer)));
#if NET8_0_OR_GREATER
            Assert.False(new ReadOnlyExSpan<int>(a, 0, 3).StartsWith(new ReadOnlyExSpan<int>(a, 0, 2), GetFalseEqualityComparer<int>()));
#endif // NET8_0_OR_GREATER
        }

        [Fact]
        public static void StartsWithMatchDifferentSpans() {
            int[] a = { 4, 5, 6 };
            int[] b = { 4, 5, 6 };

            Assert.True(new ReadOnlyExSpan<int>(a, 0, 3).StartsWith(new ReadOnlyExSpan<int>(b, 0, 3)));
            Assert.All(GetDefaultEqualityComparers<int>(), comparer => Assert.True(new ReadOnlyExSpan<int>(a, 0, 3).StartsWith(new ReadOnlyExSpan<int>(b, 0, 3), comparer)));
#if NET8_0_OR_GREATER
            Assert.False(new ReadOnlyExSpan<int>(a, 0, 3).StartsWith(new ReadOnlyExSpan<int>(b, 0, 3), GetFalseEqualityComparer<int>()));
#endif // NET8_0_OR_GREATER
        }

        [Fact]
        public static void OnStartsWithOfEqualSpansMakeSureEveryElementIsCompared() {
            for (int length = 0; length < 100; length++) {
                TIntLog log = new TIntLog();

                TInt[] first = new TInt[length];
                TInt[] second = new TInt[length];
                for (int i = 0; i < length; i++) {
                    first[i] = second[i] = new TInt(10 * (i + 1), log);
                }

                ReadOnlyExSpan<TInt> firstSpan = new ReadOnlyExSpan<TInt>(first);
                ReadOnlyExSpan<TInt> secondSpan = new ReadOnlyExSpan<TInt>(second);
                bool b = firstSpan.StartsWith(secondSpan);
                Assert.True(b);

                // Make sure each element of the array was compared once. (Strictly speaking, it would not be illegal for
                // StartsWith to compare an element more than once but that would be a non-optimal implementation and
                // a red flag. So we'll stick with the stricter test.)
                Assert.Equal(first.Length, log.Count);
                foreach (TInt elem in first) {
                    int numCompares = log.CountCompares(elem.Value, elem.Value);
                    Assert.True(numCompares == 1, $"Expected {numCompares} == 1 for element {elem.Value}.");
                }
            }
        }

        [Fact]
        public static void StartsWithNoMatch() {
            for (int length = 1; length < 32; length++) {
                for (int mismatchIndex = 0; mismatchIndex < length; mismatchIndex++) {
                    TIntLog log = new TIntLog();

                    TInt[] first = new TInt[length];
                    TInt[] second = new TInt[length];
                    for (int i = 0; i < length; i++) {
                        first[i] = second[i] = new TInt(10 * (i + 1), log);
                    }

                    second[mismatchIndex] = new TInt(second[mismatchIndex].Value + 1, log);

                    ReadOnlyExSpan<TInt> firstSpan = new ReadOnlyExSpan<TInt>(first);
                    ReadOnlyExSpan<TInt> secondSpan = new ReadOnlyExSpan<TInt>(second);
                    bool b = firstSpan.StartsWith(secondSpan);
                    Assert.False(b);

                    Assert.Equal(1, log.CountCompares(first[mismatchIndex].Value, second[mismatchIndex].Value));
                }
            }
        }

        [Fact]
        public static void MakeSureNoStartsWithChecksGoOutOfRange() {
            const int GuardValue = 77777;
            const int GuardLength = 50;

            Action<int, int> checkForOutOfRangeAccess =
                delegate (int x, int y) {
                    if (x == GuardValue || y == GuardValue)
                        throw new Exception("Detected out of range access in IndexOf()");
                };

            for (int length = 0; length < 100; length++) {
                TInt[] first = new TInt[GuardLength + length + GuardLength];
                TInt[] second = new TInt[GuardLength + length + GuardLength];
                for (int i = 0; i < first.Length; i++) {
                    first[i] = second[i] = new TInt(GuardValue, checkForOutOfRangeAccess);
                }

                for (int i = 0; i < length; i++) {
                    first[GuardLength + i] = second[GuardLength + i] = new TInt(10 * (i + 1), checkForOutOfRangeAccess);
                }

                Assert.True(new ReadOnlyExSpan<TInt>(first, GuardLength, length).StartsWith(new ReadOnlyExSpan<TInt>(second, GuardLength, length)));
                Assert.All(GetDefaultEqualityComparers<TInt>(), comparer => Assert.True(new ReadOnlyExSpan<TInt>(first, GuardLength, length).StartsWith(new ReadOnlyExSpan<TInt>(second, GuardLength, length), comparer)));
            }
        }
    }
}
