using Xunit;

namespace Zyl.ExSpans.Tests.AExSpan {
    public static partial class ASequenceCompareTo {
        [Fact]
        public static void OnSequenceCompareToOfEqualExSpansMakeSureEveryElementIsCompared() {
            for (int length = 0; length < 100; length++) {
                TIntLog log = new TIntLog();

                TInt[] first = new TInt[length];
                TInt[] second = new TInt[length];
                for (int i = 0; i < length; i++) {
                    first[i] = second[i] = new TInt(10 * (i + 1), log);
                }

                ExSpan<TInt> firstExSpan = new ExSpan<TInt>(first);
                ReadOnlyExSpan<TInt> secondExSpan = new ReadOnlyExSpan<TInt>(second);
                int result = firstExSpan.SequenceCompareTo(secondExSpan);
                Assert.Equal(0, result);

                // Make sure each element of the array was compared once. (Strictly speaking, it would not be illegal for
                // SequenceCompareTo to compare an element more than once but that would be a non-optimal implementation and
                // a red flag. So we'll stick with the stricter test.)
                Assert.Equal(first.Length, log.Count);
                foreach (TInt elem in first) {
                    int numCompares = log.CountCompares(elem.Value, elem.Value);
                    Assert.True(numCompares == 1, $"Expected {numCompares} == 1 for element {elem.Value}.");
                }
            }
        }

        [Fact]
        public static void SequenceCompareToSingleMismatch() {
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
                    int result = firstExSpan.SequenceCompareTo(secondExSpan);
                    Assert.True(result < 0);
                    Assert.Equal(1, log.CountCompares(first[mismatchIndex].Value, second[mismatchIndex].Value));

                    result = secondExSpan.SequenceCompareTo(firstExSpan);       // adds to log.CountCompares
                    Assert.True(result > 0);
                    Assert.Equal(2, log.CountCompares(first[mismatchIndex].Value, second[mismatchIndex].Value));
                }
            }
        }

        [Fact]
        public static void SequenceCompareToNoMatch() {
            for (int length = 1; length < 32; length++) {
                TIntLog log = new TIntLog();

                TInt[] first = new TInt[length];
                TInt[] second = new TInt[length];

                for (int i = 0; i < length; i++) {
                    first[i] = new TInt(i + 1, log);
                    second[i] = new TInt(length + i + 1, log);
                }

                ExSpan<TInt> firstExSpan = new ExSpan<TInt>(first);
                ReadOnlyExSpan<TInt> secondExSpan = new ReadOnlyExSpan<TInt>(second);
                int result = firstExSpan.SequenceCompareTo(secondExSpan);
                Assert.True(result < 0);
                Assert.Equal(1, log.CountCompares(firstExSpan[0].Value, secondExSpan[0].Value));

                result = secondExSpan.SequenceCompareTo(firstExSpan);       // adds to log.CountCompares
                Assert.True(result > 0);
                Assert.Equal(2, log.CountCompares(firstExSpan[0].Value, secondExSpan[0].Value));
            }
        }

        [Fact]
        public static void MakeSureNoSequenceCompareToChecksGoOutOfRange() {
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
                ExSpan<TInt> secondExSpan = new ExSpan<TInt>(second, GuardLength, length);
                int result = firstExSpan.SequenceCompareTo(secondExSpan);
                Assert.Equal(0, result);
            }
        }

        [Fact]
        public static void ZeroLengthSequenceCompareTo_String() {
            var a = new string[3];

            var first = new ExSpan<string>(a, 1, 0);
            var second = new ExSpan<string>(a, 2, 0);
            int result = first.SequenceCompareTo<string>(second);
            Assert.Equal(0, result);
        }

        [Fact]
        public static void SameExSpanSequenceCompareTo_String() {
            string[] a = { "fourth", "fifth", "sixth" };
            var span = new ExSpan<string>(a);
            int result = span.SequenceCompareTo<string>(span);
            Assert.Equal(0, result);
        }

        [Fact]
        public static void SequenceCompareToArrayImplicit_String() {
            string[] a = { "fourth", "fifth", "sixth" };
            var first = new ExSpan<string>(a, 0, 3);
            int result = first.SequenceCompareTo<string>(a);
            Assert.Equal(0, result);
        }

        [Fact]
        public static void SequenceCompareToArraySegmentImplicit_String() {
            string[] src = { "first", "second", "third" };
            string[] dst = { "fifth", "first", "second", "third", "tenth" };
            var segment = new ArraySegment<string>(dst, 1, 3);

            var first = new ExSpan<string>(src, 0, 3);
            int result = first.SequenceCompareTo<string>(segment);
            Assert.Equal(0, result);
        }

        [Fact]
        public static void LengthMismatchSequenceCompareTo_String() {
            string[] a = { "fourth", "fifth", "sixth" };
            var first = new ExSpan<string>(a, 0, 2);
            var second = new ExSpan<string>(a, 0, 3);
            int result = first.SequenceCompareTo<string>(second);
            Assert.True(result < 0);

            result = second.SequenceCompareTo<string>(first);
            Assert.True(result > 0);

            // one sequence is empty
            first = new ExSpan<string>(a, 1, 0);

            result = first.SequenceCompareTo<string>(second);
            Assert.True(result < 0);

            result = second.SequenceCompareTo<string>(first);
            Assert.True(result > 0);
        }

        [Fact]
        public static void SequenceCompareToWithSingleMismatch_String() {
            for (int length = 1; length < 32; length++) {
                for (int mismatchIndex = 0; mismatchIndex < length; mismatchIndex++) {
                    var first = new string[length];
                    var second = new string[length];
                    for (int i = 0; i < length; i++) {
                        first[i] = second[i] = $"item {i + 1}";
                    }

                    second[mismatchIndex] = (string)(second[mismatchIndex] + 1);

                    var firstExSpan = new ExSpan<string>(first);
                    var secondExSpan = new ReadOnlyExSpan<string>(second);
                    int result = firstExSpan.SequenceCompareTo<string>(secondExSpan);
                    Assert.True(result < 0);

                    result = secondExSpan.SequenceCompareTo<string>(firstExSpan);
                    Assert.True(result > 0);
                }
            }
        }

        [Fact]
        public static void SequenceCompareToNoMatch_string() {
            for (int length = 1; length < 32; length++) {
                var first = new string[length];
                var second = new string[length];

                for (int i = 0; i < length; i++) {
                    first[i] = $"item {i + 1}";
                    second[i] = $"item {int.MaxValue - i}";
                }

                var firstExSpan = new ExSpan<string>(first);
                var secondExSpan = new ReadOnlyExSpan<string>(second);
                int result = firstExSpan.SequenceCompareTo<string>(secondExSpan);
                Assert.True(result < 0);

                result = secondExSpan.SequenceCompareTo<string>(firstExSpan);
                Assert.True(result > 0);
            }
        }

        [Fact]
        public static void MakeSureNoSequenceCompareToChecksGoOutOfRange_string() {
            for (int length = 0; length < 100; length++) {
                var first = new string[length + 2];
                first[0] = "99";
                for (int k = 1; k <= length; k++)
                    first[k] = string.Empty;
                first[length + 1] = "99";

                var second = new string[length + 2];
                second[0] = "100";
                for (int k = 1; k <= length; k++)
                    second[k] = string.Empty;
                second[length + 1] = "100";

                var span1 = new ExSpan<string>(first, 1, length);
                var span2 = new ReadOnlyExSpan<string>(second, 1, length);
                int result = span1.SequenceCompareTo<string>(span2);
                Assert.Equal(0, result);
            }
        }
    }
}
