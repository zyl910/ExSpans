using System.Buffers;
using System.Text;
using Xunit;

namespace Zyl.ExSpans.Tests.AExSpan {
    public static partial class ALastIndexOfAny {
#if NET8_0_OR_GREATER && TODO // [TODO why] SearchValues methods is internal
        [Theory]
        [InlineData("a", "a", 'a', 0)]
        [InlineData("ab", "a", 'a', 0)]
        [InlineData("aab", "a", 'a', 1)]
        [InlineData("acab", "a", 'a', 2)]
        [InlineData("acab", "c", 'c', 1)]
        [InlineData("abcdefghijklmnopqrstuvwxyz", "lo", 'o', 14)]
        [InlineData("abcdefghijklmnopqrstuvwxyz", "ol", 'o', 14)]
        [InlineData("abcdefghijklmnopqrstuvwxyz", "ll", 'l', 11)]
        [InlineData("abcdefghijklmnopqrstuvwxyz", "lmr", 'r', 17)]
        [InlineData("abcdefghijklmnopqrstuvwxyz", "rml", 'r', 17)]
        [InlineData("abcdefghijklmnopqrstuvwxyz", "mlr", 'r', 17)]
        [InlineData("abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyz", "lmr", 'r', 43)]
        [InlineData("abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrzzzzzzzz", "lmr", 'r', 43)]
        [InlineData("abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqxzzzzzzzz", "lmr", 'm', 38)]
        [InlineData("abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqlzzzzzzzz", "lmr", 'l', 43)]
        [InlineData("/localhost:5000/PATH/%2FPATH2/ HTTP/1.1", " %?", ' ', 30)]
        [InlineData("/localhost:5000/PATH/%2FPATH2/?key=value HTTP/1.1", " %?", ' ', 40)]
        [InlineData("/localhost:5000/PATH/PATH2/?key=value HTTP/1.1", " %?", ' ', 37)]
        [InlineData("/localhost:5000/PATH/PATH2/ HTTP/1.1", " %?", ' ', 27)]
        public static void LastIndexOfAnyStrings_Byte(string raw, string search, char expectResult, int expectIndex) {
            byte[] buffers = Encoding.UTF8.GetBytes(raw);
            var span = new ExSpan<byte>(buffers);
            char[] searchFor = search.ToCharArray();
            byte[] searchForBytes = Encoding.UTF8.GetBytes(searchFor);

            TSize index = -1;
            if (searchFor.Length == 1) {
                index = LastIndexOf(span, (byte)searchFor[0]);
            } else if (searchFor.Length == 2) {
                index = LastIndexOfAny(span, (byte)searchFor[0], (byte)searchFor[1]);
            } else if (searchFor.Length == 3) {
                index = LastIndexOfAny(span, (byte)searchFor[0], (byte)searchFor[1], (byte)searchFor[2]);
            } else {
                index = LastIndexOfAny(span, new ReadOnlyExSpan<byte>(searchForBytes));
            }

            var found = span[index];
            Assert.Equal((byte)expectResult, found);
            Assert.Equal(expectIndex, index);
        }
#endif // NET8_0_OR_GREATER

        [Fact]
        public static void ZeroLengthLastIndexOfAny_Byte_TwoByte() {
            ExSpan<byte> span = new ExSpan<byte>(ArrayHelper.Empty<byte>());
            TSize idx = LastIndexOfAny(span, 0, 0);
            Assert.Equal(-1, idx);
        }

        [Fact]
        public static void DefaultFilledLastIndexOfAny_Byte_TwoByte() {
            Random rnd = new Random(42);

            for (int length = 0; length < byte.MaxValue; length++) {
                byte[] a = new byte[length];
                ExSpan<byte> span = new ExSpan<byte>(a);

                byte[] targets = { default, 99 };

                for (int i = 0; i < length; i++) {
                    TSize index = rnd.Next(0, 2) == 0 ? 0 : 1;
                    byte target0 = targets[index];
                    byte target1 = targets[(index + 1) % 2];
                    TSize idx = LastIndexOfAny(span, target0, target1);
                    Assert.Equal(span.Length - 1, idx);
                }
            }
        }

