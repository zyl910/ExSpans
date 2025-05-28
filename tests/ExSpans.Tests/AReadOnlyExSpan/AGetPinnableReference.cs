#if NET7_0_OR_GREATER
#define STRUCT_REF_FIELD // C# 11 - ref fields and ref scoped variables. https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/ref-struct#ref-fields
#endif // NET7_0_OR_GREATER

using System.Runtime.CompilerServices;
using Xunit;

namespace Zyl.ExSpans.Tests.AReadOnlyExSpan {
    public static partial class AGetPinnableReference {
        [Fact]
        public static void GetPinnableReferenceArray() {
            int[] a = { 91, 92, 93, 94, 95 };
            ReadOnlyExSpan<int> span = new ReadOnlyExSpan<int>(a, 1, 3);
            ref int pinnableReference = ref Unsafe.AsRef(in span.GetPinnableReference());
            Assert.True(Unsafe.AreSame(ref a[1], ref pinnableReference));
        }

        [Fact]
        public static unsafe void UsingSpanInFixed() {
            byte[] a = { 91, 92, 93, 94, 95 };
            ReadOnlyExSpan<byte> span = a;
            fixed (byte* ptr = span) {
                for (int i = 0; i < span.Length; i++) {
                    Assert.Equal(a[i], ptr[i]);
                }
            }
        }

        [Fact]
        public static unsafe void UsingEmptySpanInFixed() {
            ReadOnlyExSpan<int> span = ReadOnlyExSpan<int>.Empty;
            fixed (int* ptr = span) {
                Assert.True(ptr == null);
            }

            ReadOnlyExSpan<int> spanFromEmptyArray = ArrayHelper.Empty<int>();
            fixed (int* ptr = spanFromEmptyArray) {
                Assert.True(ptr == null);
            }
        }

        [Fact]
        public static unsafe void GetPinnableReferenceArrayPastEnd() {
            // The only real difference between GetPinnableReference() and "ref span[0]" is that
            // GetPinnableReference() of a zero-length won't throw an IndexOutOfRange but instead return a null ref.

            int[] a = { 91, 92, 93, 94, 95 };
            ReadOnlyExSpan<int> span = new ReadOnlyExSpan<int>(a, a.Length, 0);
            ref int pinnableReference = ref Unsafe.AsRef(in span.GetPinnableReference());
            ref int expected = ref Unsafe.AsRef<int>(null);
#if STRUCT_REF_FIELD
            Assert.False(Unsafe.AreSame(ref expected, ref pinnableReference));
#else
            Assert.True(Unsafe.AreSame(ref expected, ref pinnableReference));
#endif // NOT_RELATED
        }

        [Fact]
        public static unsafe void GetPinnableReferencePointer() {
            int i = 42;
            ReadOnlyExSpan<int> span = new ReadOnlyExSpan<int>(&i, 1);
            ref int pinnableReference = ref Unsafe.AsRef(in span.GetPinnableReference());
            Assert.True(Unsafe.AreSame(ref i, ref pinnableReference));
        }

        [Fact]
        public static unsafe void GetPinnableReferenceEmpty() {
            ReadOnlyExSpan<int> span = ReadOnlyExSpan<int>.Empty;
            ref int pinnableReference = ref Unsafe.AsRef(in span.GetPinnableReference());
            Assert.True(Unsafe.AreSame(ref Unsafe.AsRef<int>(null), ref pinnableReference));

            ReadOnlyExSpan<int> spanFromEmptyArray = ArrayHelper.Empty<int>();
            pinnableReference = ref Unsafe.AsRef(in spanFromEmptyArray.GetPinnableReference());
            Assert.True(Unsafe.AreSame(ref Unsafe.AsRef<int>(null), ref pinnableReference));
        }
    }
}
