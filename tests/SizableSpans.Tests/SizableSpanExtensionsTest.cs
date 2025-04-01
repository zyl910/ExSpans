#if NET9_0_OR_GREATER
#define STRUCT_REF_INTERFACE // C# 13 - ref struct interface; allows ref struct. https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-13#ref-struct-interfaces
#endif // NET9_0_OR_GREATER

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Zyl.SizableSpans.Tests {
    public class SizableSpanExtensionsTest {
        const int bufferSize = 16;

        private readonly ITestOutputHelper _output;

        public SizableSpanExtensionsTest(ITestOutputHelper output) {
            _output = output;
        }

        private ITestOutputHelper Output => _output;

        [Fact]
        public void ItemsToStringTest() {
            const nuint headerLength = (nuint)3;
            const nuint footerLength = (nuint)4;
            Span<int> sourceSpan = stackalloc int[bufferSize];
            SizableSpan<int> span = sourceSpan; // Implicit conversion Span to SizableSpan.
            span.Fill(1);
            span[(nuint)0] = 0;
            span[span.Length - 2] = 2;

            // Output - span.
            TestItem(span);

            // Output - Empty.
            TestItem(SizableSpan<long>.Empty);

            // Output - TSpan.
#if STRUCT_REF_INTERFACE
            // Try without typeSample.
            //Output.WriteLine("TSpan without typeSample: {0}", span.ItemsToString()); // CS0411 The type arguments for method 'SizableSpanExtensions.ItemsToString<T, TSpan>(TSpan, bool)' cannot be inferred from the usage. Try specifying the type arguments explicitly.
            //Output.WriteLine("TSpan without typeSample: {0}", span.ItemsToString<int, SizableSpan<int>>()); // OK. But the code is too long. So it was decided to disable it.
            // OK.
            Output.WriteLine("TSpan with typeSample: {0}", span.ItemsToString(span.GetPinnableReference()));
            Output.WriteLine("TSpan with typeSample and noPrintType: {0}", span.ItemsToString(span.GetPinnableReference(), headerLength, footerLength, false));
            Output.WriteLine("TSpan with footerLength: {0}", span.ItemsToString(span.GetPinnableReference(), headerLength, footerLength));
            Output.WriteLine("TSpan with footerLength and noPrintType: {0}", span.ItemsToString(span.GetPinnableReference(), headerLength, footerLength, false));
#endif // STRUCT_REF_INTERFACE

            void TestItem<T>(SizableSpan<T> span) {
                // Output - SizableSpan.
                Output.WriteLine("SizableSpan with typeSample: {0}", span.ItemsToString());
                Output.WriteLine("SizableSpan with typeSample and noPrintType: {0}", span.ItemsToString(headerLength, footerLength, false));
                Output.WriteLine("SizableSpan with footerLength: {0}", span.ItemsToString(headerLength, footerLength));
                Output.WriteLine("SizableSpan with footerLength and noPrintType: {0}", span.ItemsToString(headerLength, footerLength, false));

                // Output - ReadOnlySizableSpan.
                ReadOnlySizableSpan<T> spanReadOnly = span;
                Output.WriteLine("ReadOnlySizableSpan with typeSample: {0}", spanReadOnly.ItemsToString());
                Output.WriteLine("ReadOnlySizableSpan with typeSample and noPrintType: {0}", spanReadOnly.ItemsToString(headerLength, footerLength, false));
                Output.WriteLine("ReadOnlySizableSpan with footerLength: {0}", spanReadOnly.ItemsToString(headerLength, footerLength));
                Output.WriteLine("ReadOnlySizableSpan with footerLength and noPrintType: {0}", spanReadOnly.ItemsToString(headerLength, footerLength, false));
            }
        }

    }
}
