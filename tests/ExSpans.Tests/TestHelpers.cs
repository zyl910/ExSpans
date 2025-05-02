#if SIZE_UINTPTR
global using TSize = nuint; //System.UIntPtr;
global using TSize32 = System.UInt32;
#else
global using TSize = nint; //System.IntPtr;
global using TSize32 = System.Int32;
#endif // SIZE_UINTPTR
global using TUSize = nuint; //System.UIntPtr;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Zyl.ExSpans.Tests {
    public static class TestHelpers {

        public delegate void AssertThrowsAction<T>(ExSpan<T> span);

        // Cannot use standard Assert.Throws() when testing Span - Span and closures don't get along.
        public static void AssertThrows<E, T>(ExSpan<T> span, AssertThrowsAction<T> action) where E : Exception {
            try {
                action(span);
                Assert.Fail($"Expected exception: {typeof(E)}");
            } catch (Exception ex) {
                Assert.True(ex is E, $"Wrong exception thrown. Expected: {typeof(E)} Actual: {ex.GetType()}");
            }
        }

        public delegate void AssertThrowsActionReadOnly<T>(ReadOnlyExSpan<T> span);

        // Cannot use standard Assert.Throws() when testing ExSpan - ExSpan and closures don't get along.
        public static void AssertThrows<E, T>(ReadOnlyExSpan<T> span, AssertThrowsActionReadOnly<T> action) where E : Exception {
            try {
                action(span);
                Assert.Fail($"Expected exception: {typeof(E)}");
            } catch (Exception ex) {
                Assert.True(ex is E, $"Wrong exception thrown. Expected: {typeof(E)} Actual: {ex.GetType()}");
            }
        }

        public static TheoryData<string?[]?, string?[]?, bool> SequenceEqualsNullData => new TheoryData<string?[]?, string?[]?, bool>()
        {
            { new string?[] { "1", null, "2" }, new string?[] { "1", null, "2" } , true},
            { new string?[] { "1", null, "2" }, new string?[] { "1", "3", "2" } , false},
            { new string?[] { "1", null, "2" }, new string?[] { null, "3", "2" } , false},
            { new string?[] { "1", null, "2" }, new string?[] { null } , false},
            { new string?[] { "1", null, "2" }, null , false},

            { new string?[] { null, "2", "1" }, new string?[] { null, "2" } , false},

            { null, new string?[] { null }, false},
            { null, null , true},
            { null, new string[] { "1", "3", "2" } , false},
            { null, new string?[] { "1", null, "2" } , false},

            { new string?[] { "1", null, null }, new string?[] { "1", null, null }, true},
            { new string?[] { null, null, null }, new string?[] { null, null, null }, true},
        };

        public enum TestEnum {
            E0,
            E1,
            E2,
            E3,
            E4,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct TestValueTypeWithReference {
            public int I;
            public string S;
        }

    }
}
