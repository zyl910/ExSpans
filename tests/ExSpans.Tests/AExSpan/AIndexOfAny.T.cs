using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Xunit;

namespace Zyl.ExSpans.Tests.AExSpan {
    public static partial class AIndexOfAny {
        [Fact]
        public static void ZeroLengthIndexOfAny_TwoInteger() {
            var sp = new ExSpan<int>(ArrayHelper.Empty<int>());
            TSize idx = IndexOfAny(sp, 0, 0);
            Assert.Equal(-1, idx);
        }

        [Fact]
        public static void DefaultFilledIndexOfAny_TwoInteger() {
            var rnd = new Random(42);

            for (int length = 0; length < byte.MaxValue; length++) {
                var a = new int[length];
                var span = new ExSpan<int>(a);

                int[] targets = { default, 99 };

                for (int i = 0; i < length; i++) {
                    TSize index = rnd.Next(0, 2) == 0 ? 0 : 1;
                    int target0 = targets[index];
                    int target1 = targets[(index + 1) % 2];
                    TSize idx = IndexOfAny(span, target0, target1);
                    Assert.Equal(0, idx);
                }
            }
        }

        [Fact]
        public static void TestMatchIndexOfAny_TwoInteger() {
            var rnd = new Random(42);

            for (int length = 0; length < byte.MaxValue; length++) {
                var a = new int[length];
                for (int i = 0; i < length; i++) {
                    a[i] = i + 1;
                }
                var span = new ExSpan<int>(a);

                for (int targetIndex = 0; targetIndex < length; targetIndex++) {
                    int target0 = a[targetIndex];
                    int target1 = 0;
                    TSize idx = IndexOfAny(span, target0, target1);
                    Assert.Equal(targetIndex, idx);
                }

                for (int targetIndex = 0; targetIndex < length - 1; targetIndex++) {
                    TSize index = rnd.Next(0, 2) == 0 ? 0 : 1;
                    int target0 = a[targetIndex + index];
                    int target1 = a[targetIndex + (index + 1) % 2];
                    TSize idx = IndexOfAny(span, target0, target1);
                    Assert.Equal(targetIndex, idx);
                }

                for (int targetIndex = 0; targetIndex < length; targetIndex++) {
                    int target0 = 0;
                    int target1 = a[targetIndex];
                    TSize idx = IndexOfAny(span, target0, target1);
                    Assert.Equal(targetIndex, idx);
                }
            }
        }

        [Fact]
        public static void TestNoMatchIndexOfAny_TwoInteger() {
            var rnd = new Random(42);
            for (int length = 0; length < byte.MaxValue; length++) {
                var a = new int[length];
                int target0 = rnd.Next(1, 256);
                int target1 = rnd.Next(1, 256);
                var span = new ExSpan<int>(a);

                TSize idx = IndexOfAny(span, target0, target1);
                Assert.Equal(-1, idx);
            }
        }

        [Fact]
        public static void TestMultipleMatchIndexOfAny_TwoInteger() {
            for (int length = 3; length < byte.MaxValue; length++) {
                var a = new int[length];
                for (int i = 0; i < length; i++) {
                    int val = i + 1;
                    a[i] = val == 200 ? 201 : val;
                }

                a[length - 1] = 200;
                a[length - 2] = 200;
                a[length - 3] = 200;

                var span = new ExSpan<int>(a);
                TSize idx = IndexOfAny(span, 200, 200);
                Assert.Equal(length - 3, idx);
            }
        }

        [Fact]
        public static void MakeSureNoChecksGoOutOfRangeIndexOfAny_TwoInteger() {
            for (int length = 1; length < byte.MaxValue; length++) {
                var a = new int[length + 2];
                a[0] = 99;
                a[length + 1] = 98;
                var span = new ExSpan<int>(a, 1, length - 1);
                TSize index = IndexOfAny(span, 99, 98);
                Assert.Equal(-1, index);
            }

            for (int length = 1; length < byte.MaxValue; length++) {
                var a = new int[length + 2];
                a[0] = 99;
                a[length + 1] = 99;
                var span = new ExSpan<int>(a, 1, length - 1);
                TSize index = IndexOfAny(span, 99, 99);
                Assert.Equal(-1, index);
            }
        }

