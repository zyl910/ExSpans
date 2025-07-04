using Xunit;

namespace Zyl.ExSpans.Tests.AReadOnlyExSpan {
    using static AComparers;

    public static partial class ALastIndexOfSequence {
        [Fact]
        public static void LastIndexOfSequenceMatchAtStart() {
            var source = new int[] { 5, 1, 77, 2, 3, 77, 77, 4, 5, 77, 77, 77, 88, 6, 6, 77, 77, 88, 9 };
            var value = new int[] { 5, 1, 77 };

            Assert.Equal(0, new ReadOnlyExSpan<int>(source).LastIndexOf(value));
            Assert.All(GetDefaultEqualityComparers<int>(), comparer => Assert.Equal(0, new ReadOnlyExSpan<int>(source).LastIndexOf(value, comparer)));
            Assert.Equal(-1, new ReadOnlyExSpan<int>(source).LastIndexOf(value, GetFalseEqualityComparer<int>()));
        }

        [Fact]
        public static void LastIndexOfSequenceMultipleMatch() {
            var source = new int[] { 1, 2, 3, 1, 2, 3, 1, 2, 3, 1 };
            var value = new int[] { 2, 3 };

            Assert.Equal(7, new ReadOnlyExSpan<int>(source).LastIndexOf(value));
            Assert.All(GetDefaultEqualityComparers<int>(), comparer => Assert.Equal(7, new ReadOnlyExSpan<int>(source).LastIndexOf(value, comparer)));
            Assert.Equal(-1, new ReadOnlyExSpan<int>(source).LastIndexOf(value, GetFalseEqualityComparer<int>()));
        }

        [Fact]
        public static void LastIndexOfSequenceRestart() {
            var source = new int[] { 0, 1, 77, 2, 3, 77, 77, 4, 5, 77, 77, 77, 88, 6, 6, 77, 77, 8, 9, 77, 0, 1 };
            var value = new int[] { 77, 77, 88 };

            Assert.Equal(10, new ReadOnlyExSpan<int>(source).LastIndexOf(value));
            Assert.All(GetDefaultEqualityComparers<int>(), comparer => Assert.Equal(10, new ReadOnlyExSpan<int>(source).LastIndexOf(value, comparer)));
            Assert.Equal(-1, new ReadOnlyExSpan<int>(source).LastIndexOf(value, GetFalseEqualityComparer<int>()));
        }

        [Fact]
        public static void LastIndexOfSequenceNoMatch() {
            var source = new int[] { 0, 1, 77, 2, 3, 77, 77, 4, 5, 77, 77, 77, 88, 6, 6, 77, 77, 88, 9 };
            var value = new int[] { 77, 77, 88, 99 };

            Assert.Equal(-1, new ReadOnlyExSpan<int>(source).LastIndexOf(value));
            Assert.All(GetDefaultEqualityComparers<int>(), comparer => Assert.Equal(-1, new ReadOnlyExSpan<int>(source).LastIndexOf(value, comparer)));
        }

        [Fact]
        public static void LastIndexOfSequenceNotEvenAHeadMatch() {
            var source = new int[] { 0, 1, 77, 2, 3, 77, 77, 4, 5, 77, 77, 77, 88, 6, 6, 77, 77, 88, 9 };
            var value = new int[] { 100, 77, 88, 99 };

            Assert.Equal(-1, new ReadOnlyExSpan<int>(source).LastIndexOf(value));
            Assert.All(GetDefaultEqualityComparers<int>(), comparer => Assert.Equal(-1, new ReadOnlyExSpan<int>(source).LastIndexOf(value, comparer)));
        }

        [Fact]
        public static void LastIndexOfSequenceMatchAtVeryEnd() {
            var source = new int[] { 0, 1, 2, 3, 4, 5 };
            var value = new int[] { 3, 4, 5 };

            Assert.Equal(3, new ReadOnlyExSpan<int>(source).LastIndexOf(value));
            Assert.All(GetDefaultEqualityComparers<int>(), comparer => Assert.Equal(3, new ReadOnlyExSpan<int>(source).LastIndexOf(value, comparer)));
            Assert.Equal(-1, new ReadOnlyExSpan<int>(source).LastIndexOf(value, GetFalseEqualityComparer<int>()));
        }

