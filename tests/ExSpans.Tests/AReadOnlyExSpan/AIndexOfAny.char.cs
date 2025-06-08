using System.Linq;
using System.Numerics;
using System.Text;
using Xunit;

namespace Zyl.ExSpans.Tests.AReadOnlyExSpan {
    public static partial class AIndexOfAny {
        [Theory]
        [InlineData("a", "a", 'a', 0)]
        [InlineData("ab", "a", 'a', 0)]
        [InlineData("aab", "a", 'a', 0)]
        [InlineData("acab", "a", 'a', 0)]
        [InlineData("acab", "c", 'c', 1)]
        [InlineData("abcdefghijklmnopqrstuvwxyz", "lo", 'l', 11)]
        [InlineData("abcdefghijklmnopqrstuvwxyz", "ol", 'l', 11)]
        [InlineData("abcdefghijklmnopqrstuvwxyz", "ll", 'l', 11)]
        [InlineData("abcdefghijklmnopqrstuvwxyz", "lmr", 'l', 11)]
        [InlineData("abcdefghijklmnopqrstuvwxyz", "rml", 'l', 11)]
        [InlineData("abcdefghijklmnopqrstuvwxyz", "mlr", 'l', 11)]
        [InlineData("abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyz", "lmr", 'l', 11)]
        [InlineData("aaaaaaaaaaalmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyz", "lmr", 'l', 11)]
        [InlineData("aaaaaaaaaaacmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyz", "lmr", 'm', 12)]
        [InlineData("aaaaaaaaaaarmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyz", "lmr", 'r', 11)]
        [InlineData("/localhost:5000/PATH/%2FPATH2/ HTTP/1.1", " %?", '%', 21)]
        [InlineData("/localhost:5000/PATH/%2FPATH2/?key=value HTTP/1.1", " %?", '%', 21)]
        [InlineData("/localhost:5000/PATH/PATH2/?key=value HTTP/1.1", " %?", '?', 27)]
        [InlineData("/localhost:5000/PATH/PATH2/ HTTP/1.1", " %?", ' ', 27)]
        public static void IndexOfAnyStrings_Char(string raw, string search, char expectResult, int expectIndex) {
            ReadOnlyExSpan<char> span = raw.AsSpan();
            char[] searchFor = search.ToCharArray();

            TSize index = span.IndexOfAny(searchFor);
            if (searchFor.Length == 1) {
                Assert.Equal(index, span.IndexOf(searchFor[0]));
            } else if (searchFor.Length == 2) {
                Assert.Equal(index, span.IndexOfAny(searchFor[0], searchFor[1]));
            } else if (searchFor.Length == 3) {
                Assert.Equal(index, span.IndexOfAny(searchFor[0], searchFor[1], searchFor[2]));
            }

            char found = span[index];
            Assert.Equal(expectResult, found);
            Assert.Equal(expectIndex, index);
        }

        [Fact]
        public static void ZeroLengthIndexOfTwo_Char() {
            ReadOnlyExSpan<char> sp = new ReadOnlyExSpan<char>(ArrayHelper.Empty<char>());
            TSize idx = sp.IndexOfAny<char>((char)0, (char)0);
            Assert.Equal(-1, idx);
        }

        [Fact]
        public static void DefaultFilledIndexOfTwo_Char() {
            Random rnd = new Random(42);

            for (int length = 1; length <= byte.MaxValue + 1; length++) {
                char[] a = new char[length];
                ReadOnlyExSpan<char> span = new ReadOnlyExSpan<char>(a);

                char[] targets = { default, (char)99 };

                for (int i = 0; i < length; i++) {
                    TSize index = rnd.Next(0, targets.Length) == 0 ? 0 : 1;
                    char target0 = targets[index];
                    char target1 = targets[(index + 1) % 2];
                    TSize idx = span.IndexOfAny(target0, target1);
                    Assert.Equal(0, idx);
                }
            }
        }

