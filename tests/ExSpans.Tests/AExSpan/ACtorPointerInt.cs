using Xunit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Zyl.ExSpans.Tests.AExSpan {
    public static partial class ACtorPointerInt {
        [Fact]
        public static void CtorPointerInt() {
            unsafe {
                int[] a = { 90, 91, 92 };
                fixed (int* pa = a) {
                    ExSpan<int> span = new ExSpan<int>(pa, 3);
                    span.Validate(90, 91, 92);
                    Assert.True(Unsafe.AreSame(ref Unsafe.AsRef<int>(pa), ref ExMemoryMarshal.GetReference(span)));
                }
            }
        }

        [Fact]
        public static void CtorPointerNull() {
            unsafe {
                ExSpan<int> span = new ExSpan<int>((void*)null, 0);
                span.Validate();
                Assert.True(Unsafe.AreSame(ref Unsafe.AsRef<int>((void*)null), ref ExMemoryMarshal.GetReference(span)));
            }
        }

        [Fact]
        public static void CtorPointerRangeChecks() {
            unsafe {
                Assert.Throws<ArgumentOutOfRangeException>(
                    delegate () {
                        int i = 42;
                        ExSpan<int> span = new ExSpan<int>(&i, -1);
                    });
            }
        }

        [Fact]
        public static void CtorPointerNoContainsReferenceEnforcement() {
            unsafe {
                new ExSpan<int>((void*)null, 0);
                new ExSpan<int?>((void*)null, 0);
                AssertExtensions.Throws<ArgumentException>(null, () => new ExSpan<object>((void*)null, 0).DontBox());
                AssertExtensions.Throws<ArgumentException>(null, () => new ExSpan<TestHelpers.StructWithReferences>((void*)null, 0).DontBox());
            }
        }
    }
}
