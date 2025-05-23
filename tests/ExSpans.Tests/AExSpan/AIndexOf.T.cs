using Xunit;

namespace Zyl.ExSpans.Tests.AExSpan {
    public static partial class AIndexOf {
        [Fact]
        public static void ZeroLengthIndexOf() {
            ExSpan<int> sp = new ExSpan<int>(ArrayHelper.Empty<int>());
            int idx = sp.IndexOf(0);
            Assert.Equal(-1, idx);
        }

        [Fact]
        public static void TestMatch() {
            for (int length = 0; length < 32; length++) {
                int[] a = new int[length];
                for (int i = 0; i < length; i++) {
                    a[i] = 10 * (i + 1);
                }
                ExSpan<int> span = new ExSpan<int>(a);

                for (int targetIndex = 0; targetIndex < length; targetIndex++) {
                    int target = a[targetIndex];
                    int idx = span.IndexOf(target);
                    Assert.Equal(targetIndex, idx);
                }
            }
        }

        [Fact]
        public static void TestMultipleMatch() {
            for (int length = 2; length < 32; length++) {
                int[] a = new int[length];
                for (int i = 0; i < length; i++) {
                    a[i] = 10 * (i + 1);
                }

                a[length - 1] = 5555;
                a[length - 2] = 5555;

                ExSpan<int> span = new ExSpan<int>(a);
                int idx = span.IndexOf(5555);
                Assert.Equal(length - 2, idx);
            }
        }