        [Fact]
        public static void LastIndexOfSequenceJustPastVeryEnd() {
            var source = new int[] { 0, 1, 2, 3, 4, 5 };
            var value = new int[] { 3, 4, 5 };

            Assert.Equal(-1, new ReadOnlyExSpan<int>(source, 0, 5).LastIndexOf(value));
            Assert.All(GetDefaultEqualityComparers<int>(), comparer => Assert.Equal(-1, new ReadOnlyExSpan<int>(source, 0, 5).LastIndexOf(value, comparer)));
        }

        [Fact]
        public static void LastIndexOfSequenceZeroLengthValue() {
            // A zero-length value is always "found" at the end of the source.
            var source = new int[] { 0, 1, 77, 2, 3, 77, 77, 4, 5, 77, 77, 77, 88, 6, 6, 77, 77, 88, 9 };
            var value = ArrayHelper.Empty<int>();

            Assert.Equal(source.Length, new ReadOnlyExSpan<int>(source).LastIndexOf(value));
            Assert.All(GetDefaultEqualityComparers<int>(), comparer => Assert.Equal(source.Length, new ReadOnlyExSpan<int>(source).LastIndexOf(value, comparer)));
        }

        [Fact]
        public static void LastIndexOfSequenceZeroLengthSpan() {
            var source = ArrayHelper.Empty<int>();
            var value = new int[] { 1, 2, 3 };

            Assert.Equal(-1, new ReadOnlyExSpan<int>(source).LastIndexOf(value));
            Assert.All(GetDefaultEqualityComparers<int>(), comparer => Assert.Equal(-1, new ReadOnlyExSpan<int>(source).LastIndexOf(value, comparer)));
        }

        [Fact]
        public static void LastIndexOfSequenceLengthOneValue() {
            var source = new int[] { 0, 1, 2, 3, 4, 5 };
            var value = new int[] { 2 };

            Assert.Equal(2, new ReadOnlyExSpan<int>(source).LastIndexOf(value));
            Assert.All(GetDefaultEqualityComparers<int>(), comparer => Assert.Equal(2, new ReadOnlyExSpan<int>(source).LastIndexOf(value, comparer)));
            Assert.Equal(-1, new ReadOnlyExSpan<int>(source).LastIndexOf(value, GetFalseEqualityComparer<int>()));
        }

        [Fact]
        public static void LastIndexOfSequenceLengthOneValueAtVeryEnd() {
            var source = new int[] { 0, 1, 2, 3, 4, 5 };
            var value = new int[] { 5 };

            Assert.Equal(5, new ReadOnlyExSpan<int>(source).LastIndexOf(value));
            Assert.All(GetDefaultEqualityComparers<int>(), comparer => Assert.Equal(5, new ReadOnlyExSpan<int>(source).LastIndexOf(value, comparer)));
            Assert.Equal(-1, new ReadOnlyExSpan<int>(source).LastIndexOf(value, GetFalseEqualityComparer<int>()));
        }

        [Fact]
        public static void LastIndexOfSequenceLengthOneValueMultipleTimes() {
            var source = new int[] { 0, 1, 5, 3, 4, 5 };
            var value = new int[] { 5 };

            Assert.Equal(5, new ReadOnlyExSpan<int>(source).LastIndexOf(value));
            Assert.All(GetDefaultEqualityComparers<int>(), comparer => Assert.Equal(5, new ReadOnlyExSpan<int>(source).LastIndexOf(value, comparer)));
            Assert.Equal(-1, new ReadOnlyExSpan<int>(source).LastIndexOf(value, GetFalseEqualityComparer<int>()));
        }

        [Fact]
        public static void LastIndexOfSequenceLengthOneValueJustPasttVeryEnd() {
            var source = new int[] { 0, 1, 2, 3, 4, 5 };
            var value = new int[] { 5 };

            Assert.Equal(-1, new ReadOnlyExSpan<int>(source, 0, 5).LastIndexOf(value));
            Assert.All(GetDefaultEqualityComparers<int>(), comparer => Assert.Equal(-1, new ReadOnlyExSpan<int>(source, 0, 5).LastIndexOf(value, comparer)));
        }

