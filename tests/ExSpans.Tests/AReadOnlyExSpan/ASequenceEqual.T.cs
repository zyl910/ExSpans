using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Zyl.SizableSpans.Tests.Fake.Attributes;

namespace Zyl.SizableSpans.Tests.AReadOnlySizableSpan {
    partial class ASequenceEqual {

        [Fact]
        public static void ZeroLengthSequenceEqual() {
            int[] a = new int[3];

            ReadOnlySizableSpan<int> first = new ReadOnlySizableSpan<int>(a, (TSize)1, (TSize)0);
            ReadOnlySizableSpan<int> second = new ReadOnlySizableSpan<int>(a, (TSize)2, (TSize)0);

            Assert.True(first.SequenceEqual(second));
            Assert.True(first.SequenceEqual(second, null));
            Assert.True(first.SequenceEqual(second, EqualityComparer<int>.Default));
        }

        [Fact]
        public static void SameSizableSpanSequenceEqual() {
            int[] a = { 4, 5, 6 };
            ReadOnlySizableSpan<int> span = new ReadOnlySizableSpan<int>(a);

            Assert.True(span.SequenceEqual(span));
            Assert.True(span.SequenceEqual(span, null));
            Assert.True(span.SequenceEqual(span, EqualityComparer<int>.Default));
        }

        [Fact]
        public static void LengthMismatchSequenceEqual() {
            int[] a = { 4, 5, 6 };
            ReadOnlySizableSpan<int> first = new ReadOnlySizableSpan<int>(a, (TSize)0, (TSize)3);
            ReadOnlySizableSpan<int> second = new ReadOnlySizableSpan<int>(a, (TSize)0, (TSize)2);

            Assert.False(first.SequenceEqual(second));
            Assert.False(first.SequenceEqual(second, null));
            Assert.False(first.SequenceEqual(second, EqualityComparer<int>.Default));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public static void OnSequenceEqualOfEqualSizableSpansMakeSureEveryElementIsCompared(int mode) {
            for (int length = 0; length < 100; length++) {
                TIntLog log = new TIntLog();

                TInt[] first = new TInt[length];
                TInt[] second = new TInt[length];
                for (int i = 0; i < length; i++) {
                    first[i] = second[i] = new TInt(10 * (i + 1), log);
                }

                ReadOnlySizableSpan<TInt> firstSizableSpan = new ReadOnlySizableSpan<TInt>(first);
                ReadOnlySizableSpan<TInt> secondSizableSpan = new ReadOnlySizableSpan<TInt>(second);

                Assert.True(mode switch {
                    0 => firstSizableSpan.SequenceEqual(secondSizableSpan),
                    1 => firstSizableSpan.SequenceEqual(secondSizableSpan, null),
                    _ => firstSizableSpan.SequenceEqual(secondSizableSpan, EqualityComparer<TInt>.Default)
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

                    ReadOnlySizableSpan<TInt> firstSizableSpan = new ReadOnlySizableSpan<TInt>(first);
                    ReadOnlySizableSpan<TInt> secondSizableSpan = new ReadOnlySizableSpan<TInt>(second);

                    Assert.False(mode switch {
                        0 => firstSizableSpan.SequenceEqual(secondSizableSpan),
                        1 => firstSizableSpan.SequenceEqual(secondSizableSpan, null),
                        _ => firstSizableSpan.SequenceEqual(secondSizableSpan, EqualityComparer<TInt>.Default)
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

                ReadOnlySizableSpan<TInt> firstSizableSpan = new ReadOnlySizableSpan<TInt>(first, (TSize)GuardLength, (TSize)length);
                ReadOnlySizableSpan<TInt> secondSizableSpan = new ReadOnlySizableSpan<TInt>(second, (TSize)GuardLength, (TSize)length);

                Assert.True(firstSizableSpan.SequenceEqual(secondSizableSpan));
                Assert.True(firstSizableSpan.SequenceEqual(secondSizableSpan, null));
                Assert.True(firstSizableSpan.SequenceEqual(secondSizableSpan, EqualityComparer<TInt>.Default));
            }
        }

        [Theory]
        [MemberData(nameof(TestHelpers.SequenceEqualsNullData), MemberType = typeof(TestHelpers))]
        public static void SequenceEqualsNullData_String(string?[]? firstInput, string?[]? secondInput, bool expected) {
            ReadOnlySizableSpan<string?> theStrings = firstInput;

            Assert.Equal(expected, theStrings.SequenceEqual(secondInput));
            Assert.Equal(expected, theStrings.SequenceEqual(secondInput, null));
            Assert.Equal(expected, theStrings.SequenceEqual(secondInput, EqualityComparer<string?>.Default));
        }

#if NET8_0_OR_GREATER

        [Fact]
        public static void SequenceEqual_AlwaysTrueComparer() {
            EqualityComparer<int> alwaysTrueComparer = EqualityComparer<int>.Create((x, y) => true);
            Assert.False(((ReadOnlySizableSpan<int>)new int[1]).SequenceEqual(new int[2], alwaysTrueComparer));
            Assert.True(((ReadOnlySizableSpan<int>)new int[2]).SequenceEqual(new int[2], alwaysTrueComparer));
            Assert.True(((ReadOnlySizableSpan<int>)new int[2] { 1, 3 }).SequenceEqual(new int[2] { 2, 4 }, alwaysTrueComparer));
        }

        [Fact]
        public static void SequenceEqual_AlwaysFalseComparer() {
            EqualityComparer<int> alwaysFalseComparer = EqualityComparer<int>.Create((x, y) => false);
            Assert.False(((ReadOnlySizableSpan<int>)new int[1]).SequenceEqual(new int[2], alwaysFalseComparer));
            Assert.False(((ReadOnlySizableSpan<int>)new int[1]).SequenceEqual(new int[2], alwaysFalseComparer));
            Assert.False(((ReadOnlySizableSpan<int>)new int[2] { 1, 3 }).SequenceEqual(new int[2] { 2, 4 }, alwaysFalseComparer));
        }

#endif // NET8_0_OR_GREATER

        [Fact]
        public static void SequenceEqual_IgnoreCaseComparer() {
            string[] lower = new[] { "hello", "world" };
            string[] upper = new[] { "HELLO", "WORLD" };
            string[] different = new[] { "hello", "wurld" };

            Assert.True(((ReadOnlySizableSpan<string>)lower).SequenceEqual(lower));
            Assert.False(((ReadOnlySizableSpan<string>)lower).SequenceEqual(upper));
            Assert.True(((ReadOnlySizableSpan<string>)upper).SequenceEqual(upper));

            Assert.True(((ReadOnlySizableSpan<string>)lower).SequenceEqual(lower, StringComparer.OrdinalIgnoreCase));
            Assert.True(((ReadOnlySizableSpan<string>)lower).SequenceEqual(upper, StringComparer.OrdinalIgnoreCase));
            Assert.True(((ReadOnlySizableSpan<string>)upper).SequenceEqual(upper, StringComparer.OrdinalIgnoreCase));

            Assert.False(((ReadOnlySizableSpan<string>)lower).SequenceEqual(different));
            Assert.False(((ReadOnlySizableSpan<string>)lower).SequenceEqual(different, StringComparer.OrdinalIgnoreCase));
        }

    }
}
