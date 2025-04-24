using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Zyl.ExSpans.Tests.Fake.Attributes;

namespace Zyl.ExSpans.Tests.AReadOnlyExSpan {
    partial class ASequenceEqual {

        [Fact]
        public static void ZeroLengthSequenceEqual() {
            int[] a = new int[3];

            ReadOnlyExSpan<int> first = new ReadOnlyExSpan<int>(a, (TSize)1, (TSize)0);
            ReadOnlyExSpan<int> second = new ReadOnlyExSpan<int>(a, (TSize)2, (TSize)0);

            Assert.True(first.SequenceEqual(second));
            Assert.True(first.SequenceEqual(second, null));
            Assert.True(first.SequenceEqual(second, EqualityComparer<int>.Default));
        }

        [Fact]
        public static void SameExSpanSequenceEqual() {
            int[] a = { 4, 5, 6 };
            ReadOnlyExSpan<int> span = new ReadOnlyExSpan<int>(a);

            Assert.True(span.SequenceEqual(span));
            Assert.True(span.SequenceEqual(span, null));
            Assert.True(span.SequenceEqual(span, EqualityComparer<int>.Default));
        }

        [Fact]
        public static void LengthMismatchSequenceEqual() {
            int[] a = { 4, 5, 6 };
            ReadOnlyExSpan<int> first = new ReadOnlyExSpan<int>(a, (TSize)0, (TSize)3);
            ReadOnlyExSpan<int> second = new ReadOnlyExSpan<int>(a, (TSize)0, (TSize)2);

            Assert.False(first.SequenceEqual(second));
            Assert.False(first.SequenceEqual(second, null));
            Assert.False(first.SequenceEqual(second, EqualityComparer<int>.Default));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public static void OnSequenceEqualOfEqualExSpansMakeSureEveryElementIsCompared(int mode) {
            for (int length = 0; length < 100; length++) {
                TIntLog log = new TIntLog();

                TInt[] first = new TInt[length];
                TInt[] second = new TInt[length];
                for (int i = 0; i < length; i++) {
                    first[i] = second[i] = new TInt(10 * (i + 1), log);
                }

                ReadOnlyExSpan<TInt> firstExSpan = new ReadOnlyExSpan<TInt>(first);
                ReadOnlyExSpan<TInt> secondExSpan = new ReadOnlyExSpan<TInt>(second);

                Assert.True(mode switch {
                    0 => firstExSpan.SequenceEqual(secondExSpan),
                    1 => firstExSpan.SequenceEqual(secondExSpan, null),
                    _ => firstExSpan.SequenceEqual(secondExSpan, EqualityComparer<TInt>.Default)
                });

                // Make sure each element of the array was compared once. (Strictly speaking, it would not be illegal for
                // SequenceEqual to compare an element more than once but that would be a non-optimal implementation and
                // a red flag. So we'll stick with the stricter test.)
                Assert.Equal(first.Length, log.Count);
                foreach (TInt elem in first) {
                    int numCompares = log.CountCompares(elem.Value, elem.Value);
                    Assert.True(numCompares == 1, $"Expected {numCompares} == 1 for element {elem.Value}.");
                }
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public static void SequenceEqualNoMatch(int mode) {
            for (int length = 1; length < 32; length++) {
                for (int mismatchIndex = 0; mismatchIndex < length; mismatchIndex++) {
                    TIntLog log = new TIntLog();

                    TInt[] first = new TInt[length];
                    TInt[] second = new TInt[length];
                    for (int i = 0; i < length; i++) {
                        first[i] = second[i] = new TInt(10 * (i + 1), log);
                    }

                    second[mismatchIndex] = new TInt(second[mismatchIndex].Value + 1, log);

                    ReadOnlyExSpan<TInt> firstExSpan = new ReadOnlyExSpan<TInt>(first);
                    ReadOnlyExSpan<TInt> secondExSpan = new ReadOnlyExSpan<TInt>(second);

                    Assert.False(mode switch {
                        0 => firstExSpan.SequenceEqual(secondExSpan),
                        1 => firstExSpan.SequenceEqual(secondExSpan, null),
                        _ => firstExSpan.SequenceEqual(secondExSpan, EqualityComparer<TInt>.Default)
                    });

                    Assert.Equal(1, log.CountCompares(first[mismatchIndex].Value, second[mismatchIndex].Value));
                }
            }
        }

        [Fact]
        public static void MakeSureNoSequenceEqualChecksGoOutOfRange() {
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

                ReadOnlyExSpan<TInt> firstExSpan = new ReadOnlyExSpan<TInt>(first, (TSize)GuardLength, (TSize)length);
                ReadOnlyExSpan<TInt> secondExSpan = new ReadOnlyExSpan<TInt>(second, (TSize)GuardLength, (TSize)length);

                Assert.True(firstExSpan.SequenceEqual(secondExSpan));
                Assert.True(firstExSpan.SequenceEqual(secondExSpan, null));
                Assert.True(firstExSpan.SequenceEqual(secondExSpan, EqualityComparer<TInt>.Default));
            }
        }

        [Theory]
        [MemberData(nameof(TestHelpers.SequenceEqualsNullData), MemberType = typeof(TestHelpers))]
        public static void SequenceEqualsNullData_String(string?[]? firstInput, string?[]? secondInput, bool expected) {
            ReadOnlyExSpan<string?> theStrings = firstInput;

            Assert.Equal(expected, theStrings.SequenceEqual(secondInput));
            Assert.Equal(expected, theStrings.SequenceEqual(secondInput, null));
            Assert.Equal(expected, theStrings.SequenceEqual(secondInput, EqualityComparer<string?>.Default));
        }

#if NET8_0_OR_GREATER

        [Fact]
        public static void SequenceEqual_AlwaysTrueComparer() {
            EqualityComparer<int> alwaysTrueComparer = EqualityComparer<int>.Create((x, y) => true);
            Assert.False(((ReadOnlyExSpan<int>)new int[1]).SequenceEqual(new int[2], alwaysTrueComparer));
            Assert.True(((ReadOnlyExSpan<int>)new int[2]).SequenceEqual(new int[2], alwaysTrueComparer));
            Assert.True(((ReadOnlyExSpan<int>)new int[2] { 1, 3 }).SequenceEqual(new int[2] { 2, 4 }, alwaysTrueComparer));
        }

        [Fact]
        public static void SequenceEqual_AlwaysFalseComparer() {
            EqualityComparer<int> alwaysFalseComparer = EqualityComparer<int>.Create((x, y) => false);
            Assert.False(((ReadOnlyExSpan<int>)new int[1]).SequenceEqual(new int[2], alwaysFalseComparer));
            Assert.False(((ReadOnlyExSpan<int>)new int[1]).SequenceEqual(new int[2], alwaysFalseComparer));
            Assert.False(((ReadOnlyExSpan<int>)new int[2] { 1, 3 }).SequenceEqual(new int[2] { 2, 4 }, alwaysFalseComparer));
        }

#endif // NET8_0_OR_GREATER

        [Fact]
        public static void SequenceEqual_IgnoreCaseComparer() {
            string[] lower = new[] { "hello", "world" };
            string[] upper = new[] { "HELLO", "WORLD" };
            string[] different = new[] { "hello", "wurld" };

            Assert.True(((ReadOnlyExSpan<string>)lower).SequenceEqual(lower));
            Assert.False(((ReadOnlyExSpan<string>)lower).SequenceEqual(upper));
            Assert.True(((ReadOnlyExSpan<string>)upper).SequenceEqual(upper));

            Assert.True(((ReadOnlyExSpan<string>)lower).SequenceEqual(lower, StringComparer.OrdinalIgnoreCase));
            Assert.True(((ReadOnlyExSpan<string>)lower).SequenceEqual(upper, StringComparer.OrdinalIgnoreCase));
            Assert.True(((ReadOnlyExSpan<string>)upper).SequenceEqual(upper, StringComparer.OrdinalIgnoreCase));

            Assert.False(((ReadOnlyExSpan<string>)lower).SequenceEqual(different));
            Assert.False(((ReadOnlyExSpan<string>)lower).SequenceEqual(different, StringComparer.OrdinalIgnoreCase));
        }

    }
}
