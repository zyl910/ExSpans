using System.Runtime.InteropServices;
using Xunit;

namespace Zyl.ExSpans.Tests.AExSpan {
    public static partial class AToString {
        [Fact]
        public static void ToStringInt() {
            int[] a = { 91, 92, 93 };
            var span = new ExSpan<int>(a);
            Assert.Equal("System.ExSpan<Int32>[3]", span.ToString());
        }

        [Fact]
        public static void ToStringInt_Empty() {
            var span = new ExSpan<int>();
            Assert.Equal("System.ExSpan<Int32>[0]", span.ToString());
        }

        [Fact]
        public static unsafe void ToStringChar() {
            char[] a = { 'a', 'b', 'c' };
            var span = new ExSpan<char>(a);
            Assert.Equal("abc", span.ToString());

            string testString = "abcdefg";
            ReadOnlyExSpan<char> readOnlyExSpan = testString.AsExSpan();

            fixed (void* ptr = &MemoryMarshal.GetReference(readOnlyExSpan)) {
                var temp = new ExSpan<char>(ptr, readOnlyExSpan.Length);
                Assert.Equal(testString, temp.ToString());
            }
        }

        [Fact]
        public static void ToStringChar_Empty() {
            var span = new ExSpan<char>();
            Assert.Equal("", span.ToString());
        }

        [Fact]
        public static void ToStringForExSpanOfString() {
            string[] a = { "a", "b", "c" };
            var span = new ExSpan<string>(a);
            Assert.Equal("System.ExSpan<String>[3]", span.ToString());
        }

        [Fact]
        public static void ToStringExSpanOverFullStringDoesNotReturnOriginal() {
            string original = TestHelpers.BuildString(10, 42);

            ReadOnlyMemory<char> readOnlyMemory = original.AsMemory();
            Memory<char> memory = MemoryMarshal.AsMemory(readOnlyMemory);

            ExSpan<char> span = memory.ExSpan;

            string returnedString = span.ToString();
            string returnedStringUsingSlice = span.Slice(0, original.Length).ToString();

            string subString1 = span.Slice(1).ToString();
            string subString2 = span.Slice(0, 2).ToString();
            string subString3 = span.Slice(1, 2).ToString();

            Assert.Equal(original, returnedString);
            Assert.Equal(original, returnedStringUsingSlice);

            Assert.Equal(original.Substring(1), subString1);
            Assert.Equal(original.Substring(0, 2), subString2);
            Assert.Equal(original.Substring(1, 2), subString3);

            Assert.NotSame(original, returnedString);
            Assert.NotSame(original, returnedStringUsingSlice);

            Assert.NotSame(original, subString1);
            Assert.NotSame(original, subString2);
            Assert.NotSame(original, subString3);

            Assert.NotSame(subString1, subString2);
            Assert.NotSame(subString1, subString3);
            Assert.NotSame(subString2, subString3);
        }
    }
}