        [Fact]
        public static void ZeroLengthIndexOfAny_ThreeInteger() {
            var sp = new ExSpan<int>(ArrayHelper.Empty<int>());
            TSize idx = IndexOfAny(sp, 0, 0, 0);
            Assert.Equal(-1, idx);
        }

        [Fact]
        public static void DefaultFilledIndexOfAny_ThreeInteger() {
            var rnd = new Random(42);

            for (int length = 0; length < byte.MaxValue; length++) {
                var a = new int[length];
                var span = new ExSpan<int>(a);

                int[] targets = { default, 99, 98 };

                for (int i = 0; i < length; i++) {
                    TSize index = rnd.Next(0, 3);
                    int target0 = targets[index];
                    int target1 = targets[(index + 1) % 2];
                    int target2 = targets[(index + 1) % 3];
                    TSize idx = IndexOfAny(span, target0, target1, target2);
                    Assert.Equal(0, idx);
                }
            }
        }

        [Fact]
        public static void TestMatchIndexOfAny_ThreeInteger() {
            var rnd = new Random(42);

            for (int length = 0; length < byte.MaxValue; length++) {
                var a = new int[length];
                for (int i = 0; i < length; i++) {
                    a[i] = i + 1;
                }
                var span = new ExSpan<int>(a);

                for (int targetIndex = 0; targetIndex < length; targetIndex++) {
                    int target0 = a[targetIndex];
                    int target1 = 0;
                    int target2 = 0;
                    TSize idx = IndexOfAny(span, target0, target1, target2);
                    Assert.Equal(targetIndex, idx);
                }

                for (int targetIndex = 0; targetIndex < length - 2; targetIndex++) {
                    TSize index = rnd.Next(0, 3);
                    int target0 = a[targetIndex + index];
                    int target1 = a[targetIndex + (index + 1) % 2];
                    int target2 = a[targetIndex + (index + 1) % 3];
                    TSize idx = IndexOfAny(span, target0, target1, target2);
                    Assert.Equal(targetIndex, idx);
                }

                for (int targetIndex = 0; targetIndex < length; targetIndex++) {
                    int target0 = 0;
                    int target1 = 0;
                    int target2 = a[targetIndex];
                    TSize idx = IndexOfAny(span, target0, target1, target2);
                    Assert.Equal(targetIndex, idx);
                }
            }
        }

        [Fact]
        public static void TestNoMatchIndexOfAny_ThreeInteger() {
            var rnd = new Random(42);
            for (int length = 0; length < byte.MaxValue; length++) {
                var a = new int[length];
                int target0 = rnd.Next(1, 256);
                int target1 = rnd.Next(1, 256);
                int target2 = rnd.Next(1, 256);
                var span = new ExSpan<int>(a);

                TSize idx = IndexOfAny(span, target0, target1, target2);
                Assert.Equal(-1, idx);
            }
        }

        [Fact]
        public static void TestMultipleMatchIndexOfAny_ThreeInteger() {
            for (int length = 4; length < byte.MaxValue; length++) {
                var a = new int[length];
                for (int i = 0; i < length; i++) {
                    int val = i + 1;
                    a[i] = val == 200 ? 201 : val;
                }

                a[length - 1] = 200;
                a[length - 2] = 200;
                a[length - 3] = 200;
                a[length - 4] = 200;

                var span = new ExSpan<int>(a);
                TSize idx = IndexOfAny(span, 200, 200, 200);
                Assert.Equal(length - 4, idx);
            }
        }

        [Fact]
        public static void MakeSureNoChecksGoOutOfRangeIndexOfAny_ThreeInteger() {
            for (int length = 1; length < byte.MaxValue; length++) {
                var a = new int[length + 2];
                a[0] = 99;
                a[length + 1] = 98;
                var span = new ExSpan<int>(a, 1, length - 1);
                TSize index = IndexOfAny(span, 99, 98, 99);
                Assert.Equal(-1, index);
            }

            for (int length = 1; length < byte.MaxValue; length++) {
                var a = new int[length + 2];
                a[0] = 99;
                a[length + 1] = 99;
                var span = new ExSpan<int>(a, 1, length - 1);
                TSize index = IndexOfAny(span, 99, 99, 99);
                Assert.Equal(-1, index);
            }
        }