        [Fact]
        public static void TestMatchTwo_Char() {
            for (int length = Vector<short>.Count; length <= byte.MaxValue + 1; length++) {
                char[] a = Enumerable.Range(0, length).Select(i => (char)(i + 1)).ToArray();

                for (int i = 0; i < Vector<short>.Count; i++) {
                    ReadOnlyExSpan<char> span = new ReadOnlyExSpan<char>(a).Slice(i);

                    for (int targetIndex = 0; targetIndex < length - Vector<short>.Count; targetIndex++) {
                        char target0 = a[targetIndex + i];
                        char target1 = (char)0;
                        TSize idx = span.IndexOfAny(target0, target1);
                        Assert.Equal(targetIndex, idx);
                    }

                    for (int targetIndex = 0; targetIndex < length - 1 - Vector<short>.Count; targetIndex++) {
                        char target0 = a[targetIndex + i];
                        char target1 = a[targetIndex + i + 1];
                        TSize idx = span.IndexOfAny(target0, target1);
                        Assert.Equal(targetIndex, idx);
                    }

                    for (int targetIndex = 0; targetIndex < length - 1 - Vector<short>.Count; targetIndex++) {
                        char target0 = (char)0;
                        char target1 = a[targetIndex + i + 1];
                        TSize idx = span.IndexOfAny(target0, target1);
                        Assert.Equal(targetIndex + 1, idx);
                    }
                }
            }
        }

        [Fact]
        public static void TestNoMatchTwo_Char() {
            Random rnd = new Random(42);
            for (int length = 0; length <= byte.MaxValue + 1; length++) {
                char[] a = new char[length];
                char target0 = (char)rnd.Next(1, 256);
                char target1 = (char)rnd.Next(1, 256);
                ReadOnlyExSpan<char> span = new ReadOnlyExSpan<char>(a);

                TSize idx = span.IndexOfAny(target0, target1);
                Assert.Equal(-1, idx);
            }
        }

        [Fact]
        public static void TestMultipleMatchTwo_Char() {
            for (int length = 3; length <= byte.MaxValue + 1; length++) {
                char[] a = new char[length];
                for (int i = 0; i < length; i++) {
                    char val = (char)(i + 1);
                    a[i] = val == (char)200 ? (char)201 : val;
                }

                a[length - 1] = (char)200;
                a[length - 2] = (char)200;
                a[length - 3] = (char)200;

                ReadOnlyExSpan<char> span = new ReadOnlyExSpan<char>(a);
                TSize idx = span.IndexOfAny<char>((char)200, (char)200);
                Assert.Equal(length - 3, idx);
            }
        }

        [Fact]
        public static void MakeSureNoChecksGoOutOfRangeTwo_Char() {
            for (int length = 1; length <= byte.MaxValue + 1; length++) {
                char[] a = new char[length + 2];
                a[0] = (char)99;
                a[length + 1] = (char)98;
                ReadOnlyExSpan<char> span = new ReadOnlyExSpan<char>(a, 1, length - 1);
                //if (span.Length > 0) {
                //    Console.WriteLine("length={0}, span[0]={1}, span[^1]={2}", length, (short)span[0], (short)span[span.Length - 1]);
                //}
                TSize index = span.IndexOfAny<char>((char)99, (char)98);
                Assert.Equal(-1, index);
            }

            for (int length = 1; length <= byte.MaxValue + 1; length++) {
                char[] a = new char[length + 2];
                a[0] = (char)99;
                a[length + 1] = (char)99;
                ReadOnlyExSpan<char> span = new ReadOnlyExSpan<char>(a, 1, length - 1);
                TSize index = span.IndexOfAny<char>((char)99, (char)99);
                Assert.Equal(-1, index);
            }
        }

        [Fact]
        public static void ZeroLengthIndexOfThree_Char() {
            ReadOnlyExSpan<char> sp = new ReadOnlyExSpan<char>(ArrayHelper.Empty<char>());
            TSize idx = sp.IndexOfAny<char>((char)0, (char)0, (char)0);
            Assert.Equal(-1, idx);
        }

        [Fact]
        public static void DefaultFilledIndexOfThree_Char() {
            Random rnd = new Random(42);

            for (int length = 1; length <= byte.MaxValue + 1; length++) {
                char[] a = new char[length];
                ReadOnlyExSpan<char> span = new ReadOnlyExSpan<char>(a);

                char[] targets = { default, (char)99, (char)98 };

                for (int i = 0; i < length; i++) {
                    TSize index = rnd.Next(0, targets.Length);
                    char target0 = targets[index];
                    char target1 = targets[(index + 1) % 2];
                    char target2 = targets[(index + 1) % 3];
                    TSize idx = span.IndexOfAny(target0, target1, target2);
                    Assert.Equal(0, idx);
                }
            }
        }

