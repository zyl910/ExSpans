using System.Runtime.CompilerServices;
using Xunit;

namespace Zyl.ExSpans.Tests.AExSpan {
    public static partial class AGetPinnableReference {
        [Fact]
        public static void GetPinnableReferenceArray() {
            int[] a = { 91, 92, 93, 94, 95 };
            ExSpan<int> span = new ExSpan<int>(a, 1, 3);
            ref int pinnableReference = ref span.GetPinnableReference();
            Assert.True(Unsafe.AreSame(ref a[1], ref pinnableReference));
        }

        [Fact]
        public static unsafe void UsingExSpanInFixed() {
            byte[] a = { 91, 92, 93, 94, 95 };
            ExSpan<byte> span = a;
            fixed (byte* ptr = span) {
                for (int i = 0; i < span.Length; i++) {
                    Assert.Equal(a[i], ptr[i]);
                }
            }
        }

        [Fact]
        public static unsafe void UsingEmptyExSpanInFixed() {
            ExSpan<int> span = ExSpan<int>.Empty;
            fixed (int* ptr = span) {
                Assert.True(ptr == null);
            }

            ExSpan<int> spanFromEmptyArray = ArrayHelper.Empty<int>();
            fixed (int* ptr = spanFromEmptyArray) {
                Assert.True(ptr == null);
            }
        }

        [Fact]
        public static unsafe void GetPinnableReferenceArrayPastEnd() {
            // The only real difference between GetPinnableReference() and "ref span[0]" is that
            // GetPinnableReference() of a zero-length won't throw an IndexOutOfRange but instead return a null ref.

            int[] a = { 91, 92, 93, 94, 95 };
            ExSpan<int> span = new ExSpan<int>(a, a.Length, 0);
            ref int pinnableReference = ref span.GetPinnableReference();
            ref int expected = ref Unsafe.AsRef<int>(null);
            Assert.True(Unsafe.AreSame(ref expected, ref pinnableReference));
        }

        [Fact]
        public static unsafe void GetPinnableReferencePointer() {
            int i = 42;
            ExSpan<int> span = new ExSpan<int>(&i, 1);
            ref int pinnableReference = ref span.GetPinnableReference();
            Assert.True(Unsafe.AreSame(ref i, ref pinnableReference));
        }

        [Fact]
        public static unsafe void GetPinnableReferenceEmpty() {
            ExSpan<int> span = ExSpan<int>.Empty;
            ref int pinnableReference = ref span.GetPinnableReference();
            Assert.True(Unsafe.AreSame(ref Unsafe.AsRef<int>(null), ref pinnableReference));

            span = ArrayHelper.Empty<int>();
            pinnableReference = ref span.GetPinnableReference();
            Assert.True(Unsafe.AreSame(ref Unsafe.AsRef<int>(null), ref pinnableReference));
        }
    }
}
