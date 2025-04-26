using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Zyl.ExSpans.Tests.AExSpan {
    partial class ASequenceEqual {

        [Fact]
        public static void ZeroLengthSequenceEqual() {
            int[] a = new int[3];

            ExSpan<int> first = new ExSpan<int>(a, (TSize)1, (TSize)0);
            ExSpan<int> second = new ExSpan<int>(a, (TSize)2, (TSize)0);

            Assert.True(first.SequenceEqual(second));
            Assert.True(first.SequenceEqual(second, null));
            Assert.True(first.SequenceEqual(second, EqualityComparer<int>.Default));
        }

        [Fact]
        public static void SameExSpanSequenceEqual() {
            int[] a = { 4, 5, 6 };
            ExSpan<int> span = new ExSpan<int>(a);

            Assert.True(span.SequenceEqual(span));
            Assert.True(span.SequenceEqual(span, null));
            Assert.True(span.SequenceEqual(span, EqualityComparer<int>.Default));
        }

        [Fact]
        public static void LengthMismatchSequenceEqual() {
            int[] a = { 4, 5, 6 };
            ExSpan<int> first = new ExSpan<int>(a, (TSize)0, (TSize)3);
            ExSpan<int> second = new ExSpan<int>(a, (TSize)0, (TSize)2);

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

                ExSpan<TInt> firstExSpan = new ExSpan<TInt>(first);
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

                    ExSpan<TInt> firstExSpan = new ExSpan<TInt>(first);
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

                ExSpan<TInt> firstExSpan = new ExSpan<TInt>(first, (TSize)GuardLength, (TSize)length);
                ExSpan<TInt> secondExSpan = new ExSpan<TInt>(second, (TSize)GuardLength, (TSize)length);

                Assert.True(firstExSpan.SequenceEqual(secondExSpan));
                Assert.True(firstExSpan.SequenceEqual(secondExSpan, null));
                Assert.True(firstExSpan.SequenceEqual(secondExSpan, EqualityComparer<TInt>.Default));
            }
        }

        [Theory]
        [MemberData(nameof(TestHelpers.SequenceEqualsNullData), MemberType = typeof(TestHelpers))]
        public static void SequenceEqualsNullData_String(string?[]? firstInput, string?[]? secondInput, bool expected) {
            ExSpan<string?> theStrings = firstInput;

            Assert.Equal(expected, theStrings.SequenceEqual(secondInput));
            Assert.Equal(expected, theStrings.SequenceEqual((ReadOnlyExSpan<string?>)secondInput));

            Assert.Equal(expected, theStrings.SequenceEqual(secondInput, null));
            Assert.Equal(expected, theStrings.SequenceEqual((ReadOnlyExSpan<string?>)secondInput, null));

            Assert.Equal(expected, theStrings.SequenceEqual(secondInput, EqualityComparer<string?>.Default));
            Assert.Equal(expected, theStrings.SequenceEqual((ReadOnlyExSpan<string?>)secondInput, EqualityComparer<string?>.Default));
        }

        [Theory]
        [InlineData(100)]
        public static void SequenceEquals_OverriddenEqualsReturnsFalse_EqualsFalse(int length) {
            ExSpan<StructOverridingEqualsToAlwaysReturnFalse> span1 = Enumerable.Range(0, length).Select(i => new StructOverridingEqualsToAlwaysReturnFalse()).ToArray();
            Assert.False(span1.SequenceEqual(span1.ToArray()));

            ExSpan<StructImplementingIEquatableToAlwaysReturnFalse> span2 = Enumerable.Range(0, length).Select(i => new StructImplementingIEquatableToAlwaysReturnFalse()).ToArray();
            Assert.False(span2.SequenceEqual(span2.ToArray()));
        }

        private struct StructOverridingEqualsToAlwaysReturnFalse {
            public override bool Equals([NotNullWhen(true)] object? obj) => false;
            public override int GetHashCode() => 0;
        }

        private struct StructImplementingIEquatableToAlwaysReturnFalse : IEquatable<StructImplementingIEquatableToAlwaysReturnFalse> {
            public bool Equals(StructImplementingIEquatableToAlwaysReturnFalse other) => false;
        }

        [Theory]
        [InlineData(100)]
        public static void SequenceEquals_StructWithOddFieldSize_EqualsAsExpected(int length) {
            ExSpan<StructWithOddFieldSize> span1 = new StructWithOddFieldSize[length];
            ExSpan<StructWithOddFieldSize> span2 = new StructWithOddFieldSize[length];

            ExMemoryMarshal.AsBytes(span1).Fill((byte)0);
            ExMemoryMarshal.AsBytes(span2).Fill((byte)0xFF);

            for (int i = 0; i < length; i++) {
                nint ni = (TSize)i;
                span1[ni].Value1 = span2[ni].Value1 = (byte)i;
                span1[ni].Value2 = span2[ni].Value2 = (byte)(i * 2);
                span1[ni].Value3 = span2[ni].Value3 = (byte)(i * 3);
            }

            Assert.True(span1.SequenceEqual(span2));
            Assert.True(span2.SequenceEqual(span1));

            span1[(TSize)(length / 2)].Value2++;

            Assert.False(span1.SequenceEqual(span2));
            Assert.False(span2.SequenceEqual(span1));
        }

        private struct StructWithOddFieldSize {
            public byte Value1, Value2, Value3;
        }

        [Theory]
        [InlineData(100)]
        public static void SequenceEquals_StructWithOddFieldSizeAndIEquatable_EqualsAsExpected(int length) {
            ExSpan<StructWithOddFieldSizeAndIEquatable> span1 = new StructWithOddFieldSizeAndIEquatable[length];
            ExSpan<StructWithOddFieldSizeAndIEquatable> span2 = new StructWithOddFieldSizeAndIEquatable[length];

            ExMemoryMarshal.AsBytes(span1).Fill((byte)0);
            ExMemoryMarshal.AsBytes(span2).Fill((byte)0xFF);

            for (int i = 0; i < length; i++) {
                nint ni = (TSize)i;
                span1[ni].Value1 = span2[ni].Value1 = (byte)i;
                span1[ni].Value2 = span2[ni].Value2 = (byte)(i * 2);
                span1[ni].Value3 = span2[ni].Value3 = (byte)(i * 3);
            }

            Assert.True(span1.SequenceEqual(span2));
            Assert.True(span2.SequenceEqual(span1));

            span1[(TSize)(length / 2)].Value2++;

            Assert.False(span1.SequenceEqual(span2));
            Assert.False(span2.SequenceEqual(span1));
        }

        private struct StructWithOddFieldSizeAndIEquatable : IEquatable<StructWithOddFieldSizeAndIEquatable> {
            public int Value1;
            public short Value2;
            public byte Value3;

            public bool Equals(StructWithOddFieldSizeAndIEquatable other) =>
                Value1 == other.Value1 &&
                Value2 == other.Value2 &&
                Value3 == other.Value3;

            public override bool Equals([NotNullWhen(true)] object? obj) =>
                obj is StructWithOddFieldSizeAndIEquatable other &&
                Equals(other);

            public override int GetHashCode() =>
                Value1.GetHashCode() ^ Value2.GetHashCode() ^ Value3.GetHashCode(); //HashCode.Combine(Value1, Value2, Value3);
        }

        [Theory]
        [InlineData(100)]
        public static void SequenceEquals_StructWithExplicitFieldSizeAndNoFields_EqualsAsExpected(int length) {
            ExSpan<StructWithExplicitFieldSizeAndNoFields> span1 = new StructWithExplicitFieldSizeAndNoFields[length];
            ExSpan<StructWithExplicitFieldSizeAndNoFields> span2 = new StructWithExplicitFieldSizeAndNoFields[length];

            ExMemoryMarshal.AsBytes(span1).Fill((byte)0);
            ExMemoryMarshal.AsBytes(span2).Fill((byte)0xFF);

            Assert.True(span1.SequenceEqual(span2));
            Assert.True(span2.SequenceEqual(span1));
        }

        [StructLayout(LayoutKind.Sequential, Size = 64)]
        private struct StructWithExplicitFieldSizeAndNoFields {
        }

        [Theory]
        [InlineData(100)]
        public static void SequenceEquals_StructWithExplicitFieldSizeAndFields_EqualsAsExpected(int length) {
            ExSpan<StructWithExplicitFieldSizeAndFields> span1 = new StructWithExplicitFieldSizeAndFields[length];
            ExSpan<StructWithExplicitFieldSizeAndFields> span2 = new StructWithExplicitFieldSizeAndFields[length];

            ExMemoryMarshal.AsBytes(span1).Fill((byte)0);
            ExMemoryMarshal.AsBytes(span2).Fill((byte)0xFF);

            for (int i = 0; i < length; i++) {
                span1[(TSize)i].Value = span2[(TSize)i].Value = i;
            }

            Assert.True(span1.SequenceEqual(span2));
            Assert.True(span2.SequenceEqual(span1));

            span1[(TSize)(length / 2)].Value++;

            Assert.False(span1.SequenceEqual(span2));
            Assert.False(span2.SequenceEqual(span1));
        }

        [StructLayout(LayoutKind.Sequential, Size = 64)]
        private struct StructWithExplicitFieldSizeAndFields {
            public int Value;
        }

        [Theory]
        [InlineData(100)]
        public static void SequenceEquals_StructWithDoubleField_EqualsAsExpected(int length) {
            ExSpan<StructWithDoubleField> span1 = new StructWithDoubleField[length];
            ExSpan<StructWithDoubleField> span2 = new StructWithDoubleField[length];

            ExMemoryMarshal.AsBytes(span1).Fill((byte)0);
            ExMemoryMarshal.AsBytes(span2).Fill((byte)0xFF);

            for (int i = 0; i < length; i++) {
                span1[(TSize)i].Value = span2[(TSize)i].Value = i;
            }

            Assert.True(span1.SequenceEqual(span2));
            Assert.True(span2.SequenceEqual(span1));

            span1[(TSize)(length / 2)].Value++;

            Assert.False(span1.SequenceEqual(span2));
            Assert.False(span2.SequenceEqual(span1));
        }

        private struct StructWithDoubleField {
            public double Value;
        }

    }
}