        [Fact]
        public static void TestMatchThree_Char() {
            for (int length = Vector<short>.Count; length <= byte.MaxValue + 1; length++) {
                char[] a = Enumerable.Range(0, length).Select(i => (char)(i + 1)).ToArray();
                for (int i = 0; i < Vector<short>.Count; i++) {
                    ReadOnlyExSpan<char> span = new ReadOnlyExSpan<char>(a).Slice(i);

                    for (int targetIndex = 0; targetIndex < length - Vector<short>.Count; targetIndex++) {
                        char target0 = a[targetIndex + i];
                        char target1 = (char)0;
                        char target2 = (char)0;
                        TSize idx = span.IndexOfAny(target0, target1, target2);
                        Assert.Equal(targetIndex, idx);
                    }

                    for (int targetIndex = 0; targetIndex < length - 2 - Vector<short>.Count; targetIndex++) {
                        char target0 = a[targetIndex + i];
                        char target1 = a[targetIndex + i + 1];
                        char target2 = a[targetIndex + i + 2];
                        TSize idx = span.IndexOfAny(target0, target1, target2);
                        Assert.Equal(targetIndex, idx);
                    }

                    for (int targetIndex = 0; targetIndex < length - 2 - Vector<short>.Count; targetIndex++) {
                        char target0 = (char)0;
                        char target1 = (char)0;
                        char target2 = a[targetIndex + i + 2];
                        TSize idx = span.IndexOfAny(target0, target1, target2);
                        Assert.Equal(targetIndex + 2, idx);
                    }
                }
            }
        }

        [Fact]
        public static void TestNoMatchThree_Char() {
            Random rnd = new Random(42);
            for (int length = 0; length <= byte.MaxValue + 1; length++) {
                char[] a = new char[length];
                char target0 = (char)rnd.Next(1, 256);
                char target1 = (char)rnd.Next(1, 256);
                char target2 = (char)rnd.Next(1, 256);
                ReadOnlyExSpan<char> span = new ReadOnlyExSpan<char>(a);

                TSize idx = span.IndexOfAny(target0, target1, target2);
                Assert.Equal(-1, idx);
            }
        }

        [Fact]
        public static void TestMultipleMatchThree_Char() {
            for (int length = 4; length <= byte.MaxValue + 1; length++) {
                char[] a = new char[length];
                for (int i = 0; i < length; i++) {
                    char val = (char)(i + 1);
                    a[i] = val == (char)200 ? (char)201 : val;
                }

                a[length - 1] = (char)200;
                a[length - 2] = (char)200;
                a[length - 3] = (char)200;
                a[length - 4] = (char)200;

                ReadOnlyExSpan<char> span = new ReadOnlyExSpan<char>(a);
                TSize idx = span.IndexOfAny<char>((char)200, (char)200, (char)200);
                Assert.Equal(length - 4, idx);
            }
        }

        [Fact]
        public static void MakeSureNoChecksGoOutOfRangeThree_Char() {
            for (int length = 1; length <= byte.MaxValue + 1; length++) {
                char[] a = new char[length + 2];
                a[0] = (char)99;
                a[length + 1] = (char)98;
                ReadOnlyExSpan<char> span = new ReadOnlyExSpan<char>(a, 1, length - 1);
                TSize index = span.IndexOfAny<char>((char)99, (char)98, (char)99);
                Assert.Equal(-1, index);
            }

            for (int length = 1; length <= byte.MaxValue + 1; length++) {
                char[] a = new char[length + 2];
                a[0] = (char)99;
                a[length + 1] = (char)99;
                ReadOnlyExSpan<char> span = new ReadOnlyExSpan<char>(a, 1, length - 1);
                TSize index = span.IndexOfAny<char>((char)99, (char)99, (char)99);
                Assert.Equal(-1, index);
            }
        }

        [Fact]
        public static void ZeroLengthIndexOfFour_Char() {
            ReadOnlyExSpan<char> sp = new ReadOnlyExSpan<char>(ArrayHelper.Empty<char>());
            ReadOnlyExSpan<char> values = new char[] { (char)0, (char)0, (char)0, (char)0 };
            TSize idx = sp.IndexOfAny<char>(values);
            Assert.Equal(-1, idx);
        }