        [Fact]
        public static void LastIndexOfSequenceMatchAtStart_String() {
            var source = new string[] { "5", "1", "77", "2", "3", "77", "77", "4", "5", "77", "77", "77", "88", "6", "6", "77", "77", "88", "9" };
            var value = new string[] { "5", "1", "77" };

            Assert.Equal(0, new ReadOnlyExSpan<string>(source).LastIndexOf(value));
            Assert.All(GetDefaultEqualityComparers<string>(), comparer => Assert.Equal(0, new ReadOnlyExSpan<string>(source).LastIndexOf(value, comparer)));
            Assert.Equal(-1, new ReadOnlyExSpan<string>(source).LastIndexOf(value, GetFalseEqualityComparer<string>()));
        }

        [Fact]
        public static void LastIndexOfSequenceMultipleMatch_String() {
            var source = new string[] { "1", "2", "3", "1", "2", "3", "1", "2", "3" };
            var value = new string[] { "2", "3" };

            Assert.Equal(7, new ReadOnlyExSpan<string>(source).LastIndexOf(value));
            Assert.All(GetDefaultEqualityComparers<string>(), comparer => Assert.Equal(7, new ReadOnlyExSpan<string>(source).LastIndexOf(value, comparer)));
            Assert.Equal(-1, new ReadOnlyExSpan<string>(source).LastIndexOf(value, GetFalseEqualityComparer<string>()));
        }

        [Fact]
        public static void LastIndexOfSequenceRestart_String() {
            var source = new string[] { "0", "1", "77", "2", "3", "77", "77", "4", "5", "77", "77", "77", "88", "6", "6", "77", "77", "8", "9", "77", "0", "1" };
            var value = new string[] { "77", "77", "88" };

            Assert.Equal(10, new ReadOnlyExSpan<string>(source).LastIndexOf(value));
            Assert.All(GetDefaultEqualityComparers<string>(), comparer => Assert.Equal(10, new ReadOnlyExSpan<string>(source).LastIndexOf(value, comparer)));
            Assert.Equal(-1, new ReadOnlyExSpan<string>(source).LastIndexOf(value, GetFalseEqualityComparer<string>()));
        }

        [Fact]
        public static void LastIndexOfSequenceNoMatch_String() {
            var source = new string[] { "0", "1", "77", "2", "3", "77", "77", "4", "5", "77", "77", "77", "88", "6", "6", "77", "77", "88", "9" };
            var value = new string[] { "77", "77", "88", "99" };

            Assert.Equal(-1, new ReadOnlyExSpan<string>(source).LastIndexOf(value));
            Assert.All(GetDefaultEqualityComparers<string>(), comparer => Assert.Equal(-1, new ReadOnlyExSpan<string>(source).LastIndexOf(value, comparer)));
        }

        [Fact]
        public static void LastIndexOfSequenceNotEvenAHeadMatch_String() {
            var source = new string[] { "0", "1", "77", "2", "3", "77", "77", "4", "5", "77", "77", "77", "88", "6", "6", "77", "77", "88", "9" };
            var value = new string[] { "100", "77", "88", "99" };

            Assert.Equal(-1, new ReadOnlyExSpan<string>(source).LastIndexOf(value));
            Assert.All(GetDefaultEqualityComparers<string>(), comparer => Assert.Equal(-1, new ReadOnlyExSpan<string>(source).LastIndexOf(value, comparer)));
        }

        [Fact]
        public static void LastIndexOfSequenceMatchAtVeryEnd_String() {
            var source = new string[] { "0", "1", "2", "3", "4", "5" };
            var value = new string[] { "3", "4", "5" };

            Assert.Equal(3, new ReadOnlyExSpan<string>(source).LastIndexOf(value));
            Assert.All(GetDefaultEqualityComparers<string>(), comparer => Assert.Equal(3, new ReadOnlyExSpan<string>(source).LastIndexOf(value, comparer)));
            Assert.Equal(-1, new ReadOnlyExSpan<string>(source).LastIndexOf(value, GetFalseEqualityComparer<string>()));
        }