        [Fact]
        public static void TestMatchLastIndexOfAny_Byte_TwoByte() {
            for (int length = 0; length < byte.MaxValue; length++) {
                byte[] a = new byte[length];
                for (int i = 0; i < length; i++) {
                    a[i] = (byte)(i + 1);
                }
                ExSpan<byte> span = new ExSpan<byte>(a);

                for (int targetIndex = 0; targetIndex < length; targetIndex++) {
                    byte target0 = a[targetIndex];
                    byte target1 = 0;
                    TSize idx = LastIndexOfAny(span, target0, target1);
                    Assert.Equal(targetIndex, idx);
                }

                for (int targetIndex = 0; targetIndex < length - 1; targetIndex++) {
                    byte target0 = a[targetIndex];
                    byte target1 = a[targetIndex + 1];
                    TSize idx = LastIndexOfAny(span, target0, target1);
                    Assert.Equal(targetIndex + 1, idx);
                }

                for (int targetIndex = 0; targetIndex < length - 1; targetIndex++) {
                    byte target0 = 0;
                    byte target1 = a[targetIndex + 1];
                    TSize idx = LastIndexOfAny(span, target0, target1);
                    Assert.Equal(targetIndex + 1, idx);
                }
            }
        }

        [Fact]
        public static void TestNoMatchLastIndexOfAny_Byte_TwoByte() {
            var rnd = new Random(42);
            for (int length = 0; length < byte.MaxValue; length++) {
                byte[] a = new byte[length];
                byte target0 = (byte)rnd.Next(1, 256);
                byte target1 = (byte)rnd.Next(1, 256);
                ExSpan<byte> span = new ExSpan<byte>(a);

                TSize idx = LastIndexOfAny(span, target0, target1);
                Assert.Equal(-1, idx);
            }
        }

        [Fact]
        public static void TestMultipleMatchLastIndexOfAny_Byte_TwoByte() {
            for (int length = 3; length < byte.MaxValue; length++) {
                byte[] a = new byte[length];
                for (int i = 0; i < length; i++) {
                    byte val = (byte)(i + 1);
                    a[i] = val == 200 ? (byte)201 : val;
                }

                a[length - 1] = 200;
                a[length - 2] = 200;
                a[length - 3] = 200;

                ExSpan<byte> span = new ExSpan<byte>(a);
                TSize idx = LastIndexOfAny(span, 200, 200);
                Assert.Equal(length - 1, idx);
            }
        }

        [Fact]
        public static void MakeSureNoChecksGoOutOfRangeLastIndexOfAny_Byte_TwoByte() {
            for (int length = 1; length < byte.MaxValue; length++) {
                byte[] a = new byte[length + 2];
                a[0] = 99;
                a[length + 1] = 98;
                ExSpan<byte> span = new ExSpan<byte>(a, 1, length - 1);
                TSize index = LastIndexOfAny(span, 99, 98);
                Assert.Equal(-1, index);
            }

            for (int length = 1; length < byte.MaxValue; length++) {
                byte[] a = new byte[length + 2];
                a[0] = 99;
                a[length + 1] = 99;
                ExSpan<byte> span = new ExSpan<byte>(a, 1, length - 1);
                TSize index = LastIndexOfAny(span, 99, 99);
                Assert.Equal(-1, index);
            }
        }

        [Fact]
        public static void ZeroLengthIndexOf_Byte_ThreeByte() {
            ExSpan<byte> span = new ExSpan<byte>(ArrayHelper.Empty<byte>());
            TSize idx = LastIndexOfAny(span, 0, 0, 0);
            Assert.Equal(-1, idx);
        }

        [Fact]
        public static void DefaultFilledLastIndexOfAny_Byte_ThreeByte() {
            Random rnd = new Random(42);

            for (int length = 0; length < byte.MaxValue; length++) {
                byte[] a = new byte[length];
                ExSpan<byte> span = new ExSpan<byte>(a);

                byte[] targets = { default, 99, 98 };

                for (int i = 0; i < length; i++) {
                    TSize index = rnd.Next(0, 3);
                    byte target0 = targets[index];
                    byte target1 = targets[(index + 1) % 2];
                    byte target2 = targets[(index + 1) % 3];
                    TSize idx = LastIndexOfAny(span, target0, target1, target2);
                    Assert.Equal(span.Length - 1, idx);
                }
            }
        }