        [Fact]
        public static void DefaultFilledIndexOfFour_Char() {
            Random rnd = new Random(42);

            for (int length = 1; length <= byte.MaxValue + 1; length++) {
                char[] a = new char[length];
                ReadOnlyExSpan<char> span = new ReadOnlyExSpan<char>(a);

                char[] targets = { default, (char)99, (char)98, (char)97 };

                for (int i = 0; i < length; i++) {
                    TSize index = rnd.Next(0, targets.Length);
                    ReadOnlyExSpan<char> values = new char[] { (char)targets[index], (char)targets[(index + 1) % 2], (char)targets[(index + 1) % 3], (char)targets[(index + 1) % 4] };
                    TSize idx = span.IndexOfAny(values);
                    Assert.Equal(0, idx);
                }
            }
        }

        [Fact]
        public static void TestMatchFour_Char() {
            for (int length = Vector<short>.Count; length <= byte.MaxValue + 1; length++) {
                char[] a = Enumerable.Range(0, length).Select(i => (char)(i + 1)).ToArray();

                for (int i = 0; i < Vector<short>.Count; i++) {
                    ReadOnlyExSpan<char> span = new ReadOnlyExSpan<char>(a).Slice(i);

                    for (int targetIndex = 0; targetIndex < length - Vector<short>.Count; targetIndex++) {
                        ReadOnlyExSpan<char> values = new char[] { (char)a[targetIndex + i], (char)0, (char)0, (char)0 };
                        TSize idx = span.IndexOfAny(values);
                        Assert.Equal(targetIndex, idx);
                    }

                    for (int targetIndex = 0; targetIndex < length - 3 - Vector<short>.Count; targetIndex++) {
                        ReadOnlyExSpan<char> values = new char[] { (char)a[targetIndex + i], (char)a[targetIndex + i + 1], (char)a[targetIndex + i + 2], (char)a[targetIndex + i + 3] };
                        TSize idx = span.IndexOfAny(values);
                        Assert.Equal(targetIndex, idx);
                    }

                    for (int targetIndex = 0; targetIndex < length - 3 - Vector<short>.Count; targetIndex++) {
                        ReadOnlyExSpan<char> values = new char[] { (char)0, (char)0, (char)0, (char)a[targetIndex + i + 3] };
                        TSize idx = span.IndexOfAny(values);
                        Assert.Equal(targetIndex + 3, idx);
                    }
                }
            }
        }

        [Fact]
        public static void TestNoMatchFour_Char() {
            Random rnd = new Random(42);
            for (int length = 0; length <= byte.MaxValue + 1; length++) {
                char[] a = new char[length];
                ReadOnlyExSpan<char> values = new char[] { (char)rnd.Next(1, 256), (char)rnd.Next(1, 256), (char)rnd.Next(1, 256), (char)rnd.Next(1, 256) };
                ReadOnlyExSpan<char> span = new ReadOnlyExSpan<char>(a);

                TSize idx = span.IndexOfAny(values);
                Assert.Equal(-1, idx);
            }
        }

        [Fact]
        public static void TestMultipleMatchFour_Char() {
            for (int length = 5; length <= byte.MaxValue + 1; length++) {
                char[] a = new char[length];
                for (int i = 0; i < length; i++) {
                    char val = (char)(i + 1);
                    a[i] = val == (char)200 ? (char)201 : val;
                }

                a[length - 1] = (char)200;
                a[length - 2] = (char)200;
                a[length - 3] = (char)200;
                a[length - 4] = (char)200;
                a[length - 5] = (char)200;

                ReadOnlyExSpan<char> span = new ReadOnlyExSpan<char>(a);
                ReadOnlyExSpan<char> values = new char[] { (char)200, (char)200, (char)200, (char)200 };
                TSize idx = span.IndexOfAny<char>(values);
                Assert.Equal(length - 5, idx);
            }
        }

        [Fact]
        public static void MakeSureNoChecksGoOutOfRangeFour_Char() {
            for (int length = 1; length <= byte.MaxValue + 1; length++) {
                char[] a = new char[length + 2];
                a[0] = (char)99;
                a[length + 1] = (char)98;
                ReadOnlyExSpan<char> span = new ReadOnlyExSpan<char>(a, 1, length - 1);
                ReadOnlyExSpan<char> values = new char[] { (char)99, (char)98, (char)99, (char)99 };
                TSize index = span.IndexOfAny<char>(values);
                Assert.Equal(-1, index);
            }

            for (int length = 1; length <= byte.MaxValue + 1; length++) {
                char[] a = new char[length + 2];
                a[0] = (char)99;
                a[length + 1] = (char)99;
                ReadOnlyExSpan<char> span = new ReadOnlyExSpan<char>(a, 1, length - 1);
                ReadOnlyExSpan<char> values = new char[] { (char)99, (char)99, (char)99, (char)99 };
                TSize index = span.IndexOfAny<char>(values);
                Assert.Equal(-1, index);
            }
        }