        [Fact]
        public static void LastIndexOfSequenceJustPastVeryEnd_String() {
            var source = new string[] { "0", "1", "2", "3", "4", "5" };
            var value = new string[] { "3", "4", "5" };

            Assert.Equal(-1, new ReadOnlyExSpan<string>(source, 0, 5).LastIndexOf(value));
            Assert.All(GetDefaultEqualityComparers<string>(), comparer => Assert.Equal(-1, new ReadOnlyExSpan<string>(source, 0, 5).LastIndexOf(value, comparer)));
        }

        [Fact]
        public static void LastIndexOfSequenceZeroLengthValue_String() {
            // A zero-length value is always "found" at the end of the source.
            var source = new string[] { "0", "1", "77", "2", "3", "77", "77", "4", "5", "77", "77", "77", "88", "6", "6", "77", "77", "88", "9" };
            var value = ArrayHelper.Empty<string>();

            Assert.Equal(source.Length, new ReadOnlyExSpan<string>(source).LastIndexOf(value));
            Assert.All(GetDefaultEqualityComparers<string>(), comparer => Assert.Equal(source.Length, new ReadOnlyExSpan<string>(source).LastIndexOf(value, comparer)));
        }

        [Fact]
        public static void LastIndexOfSequenceZeroLengthSpan_String() {
            var source = ArrayHelper.Empty<string>();
            var value = new string[] { "1", "2", "3" };

            Assert.Equal(-1, new ReadOnlyExSpan<string>(source).LastIndexOf(value));
            Assert.All(GetDefaultEqualityComparers<string>(), comparer => Assert.Equal(-1, new ReadOnlyExSpan<string>(source).LastIndexOf(value, comparer)));
        }

        [Fact]
        public static void LastIndexOfSequenceLengthOneValue_String() {
            var source = new string[] { "0", "1", "2", "3", "4", "5" };
            var value = new string[] { "2" };

            Assert.Equal(2, new ReadOnlyExSpan<string>(source).LastIndexOf(value));
            Assert.All(GetDefaultEqualityComparers<string>(), comparer => Assert.Equal(2, new ReadOnlyExSpan<string>(source).LastIndexOf(value, comparer)));
            Assert.Equal(-1, new ReadOnlyExSpan<string>(source).LastIndexOf(value, GetFalseEqualityComparer<string>()));
        }

        [Fact]
        public static void LastIndexOfSequenceLengthOneValueAtVeryEnd_String() {
            var source = new string[] { "0", "1", "2", "3", "4", "5" };
            var value = new string[] { "5" };

            Assert.Equal(5, new ReadOnlyExSpan<string>(source).LastIndexOf(value));
            Assert.All(GetDefaultEqualityComparers<string>(), comparer => Assert.Equal(5, new ReadOnlyExSpan<string>(source).LastIndexOf(value, comparer)));
            Assert.Equal(-1, new ReadOnlyExSpan<string>(source).LastIndexOf(value, GetFalseEqualityComparer<string>()));
        }

        [Fact]
        public static void LastIndexOfSequenceLengthOneValueJustPasttVeryEnd_String() {
            var source = new string[] { "0", "1", "2", "3", "4", "5" };
            var value = new string[] { "5" };

            Assert.Equal(-1, new ReadOnlyExSpan<string>(source, 0, 5).LastIndexOf(value));
            Assert.All(GetDefaultEqualityComparers<string>(), comparer => Assert.Equal(-1, new ReadOnlyExSpan<string>(source, 0, 5).LastIndexOf(value, comparer)));
        }

#nullable disable
        [Theory]
        [MemberData(nameof(TestHelpers.LastIndexOfNullSequenceData), MemberType = typeof(TestHelpers))]
        public static void LastIndexOfNullSequence_String(string[] source, string[] target, int expected) {
            Assert.Equal(expected, new ReadOnlyExSpan<string>(source).LastIndexOf(target));
            Assert.All(GetDefaultEqualityComparers<string>(), comparer => Assert.Equal(expected, new ReadOnlyExSpan<string>(source).LastIndexOf(target, comparer)));
        }
#nullable restore
    }
}