        [Fact]
        public static void TestMatchLastIndexOfAny_Byte_ThreeByte() {
            for (int length = 0; length < byte.MaxValue; length++) {
                byte[] a = new byte[length];
                for (int i = 0; i < length; i++) {
                    a[i] = (byte)(i + 1);
                }
                ExSpan<byte> span = new ExSpan<byte>(a);

                for (int targetIndex = 0; targetIndex < length; targetIndex++) {
                    byte target0 = a[targetIndex];
                    byte target1 = 0;
                    byte target2 = 0;
                    TSize idx = LastIndexOfAny(span, target0, target1, target2);
                    Assert.Equal(targetIndex, idx);
                }

                for (int targetIndex = 0; targetIndex < length - 2; targetIndex++) {
                    byte target0 = a[targetIndex];
                    byte target1 = a[targetIndex + 1];
                    byte target2 = a[targetIndex + 2];
                    TSize idx = LastIndexOfAny(span, target0, target1, target2);
                    Assert.Equal(targetIndex + 2, idx);
                }

                for (int targetIndex = 0; targetIndex < length - 2; targetIndex++) {
                    byte target0 = 0;
                    byte target1 = 0;
                    byte target2 = a[targetIndex + 2];
                    TSize idx = LastIndexOfAny(span, target0, target1, target2);
                    Assert.Equal(targetIndex + 2, idx);
                }
            }
        }

        [Fact]
        public static void TestNoMatchLastIndexOfAny_Byte_ThreeByte() {
            var rnd = new Random(42);
            for (int length = 0; length < byte.MaxValue; length++) {
                byte[] a = new byte[length];
                byte target0 = (byte)rnd.Next(1, 256);
                byte target1 = (byte)rnd.Next(1, 256);
                byte target2 = (byte)rnd.Next(1, 256);
                ExSpan<byte> span = new ExSpan<byte>(a);

                TSize idx = LastIndexOfAny(span, target0, target1, target2);
                Assert.Equal(-1, idx);
            }
        }

        [Fact]
        public static void TestMultipleMatchLastIndexOfAny_Byte_ThreeByte() {
            for (int length = 4; length < byte.MaxValue; length++) {
                byte[] a = new byte[length];
                for (int i = 0; i < length; i++) {
                    byte val = (byte)(i + 1);
                    a[i] = val == 200 ? (byte)201 : val;
                }

                a[length - 1] = 200;
                a[length - 2] = 200;
                a[length - 3] = 200;
                a[length - 4] = 200;

                ExSpan<byte> span = new ExSpan<byte>(a);
                TSize idx = LastIndexOfAny(span, 200, 200, 200);
                Assert.Equal(length - 1, idx);
            }
        }

        [Fact]
        public static void MakeSureNoChecksGoOutOfRangeLastIndexOfAny_Byte_ThreeByte() {
            for (int length = 1; length < byte.MaxValue; length++) {
                byte[] a = new byte[length + 2];
                a[0] = 99;
                a[length + 1] = 98;
                ExSpan<byte> span = new ExSpan<byte>(a, 1, length - 1);
                TSize index = LastIndexOfAny(span, 99, 98, 99);
                Assert.Equal(-1, index);
            }

            for (int length = 1; length < byte.MaxValue; length++) {
                byte[] a = new byte[length + 2];
                a[0] = 99;
                a[length + 1] = 99;
                ExSpan<byte> span = new ExSpan<byte>(a, 1, length - 1);
                TSize index = LastIndexOfAny(span, 99, 99, 99);
                Assert.Equal(-1, index);
            }
        }

#if NET8_0_OR_GREATER && TODO // [TODO why] SearchValues methods is internal
        [Fact]
        public static void ZeroLengthLastIndexOfAny_Byte_ManyByte() {
            ExSpan<byte> span = new ExSpan<byte>(ArrayHelper.Empty<byte>());
            var values = new ReadOnlyExSpan<byte>(new byte[] { 0, 0, 0, 0 });
            TSize idx = LastIndexOfAny(span, values);
            Assert.Equal(-1, idx);

            values = new ReadOnlyExSpan<byte>(new byte[] { });
            idx = LastIndexOfAny(span, values);
            Assert.Equal(-1, idx);
        }