        [Fact]
        public static void ZeroLengthIndexOfFive_Char() {
            ReadOnlyExSpan<char> sp = new ReadOnlyExSpan<char>(ArrayHelper.Empty<char>());
            ReadOnlyExSpan<char> values = new char[] { (char)0, (char)0, (char)0, (char)0, (char)0 };
            TSize idx = sp.IndexOfAny<char>(values);
            Assert.Equal(-1, idx);
        }

        [Fact]
        public static void DefaultFilledIndexOfFive_Char() {
            Random rnd = new Random(42);

            for (int length = 1; length <= byte.MaxValue + 1; length++) {
                char[] a = new char[length];
                ReadOnlyExSpan<char> span = new ReadOnlyExSpan<char>(a);

                char[] targets = { default, (char)99, (char)98, (char)97, (char)96 };

                for (int i = 0; i < length; i++) {
                    TSize index = rnd.Next(0, targets.Length);
                    ReadOnlyExSpan<char> values = new char[] { (char)targets[index], (char)targets[(index + 1) % 2], (char)targets[(index + 1) % 3], (char)targets[(index + 1) % 4], (char)targets[(index + 1) % 5] };
                    TSize idx = span.IndexOfAny(values);
                    Assert.Equal(0, idx);
                }
            }
        }

        [Fact]
        public static void TestMatchFive_Char() {
            for (int length = Vector<short>.Count; length <= byte.MaxValue + 1; length++) {
                char[] a = Enumerable.Range(0, length).Select(i => (char)(i + 1)).ToArray();
                for (int i = 0; i < Vector<short>.Count; i++) {
                    ReadOnlyExSpan<char> span = new ReadOnlyExSpan<char>(a).Slice(i);

                    for (int targetIndex = 0; targetIndex < length - Vector<short>.Count; targetIndex++) {
                        ReadOnlyExSpan<char> values = new char[] { (char)a[targetIndex + i], (char)0, (char)0, (char)0, (char)0 };
                        TSize idx = span.IndexOfAny(values);
                        Assert.Equal(targetIndex, idx);
                    }

                    for (int targetIndex = 0; targetIndex < length - 4 - Vector<short>.Count; targetIndex++) {
                        ReadOnlyExSpan<char> values = new char[] { (char)a[targetIndex + i], (char)a[targetIndex + i + 1], (char)a[targetIndex + i + 2], (char)a[targetIndex + i + 3], (char)a[targetIndex + i + 4] };
                        TSize idx = span.IndexOfAny(values);
                        Assert.Equal(targetIndex, idx);
                    }

                    for (int targetIndex = 0; targetIndex < length - 4 - Vector<short>.Count; targetIndex++) {
                        ReadOnlyExSpan<char> values = new char[] { (char)0, (char)0, (char)0, (char)0, (char)a[targetIndex + i + 4] };
                        TSize idx = span.IndexOfAny(values);
                        Assert.Equal(targetIndex + 4, idx);
                    }
                }
            }
        }

        [Fact]
        public static void TestNoMatchFive_Char() {
            Random rnd = new Random(42);
            for (int length = 0; length <= byte.MaxValue + 1; length++) {
                char[] a = new char[length];
                ReadOnlyExSpan<char> values = new char[] { (char)rnd.Next(1, 256), (char)rnd.Next(1, 256), (char)rnd.Next(1, 256), (char)rnd.Next(1, 256), (char)rnd.Next(1, 256) };
                ReadOnlyExSpan<char> span = new ReadOnlyExSpan<char>(a);

                TSize idx = span.IndexOfAny(values);
                Assert.Equal(-1, idx);
            }
        }

