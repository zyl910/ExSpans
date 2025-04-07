#if NET9_0_OR_GREATER
#define ALLOWS_REF_STRUCT // C# 13 - ref struct interface; allows ref struct. https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-13#ref-struct-interfaces
#endif // NET9_0_OR_GREATER

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Zyl.SizableSpans.Reflection;

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
            const ItemsToStringFlags stringFlags = ItemsToStringFlags.HideType;
            const TypeNameFlags nameFlags = TypeNameFlags.ShowNamespace | TypeNameFlags.SubShowNamespace;
            const nuint headerLength = (nuint)3;
            const nuint footerLength = (nuint)4;
            Span<int> sourceSpan = stackalloc int[bufferSize];
            SizableSpan<int> span = sourceSpan; // Implicit conversion Span to SizableSpan.
            span.Fill(1);
            span[(nuint)0] = 0;
            span[span.Length - 2] = 2;

            // Output - span.
            CallSizableSpan(span);

            // Output - Empty.
            CallSizableSpan(SizableSpan<long>.Empty);

            // Output - TSpan.
#if ALLOWS_REF_STRUCT
            CallTSpan(span, span.GetPinnableReference());
            Output.WriteLine("TSpan-SizableSpan with typeSample: {0}", span.ItemsToString(span.GetPinnableReference()));
            Output.WriteLine("TSpan-SizableSpan with typeSample and stringFlags: {0}", span.ItemsToString(span.GetPinnableReference(), null, stringFlags));
            Output.WriteLine("TSpan-SizableSpan with typeSample and nameFlags: {0}", span.ItemsToString(span.GetPinnableReference(), null, stringFlags, nameFlags));
            Output.WriteLine("TSpan-SizableSpan with footerLength: {0}", span.ItemsToString(span.GetPinnableReference(), headerLength, footerLength));
            Output.WriteLine("TSpan-SizableSpan with footerLength and stringFlags-HideType: {0}", span.ItemsToString(span.GetPinnableReference(), headerLength, footerLength, null, stringFlags));
            Output.WriteLine("TSpan-SizableSpan with footerLength and stringFlags-HideLength: {0}", span.ItemsToString(span.GetPinnableReference(), headerLength, footerLength, null, ItemsToStringFlags.HideLength));
            Output.WriteLine("TSpan-SizableSpan with footerLength and stringFlags-HideBrace: {0}", span.ItemsToString(span.GetPinnableReference(), headerLength, footerLength, null, ItemsToStringFlags.HideBrace));
            Output.WriteLine("TSpan-SizableSpan with footerLength and stringFlags-All: {0}", span.ItemsToString(span.GetPinnableReference(), headerLength, footerLength, null, ItemsToStringFlagsUtil.All));
            Output.WriteLine("TSpan-SizableSpan with footerLength and itemFormater: {0}", span.ItemsToString(span.GetPinnableReference(), headerLength, footerLength, ItemFormaters.Hex));
            Output.WriteLine("TSpan-SizableSpan with headerLength and footerLength less: {0}", span.ItemsToString(span.GetPinnableReference(), 5, 10));
            Output.WriteLine("TSpan-SizableSpan with headerLength and footerLength equal: {0}", span.ItemsToString(span.GetPinnableReference(), 5, 11));
            Output.WriteLine("TSpan-SizableSpan with headerLength and footerLength greater: {0}", span.ItemsToString(span.GetPinnableReference(), 5, 12));
#endif // ALLOWS_REF_STRUCT

            void CallSizableSpan<T>(SizableSpan<T> span) {
                // Output - SizableSpan.
                Output.WriteLine("SizableSpan: {0}", span.ItemsToString());
                Output.WriteLine("SizableSpan with stringFlags: {0}", span.ItemsToString(null, stringFlags));
                Output.WriteLine("SizableSpan with footerLength: {0}", span.ItemsToString(headerLength, footerLength));
                Output.WriteLine("SizableSpan with footerLength and stringFlags: {0}", span.ItemsToString(headerLength, footerLength, null, stringFlags));

                // Output - ReadOnlySizableSpan.
                ReadOnlySizableSpan<T> spanReadOnly = span;
                Output.WriteLine("ReadOnlySizableSpan: {0}", spanReadOnly.ItemsToString());
                Output.WriteLine("ReadOnlySizableSpan with stringFlags: {0}", spanReadOnly.ItemsToString(null, stringFlags));
                Output.WriteLine("ReadOnlySizableSpan with nameFlags: {0}", spanReadOnly.ItemsToString(null, stringFlags, nameFlags));
                Output.WriteLine("ReadOnlySizableSpan with footerLength: {0}", spanReadOnly.ItemsToString(headerLength, footerLength));
                Output.WriteLine("ReadOnlySizableSpan with footerLength and stringFlags: {0}", spanReadOnly.ItemsToString(headerLength, footerLength, null, stringFlags));
            }

#if ALLOWS_REF_STRUCT
            void CallTSpan<T, TSpan>(TSpan span, in T typeSample)
                    where TSpan : IReadOnlySizableSpanBase<T>, allows ref struct {
                // Try without typeSample.
                //Output.WriteLine("TSpan without typeSample: {0}", span.ItemsToString()); // CS0411 The type arguments for method 'SizableSpanExtensions.ItemsToString<T, TSpan>(TSpan, bool)' cannot be inferred from the usage. Try specifying the type arguments explicitly.
                //Output.WriteLine("TSpan without typeSample: {0}", span.ItemsToString<int, SizableSpan<int>>()); // OK. But the code is too long. So it was decided to disable it.
                //Output.WriteLine("TSpan use itemFormater: {0}", span.ItemsToString(ItemFormaters.Hex)); // CS0411 The type arguments for method 'SizableSpanExtensions.ItemsToString<T, TSpan>(TSpan, Func<nuint, T, string>?, bool)' cannot be inferred from the usage. Try specifying the type arguments explicitly.
                // OK.
                Output.WriteLine("TSpan with typeSample: {0}", span.ItemsToString(in span.GetPinnableReadOnlyReference())); // Can work with GetInnableReadOnlyReference without use typeSample parameter.
                Output.WriteLine("TSpan with typeSample and stringFlags: {0}", span.ItemsToString(in typeSample, null, stringFlags));
                Output.WriteLine("TSpan with headerLength: {0}", span.ItemsToString(typeSample, headerLength)); // It can omit the in keyword.
                Output.WriteLine("TSpan with footerLength: {0}", span.ItemsToString(typeSample, headerLength, footerLength));
                Output.WriteLine("TSpan with footerLength and stringFlags: {0}", span.ItemsToString(typeSample, headerLength, footerLength, null, stringFlags));
            }
#endif // ALLOWS_REF_STRUCT
        }

    }
}