        [Fact]
        public static void DefaultFilledLastIndexOfAny_Byte_ManyByte() {
            for (int length = 0; length < byte.MaxValue; length++) {
                byte[] a = new byte[length];
                ExSpan<byte> span = new ExSpan<byte>(a);

                var values = new ReadOnlyExSpan<byte>(new byte[] { default, 99, 98, 0 });

                for (int i = 0; i < length; i++) {
                    TSize idx = LastIndexOfAny(span, values);
                    Assert.Equal(span.Length - 1, idx);
                }
            }
        }

        [Fact]
        public static void TestMatchLastIndexOfAny_Byte_ManyByte() {
            for (int length = 0; length < byte.MaxValue; length++) {
                byte[] a = new byte[length];
                for (int i = 0; i < length; i++) {
                    a[i] = (byte)(i + 1);
                }
                ExSpan<byte> span = new ExSpan<byte>(a);

                for (int targetIndex = 0; targetIndex < length; targetIndex++) {
                    var values = new ReadOnlyExSpan<byte>(new byte[] { a[targetIndex], 0, 0, 0 });
                    TSize idx = LastIndexOfAny(span, values);
                    Assert.Equal(targetIndex, idx);
                }

                for (int targetIndex = 0; targetIndex < length - 3; targetIndex++) {
                    var values = new ReadOnlyExSpan<byte>(new byte[] { a[targetIndex], a[targetIndex + 1], a[targetIndex + 2], a[targetIndex + 3] });
                    TSize idx = LastIndexOfAny(span, values);
                    Assert.Equal(targetIndex + 3, idx);
                }

                for (int targetIndex = 0; targetIndex < length - 3; targetIndex++) {
                    var values = new ReadOnlyExSpan<byte>(new byte[] { 0, 0, 0, a[targetIndex + 3] });
                    TSize idx = LastIndexOfAny(span, values);
                    Assert.Equal(targetIndex + 3, idx);
                }
            }
        }

        [Fact]
        public static void TestMatchValuesLargerLastIndexOfAny_Byte_ManyByte() {
            var rnd = new Random(42);
            for (int length = 2; length < byte.MaxValue; length++) {
                byte[] a = new byte[length];
                int expectedIndex = length / 2;
                for (int i = 0; i < length; i++) {
                    if (i == expectedIndex) {
                        continue;
                    }
                    a[i] = 255;
                }
                ExSpan<byte> span = new ExSpan<byte>(a);

                byte[] targets = new byte[length * 2];
                for (int i = 0; i < targets.Length; i++) {
                    if (i == length + 1) {
                        continue;
                    }
                    targets[i] = (byte)rnd.Next(1, 255);
                }

                var values = new ReadOnlyExSpan<byte>(targets);
                TSize idx = LastIndexOfAny(span, values);
                Assert.Equal(expectedIndex, idx);
            }
        }

        [Fact]
        public static void TestNoMatchLastIndexOfAny_Byte_ManyByte() {
            var rnd = new Random(42);
            for (int length = 1; length < byte.MaxValue; length++) {
                byte[] a = new byte[length];
                byte[] targets = new byte[length];
                for (int i = 0; i < targets.Length; i++) {
                    targets[i] = (byte)rnd.Next(1, 256);
                }
                ExSpan<byte> span = new ExSpan<byte>(a);
                var values = new ReadOnlyExSpan<byte>(targets);

                TSize idx = LastIndexOfAny(span, values);
                Assert.Equal(-1, idx);
            }
        }

        [Fact]
        public static void TestNoMatchValuesLargerLastIndexOfAny_Byte_ManyByte() {
            var rnd = new Random(42);
            for (int length = 1; length < byte.MaxValue; length++) {
                byte[] a = new byte[length];
                byte[] targets = new byte[length * 2];
                for (int i = 0; i < targets.Length; i++) {
                    targets[i] = (byte)rnd.Next(1, 256);
                }
                ExSpan<byte> span = new ExSpan<byte>(a);
                var values = new ReadOnlyExSpan<byte>(targets);

                TSize idx = LastIndexOfAny(span, values);
                Assert.Equal(-1, idx);
            }
        }

