using Xunit;

namespace Zyl.ExSpans.Tests.AReadOnlyExSpan {
    using static AComparers;

    public static partial class AIndexOfSequence {
        [Fact]
        public static void IndexOfSequenceMatchAtStart() {
            var source = new int[] { 5, 1, 77, 2, 3, 77, 77, 4, 5, 77, 77, 77, 88, 6, 6, 77, 77, 88, 9 };
            var value = new int[] { 5, 1, 77 };

            Assert.Equal(0, new ReadOnlyExSpan<int>(source).IndexOf(value));
            Assert.All(GetDefaultEqualityComparers<int>(), comparer => Assert.Equal(0, new ReadOnlyExSpan<int>(source).IndexOf(value, comparer)));
            Assert.Equal(-1, new ReadOnlyExSpan<int>(source).IndexOf(value, GetFalseEqualityComparer<int>()));
        }

        [Fact]
        public static void IndexOfSequenceMultipleMatch() {
            var source = new int[] { 1, 2, 3, 1, 2, 3, 1, 2, 3 };
            var value = new int[] { 2, 3 };

            Assert.Equal(1, new ReadOnlyExSpan<int>(source).IndexOf(value));
            Assert.All(GetDefaultEqualityComparers<int>(), comparer => Assert.Equal(1, new ReadOnlyExSpan<int>(source).IndexOf(value, comparer)));
            Assert.Equal(-1, new ReadOnlyExSpan<int>(source).IndexOf(value, GetFalseEqualityComparer<int>()));
        }

        [Fact]
        public static void IndexOfSequenceRestart() {
            var source = new int[] { 0, 1, 77, 2, 3, 77, 77, 4, 5, 77, 77, 77, 88, 6, 6, 77, 77, 88, 9 };
            var value = new int[] { 77, 77, 88 };

            Assert.Equal(10, new ReadOnlyExSpan<int>(source).IndexOf(value));
            Assert.All(GetDefaultEqualityComparers<int>(), comparer => Assert.Equal(10, new ReadOnlyExSpan<int>(source).IndexOf(value, comparer)));
            Assert.Equal(-1, new ReadOnlyExSpan<int>(source).IndexOf(value, GetFalseEqualityComparer<int>()));
        }

        [Fact]
        public static void IndexOfSequenceNoMatch() {
            var source = new int[] { 0, 1, 77, 2, 3, 77, 77, 4, 5, 77, 77, 77, 88, 6, 6, 77, 77, 88, 9 };
            var value = new int[] { 77, 77, 88, 99 };

            Assert.Equal(-1, new ReadOnlyExSpan<int>(source).IndexOf(value));
            Assert.All(GetDefaultEqualityComparers<int>(), comparer => Assert.Equal(-1, new ReadOnlyExSpan<int>(source).IndexOf(value, comparer)));
        }

        [Fact]
        public static void IndexOfSequenceNotEvenAHeadMatch() {
            var source = new int[] { 0, 1, 77, 2, 3, 77, 77, 4, 5, 77, 77, 77, 88, 6, 6, 77, 77, 88, 9 };
            var value = new int[] { 100, 77, 88, 99 };

            Assert.Equal(-1, new ReadOnlyExSpan<int>(source).IndexOf(value));
            Assert.All(GetDefaultEqualityComparers<int>(), comparer => Assert.Equal(-1, new ReadOnlyExSpan<int>(source).IndexOf(value, comparer)));
        }

        [Fact]
        public static void IndexOfSequenceMatchAtVeryEnd() {
            var source = new int[] { 0, 1, 2, 3, 4, 5 };
            var value = new int[] { 3, 4, 5 };

            Assert.Equal(3, new ReadOnlyExSpan<int>(source).IndexOf(value));
            Assert.All(GetDefaultEqualityComparers<int>(), comparer => Assert.Equal(3, new ReadOnlyExSpan<int>(source).IndexOf(value, comparer)));
        }

        [Fact]
        public static void IndexOfSequenceJustPastVeryEnd() {
            var source = new int[] { 0, 1, 2, 3, 4, 5 };
            var value = new int[] { 3, 4, 5 };

            Assert.Equal(-1, new ReadOnlyExSpan<int>(source, 0, 5).IndexOf(value));
            Assert.All(GetDefaultEqualityComparers<int>(), comparer => Assert.Equal(-1, new ReadOnlyExSpan<int>(source, 0, 5).IndexOf(value, comparer)));
        }

        [Fact]
        public static void IndexOfSequenceZeroLengthValue() {
            // A zero-length value is always "found" at the start of the source.
            var source = new int[] { 0, 1, 77, 2, 3, 77, 77, 4, 5, 77, 77, 77, 88, 6, 6, 77, 77, 88, 9 };
            var value = ArrayHelper.Empty<int>();

            Assert.Equal(0, new ReadOnlyExSpan<int>(source).IndexOf(value));
            Assert.All(GetDefaultEqualityComparers<int>(), comparer => Assert.Equal(0, new ReadOnlyExSpan<int>(source).IndexOf(value, comparer)));
        }

