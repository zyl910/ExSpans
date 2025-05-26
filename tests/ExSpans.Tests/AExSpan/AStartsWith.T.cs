using Xunit;

namespace Zyl.ExSpans.Tests.AExSpan {
    public static partial class AStartsWith {
        [Fact]
        public static void ZeroLengthStartsWith() {
            int[] a = new int[3];

            ExSpan<int> first = new ExSpan<int>(a, 1, 0);
            ReadOnlyExSpan<int> second = new ReadOnlyExSpan<int>(a, 2, 0);
            bool b = first.StartsWith(second);
            Assert.True(b);
        }

        [Fact]
        public static void SameExSpanStartsWith() {
            int[] a = { 4, 5, 6 };
            ExSpan<int> span = new ExSpan<int>(a);
            bool b = span.StartsWith(span);
            Assert.True(b);
        }

        [Fact]
        public static void LengthMismatchStartsWith() {
            int[] a = { 4, 5, 6 };
            ExSpan<int> first = new ExSpan<int>(a, 0, 2);
            ReadOnlyExSpan<int> second = new ReadOnlyExSpan<int>(a, 0, 3);
            bool b = first.StartsWith(second);
            Assert.False(b);
        }

        [Fact]
        public static void StartsWithMatch() {
            int[] a = { 4, 5, 6 };
            ExSpan<int> span = new ExSpan<int>(a, 0, 3);
            ReadOnlyExSpan<int> slice = new ReadOnlyExSpan<int>(a, 0, 2);
            bool b = span.StartsWith(slice);
            Assert.True(b);
        }

        [Fact]
        public static void StartsWithMatchDifferentExSpans() {
            int[] a = { 4, 5, 6 };
            int[] b = { 4, 5, 6 };
            ExSpan<int> span = new ExSpan<int>(a, 0, 3);
            ReadOnlyExSpan<int> slice = new ReadOnlyExSpan<int>(b, 0, 3);
            bool c = span.StartsWith(slice);
            Assert.True(c);
        }

        [Fact]
        public static void OnStartsWithOfEqualExSpansMakeSureEveryElementIsCompared() {
            for (int length = 0; length < 100; length++) {
                TIntLog log = new TIntLog();

                TInt[] first = new TInt[length];
                TInt[] second = new TInt[length];
                for (int i = 0; i < length; i++) {
                    first[i] = second[i] = new TInt(10 * (i + 1), log);
                }

                ExSpan<TInt> firstExSpan = new ExSpan<TInt>(first);
                ReadOnlyExSpan<TInt> secondExSpan = new ReadOnlyExSpan<TInt>(second);
                bool b = firstExSpan.StartsWith(secondExSpan);
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

                    ExSpan<TInt> firstExSpan = new ExSpan<TInt>(first);
                    ReadOnlyExSpan<TInt> secondExSpan = new ReadOnlyExSpan<TInt>(second);
                    bool b = firstExSpan.StartsWith(secondExSpan);
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

                ExSpan<TInt> firstExSpan = new ExSpan<TInt>(first, GuardLength, length);
                ReadOnlyExSpan<TInt> secondExSpan = new ReadOnlyExSpan<TInt>(second, GuardLength, length);
                bool b = firstExSpan.StartsWith(secondExSpan);
                Assert.True(b);
            }
        }

#nullable disable
        [Fact]
        public static void StartsWithSingle() {
            ReadOnlyExSpan<char> chars = (char[])[];
            Assert.False(chars.StartsWith('\0'));
            Assert.False(chars.StartsWith('f'));

            chars = "foo".AsExSpan();
            Assert.True(chars.StartsWith(chars[0]));
            Assert.True(chars.StartsWith('f'));
            Assert.False(chars.StartsWith('o'));

            scoped ReadOnlyExSpan<string> strings = (string[])[];
            Assert.False(strings.StartsWith((string)null));
            Assert.False(strings.StartsWith("foo"));

            strings = (string[])["foo", "bar"];
            Assert.True(strings.StartsWith(strings[0]));
            Assert.True(strings.StartsWith("foo"));
            Assert.True(strings.StartsWith("*foo".Substring(1)));
            Assert.False(strings.StartsWith("bar"));
            Assert.False(strings.StartsWith((string)null));

            strings = (string[])[null, "bar"];
            Assert.True(strings.StartsWith(strings[0]));
            Assert.True(strings.StartsWith((string)null));
            Assert.False(strings.StartsWith("bar"));
        }
#nullable restore
    }
}