        [Fact]
        public static void TestMultipleMatchLastIndexOfAny_Byte_ManyByte() {
            for (int length = 5; length < byte.MaxValue; length++) {
                byte[] a = new byte[length];
                for (int i = 0; i < length; i++) {
                    byte val = (byte)(i + 1);
                    a[i] = val == 200 ? (byte)201 : val;
                }

                a[length - 1] = 200;
                a[length - 2] = 200;
                a[length - 3] = 200;
                a[length - 4] = 200;
                a[length - 5] = 200;

                ExSpan<byte> span = new ExSpan<byte>(a);
                var values = new ReadOnlyExSpan<byte>(new byte[] { 200, 200, 200, 200, 200, 200, 200, 200, 200 });
                TSize idx = LastIndexOfAny(span, values);
                Assert.Equal(length - 1, idx);
            }
        }

        [Fact]
        public static void MakeSureNoChecksGoOutOfRangeLastIndexOfAny_Byte_ManyByte() {
            for (int length = 1; length < byte.MaxValue; length++) {
                byte[] a = new byte[length + 2];
                a[0] = 99;
                a[length + 1] = 98;
                ExSpan<byte> span = new ExSpan<byte>(a, 1, length - 1);
                var values = new ReadOnlyExSpan<byte>(new byte[] { 99, 98, 99, 98, 99, 98 });
                TSize index = LastIndexOfAny(span, values);
                Assert.Equal(-1, index);
            }

            for (int length = 1; length < byte.MaxValue; length++) {
                byte[] a = new byte[length + 2];
                a[0] = 99;
                a[length + 1] = 99;
                ExSpan<byte> span = new ExSpan<byte>(a, 1, length - 1);
                var values = new ReadOnlyExSpan<byte>(new byte[] { 99, 99, 99, 99, 99, 99 });
                TSize index = LastIndexOfAny(span, values);
                Assert.Equal(-1, index);
            }
        }
#endif // NET8_0_OR_GREATER

        private static TSize LastIndexOf(ExSpan<byte> span, byte value) {
            TSize index = span.LastIndexOf(value);
#if NET8_0_OR_GREATER && TODO // [TODO why] SearchValues methods is internal
            Assert.Equal(index, span.LastIndexOfAny(SearchValues.Create(stackalloc byte[] { value })));
#else
            Assert.Equal(index, span.LastIndexOf(value));
#endif // NET8_0_OR_GREATER
            return index;
        }

        private static TSize LastIndexOfAny(ExSpan<byte> span, byte value0, byte value1) {
            TSize index = span.LastIndexOfAny(value0, value1);
#if NET8_0_OR_GREATER && TODO // [TODO why] SearchValues methods is internal
            Assert.Equal(index, span.LastIndexOfAny(SearchValues.Create(stackalloc byte[] { value0, value1 })));
#else
            Assert.Equal(index, span.LastIndexOfAny(value0, value1));
#endif // NET8_0_OR_GREATER
            return index;
        }

        private static TSize LastIndexOfAny(ExSpan<byte> span, byte value0, byte value1, byte value2) {
            TSize index = span.LastIndexOfAny(value0, value1, value2);
#if NET8_0_OR_GREATER && TODO // [TODO why] SearchValues methods is internal
            Assert.Equal(index, span.LastIndexOfAny(SearchValues.Create(stackalloc byte[] { value0, value1, value2 })));
#else
            Assert.Equal(index, span.LastIndexOfAny(value0, value1, value2));
#endif // NET8_0_OR_GREATER
            return index;
        }

#if NET8_0_OR_GREATER && TODO // [TODO why] SearchValues methods is internal
        private static TSize LastIndexOfAny(ExSpan<byte> span, ReadOnlyExSpan<byte> values) {
            TSize index = span.LastIndexOfAny(values);
            Assert.Equal(index, span.LastIndexOfAny(SearchValues.Create(values)));
            return index;
        }
#else
#endif // NET8_0_OR_GREATER
    }
}
