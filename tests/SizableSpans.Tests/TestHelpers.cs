global using TSize = System.UIntPtr;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Zyl.SizableSpans.Tests {
    public static class TestHelpers {

        public delegate void AssertThrowsAction<T>(SizableSpan<T> span);

        // Cannot use standard Assert.Throws() when testing Span - Span and closures don't get along.
        public static void AssertThrows<E, T>(SizableSpan<T> span, AssertThrowsAction<T> action) where E : Exception {
            try {
                action(span);
                Assert.Fail($"Expected exception: {typeof(E)}");
            } catch (Exception ex) {
                Assert.True(ex is E, $"Wrong exception thrown. Expected: {typeof(E)} Actual: {ex.GetType()}");
            }
        }

    }
}