        [Fact]
        public static void ZeroLengthIndexOfAny_ManyInteger() {
            var sp = new ExSpan<int>(ArrayHelper.Empty<int>());
            var values = new ReadOnlyExSpan<int>(new int[] { 0, 0, 0, 0 });
            TSize idx = IndexOfAny(sp, values);
            Assert.Equal(-1, idx);

            values = new ReadOnlyExSpan<int>(new int[] { });
            idx = IndexOfAny(sp, values);
            Assert.Equal(-1, idx);
        }

        [Fact]
        public static void DefaultFilledIndexOfAny_ManyInteger() {
            for (int length = 0; length < byte.MaxValue; length++) {
                var a = new int[length];
                var span = new ExSpan<int>(a);

                var values = new ReadOnlyExSpan<int>(new int[] { default, 99, 98, 0 });

                for (int i = 0; i < length; i++) {
                    TSize idx = IndexOfAny(span, values);
                    Assert.Equal(0, idx);
                }
            }
        }

        [Fact]
        public static void TestMatchIndexOfAny_ManyInteger() {
            var rnd = new Random(42);

            for (int length = 0; length < byte.MaxValue; length++) {
                var a = new int[length];
                for (int i = 0; i < length; i++) {
                    a[i] = i + 1;
                }
                var span = new ExSpan<int>(a);

                for (int targetIndex = 0; targetIndex < length; targetIndex++) {
                    var values = new ReadOnlyExSpan<int>(new int[] { a[targetIndex], 0, 0, 0 });
                    TSize idx = IndexOfAny(span, values);
                    Assert.Equal(targetIndex, idx);
                }

                for (int targetIndex = 0; targetIndex < length - 3; targetIndex++) {
                    TSize index = rnd.Next(0, 4) == 0 ? 0 : 1;
                    var values = new ReadOnlyExSpan<int>(new int[]
                        {
                            a[targetIndex + index],
                            a[targetIndex + (index + 1) % 2],
                            a[targetIndex + (index + 1) % 3],
                            a[targetIndex + (index + 1) % 4]
                        });
                    TSize idx = IndexOfAny(span, values);
                    Assert.Equal(targetIndex, idx);
                }

                for (int targetIndex = 0; targetIndex < length; targetIndex++) {
                    var values = new ReadOnlyExSpan<int>(new int[] { 0, 0, 0, a[targetIndex] });
                    TSize idx = IndexOfAny(span, values);
                    Assert.Equal(targetIndex, idx);
                }
            }
        }

        [Fact]
        public static void TestMatchValuesLargerIndexOfAny_ManyInteger() {
            var rnd = new Random(42);
            for (int length = 2; length < byte.MaxValue; length++) {
                var a = new int[length];
                int expectedIndex = length / 2;
                for (int i = 0; i < length; i++) {
                    if (i == expectedIndex) {
                        continue;
                    }
                    a[i] = 255;
                }
                var span = new ExSpan<int>(a);

                var targets = new int[length * 2];
                for (int i = 0; i < targets.Length; i++) {
                    if (i == length + 1) {
                        continue;
                    }
                    targets[i] = rnd.Next(1, 255);
                }

                var values = new ReadOnlyExSpan<int>(targets);
                TSize idx = IndexOfAny(span, values);
                Assert.Equal(expectedIndex, idx);
            }
        }

        [Fact]
        public static void TestNoMatchIndexOfAny_ManyInteger() {
            var rnd = new Random(42);
            for (int length = 1; length < byte.MaxValue; length++) {
                var a = new int[length];
                var targets = new int[length];
                for (int i = 0; i < targets.Length; i++) {
                    targets[i] = rnd.Next(1, 256);
                }
                var span = new ExSpan<int>(a);
                var values = new ReadOnlyExSpan<int>(targets);

                TSize idx = IndexOfAny(span, values);
                Assert.Equal(-1, idx);
            }
        }

        [Fact]
        public static void TestNoMatchValuesLargerIndexOfAny_ManyInteger() {
            var rnd = new Random(42);
            for (int length = 1; length < byte.MaxValue; length++) {
                var a = new int[length];
                var targets = new int[length * 2];
                for (int i = 0; i < targets.Length; i++) {
                    targets[i] = rnd.Next(1, 256);
                }
                var span = new ExSpan<int>(a);
                var values = new ReadOnlyExSpan<int>(targets);

                TSize idx = IndexOfAny(span, values);
                Assert.Equal(-1, idx);
            }
        }

