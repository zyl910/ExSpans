using Xunit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Zyl.ExSpans.Tests.AReadOnlyExSpan {
    public static partial class ACtorPointerInt {
        [Fact]
        public static void CtorPointerInt() {
            unsafe {
                int[] a = { 90, 91, 92 };
                fixed (int* pa = a) {
                    ReadOnlyExSpan<int> span = new ReadOnlyExSpan<int>(pa, 3);
                    span.Validate(90, 91, 92);
                    Assert.True(Unsafe.AreSame(ref Unsafe.AsRef<int>(pa), ref Unsafe.AsRef(in ExMemoryMarshal.GetReference(span))));
                }
            }
        }

        [Fact]
        public static void CtorPointerNull() {
            unsafe {
                ReadOnlyExSpan<int> span = new ReadOnlyExSpan<int>((void*)null, 0);
                span.Validate();
                Assert.True(Unsafe.AreSame(ref Unsafe.AsRef<int>((void*)null), ref Unsafe.AsRef(in ExMemoryMarshal.GetReference(span))));
            }
        }

        [Fact]
        public static void CtorPointerRangeChecks() {
            unsafe {
                Assert.Throws<ArgumentOutOfRangeException>(
                    delegate () {
                        int i = 42;
                        ReadOnlyExSpan<int> span = new ReadOnlyExSpan<int>(&i, -1);
                    });
            }
        }

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_0_OR_GREATER
        [Fact]
        public static void CtorPointerNoContainsReferenceEnforcement() {
            unsafe {
                new ReadOnlyExSpan<int>((void*)null, 0);
                new ReadOnlyExSpan<int?>((void*)null, 0);
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                AssertExtensions.Throws<ArgumentException>(null, () => new ReadOnlyExSpan<object>((void*)null, 0).DontBox());
                AssertExtensions.Throws<ArgumentException>(null, () => new ReadOnlyExSpan<TestHelpers.StructWithReferences>((void*)null, 0).DontBox());
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            }
        }
#endif
    }
}
