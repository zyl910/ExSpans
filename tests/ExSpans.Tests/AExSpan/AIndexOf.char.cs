using Xunit;

namespace Zyl.ExSpans.Tests.AExSpan {
    public static partial class AIndexOf {
        [Fact]
        public static void ZeroLengthIndexOf_Char() {
            ExSpan<char> sp = new ExSpan<char>(ArrayHelper.Empty<char>());
            TSize idx = sp.IndexOf((char)0);
            Assert.Equal(-1, idx);
        }

        [Fact]
        public static void TestMatch_Char() {
            for (int length = 0; length < 32; length++) {
                char[] a = new char[length];
                for (int i = 0; i < length; i++) {
                    a[i] = (char)(i + 1);
                }
                ExSpan<char> span = new ExSpan<char>(a);

                for (int targetIndex = 0; targetIndex < length; targetIndex++) {
                    char target = a[targetIndex];
                    TSize idx = span.IndexOf(target);
                    Assert.Equal(targetIndex, idx);
                }
            }
        }

        [Fact]
        public static void TestMultipleMatch_Char() {
            for (int length = 2; length < 32; length++) {
                char[] a = new char[length];
                for (int i = 0; i < length; i++) {
                    a[i] = (char)(i + 1);
                }

                a[length - 1] = (char)200;
                a[length - 2] = (char)200;

                ExSpan<char> span = new ExSpan<char>(a);
                TSize idx = span.IndexOf((char)200);
                Assert.Equal(length - 2, idx);
            }
        }

        [Fact]
        public static void MakeSureNoChecksGoOutOfRange_Char() {
            for (int length = 0; length < 100; length++) {
                char[] a = new char[length + 2];
                a[0] = '9';
                a[length + 1] = '9';
                ExSpan<char> span = new ExSpan<char>(a, 1, length);
                TSize index = span.IndexOf('9');
                Assert.Equal(-1, index);
            }
        }

        [Fact]
        public static void IndexOfWorksOnAvx512_Char() {
            // Regression test for https://github.com/dotnet/runtime/issues/89512

            var arr = new char[64];
            arr[1] = (char)(1);
            Assert.Equal(1, arr.AsExSpan().IndexOf((char)(1)));
        }
    }
}