        [Fact]
        public static void TestMultipleMatchIndexOfAny_ManyInteger() {
            for (int length = 5; length < byte.MaxValue; length++) {
                var a = new int[length];
                for (int i = 0; i < length; i++) {
                    int val = i + 1;
                    a[i] = val == 200 ? 201 : val;
                }

                a[length - 1] = 200;
                a[length - 2] = 200;
                a[length - 3] = 200;
                a[length - 4] = 200;
                a[length - 5] = 200;

                var span = new ExSpan<int>(a);
                var values = new ReadOnlyExSpan<int>(new int[] { 200, 200, 200, 200, 200, 200, 200, 200, 200 });
                TSize idx = IndexOfAny(span, values);
                Assert.Equal(length - 5, idx);
            }
        }

        [Fact]
        public static void MakeSureNoChecksGoOutOfRangeIndexOfAny_ManyInteger() {
            for (int length = 1; length < byte.MaxValue; length++) {
                var a = new int[length + 2];
                a[0] = 99;
                a[length + 1] = 98;
                var span = new ExSpan<int>(a, 1, length - 1);
                var values = new ExSpan<int>(new int[] { 99, 98, 99, 98, 99, 98 });
                TSize index = IndexOfAny(span, values);
                Assert.Equal(-1, index);
            }

            for (int length = 1; length < byte.MaxValue; length++) {
                var a = new int[length + 2];
                a[0] = 99;
                a[length + 1] = 99;
                var span = new ExSpan<int>(a, 1, length - 1);
                var values = new ReadOnlyExSpan<int>(new int[] { 99, 99, 99, 99, 99, 99 });
                TSize index = IndexOfAny(span, values);
                Assert.Equal(-1, index);
            }
        }

        [Fact]
        public static void ZeroLengthIndexOfAny_TwoString() {
            var sp = new ExSpan<string>(ArrayHelper.Empty<string>());
            TSize idx = IndexOfAny(sp, "0", "0");
            Assert.Equal(-1, idx);
        }

        [Fact]
        public static void DefaultFilledIndexOfAny_TwoString() {
            var rnd = new Random(42);

            for (int length = 0; length < byte.MaxValue; length++) {
                var a = new string[length];
                var tempExSpan = new ExSpan<string>(a);
                tempExSpan.Fill("");
                ExSpan<string> span = tempExSpan;

                string[] targets = { "", "99" };

                for (int i = 0; i < length; i++) {
                    TSize index = rnd.Next(0, 2) == 0 ? 0 : 1;
                    string target0 = targets[index];
                    string target1 = targets[(index + 1) % 2];
                    TSize idx = IndexOfAny(span, target0, target1);
                    Assert.Equal(0, idx);
                }
            }
        }

        [Fact]
        public static void TestMatchIndexOfAny_TwoString() {
            var rnd = new Random(42);

            for (int length = 0; length < byte.MaxValue; length++) {
                var a = new string[length];
                for (int i = 0; i < length; i++) {
                    a[i] = (i + 1).ToString();
                }
                var span = new ExSpan<string>(a);

                for (int targetIndex = 0; targetIndex < length; targetIndex++) {
                    string target0 = a[targetIndex];
                    string target1 = "0";
                    TSize idx = IndexOfAny(span, target0, target1);
                    Assert.Equal(targetIndex, idx);
                }

                for (int targetIndex = 0; targetIndex < length - 1; targetIndex++) {
                    TSize index = rnd.Next(0, 2) == 0 ? 0 : 1;
                    string target0 = a[targetIndex + index];
                    string target1 = a[targetIndex + (index + 1) % 2];
                    TSize idx = IndexOfAny(span, target0, target1);
                    Assert.Equal(targetIndex, idx);
                }

                for (int targetIndex = 0; targetIndex < length - 1; targetIndex++) {
                    string target0 = "0";
                    string target1 = a[targetIndex + 1];
                    TSize idx = IndexOfAny(span, target0, target1);
                    Assert.Equal(targetIndex + 1, idx);
                }
            }
        }

