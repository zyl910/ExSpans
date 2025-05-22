using Xunit;
using System.Collections.Generic;

namespace Zyl.ExSpans.Tests.AReadOnlyExSpan {
    public partial class AToUpperLower {
        public static IEnumerable<object[]> MemoryExtensionsToUpperLowerOverlapping() {
            // full overlap, overlap in the middle, overlap at start, overlap at the end

            yield return new TestHelpers.AssertThrowsAction<char>[] { (ExSpan<char> buffer) => ((ReadOnlyExSpan<char>)buffer).ToLower(buffer, null) };
            yield return new TestHelpers.AssertThrowsAction<char>[] { (ExSpan<char> buffer) => ((ReadOnlyExSpan<char>)buffer).ToLower(buffer.Slice(1, 1), null) };
            yield return new TestHelpers.AssertThrowsAction<char>[] { (ExSpan<char> buffer) => ((ReadOnlyExSpan<char>)buffer).ToLower(buffer.Slice(0, 1), null) };
            yield return new TestHelpers.AssertThrowsAction<char>[] { (ExSpan<char> buffer) => ((ReadOnlyExSpan<char>)buffer).ToLower(buffer.Slice(2, 1), null) };

            yield return new TestHelpers.AssertThrowsAction<char>[] { (ExSpan<char> buffer) => ExMemoryExtensions.ToLower(buffer, buffer, null) };
            yield return new TestHelpers.AssertThrowsAction<char>[] { (ExSpan<char> buffer) => ExMemoryExtensions.ToLower(buffer, buffer.Slice(1, 1), null) };
            yield return new TestHelpers.AssertThrowsAction<char>[] { (ExSpan<char> buffer) => ExMemoryExtensions.ToLower(buffer, buffer.Slice(0, 1), null) };
            yield return new TestHelpers.AssertThrowsAction<char>[] { (ExSpan<char> buffer) => ExMemoryExtensions.ToLower(buffer, buffer.Slice(2, 1), null) };

            yield return new TestHelpers.AssertThrowsAction<char>[] { (ExSpan<char> buffer) => ((ReadOnlyExSpan<char>)buffer).ToLowerInvariant(buffer) };
            yield return new TestHelpers.AssertThrowsAction<char>[] { (ExSpan<char> buffer) => ((ReadOnlyExSpan<char>)buffer).ToLowerInvariant(buffer.Slice(1, 1)) };
            yield return new TestHelpers.AssertThrowsAction<char>[] { (ExSpan<char> buffer) => ((ReadOnlyExSpan<char>)buffer).ToLowerInvariant(buffer.Slice(0, 1)) };
            yield return new TestHelpers.AssertThrowsAction<char>[] { (ExSpan<char> buffer) => ((ReadOnlyExSpan<char>)buffer).ToLowerInvariant(buffer.Slice(2, 1)) };

            yield return new TestHelpers.AssertThrowsAction<char>[] { (ExSpan<char> buffer) => ExMemoryExtensions.ToLowerInvariant(buffer, buffer) };
            yield return new TestHelpers.AssertThrowsAction<char>[] { (ExSpan<char> buffer) => ExMemoryExtensions.ToLowerInvariant(buffer, buffer.Slice(1, 1)) };
            yield return new TestHelpers.AssertThrowsAction<char>[] { (ExSpan<char> buffer) => ExMemoryExtensions.ToLowerInvariant(buffer, buffer.Slice(0, 1)) };
            yield return new TestHelpers.AssertThrowsAction<char>[] { (ExSpan<char> buffer) => ExMemoryExtensions.ToLowerInvariant(buffer, buffer.Slice(2, 1)) };

            yield return new TestHelpers.AssertThrowsAction<char>[] { (ExSpan<char> buffer) => ((ReadOnlyExSpan<char>)buffer).ToUpper(buffer, null) };
            yield return new TestHelpers.AssertThrowsAction<char>[] { (ExSpan<char> buffer) => ((ReadOnlyExSpan<char>)buffer).ToUpper(buffer.Slice(1, 1), null) };
            yield return new TestHelpers.AssertThrowsAction<char>[] { (ExSpan<char> buffer) => ((ReadOnlyExSpan<char>)buffer).ToUpper(buffer.Slice(0, 1), null) };
            yield return new TestHelpers.AssertThrowsAction<char>[] { (ExSpan<char> buffer) => ((ReadOnlyExSpan<char>)buffer).ToUpper(buffer.Slice(2, 1), null) };

            yield return new TestHelpers.AssertThrowsAction<char>[] { (ExSpan<char> buffer) => ExMemoryExtensions.ToUpper(buffer, buffer, null) };
            yield return new TestHelpers.AssertThrowsAction<char>[] { (ExSpan<char> buffer) => ExMemoryExtensions.ToUpper(buffer, buffer.Slice(1, 1), null) };
            yield return new TestHelpers.AssertThrowsAction<char>[] { (ExSpan<char> buffer) => ExMemoryExtensions.ToUpper(buffer, buffer.Slice(0, 1), null) };
            yield return new TestHelpers.AssertThrowsAction<char>[] { (ExSpan<char> buffer) => ExMemoryExtensions.ToUpper(buffer, buffer.Slice(2, 1), null) };

            yield return new TestHelpers.AssertThrowsAction<char>[] { (ExSpan<char> buffer) => ((ReadOnlyExSpan<char>)buffer).ToUpperInvariant(buffer) };
            yield return new TestHelpers.AssertThrowsAction<char>[] { (ExSpan<char> buffer) => ((ReadOnlyExSpan<char>)buffer).ToUpperInvariant(buffer.Slice(1, 1)) };
            yield return new TestHelpers.AssertThrowsAction<char>[] { (ExSpan<char> buffer) => ((ReadOnlyExSpan<char>)buffer).ToUpperInvariant(buffer.Slice(0, 1)) };
            yield return new TestHelpers.AssertThrowsAction<char>[] { (ExSpan<char> buffer) => ((ReadOnlyExSpan<char>)buffer).ToUpperInvariant(buffer.Slice(2, 1)) };

            yield return new TestHelpers.AssertThrowsAction<char>[] { (ExSpan<char> buffer) => ExMemoryExtensions.ToUpperInvariant(buffer, buffer) };
            yield return new TestHelpers.AssertThrowsAction<char>[] { (ExSpan<char> buffer) => ExMemoryExtensions.ToUpperInvariant(buffer, buffer.Slice(1, 1)) };
            yield return new TestHelpers.AssertThrowsAction<char>[] { (ExSpan<char> buffer) => ExMemoryExtensions.ToUpperInvariant(buffer, buffer.Slice(0, 1)) };
            yield return new TestHelpers.AssertThrowsAction<char>[] { (ExSpan<char> buffer) => ExMemoryExtensions.ToUpperInvariant(buffer, buffer.Slice(2, 1)) };
        }

        [Theory]
        [MemberData(nameof(MemoryExtensionsToUpperLowerOverlapping))]
        public static void MemoryExtensionsToUpperLowerOverlappingThrows(TestHelpers.AssertThrowsAction<char> action) {
            ExSpan<char> buffer = new char[] { 'a', 'b', 'c', 'd' };
            TestHelpers.AssertThrows<InvalidOperationException, char>(buffer, action);
        }
    }
}