        [Fact]
        public static void IndexOfSequenceZeroLengthSpan() {
            var source = ArrayHelper.Empty<int>();
            var value = new int[] { 1, 2, 3 };

            Assert.Equal(-1, new ReadOnlyExSpan<int>(source).IndexOf(value));
            Assert.All(GetDefaultEqualityComparers<int>(), comparer => Assert.Equal(-1, new ReadOnlyExSpan<int>(source).IndexOf(value, comparer)));
        }

        [Fact]
        public static void IndexOfSequenceLengthOneValue() {
            var source = new int[] { 0, 1, 2, 3, 4, 5 };
            var value = new int[] { 2 };

            Assert.Equal(2, new ReadOnlyExSpan<int>(source).IndexOf(value));
            Assert.All(GetDefaultEqualityComparers<int>(), comparer => Assert.Equal(2, new ReadOnlyExSpan<int>(source).IndexOf(value, comparer)));
            Assert.Equal(-1, new ReadOnlyExSpan<int>(source).IndexOf(value, GetFalseEqualityComparer<int>()));
        }

        [Fact]
        public static void IndexOfSequenceLengthOneValueAtVeryEnd() {
            var source = new int[] { 0, 1, 2, 3, 4, 5 };
            var value = new int[] { 5 };

            Assert.Equal(5, new ReadOnlyExSpan<int>(source).IndexOf(value));
            Assert.All(GetDefaultEqualityComparers<int>(), comparer => Assert.Equal(5, new ReadOnlyExSpan<int>(source).IndexOf(value, comparer)));
            Assert.Equal(-1, new ReadOnlyExSpan<int>(source).IndexOf(value, GetFalseEqualityComparer<int>()));
        }

        [Fact]
        public static void IndexOfSequenceLengthOneValueJustPasttVeryEnd() {
            var source = new int[] { 0, 1, 2, 3, 4, 5 };
            var value = new int[] { 5 };

            Assert.Equal(-1, new ReadOnlyExSpan<int>(source, 0, 5).IndexOf(value));
            Assert.All(GetDefaultEqualityComparers<int>(), comparer => Assert.Equal(-1, new ReadOnlyExSpan<int>(source, 0, 5).IndexOf(value, comparer)));
        }

        [Fact]
        public static void IndexOfSequenceMatchAtStart_String() {
            var source = new string[] { "5", "1", "77", "2", "3", "77", "77", "4", "5", "77", "77", "77", "88", "6", "6", "77", "77", "88", "9" };
            var value = new string[] { "5", "1", "77" };

            Assert.Equal(0, new ReadOnlyExSpan<string>(source).IndexOf(value));
            Assert.All(GetDefaultEqualityComparers<string>(), comparer => Assert.Equal(0, new ReadOnlyExSpan<string>(source).IndexOf(value, comparer)));
            Assert.Equal(-1, new ReadOnlyExSpan<string>(source).IndexOf(value, GetFalseEqualityComparer<string>()));
        }

        [Fact]
        public static void IndexOfSequenceMultipleMatch_String() {
            var source = new string[] { "1", "2", "3", "1", "2", "3", "1", "2", "3" };
            var value = new string[] { "2", "3" };

            Assert.Equal(1, new ReadOnlyExSpan<string>(source).IndexOf(value));
            Assert.All(GetDefaultEqualityComparers<string>(), comparer => Assert.Equal(1, new ReadOnlyExSpan<string>(source).IndexOf(value, comparer)));
            Assert.Equal(-1, new ReadOnlyExSpan<string>(source).IndexOf(value, GetFalseEqualityComparer<string>()));
        }

        [Fact]
        public static void IndexOfSequenceRestart_String() {
            var source = new string[] { "0", "1", "77", "2", "3", "77", "77", "4", "5", "77", "77", "77", "88", "6", "6", "77", "77", "88", "9" };
            var value = new string[] { "77", "77", "88" };

            Assert.Equal(10, source.AsExSpan().IndexOf(value));
            Assert.All(GetDefaultEqualityComparers<string>(), comparer => Assert.Equal(10, source.AsExSpan().AsReadOnlyExSpan().IndexOf(value, comparer)));
            Assert.Equal(-1, source.AsExSpan().AsReadOnlyExSpan().IndexOf(value.AsExSpan(), GetFalseEqualityComparer<string>()));
        }

        [Fact]
        public static void IndexOfSequenceNoMatch_String() {
            var source = new string[] { "0", "1", "77", "2", "3", "77", "77", "4", "5", "77", "77", "77", "88", "6", "6", "77", "77", "88", "9" };
            var value = new string[] { "77", "77", "88", "99" };

            Assert.Equal(-1, new ReadOnlyExSpan<string>(source).IndexOf(value));
            Assert.All(GetDefaultEqualityComparers<string>(), comparer => Assert.Equal(-1, new ReadOnlyExSpan<string>(source).IndexOf(value, comparer)));
        }