        [Fact]
        public static void TestNoMatchIndexOfAny_TwoString() {
            var rnd = new Random(42);
            for (int length = 0; length < byte.MaxValue; length++) {
                var a = new string[length];
                string target0 = rnd.Next(1, 256).ToString();
                string target1 = rnd.Next(1, 256).ToString();
                var span = new ExSpan<string>(a);

                TSize idx = IndexOfAny(span, target0, target1);
                Assert.Equal(-1, idx);
            }
        }

        [Fact]
        public static void TestMultipleMatchIndexOfAny_TwoString() {
            for (int length = 3; length < byte.MaxValue; length++) {
                var a = new string[length];
                for (int i = 0; i < length; i++) {
                    string val = (i + 1).ToString();
                    a[i] = val == "200" ? "201" : val;
                }

                a[length - 1] = "200";
                a[length - 2] = "200";
                a[length - 3] = "200";

                var span = new ExSpan<string>(a);
                TSize idx = IndexOfAny(span, "200", "200");
                Assert.Equal(length - 3, idx);
            }
        }

        [Fact]
        public static void MakeSureNoChecksGoOutOfRangeIndexOfAny_TwoString() {
            for (int length = 1; length < byte.MaxValue; length++) {
                var a = new string[length + 2];
                a[0] = "99";
                a[length + 1] = "98";
                var span = new ExSpan<string>(a, 1, length - 1);
                TSize index = IndexOfAny(span, "99", "98");
                Assert.Equal(-1, index);
            }

            for (int length = 1; length < byte.MaxValue; length++) {
                var a = new string[length + 2];
                a[0] = "99";
                a[length + 1] = "99";
                var span = new ExSpan<string>(a, 1, length - 1);
                TSize index = IndexOfAny(span, "99", "99");
                Assert.Equal(-1, index);
            }
        }

        [Fact]
        public static void ZeroLengthIndexOf_ThreeString() {
            var sp = new ExSpan<string>(ArrayHelper.Empty<string>());
            TSize idx = IndexOfAny(sp, "0", "0", "0");
            Assert.Equal(-1, idx);
        }

        [Fact]
        public static void DefaultFilledIndexOfAny_ThreeString() {
            var rnd = new Random(42);

            for (int length = 0; length < byte.MaxValue; length++) {
                var a = new string[length];
                var tempExSpan = new ExSpan<string>(a);
                tempExSpan.Fill("");
                ExSpan<string> span = tempExSpan;

                string[] targets = { "", "99", "98" };

                for (int i = 0; i < length; i++) {
                    TSize index = rnd.Next(0, 3);
                    string target0 = targets[index];
                    string target1 = targets[(index + 1) % 2];
                    string target2 = targets[(index + 1) % 3];
                    TSize idx = IndexOfAny(span, target0, target1, target2);
                    Assert.Equal(0, idx);
                }
            }
        }

        [Fact]
        public static void TestMatchIndexOfAny_ThreeString() {
            Random rnd = new Random(42);

            for (int length = 0; length < byte.MaxValue; length++) {
                var a = new string[length];
                for (int i = 0; i < length; i++) {
                    a[i] = (i + 1).ToString();
                }
                var span = new ExSpan<string>(a);

                for (int targetIndex = 0; targetIndex < length; targetIndex++) {
                    string target0 = a[targetIndex];
                    string target1 = "0";
                    string target2 = "0";
                    TSize idx = IndexOfAny(span, target0, target1, target2);
                    Assert.Equal(targetIndex, idx);
                }

                for (int targetIndex = 0; targetIndex < length - 2; targetIndex++) {
                    TSize index = rnd.Next(0, 3) == 0 ? 0 : 1;
                    string target0 = a[targetIndex + index];
                    string target1 = a[targetIndex + (index + 1) % 2];
                    string target2 = a[targetIndex + (index + 1) % 3];
                    TSize idx = IndexOfAny(span, target0, target1, target2);
                    Assert.Equal(targetIndex, idx);
                }

                for (int targetIndex = 0; targetIndex < length; targetIndex++) {
                    string target0 = "0";
                    string target1 = "0";
                    string target2 = a[targetIndex];
                    TSize idx = IndexOfAny(span, target0, target1, target2);
                    Assert.Equal(targetIndex, idx);
                }
            }
        }

