﻿using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Xunit;

namespace Zyl.ExSpans.Tests.AExSpan {
#if NET8_0_OR_GREATER && TODO // [TODO why] SearchValues methods is internal
    public static partial class ASearchValues {
        private static readonly Func<SearchValues<byte>, byte[]> s_getValuesByteMethod =
            typeof(SearchValues<byte>).GetMethod("GetValues", BindingFlags.NonPublic | BindingFlags.Instance).CreateDelegate<Func<SearchValues<byte>, byte[]>>();

        private static readonly Func<SearchValues<char>, char[]> s_getValuesCharMethod =
            typeof(SearchValues<char>).GetMethod("GetValues", BindingFlags.NonPublic | BindingFlags.Instance).CreateDelegate<Func<SearchValues<char>, char[]>>();

        public static IEnumerable<object[]> Values_MemberData() {
            string[] values = new[]
            {
                "",
                "\0",
                "a",
                "ab",
                "ac",
                "abc",
                "aei",
                "abcd",
                "aeio",
                "aeiou",
                "Aabc",
                "Aabcd",
                "abceiou",
                "123456789",
                "123456789123",
                "abcdefgh",
                "abcdefghIJK",
                "aa",
                "aaa",
                "aaaa",
                "aaaaa",
                "Aa",
                "AaBb",
                "AaBbCc",
                "[]{}",
                "\uFFF0",
                "\uFFF0\uFFF2",
                "\uFFF0\uFFF2\uFFF4",
                "\uFFF0\uFFF2\uFFF4\uFFF6",
                "\uFFF0\uFFF2\uFFF4\uFFF6\uFFF8",
                "\uFFF0\uFFF2\uFFF4\uFFF6\uFFF8\uFFFA",
                "\u0000\u0001\u0002\u0003\u0004\u0005",
                "\u0001\u0002\u0003\u0004\u0005\u0006",
                "\u0001\u0002\u0003\u0004\u0005\u0007",
                "\u007E\u007F\u0080\u0081\u0082\u0083",
                "\u007E\u007F\u0080\u0081\u0082\u0084",
                "\u007E\u007F\u0080\u0081\u0082",
                "\u007E\u007F\u0080\u0081\u0083",
                "\u00FE\u00FF\u0100\u0101\u0102\u0103",
                "\u00FE\u00FF\u0100\u0101\u0102\u0104",
                "\u00FE\u00FF\u0100\u0101\u0102",
                "\u00FE\u00FF\u0100\u0101\u0103",
                "\uFFFF\uFFFE\uFFFD\uFFFC\uFFFB\uFFFA",
                "\uFFFF\uFFFE\uFFFD\uFFFC\uFFFB\uFFFB",
                "\uFFFF\uFFFE\uFFFD\uFFFC\uFFFB\uFFF9",
                new string('\u0080', 256) + '\u0082',
                new string('\u0080', 100) + '\uF000',
                new string('\u0080', 256) + '\uF000',
                string.Concat(Enumerable.Range(128, 255).Select(i => (char)i)),
                string.Concat(Enumerable.Range(128, 257).Select(i => (char)i)),
                string.Concat(Enumerable.Range(128, 254).Select(i => (char)i)) + '\uF000',
                string.Concat(Enumerable.Range(128, 256).Select(i => (char)i)) + '\uF000',
                '\0' + string.Concat(Enumerable.Range(2, char.MaxValue - 1).Select(i => (char)i)),
            };

            foreach (string value in values) {
                yield return Pair(value);
                yield return Pair('a' + value);
                yield return Pair('\0' + value);
                yield return Pair('\u0001' + value);
                yield return Pair('\u00FE' + value);
                yield return Pair('\u00FF' + value);
                yield return Pair('\uFF00' + value);

                // Test some more duplicates
                if (value.Length > 0) {
                    yield return Pair(value + value[0]);
                    yield return Pair(value[0] + value);
                    yield return Pair(value + value);
                }
            }

            static object[] Pair(string value) => new object[] { value, Encoding.Latin1.GetBytes(value) };
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public static void AsciiNeedle_ProperlyHandlesEdgeCases_Char(bool needleContainsZero) {
            // There is some special handling we have to do for ASCII needles to properly filter out non-ASCII results
            ReadOnlyExSpan<char> needleValues = needleContainsZero ? "AEIOU\0" : "AEIOU!";
            SearchValues<char> needle = SearchValues.Create(needleValues);

            ReadOnlyExSpan<char> repeatingHaystack = "AaAaAaAaAaAa";
            Assert.Equal(0, repeatingHaystack.IndexOfAny(needle));
            Assert.Equal(1, repeatingHaystack.IndexOfAnyExcept(needle));
            Assert.Equal(10, repeatingHaystack.LastIndexOfAny(needle));
            Assert.Equal(11, repeatingHaystack.LastIndexOfAnyExcept(needle));

            ReadOnlyExSpan<char> haystackWithZeroes = "Aa\0Aa\0Aa\0";
            Assert.Equal(0, haystackWithZeroes.IndexOfAny(needle));
            Assert.Equal(1, haystackWithZeroes.IndexOfAnyExcept(needle));
            Assert.Equal(needleContainsZero ? 8 : 6, haystackWithZeroes.LastIndexOfAny(needle));
            Assert.Equal(needleContainsZero ? 7 : 8, haystackWithZeroes.LastIndexOfAnyExcept(needle));

            ExSpan<char> haystackWithOffsetNeedle = new char[100];
            for (int i = 0; i < haystackWithOffsetNeedle.Length; i++) {
                haystackWithOffsetNeedle[i] = (char)(128 + needleValues[i % needleValues.Length]);
            }

            Assert.Equal(-1, haystackWithOffsetNeedle.IndexOfAny(needle));
            Assert.Equal(0, haystackWithOffsetNeedle.IndexOfAnyExcept(needle));
            Assert.Equal(-1, haystackWithOffsetNeedle.LastIndexOfAny(needle));
            Assert.Equal(haystackWithOffsetNeedle.Length - 1, haystackWithOffsetNeedle.LastIndexOfAnyExcept(needle));

            // Mix matching characters back in
            for (int i = 0; i < haystackWithOffsetNeedle.Length; i += 3) {
                haystackWithOffsetNeedle[i] = needleValues[i % needleValues.Length];
            }

            Assert.Equal(0, haystackWithOffsetNeedle.IndexOfAny(needle));
            Assert.Equal(1, haystackWithOffsetNeedle.IndexOfAnyExcept(needle));
            Assert.Equal(haystackWithOffsetNeedle.Length - 1, haystackWithOffsetNeedle.LastIndexOfAny(needle));
            Assert.Equal(haystackWithOffsetNeedle.Length - 2, haystackWithOffsetNeedle.LastIndexOfAnyExcept(needle));

            // With chars, the lower byte could be matching, but we have to check that the higher byte is also 0
            for (int i = 0; i < haystackWithOffsetNeedle.Length; i++) {
                haystackWithOffsetNeedle[i] = (char)(((i + 1) * 256) + needleValues[i % needleValues.Length]);
            }

            Assert.Equal(-1, haystackWithOffsetNeedle.IndexOfAny(needle));
            Assert.Equal(0, haystackWithOffsetNeedle.IndexOfAnyExcept(needle));
            Assert.Equal(-1, haystackWithOffsetNeedle.LastIndexOfAny(needle));
            Assert.Equal(haystackWithOffsetNeedle.Length - 1, haystackWithOffsetNeedle.LastIndexOfAnyExcept(needle));

            // Mix matching characters back in
            for (int i = 0; i < haystackWithOffsetNeedle.Length; i += 3) {
                haystackWithOffsetNeedle[i] = needleValues[i % needleValues.Length];
            }

            Assert.Equal(0, haystackWithOffsetNeedle.IndexOfAny(needle));
            Assert.Equal(1, haystackWithOffsetNeedle.IndexOfAnyExcept(needle));
            Assert.Equal(haystackWithOffsetNeedle.Length - 1, haystackWithOffsetNeedle.LastIndexOfAny(needle));
            Assert.Equal(haystackWithOffsetNeedle.Length - 2, haystackWithOffsetNeedle.LastIndexOfAnyExcept(needle));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public static void AsciiNeedle_ProperlyHandlesEdgeCases_Byte(bool needleContainsZero) {
            // There is some special handling we have to do for ASCII needles to properly filter out non-ASCII results
            ReadOnlyExSpan<byte> needleValues = needleContainsZero ? "AEIOU\0"u8 : "AEIOU!"u8;
            SearchValues<byte> needle = SearchValues.Create(needleValues);

            ReadOnlyExSpan<byte> repeatingHaystack = "AaAaAaAaAaAa"u8;
            Assert.Equal(0, repeatingHaystack.IndexOfAny(needle));
            Assert.Equal(1, repeatingHaystack.IndexOfAnyExcept(needle));
            Assert.Equal(10, repeatingHaystack.LastIndexOfAny(needle));
            Assert.Equal(11, repeatingHaystack.LastIndexOfAnyExcept(needle));

            ReadOnlyExSpan<byte> haystackWithZeroes = "Aa\0Aa\0Aa\0"u8;
            Assert.Equal(0, haystackWithZeroes.IndexOfAny(needle));
            Assert.Equal(1, haystackWithZeroes.IndexOfAnyExcept(needle));
            Assert.Equal(needleContainsZero ? 8 : 6, haystackWithZeroes.LastIndexOfAny(needle));
            Assert.Equal(needleContainsZero ? 7 : 8, haystackWithZeroes.LastIndexOfAnyExcept(needle));

            ExSpan<byte> haystackWithOffsetNeedle = new byte[100];
            for (int i = 0; i < haystackWithOffsetNeedle.Length; i++) {
                haystackWithOffsetNeedle[i] = (byte)(128 + needleValues[i % needleValues.Length]);
            }

            Assert.Equal(-1, haystackWithOffsetNeedle.IndexOfAny(needle));
            Assert.Equal(0, haystackWithOffsetNeedle.IndexOfAnyExcept(needle));
            Assert.Equal(-1, haystackWithOffsetNeedle.LastIndexOfAny(needle));
            Assert.Equal(haystackWithOffsetNeedle.Length - 1, haystackWithOffsetNeedle.LastIndexOfAnyExcept(needle));

            // Mix matching characters back in
            for (int i = 0; i < haystackWithOffsetNeedle.Length; i += 3) {
                haystackWithOffsetNeedle[i] = needleValues[i % needleValues.Length];
            }

            Assert.Equal(0, haystackWithOffsetNeedle.IndexOfAny(needle));
            Assert.Equal(1, haystackWithOffsetNeedle.IndexOfAnyExcept(needle));
            Assert.Equal(haystackWithOffsetNeedle.Length - 1, haystackWithOffsetNeedle.LastIndexOfAny(needle));
            Assert.Equal(haystackWithOffsetNeedle.Length - 2, haystackWithOffsetNeedle.LastIndexOfAnyExcept(needle));
        }

        [Theory]
        [MemberData(nameof(Values_MemberData))]
        public static void SearchValues_Contains(string needle, byte[] byteNeedle) {
            Test(needle, SearchValues.Create(needle));
            Test(byteNeedle, SearchValues.Create(byteNeedle));

            static void Test<T>(ReadOnlyExSpan<T> needle, SearchValues<T> values) where T : struct, INumber<T>, IMinMaxValue<T> {
                HashSet<T> needleSet = needle.ToArray().ToHashSet();

                for (int i = int.CreateChecked(T.MaxValue); i >= 0; i--) {
                    T t = T.CreateChecked(i);
                    Assert.Equal(needleSet.Contains(t), values.Contains(t));
                }
            }
        }

        [Theory]
        [MemberData(nameof(Values_MemberData))]
        [ActiveIssue("https://github.com/dotnet/runtime/issues/80875", TestPlatforms.iOS | TestPlatforms.tvOS)]
        public static void SearchValues_GetValues(string needle, byte[] byteNeedle) {
            char[] charValuesActual = s_getValuesCharMethod(SearchValues.Create(needle));
            byte[] byteValuesActual = s_getValuesByteMethod(SearchValues.Create(byteNeedle));

            Assert.Equal(new HashSet<char>(needle).Order().ToArray(), new HashSet<char>(charValuesActual).Order().ToArray());
            Assert.Equal(new HashSet<byte>(byteNeedle).Order().ToArray(), new HashSet<byte>(byteValuesActual).Order().ToArray());
        }

        [Fact]
        [OuterLoop("Takes about a second to execute")]
        public static void TestIndexOfAny_RandomInputs_Byte() {
            SearchValuesTestHelper.TestRandomInputs(
                expected: IndexOfAnyReferenceImpl,
                indexOfAny: (searchSpace, values) => searchSpace.IndexOfAny(values),
                searchValues: (searchSpace, values) => searchSpace.IndexOfAny(values),
                searchValuesContains: (searchSpace, values) => searchSpace.ContainsAny(values));

            static int IndexOfAnyReferenceImpl(ReadOnlyExSpan<byte> searchSpace, ReadOnlyExSpan<byte> values) {
                for (int i = 0; i < searchSpace.Length; i++) {
                    if (values.Contains(searchSpace[i])) {
                        return i;
                    }
                }

                return -1;
            }
        }

        [Fact]
        [OuterLoop("Takes about a second to execute")]
        public static void TestIndexOfAny_RandomInputs_Char() {
            SearchValuesTestHelper.TestRandomInputs(
                expected: IndexOfAnyReferenceImpl,
                indexOfAny: (searchSpace, values) => searchSpace.IndexOfAny(values),
                searchValues: (searchSpace, values) => searchSpace.IndexOfAny(values),
                searchValuesContains: (searchSpace, values) => searchSpace.ContainsAny(values));

            static int IndexOfAnyReferenceImpl(ReadOnlyExSpan<char> searchSpace, ReadOnlyExSpan<char> values) {
                for (int i = 0; i < searchSpace.Length; i++) {
                    if (values.Contains(searchSpace[i])) {
                        return i;
                    }
                }

                return -1;
            }
        }

        [Fact]
        [OuterLoop("Takes about a second to execute")]
        public static void TestLastIndexOfAny_RandomInputs_Byte() {
            SearchValuesTestHelper.TestRandomInputs(
                expected: LastIndexOfAnyReferenceImpl,
                indexOfAny: (searchSpace, values) => searchSpace.LastIndexOfAny(values),
                searchValues: (searchSpace, values) => searchSpace.LastIndexOfAny(values),
                searchValuesContains: (searchSpace, values) => searchSpace.ContainsAny(values));

            static int LastIndexOfAnyReferenceImpl(ReadOnlyExSpan<byte> searchSpace, ReadOnlyExSpan<byte> values) {
                for (int i = searchSpace.Length - 1; i >= 0; i--) {
                    if (values.Contains(searchSpace[i])) {
                        return i;
                    }
                }

                return -1;
            }
        }

        [Fact]
        [OuterLoop("Takes about a second to execute")]
        public static void TestLastIndexOfAny_RandomInputs_Char() {
            SearchValuesTestHelper.TestRandomInputs(
                expected: LastIndexOfAnyReferenceImpl,
                indexOfAny: (searchSpace, values) => searchSpace.LastIndexOfAny(values),
                searchValues: (searchSpace, values) => searchSpace.LastIndexOfAny(values),
                searchValuesContains: (searchSpace, values) => searchSpace.ContainsAny(values));

            static int LastIndexOfAnyReferenceImpl(ReadOnlyExSpan<char> searchSpace, ReadOnlyExSpan<char> values) {
                for (int i = searchSpace.Length - 1; i >= 0; i--) {
                    if (values.Contains(searchSpace[i])) {
                        return i;
                    }
                }

                return -1;
            }
        }

        [Fact]
        [OuterLoop("Takes about a second to execute")]
        public static void TestIndexOfAnyExcept_RandomInputs_Byte() {
            SearchValuesTestHelper.TestRandomInputs(
                expected: IndexOfAnyExceptReferenceImpl,
                indexOfAny: (searchSpace, values) => searchSpace.IndexOfAnyExcept(values),
                searchValues: (searchSpace, values) => searchSpace.IndexOfAnyExcept(values),
                searchValuesContains: (searchSpace, values) => searchSpace.ContainsAnyExcept(values));

            static int IndexOfAnyExceptReferenceImpl(ReadOnlyExSpan<byte> searchSpace, ReadOnlyExSpan<byte> values) {
                for (int i = 0; i < searchSpace.Length; i++) {
                    if (!values.Contains(searchSpace[i])) {
                        return i;
                    }
                }

                return -1;
            }
        }

        [Fact]
        [OuterLoop("Takes about a second to execute")]
        public static void TestIndexOfAnyExcept_RandomInputs_Char() {
            SearchValuesTestHelper.TestRandomInputs(
                expected: IndexOfAnyExceptReferenceImpl,
                indexOfAny: (searchSpace, values) => searchSpace.IndexOfAnyExcept(values),
                searchValues: (searchSpace, values) => searchSpace.IndexOfAnyExcept(values),
                searchValuesContains: (searchSpace, values) => searchSpace.ContainsAnyExcept(values));

            static int IndexOfAnyExceptReferenceImpl(ReadOnlyExSpan<char> searchSpace, ReadOnlyExSpan<char> values) {
                for (int i = 0; i < searchSpace.Length; i++) {
                    if (!values.Contains(searchSpace[i])) {
                        return i;
                    }
                }

                return -1;
            }
        }

        [Fact]
        [OuterLoop("Takes about a second to execute")]
        public static void TestLastIndexOfAnyExcept_RandomInputs_Byte() {
            SearchValuesTestHelper.TestRandomInputs(
                expected: LastIndexOfAnyExceptReferenceImpl,
                indexOfAny: (searchSpace, values) => searchSpace.LastIndexOfAnyExcept(values),
                searchValues: (searchSpace, values) => searchSpace.LastIndexOfAnyExcept(values),
                searchValuesContains: (searchSpace, values) => searchSpace.ContainsAnyExcept(values));

            static int LastIndexOfAnyExceptReferenceImpl(ReadOnlyExSpan<byte> searchSpace, ReadOnlyExSpan<byte> values) {
                for (int i = searchSpace.Length - 1; i >= 0; i--) {
                    if (!values.Contains(searchSpace[i])) {
                        return i;
                    }
                }

                return -1;
            }
        }

        [Fact]
        [OuterLoop("Takes about a second to execute")]
        public static void TestLastIndexOfAnyExcept_RandomInputs_Char() {
            SearchValuesTestHelper.TestRandomInputs(
                expected: LastIndexOfAnyExceptReferenceImpl,
                indexOfAny: (searchSpace, values) => searchSpace.LastIndexOfAnyExcept(values),
                searchValues: (searchSpace, values) => searchSpace.LastIndexOfAnyExcept(values),
                searchValuesContains: (searchSpace, values) => searchSpace.ContainsAnyExcept(values));

            static int LastIndexOfAnyExceptReferenceImpl(ReadOnlyExSpan<char> searchSpace, ReadOnlyExSpan<char> values) {
                for (int i = searchSpace.Length - 1; i >= 0; i--) {
                    if (!values.Contains(searchSpace[i])) {
                        return i;
                    }
                }

                return -1;
            }
        }

        private static class SearchValuesTestHelper {
            private const int MaxNeedleLength = 10;
            private const int MaxHaystackLength = 200;

            private static readonly char[] s_randomAsciiChars;
            private static readonly char[] s_randomLatin1Chars;
            private static readonly char[] s_randomChars;
            private static readonly byte[] s_randomAsciiBytes;
            private static readonly byte[] s_randomBytes;

            static SearchValuesTestHelper() {
                s_randomAsciiChars = new char[100 * 1024];
                s_randomLatin1Chars = new char[100 * 1024];
                s_randomChars = new char[1024 * 1024];
                s_randomBytes = new byte[100 * 1024];

                var rng = new Random(42);

                for (int i = 0; i < s_randomAsciiChars.Length; i++) {
                    s_randomAsciiChars[i] = (char)rng.Next(0, 128);
                }

                for (int i = 0; i < s_randomLatin1Chars.Length; i++) {
                    s_randomLatin1Chars[i] = (char)rng.Next(0, 256);
                }

                rng.NextBytes(MemoryMarshal.Cast<char, byte>(s_randomChars.AsExSpan()));

                s_randomAsciiBytes = Encoding.ASCII.GetBytes(s_randomAsciiChars);

                rng.NextBytes(s_randomBytes);
            }

            public delegate int IndexOfAnySearchDelegate<T>(ReadOnlyExSpan<T> searchSpace, ReadOnlyExSpan<T> values) where T : IEquatable<T>?;

            public delegate int SearchValuesSearchDelegate<T>(ReadOnlyExSpan<T> searchSpace, SearchValues<T> values) where T : IEquatable<T>?;

            public delegate bool SearchValuesContainsDelegate<T>(ReadOnlyExSpan<T> searchSpace, SearchValues<T> values) where T : IEquatable<T>?;

            public static void TestRandomInputs(
                IndexOfAnySearchDelegate<byte> expected,
                IndexOfAnySearchDelegate<byte> indexOfAny,
                SearchValuesSearchDelegate<byte> searchValues,
                SearchValuesContainsDelegate<byte> searchValuesContains) {
                var rng = new Random(42);

                for (int iterations = 0; iterations < 1_000_000; iterations++) {
                    // There are more interesting corner cases with ASCII needles, test those more.
                    Test(rng, s_randomBytes, s_randomAsciiBytes, expected, indexOfAny, searchValues, searchValuesContains);

                    Test(rng, s_randomBytes, s_randomBytes, expected, indexOfAny, searchValues, searchValuesContains);
                }
            }

            public static void TestRandomInputs(
                IndexOfAnySearchDelegate<char> expected,
                IndexOfAnySearchDelegate<char> indexOfAny,
                SearchValuesSearchDelegate<char> searchValues,
                SearchValuesContainsDelegate<char> searchValuesContains) {
                var rng = new Random(42);

                for (int iterations = 0; iterations < 1_000_000; iterations++) {
                    // There are more interesting corner cases with ASCII needles, test those more.
                    Test(rng, s_randomChars, s_randomAsciiChars, expected, indexOfAny, searchValues, searchValuesContains);

                    Test(rng, s_randomChars, s_randomLatin1Chars, expected, indexOfAny, searchValues, searchValuesContains);

                    Test(rng, s_randomChars, s_randomChars, expected, indexOfAny, searchValues, searchValuesContains);
                }
            }

            private static void Test<T>(Random rng, ReadOnlyExSpan<T> haystackRandom, ReadOnlyExSpan<T> needleRandom,
                IndexOfAnySearchDelegate<T> expected, IndexOfAnySearchDelegate<T> indexOfAny,
                SearchValuesSearchDelegate<T> searchValues, SearchValuesContainsDelegate<T> searchValuesContains)
                where T : struct, INumber<T>, IMinMaxValue<T> {
                ReadOnlyExSpan<T> haystack = GetRandomSlice(rng, haystackRandom, MaxHaystackLength);
                ReadOnlyExSpan<T> needle = GetRandomSlice(rng, needleRandom, MaxNeedleLength);

                SearchValues<T> searchValuesInstance = (SearchValues<T>)(object)(typeof(T) == typeof(byte)
                    ? SearchValues.Create(MemoryMarshal.CreateReadOnlyExSpan(ref Unsafe.As<T, byte>(ref ExMemoryMarshal.GetReference(needle)), needle.Length))
                    : SearchValues.Create(MemoryMarshal.CreateReadOnlyExSpan(ref Unsafe.As<T, char>(ref ExMemoryMarshal.GetReference(needle)), needle.Length)));

                int expectedIndex = expected(haystack, needle);
                int indexOfAnyIndex = indexOfAny(haystack, needle);
                int searchValuesIndex = searchValues(haystack, searchValuesInstance);
                bool searchValuesContainsResult = searchValuesContains(haystack, searchValuesInstance);

                if (expectedIndex != indexOfAnyIndex) {
                    AssertionFailed(haystack, needle, searchValuesInstance, expectedIndex, indexOfAnyIndex, nameof(indexOfAny));
                }

                if (expectedIndex != searchValuesIndex) {
                    AssertionFailed(haystack, needle, searchValuesInstance, expectedIndex, searchValuesIndex, nameof(searchValues));
                }

                if ((expectedIndex >= 0) != searchValuesContainsResult) {
                    AssertionFailed(haystack, needle, searchValuesInstance, expectedIndex, searchValuesContainsResult ? 0 : -1, nameof(searchValuesContainsResult));
                }
            }

            private static ReadOnlyExSpan<T> GetRandomSlice<T>(Random rng, ReadOnlyExSpan<T> span, int maxLength) {
                ReadOnlyExSpan<T> slice = span.Slice(rng.Next(span.Length + 1));
                return slice.Slice(0, Math.Min(slice.Length, rng.Next(maxLength + 1)));
            }

            private static void AssertionFailed<T>(ReadOnlyExSpan<T> haystack, ReadOnlyExSpan<T> needle, SearchValues<T> searchValues, int expected, int actual, string approach)
                where T : INumber<T> {
                Type implType = searchValues.GetType();
                string impl = $"{implType.Name} [{string.Join(", ", implType.GenericTypeArguments.Select(t => t.Name))}]";

                string readableHaystack = string.Join(", ", haystack.ToArray().Select(c => int.CreateChecked(c)));
                string readableNeedle = string.Join(", ", needle.ToArray().Select(c => int.CreateChecked(c)));

                Assert.Fail($"Expected {expected}, got {approach}={actual} for impl='{impl}', needle='{readableNeedle}', haystack='{readableHaystack}'");
            }
        }
    }
#endif // NET8_0_OR_GREATER
}