        [Fact]
        public static void IndexOfSequenceNotEvenAHeadMatch_String() {
            var source = new string[] { "0", "1", "77", "2", "3", "77", "77", "4", "5", "77", "77", "77", "88", "6", "6", "77", "77", "88", "9" };
            var value = new string[] { "100", "77", "88", "99" };

            Assert.Equal(-1, new ReadOnlyExSpan<string>(source).IndexOf(value));
            Assert.All(GetDefaultEqualityComparers<string>(), comparer => Assert.Equal(-1, new ReadOnlyExSpan<string>(source).IndexOf(value, comparer)));
        }

        [Fact]
        public static void IndexOfSequenceMatchAtVeryEnd_String() {
            var source = new string[] { "0", "1", "2", "3", "4", "5" };
            var value = new string[] { "3", "4", "5" };

            Assert.Equal(3, new ReadOnlyExSpan<string>(source).IndexOf(value));
            Assert.All(GetDefaultEqualityComparers<string>(), comparer => Assert.Equal(3, new ReadOnlyExSpan<string>(source).IndexOf(value, comparer)));
            Assert.Equal(-1, new ReadOnlyExSpan<string>(source).IndexOf(value, GetFalseEqualityComparer<string>()));
        }

        [Fact]
        public static void IndexOfSequenceJustPastVeryEnd_String() {
            var source = new string[] { "0", "1", "2", "3", "4", "5" };
            var value = new string[] { "3", "4", "5" };

            Assert.Equal(-1, source.AsExSpan(0, 5).IndexOf(value));
            Assert.All(GetDefaultEqualityComparers<string>(), comparer => Assert.Equal(-1, source.AsExSpan(0, 5).AsReadOnlyExSpan().IndexOf(value, comparer)));
        }

        [Fact]
        public static void IndexOfSequenceZeroLengthValue_String() {
            // A zero-length value is always "found" at the start of the source.
            var source = new string[] { "0", "1", "77", "2", "3", "77", "77", "4", "5", "77", "77", "77", "88", "6", "6", "77", "77", "88", "9" };
            var value = ArrayHelper.Empty<string>();

            Assert.Equal(0, new ReadOnlyExSpan<string>(source).IndexOf(value));
            Assert.All(GetDefaultEqualityComparers<string>(), comparer => Assert.Equal(0, new ReadOnlyExSpan<string>(source).IndexOf(value, comparer)));
        }

        [Fact]
        public static void IndexOfSequenceZeroLengthSpan_String() {
            var source = ArrayHelper.Empty<string>();
            var value = new string[] { "1", "2", "3" };

            Assert.Equal(-1, new ReadOnlyExSpan<string>(source).IndexOf(value));
            Assert.All(GetDefaultEqualityComparers<string>(), comparer => Assert.Equal(-1, new ReadOnlyExSpan<string>(source).IndexOf(value, comparer)));
        }

        [Fact]
        public static void IndexOfSequenceLengthOneValue_String() {
            var source = new string[] { "0", "1", "2", "3", "4", "5" };
            var value = new string[] { "2" };

            Assert.Equal(2, new ReadOnlyExSpan<string>(source).IndexOf(value));
            Assert.All(GetDefaultEqualityComparers<string>(), comparer => Assert.Equal(2, new ReadOnlyExSpan<string>(source).IndexOf(value, comparer)));
            Assert.Equal(-1, new ReadOnlyExSpan<string>(source).IndexOf(value, GetFalseEqualityComparer<string>()));
        }

        [Fact]
        public static void IndexOfSequenceLengthOneValueAtVeryEnd_String() {
            var source = new string[] { "0", "1", "2", "3", "4", "5" };
            var value = new string[] { "5" };

            Assert.Equal(5, new ReadOnlyExSpan<string>(source).IndexOf(value));
            Assert.All(GetDefaultEqualityComparers<string>(), comparer => Assert.Equal(5, new ReadOnlyExSpan<string>(source).IndexOf(value, comparer)));
            Assert.Equal(-1, new ReadOnlyExSpan<string>(source).IndexOf(value, GetFalseEqualityComparer<string>()));
        }

        [Fact]
        public static void IndexOfSequenceLengthOneValueJustPasttVeryEnd_String() {
            var source = new string[] { "0", "1", "2", "3", "4", "5" };
            var value = new string[] { "5" };

            Assert.Equal(-1, source.AsExSpan(0, 5).IndexOf(value));
            Assert.All(GetDefaultEqualityComparers<string>(), comparer => Assert.Equal(-1, source.AsExSpan(0, 5).AsReadOnlyExSpan().IndexOf(value, comparer)));
        }

        [Theory]
        [MemberData(nameof(TestHelpers.IndexOfNullSequenceData), MemberType = typeof(TestHelpers))]
        public static void IndexOfNullSequence_String(string[] source, string[] target, int expected) {
            Assert.Equal(expected, new ReadOnlyExSpan<string>(source).IndexOf(target));
            Assert.All(GetDefaultEqualityComparers<string>(), comparer => Assert.Equal(expected, new ReadOnlyExSpan<string>(source).IndexOf(target, comparer)));
        }
    }
}