        [Fact]
        public static void TestNoMatchIndexOfAny_ThreeString() {
            var rnd = new Random(42);
            for (int length = 0; length < byte.MaxValue; length++) {
                var a = new string[length];
                string target0 = rnd.Next(1, 256).ToString();
                string target1 = rnd.Next(1, 256).ToString();
                string target2 = rnd.Next(1, 256).ToString();
                var span = new ExSpan<string>(a);

                TSize idx = IndexOfAny(span, target0, target1, target2);
                Assert.Equal(-1, idx);
            }
        }

        [Fact]
        public static void TestMultipleMatchIndexOfAny_ThreeString() {
            for (int length = 4; length < byte.MaxValue; length++) {
                var a = new string[length];
                for (int i = 0; i < length; i++) {
                    string val = (i + 1).ToString();
                    a[i] = val == "200" ? "201" : val;
                }

                a[length - 1] = "200";
                a[length - 2] = "200";
                a[length - 3] = "200";
                a[length - 4] = "200";

                var span = new ExSpan<string>(a);
                TSize idx = IndexOfAny(span, "200", "200", "200");
                Assert.Equal(length - 4, idx);
            }
        }

        [Fact]
        public static void MakeSureNoChecksGoOutOfRangeIndexOfAny_ThreeString() {
            for (int length = 1; length < byte.MaxValue; length++) {
                var a = new string[length + 2];
                a[0] = "99";
                a[length + 1] = "98";
                var span = new ExSpan<string>(a, 1, length - 1);
                TSize index = IndexOfAny(span, "99", "98", "99");
                Assert.Equal(-1, index);
            }

            for (int length = 1; length < byte.MaxValue; length++) {
                var a = new string[length + 2];
                a[0] = "99";
                a[length + 1] = "99";
                var span = new ExSpan<string>(a, 1, length - 1);
                TSize index = IndexOfAny(span, "99", "99", "99");
                Assert.Equal(-1, index);
            }
        }

        [Fact]
        public static void ZeroLengthIndexOfAny_ManyString() {
            var sp = new ExSpan<string>(ArrayHelper.Empty<string>());
            var values = new ReadOnlyExSpan<string>(new string[] { "0", "0", "0", "0" });
            TSize idx = IndexOfAny(sp, values);
            Assert.Equal(-1, idx);

            values = new ReadOnlyExSpan<string>(new string[] { });
            idx = IndexOfAny(sp, values);
            Assert.Equal(-1, idx);
        }

        [Fact]
        public static void DefaultFilledIndexOfAny_ManyString() {
            for (int length = 0; length < byte.MaxValue; length++) {
                var a = new string[length];
                var tempExSpan = new ExSpan<string>(a);
                tempExSpan.Fill("");
                ExSpan<string> span = tempExSpan;

                var values = new ReadOnlyExSpan<string>(new string[] { "", "99", "98", "0" });

                for (int i = 0; i < length; i++) {
                    TSize idx = IndexOfAny(span, values);
                    Assert.Equal(0, idx);
                }
            }
        }

        [Fact]
        public static void TestMatchIndexOfAny_ManyString() {
            Random rnd = new Random(42);

            for (int length = 0; length < byte.MaxValue; length++) {
                var a = new string[length];
                for (int i = 0; i < length; i++) {
                    a[i] = (i + 1).ToString();
                }
                var span = new ExSpan<string>(a);

                for (int targetIndex = 0; targetIndex < length; targetIndex++) {
                    var values = new ReadOnlyExSpan<string>(new string[] { a[targetIndex], "0", "0", "0" });
                    TSize idx = IndexOfAny(span, values);
                    Assert.Equal(targetIndex, idx);
                }

                for (int targetIndex = 0; targetIndex < length - 3; targetIndex++) {
                    TSize index = rnd.Next(0, 4) == 0 ? 0 : 1;
                    var values = new ReadOnlyExSpan<string>(new string[]
                    {
                        a[targetIndex + index],
                        a[targetIndex + (index + 1) % 2],
                        a[targetIndex + (index + 1) % 3],
                        a[targetIndex + (index + 1) % 4]
                    });
                    TSize idx = IndexOfAny(span, values);
                    Assert.Equal(targetIndex, idx);
                }

                for (int targetIndex = 0; targetIndex < length; targetIndex++) {
                    var values = new ReadOnlyExSpan<string>(new string[] { "0", "0", "0", a[targetIndex] });
                    TSize idx = IndexOfAny(span, values);
                    Assert.Equal(targetIndex, idx);
                }
            }
        }