        [Fact]
        public static void TestMultipleMatchFive_Char() {
            for (int length = 6; length <= byte.MaxValue + 1; length++) {
                char[] a = new char[length];
                for (int i = 0; i < length; i++) {
                    char val = (char)(i + 1);
                    a[i] = val == (char)200 ? (char)201 : val;
                }

                a[length - 1] = (char)200;
                a[length - 2] = (char)200;
                a[length - 3] = (char)200;
                a[length - 4] = (char)200;
                a[length - 5] = (char)200;
                a[length - 6] = (char)200;

                ReadOnlyExSpan<char> span = new ReadOnlyExSpan<char>(a);
                ReadOnlyExSpan<char> values = new char[] { (char)200, (char)200, (char)200, (char)200, (char)200 };
                TSize idx = span.IndexOfAny<char>(values);
                Assert.Equal(length - 6, idx);
            }
        }

        [Fact]
        public static void MakeSureNoChecksGoOutOfRangeFive_Char() {
            for (int length = 1; length <= byte.MaxValue + 1; length++) {
                char[] a = new char[length + 2];
                a[0] = (char)99;
                a[length + 1] = (char)98;
                ReadOnlyExSpan<char> span = new ReadOnlyExSpan<char>(a, 1, length - 1);
                ReadOnlyExSpan<char> values = new char[] { (char)99, (char)98, (char)99, (char)99, (char)99 };
                TSize index = span.IndexOfAny<char>(values);
                Assert.Equal(-1, index);
            }

            for (int length = 1; length <= byte.MaxValue + 1; length++) {
                char[] a = new char[length + 2];
                a[0] = (char)99;
                a[length + 1] = (char)99;
                ReadOnlyExSpan<char> span = new ReadOnlyExSpan<char>(a, 1, length - 1);
                ReadOnlyExSpan<char> values = new char[] { (char)99, (char)99, (char)99, (char)99, (char)99 };
                TSize index = span.IndexOfAny<char>(values);
                Assert.Equal(-1, index);
            }
        }

        [Fact]
        public static void ZeroLengthIndexOfMany_Char() {
            ReadOnlyExSpan<char> sp = new ReadOnlyExSpan<char>(ArrayHelper.Empty<char>());
            ReadOnlyExSpan<char> values = new ReadOnlyExSpan<char>(new char[] { (char)0, (char)0, (char)0, (char)0, (char)0, (char)0 });
            TSize idx = sp.IndexOfAny(values);
            Assert.Equal(-1, idx);

            values = new ReadOnlyExSpan<char>(new char[] { });
            idx = sp.IndexOfAny(values);
            Assert.Equal(-1, idx);
        }

        [Fact]
        public static void DefaultFilledIndexOfMany_Char() {
            for (int length = 1; length <= byte.MaxValue + 1; length++) {
                char[] a = new char[length];
                ReadOnlyExSpan<char> span = new ReadOnlyExSpan<char>(a);

                ReadOnlyExSpan<char> values = new ReadOnlyExSpan<char>(new char[] { default, (char)99, (char)98, (char)97, (char)96, (char)0 });

                for (int i = 0; i < length; i++) {
                    TSize idx = span.IndexOfAny(values);
                    Assert.Equal(0, idx);
                }
            }
        }

        [Fact]
        public static void TestMatchMany_Char() {
            for (int length = 1; length <= byte.MaxValue + 1; length++) {
                char[] a = Enumerable.Range(0, length).Select(i => (char)(i + 1)).ToArray();
                ReadOnlyExSpan<char> span = new ReadOnlyExSpan<char>(a);

                for (int targetIndex = 0; targetIndex < length; targetIndex++) {
                    ReadOnlyExSpan<char> values = new ReadOnlyExSpan<char>(new char[] { a[targetIndex], (char)0, (char)0, (char)0, (char)0, (char)0 });
                    TSize idx = span.IndexOfAny(values);
                    Assert.Equal(targetIndex, idx);
                }

                for (int targetIndex = 0; targetIndex < length - 5; targetIndex++) {
                    ReadOnlyExSpan<char> values = new ReadOnlyExSpan<char>(new char[] { a[targetIndex], a[targetIndex + 1], a[targetIndex + 2], a[targetIndex + 3], a[targetIndex + 4], a[targetIndex + 5] });
                    TSize idx = span.IndexOfAny(values);
                    Assert.Equal(targetIndex, idx);
                }

                for (int targetIndex = 0; targetIndex < length - 5; targetIndex++) {
                    ReadOnlyExSpan<char> values = new ReadOnlyExSpan<char>(new char[] { (char)0, (char)0, (char)0, (char)0, (char)0, a[targetIndex + 5] });
                    TSize idx = span.IndexOfAny(values);
                    Assert.Equal(targetIndex + 5, idx);
                }
            }
        }