        [Fact]
        public static void OnNoMatchMakeSureEveryElementIsCompared() {
            for (int length = 0; length < 100; length++) {
                TIntLog log = new TIntLog();

                TInt[] a = new TInt[length];
                for (int i = 0; i < length; i++) {
                    a[i] = new TInt(10 * (i + 1), log);
                }
                ExSpan<TInt> span = new ExSpan<TInt>(a);
                int idx = span.IndexOf(new TInt(9999, log));
                Assert.Equal(-1, idx);

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
        public static void MakeSureNoChecksGoOutOfRange() {
            const int GuardValue = 77777;
            const int GuardLength = 50;

            Action<int, int> checkForOutOfRangeAccess =
                delegate (int x, int y) {
                    if (x == GuardValue || y == GuardValue)
                        throw new Exception("Detected out of range access in IndexOf()");
                };

            for (int length = 0; length < 100; length++) {
                TInt[] a = new TInt[GuardLength + length + GuardLength];
                for (int i = 0; i < a.Length; i++) {
                    a[i] = new TInt(GuardValue, checkForOutOfRangeAccess);
                }

                for (int i = 0; i < length; i++) {
                    a[GuardLength + i] = new TInt(10 * (i + 1), checkForOutOfRangeAccess);
                }

                ExSpan<TInt> span = new ExSpan<TInt>(a, GuardLength, length);
                int idx = span.IndexOf(new TInt(9999, checkForOutOfRangeAccess));
                Assert.Equal(-1, idx);
            }
        }

        [Fact]
        public static void ZeroLengthIndexOf_String() {
            ExSpan<string> sp = new ExSpan<string>(ArrayHelper.Empty<string>());
            int idx = sp.IndexOf("a");
            Assert.Equal(-1, idx);
        }

        [Fact]
        public static void TestMatchIndexOf_String() {
            for (int length = 0; length < 32; length++) {
                string[] a = new string[length];
                for (int i = 0; i < length; i++) {
                    a[i] = (10 * (i + 1)).ToString();
                }
                ExSpan<string> span = new ExSpan<string>(a);

                for (int targetIndex = 0; targetIndex < length; targetIndex++) {
                    string target = a[targetIndex];
                    int idx = span.IndexOf(target);
                    Assert.Equal(targetIndex, idx);
                }
            }
        }

        [Fact]
        public static void TestNoMatchIndexOf_String() {
            var rnd = new Random(42);
            for (int length = 0; length <= byte.MaxValue; length++) {
                string[] a = new string[length];
                string target = (rnd.Next(0, 256)).ToString();
                for (int i = 0; i < length; i++) {
                    string val = (i + 1).ToString();
                    a[i] = val == target ? (target + 1) : val;
                }
                ExSpan<string> span = new ExSpan<string>(a);

                int idx = span.IndexOf(target);
                Assert.Equal(-1, idx);
            }
        }

        [Fact]
        public static void TestMultipleMatchIndexOf_String() {
            for (int length = 2; length < 32; length++) {
                string[] a = new string[length];
                for (int i = 0; i < length; i++) {
                    a[i] = (10 * (i + 1)).ToString();
                }

                a[length - 1] = "5555";
                a[length - 2] = "5555";

                ExSpan<string> span = new ExSpan<string>(a);
                int idx = span.IndexOf("5555");
                Assert.Equal(length - 2, idx);
            }
        }

        [Theory]
        [MemberData(nameof(TestHelpers.IndexOfNullData), MemberType = typeof(TestHelpers))]
        public static void IndexOfNull_String(string[] spanInput, int expected) {
            ExSpan<string> theStrings = spanInput;
            Assert.Equal(expected, theStrings.IndexOf((string)null));
        }

        [Fact]
        public static void NotBitwiseEquatableUsesCustomIEquatableImplementationForActualComparison() {
            const byte Ten = 10, NotTen = 11;
            for (int length = 1; length < 100; length++) {
                TwoBytes[] array = new TwoBytes[length];
                for (int i = 0; i < length; i++) {
                    array[i] = new TwoBytes(Ten, (byte)i);
                }

                ExSpan<TwoBytes> span = new ExSpan<TwoBytes>(array);
                ReadOnlyExSpan<TwoBytes> ros = new ReadOnlyExSpan<TwoBytes>(array);

                ReadOnlyExSpan<TwoBytes> noMatch2 = new TwoBytes[2] { new TwoBytes(10, NotTen), new TwoBytes(10, NotTen) };
                Assert.Equal(-1, span.IndexOfAny(noMatch2));
                Assert.Equal(-1, ros.IndexOfAny(noMatch2));
                Assert.Equal(-1, span.LastIndexOfAny(noMatch2));
                Assert.Equal(-1, ros.LastIndexOfAny(noMatch2));

                ReadOnlyExSpan<TwoBytes> noMatch3 = new TwoBytes[3] { new TwoBytes(10, NotTen), new TwoBytes(10, NotTen), new TwoBytes(10, NotTen) };
                Assert.Equal(-1, span.IndexOfAny(noMatch3));
                Assert.Equal(-1, ros.IndexOfAny(noMatch3));
                Assert.Equal(-1, span.LastIndexOfAny(noMatch3));
                Assert.Equal(-1, ros.LastIndexOfAny(noMatch3));

                ReadOnlyExSpan<TwoBytes> match2 = new TwoBytes[2] { new TwoBytes(0, Ten), new TwoBytes(0, Ten) };
                Assert.Equal(0, span.IndexOfAny(match2));
                Assert.Equal(0, ros.IndexOfAny(match2));
                Assert.Equal(0, span.LastIndexOfAny(match2));
                Assert.Equal(0, ros.LastIndexOfAny(match2));

                ReadOnlyExSpan<TwoBytes> match3 = new TwoBytes[3] { new TwoBytes(0, Ten), new TwoBytes(0, Ten), new TwoBytes(0, Ten) };
                Assert.Equal(0, span.IndexOfAny(match3));
                Assert.Equal(0, ros.IndexOfAny(match3));
                Assert.Equal(0, span.LastIndexOfAny(match3));
                Assert.Equal(0, ros.LastIndexOfAny(match3));
            }
        }

        [Fact]
        public static void IndexOfWorksOnAvx512_Integer() {
            // Regression test for https://github.com/dotnet/runtime/issues/89512

            var arr = new int[32];
            arr[1] = 1;
            Assert.Equal(1, arr.AsExSpan().IndexOf(1));
        }

        private readonly struct TwoBytes : IEquatable<TwoBytes> {
            private readonly byte _first, _second;

            public TwoBytes(byte first, byte second) {
                _first = first;
                _second = second;
            }

            // it compares different fields on purpose
            public bool Equals(TwoBytes other) => _first == other._second && _second == other._first;
        }
    }
}