        [Fact]
        public static void TestMatchValuesLargerIndexOfAny_ManyString() {
            var rnd = new Random(42);
            for (int length = 2; length < byte.MaxValue; length++) {
                var a = new string[length];
                int expectedIndex = length / 2;
                for (int i = 0; i < length; i++) {
                    if (i == expectedIndex) {
                        a[i] = "val";
                        continue;
                    }
                    a[i] = "255";
                }
                var span = new ExSpan<string>(a);

                var targets = new string[length * 2];
                for (int i = 0; i < targets.Length; i++) {
                    if (i == length + 1) {
                        targets[i] = "val";
                        continue;
                    }
                    targets[i] = rnd.Next(1, 255).ToString();
                }

                var values = new ReadOnlyExSpan<string>(targets);
                TSize idx = IndexOfAny(span, values);
                Assert.Equal(expectedIndex, idx);
            }
        }

        [Fact]
        public static void TestNoMatchIndexOfAny_ManyString() {
            var rnd = new Random(42);
            for (int length = 1; length < byte.MaxValue; length++) {
                var a = new string[length];
                var targets = new string[length];
                for (int i = 0; i < targets.Length; i++) {
                    targets[i] = rnd.Next(1, 256).ToString();
                }
                var span = new ExSpan<string>(a);
                var values = new ReadOnlyExSpan<string>(targets);

                TSize idx = IndexOfAny(span, values);
                Assert.Equal(-1, idx);
            }
        }

        [Fact]
        public static void TestNoMatchValuesLargerIndexOfAny_ManyString() {
            var rnd = new Random(42);
            for (int length = 1; length < byte.MaxValue; length++) {
                var a = new string[length];
                var targets = new string[length * 2];
                for (int i = 0; i < targets.Length; i++) {
                    targets[i] = rnd.Next(1, 256).ToString();
                }
                var span = new ExSpan<string>(a);
                var values = new ReadOnlyExSpan<string>(targets);

                TSize idx = IndexOfAny(span, values);
                Assert.Equal(-1, idx);
            }
        }

        [Fact]
        public static void TestMultipleMatchIndexOfAny_ManyString() {
            for (int length = 5; length < byte.MaxValue; length++) {
                var a = new string[length];
                for (int i = 0; i < length; i++) {
                    string val = (i + 1).ToString();
                    a[i] = val == "200" ? "201" : val;
                }

                a[length - 1] = "200";
                a[length - 2] = "200";
                a[length - 3] = "200";
                a[length - 4] = "200";
                a[length - 5] = "200";

                var span = new ExSpan<string>(a);
                var values = new ReadOnlyExSpan<string>(new string[] { "200", "200", "200", "200", "200", "200", "200", "200", "200" });
                TSize idx = IndexOfAny(span, values);
                Assert.Equal(length - 5, idx);
            }
        }

        [Fact]
        public static void MakeSureNoChecksGoOutOfRangeIndexOfAny_ManyString() {
            for (int length = 1; length < byte.MaxValue; length++) {
                var a = new string[length + 2];
                a[0] = "99";
                a[length + 1] = "98";
                var span = new ExSpan<string>(a, 1, length - 1);
                var values = new ReadOnlyExSpan<string>(new string[] { "99", "98", "99", "98", "99", "98" });
                TSize index = IndexOfAny(span, values);
                Assert.Equal(-1, index);
            }

            for (int length = 1; length < byte.MaxValue; length++) {
                var a = new string[length + 2];
                a[0] = "99";
                a[length + 1] = "99";
                var span = new ExSpan<string>(a, 1, length - 1);
                var values = new ReadOnlyExSpan<string>(new string[] { "99", "99", "99", "99", "99", "99" });
                TSize index = IndexOfAny(span, values);
                Assert.Equal(-1, index);
            }
        }

        [Fact]
        public static void IndexOfAnyExceptWorksOnAvx512_Integer() {
            // Regression test for https://github.com/dotnet/runtime/issues/89512

            var arr = new int[32];
            arr[1] = 1;
            Assert.Equal(1, arr.AsExSpan().IndexOfAnyExcept(0));
        }