        [Fact]
        public static void TestMatchValuesLargerMany_Char() {
            Random rnd = new Random(42);
            for (int length = 2; length <= byte.MaxValue + 1; length++) {
                char[] a = new char[length];
                int expectedIndex = length / 2;
                for (int i = 0; i < length; i++) {
                    if (i == expectedIndex) {
                        continue;
                    }
                    a[i] = (char)255;
                }
                ReadOnlyExSpan<char> span = new ReadOnlyExSpan<char>(a);

                char[] targets = new char[length * 2];
                for (int i = 0; i < targets.Length; i++) {
                    if (i == length + 1) {
                        continue;
                    }
                    targets[i] = (char)rnd.Next(1, 255);
                }

                ReadOnlyExSpan<char> values = new ReadOnlyExSpan<char>(targets);
                TSize idx = span.IndexOfAny(values);
                Assert.Equal(expectedIndex, idx);
            }
        }

        [Fact]
        public static void TestNoMatchMany_Char() {
            Random rnd = new Random(42);
            for (int length = 1; length <= byte.MaxValue + 1; length++) {
                char[] a = new char[length];
                char[] targets = new char[length];
                for (int i = 0; i < targets.Length; i++) {
                    targets[i] = (char)rnd.Next(1, 256);
                }
                ReadOnlyExSpan<char> span = new ReadOnlyExSpan<char>(a);
                ReadOnlyExSpan<char> values = new ReadOnlyExSpan<char>(targets);

                TSize idx = span.IndexOfAny(values);
                Assert.Equal(-1, idx);
            }
        }

        [Fact]
        public static void TestNoMatchValuesLargerMany_Char() {
            Random rnd = new Random(42);
            for (int length = 1; length <= byte.MaxValue + 1; length++) {
                char[] a = new char[length];
                char[] targets = new char[length * 2];
                for (int i = 0; i < targets.Length; i++) {
                    targets[i] = (char)rnd.Next(1, 256);
                }
                ReadOnlyExSpan<char> span = new ReadOnlyExSpan<char>(a);
                ReadOnlyExSpan<char> values = new ReadOnlyExSpan<char>(targets);

                TSize idx = span.IndexOfAny(values);
                Assert.Equal(-1, idx);
            }
        }

        [Fact]
        public static void TestMultipleMatchMany_Char() {
            for (int length = 5; length <= byte.MaxValue + 1; length++) {
                char[] a = new char[length];
                for (int i = 0; i < length; i++) {
                    char val = (char)(i + 1);
                    a[i] = val == 200 ? (char)201 : val;
                }

                a[length - 1] = (char)200;
                a[length - 2] = (char)200;
                a[length - 3] = (char)200;
                a[length - 4] = (char)200;
                a[length - 5] = (char)200;

                ReadOnlyExSpan<char> span = new ReadOnlyExSpan<char>(a);
                ReadOnlyExSpan<char> values = new ReadOnlyExSpan<char>(new char[] { (char)200, (char)200, (char)200, (char)200, (char)200, (char)200, (char)200, (char)200, (char)200 });
                TSize idx = span.IndexOfAny(values);
                Assert.Equal(length - 5, idx);
            }
        }

        [Fact]
        public static void MakeSureNoChecksGoOutOfRangeMany_Char() {
            for (int length = 1; length <= byte.MaxValue + 1; length++) {
                char[] a = new char[length + 2];
                a[0] = (char)99;
                a[length + 1] = (char)98;
                ReadOnlyExSpan<char> span = new ReadOnlyExSpan<char>(a, 1, length - 1);
                ReadOnlyExSpan<char> values = new ReadOnlyExSpan<char>(new char[] { (char)99, (char)98, (char)99, (char)98, (char)99, (char)98 });
                TSize index = span.IndexOfAny(values);
                Assert.Equal(-1, index);
            }

            for (int length = 1; length <= byte.MaxValue + 1; length++) {
                char[] a = new char[length + 2];
                a[0] = (char)99;
                a[length + 1] = (char)99;
                ReadOnlyExSpan<char> span = new ReadOnlyExSpan<char>(a, 1, length - 1);
                ReadOnlyExSpan<char> values = new ReadOnlyExSpan<char>(new char[] { (char)99, (char)99, (char)99, (char)99, (char)99, (char)99 });
                TSize index = span.IndexOfAny(values);
                Assert.Equal(-1, index);
            }
        }
    }
}