        [Theory]
        [MemberData(nameof(TestHelpers.IndexOfAnyNullSequenceData), MemberType = typeof(TestHelpers))]
        public static void IndexOfAnyNullSequence_String(string[] spanInput, string[] searchInput, int expected) {
            ExSpan<string> theStrings = spanInput;
            Assert.Equal(expected, IndexOfAny(theStrings, searchInput));

            if (searchInput != null) {
                if (searchInput.Length >= 3) {
                    Assert.Equal(expected, IndexOfAny(theStrings, searchInput[0], searchInput[1], searchInput[2]));
                }

                if (searchInput.Length >= 2) {
                    Assert.Equal(expected, IndexOfAny(theStrings, searchInput[0], searchInput[1]));
                }
            }
        }

        private static TSize IndexOf<T>(ExSpan<T> span, T value) where T : IEquatable<T>? {
            TSize index = span.IndexOf(value);
            Assert.Equal(index, ((ReadOnlyExSpan<T>)span).IndexOf(value));

            Assert.Equal(index >= 0, span.Contains(value));
            Assert.Equal(index >= 0, ((ReadOnlyExSpan<T>)span).Contains(value));

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
            AssertSearchValues(span, new ReadOnlyExSpan<T>(in value), index);
#endif // NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
            return index;
        }

        private static TSize IndexOfAny<T>(ExSpan<T> span, T value0, T value1) where T : IEquatable<T>? {
            TSize index = span.IndexOfAny(value0, value1);
            Assert.Equal(index, ((ReadOnlyExSpan<T>)span).IndexOfAny(value0, value1));

            Assert.Equal(index >= 0, span.ContainsAny(value0, value1));
            Assert.Equal(index >= 0, ((ReadOnlyExSpan<T>)span).ContainsAny(value0, value1));

            AssertSearchValues<T>(span, new[] { value0, value1 }, index);
            return index;
        }

        private static TSize IndexOfAny<T>(ExSpan<T> span, T value0, T value1, T value2) where T : IEquatable<T>? {
            TSize index = span.IndexOfAny(value0, value1, value2);
            Assert.Equal(index, ((ReadOnlyExSpan<T>)span).IndexOfAny(value0, value1, value2));

            Assert.Equal(index >= 0, span.ContainsAny(value0, value1, value2));
            Assert.Equal(index >= 0, ((ReadOnlyExSpan<T>)span).ContainsAny(value0, value1, value2));

            AssertSearchValues<T>(span, new[] { value0, value1, value2 }, index);
            return index;
        }

        private static TSize IndexOfAny<T>(ExSpan<T> span, ReadOnlyExSpan<T> values) where T : IEquatable<T>? {
            TSize index = span.IndexOfAny(values);
            Assert.Equal(index, ((ReadOnlyExSpan<T>)span).IndexOfAny(values));

            Assert.Equal(index >= 0, span.ContainsAny(values));
            Assert.Equal(index >= 0, ((ReadOnlyExSpan<T>)span).ContainsAny(values));

            AssertSearchValues(span, values, index);
            return index;
        }

        private static void AssertSearchValues<T>(ExSpan<T> span, ReadOnlyExSpan<T> values, TSize expectedIndex) where T : IEquatable<T>? {
#if NET8_0_OR_GREATER && TODO // [TODO why] SearchValues methods is internal
            if (typeof(T) == typeof(byte) || typeof(T) == typeof(char)) {
                SearchValues<T> searchValuesInstance = (SearchValues<T>)(object)(typeof(T) == typeof(byte)
                    ? SearchValues.Create(ExMemoryMarshal.CreateReadOnlyExSpan(ref Unsafe.As<T, byte>(ref ExMemoryMarshal.GetReference(values)), values.Length).AsReadOnlySpan())
                    : SearchValues.Create(ExMemoryMarshal.CreateReadOnlyExSpan(ref Unsafe.As<T, char>(ref ExMemoryMarshal.GetReference(values)), values.Length).AsReadOnlySpan()));

                Assert.Equal(expectedIndex, span.IndexOfAny(searchValuesInstance));
                Assert.Equal(expectedIndex, ((ReadOnlyExSpan<T>)span).IndexOfAny(searchValuesInstance));

                Assert.Equal(expectedIndex >= 0, span.ContainsAny(searchValuesInstance));
                Assert.Equal(expectedIndex >= 0, ((ReadOnlyExSpan<T>)span).ContainsAny(searchValuesInstance));
            }
#endif // NET8_0_OR_GREATER
        }
    }
}
