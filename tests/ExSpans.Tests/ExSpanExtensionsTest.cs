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
using Zyl.ExSpans.Reflection;

namespace Zyl.ExSpans.Tests {
    public class ExSpanExtensionsTest {
        const int bufferSize = 16;

        private readonly ITestOutputHelper _output;

        public ExSpanExtensionsTest(ITestOutputHelper output) {
            _output = output;
        }

        private ITestOutputHelper Output => _output;

        [Fact]
        public void ItemsAppendStringToTest() {
            const ItemsToStringFlags stringFlagsDefault = ItemsToStringFlags.Default;
            const ItemsToStringFlags stringFlags = ItemsToStringFlags.HideType;
            const TypeNameFlags nameFlags = TypeNameFlags.ShowNamespace | TypeNameFlags.SubShowNamespace;
            TSize headerLength = (TSize)3;
            TSize footerLength = (TSize)4;
            string newLine = Environment.NewLine;
            StringBuilder stringBuilder = new StringBuilder();
            Action<string> output = delegate (string str) {
                stringBuilder.Append(str);
            };
            Span<int> sourceSpan = stackalloc int[bufferSize];
            ExSpan<int> span = sourceSpan; // Implicit conversion Span to ExSpan.
            span.Fill(1);
            span[(TSize)0] = 0;
            span[span.Length - 2] = 2;

            // Output - Span.
            CallExSpan(output, span);

            // Output - Empty.
            CallExSpan(output, ExSpan<long>.Empty);

            // Output - TSpan.
#if ALLOWS_REF_STRUCT
            CallTSpan(output, span, span.GetPinnableReference());
            output("TSpan-ExSpan with typeSample: "); span.ItemsAppendStringTo(span.GetPinnableReference(), output); output(newLine);
            output("TSpan-ExSpan with typeSample and stringFlags: "); span.ItemsAppendStringTo(span.GetPinnableReference(), output, null, stringFlags); output(newLine);
            output("TSpan-ExSpan with typeSample and nameFlags: "); span.ItemsAppendStringTo(span.GetPinnableReference(), output, null, stringFlagsDefault, nameFlags); output(newLine);
            output("TSpan-ExSpan with footerLength: "); span.ItemsAppendStringTo(span.GetPinnableReference(), output, headerLength, footerLength); output(newLine);
            output("TSpan-ExSpan with footerLength and stringFlags-HideType: "); span.ItemsAppendStringTo(span.GetPinnableReference(), output, headerLength, footerLength, null, stringFlags); output(newLine);
            output("TSpan-ExSpan with footerLength and stringFlags-HideLength: "); span.ItemsAppendStringTo(span.GetPinnableReference(), output, headerLength, footerLength, null, ItemsToStringFlags.HideLength); output(newLine);
            output("TSpan-ExSpan with footerLength and stringFlags-HideBrace: "); span.ItemsAppendStringTo(span.GetPinnableReference(), output, headerLength, footerLength, null, ItemsToStringFlags.HideBrace); output(newLine);
            output("TSpan-ExSpan with footerLength and stringFlags-All: "); span.ItemsAppendStringTo(span.GetPinnableReference(), output, headerLength, footerLength, null, ItemsToStringFlagsUtil.All); output(newLine);
            output("TSpan-ExSpan with footerLength and itemFormater: "); span.ItemsAppendStringTo(span.GetPinnableReference(), output, headerLength, footerLength, ItemFormaters.Hex); output(newLine);
            output("TSpan-ExSpan with headerLength and footerLength less: "); span.ItemsAppendStringTo(span.GetPinnableReference(), output, 5, 10); output(newLine);
            output("TSpan-ExSpan with headerLength and footerLength equal: "); span.ItemsAppendStringTo(span.GetPinnableReference(), output, 5, 11); output(newLine);
            output("TSpan-ExSpan with headerLength and footerLength greater: "); span.ItemsAppendStringTo(span.GetPinnableReference(), output, 5, 12); output(newLine);
#endif // ALLOWS_REF_STRUCT

            // done.
            Output.WriteLine(stringBuilder.ToString());

            void CallExSpan<T>(Action<string> output, ExSpan<T> span) {
                // Output - ExSpan.
                output("ExSpan: "); span.ItemsAppendStringTo(output); output(newLine);
                output("ExSpan with stringFlags: "); span.ItemsAppendStringTo(output, null, stringFlags); output(newLine);
                output("ExSpan with footerLength: "); span.ItemsAppendStringTo(output, headerLength, footerLength); output(newLine);
                output("ExSpan with footerLength and stringFlags: "); span.ItemsAppendStringTo(output, headerLength, footerLength, null, stringFlags); output(newLine);

                // Output - ReadOnlyExSpan.
                ReadOnlyExSpan<T> spanReadOnly = span;
                output("ReadOnlyExSpan: "); spanReadOnly.ItemsAppendStringTo(output); output(newLine);
                output("ReadOnlyExSpan with stringFlags: "); spanReadOnly.ItemsAppendStringTo(output, null, stringFlags); output(newLine);
                output("ReadOnlyExSpan with nameFlags: "); spanReadOnly.ItemsAppendStringTo(output, null, stringFlagsDefault, nameFlags); output(newLine);
                output("ReadOnlyExSpan with footerLength: "); spanReadOnly.ItemsAppendStringTo(output, headerLength, footerLength); output(newLine);
                output("ReadOnlyExSpan with footerLength and stringFlags: "); spanReadOnly.ItemsAppendStringTo(output, headerLength, footerLength, null, stringFlags); output(newLine);
            }

#if ALLOWS_REF_STRUCT
            void CallTSpan<T, TSpan>(Action<string> output, TSpan span, in T typeSample)
                    where TSpan : IReadOnlyExSpanBase<T>, allows ref struct {
                output("TSpan with typeSample: "); span.ItemsAppendStringTo(in span.GetPinnableReadOnlyReference(), output); output(newLine); // Can work with GetPinnableReadOnlyReference without use typeSample parameter.
                output("TSpan with typeSample and stringFlags: "); span.ItemsAppendStringTo(in typeSample, output, null, stringFlags); output(newLine);
                output("TSpan with headerLength: "); span.ItemsAppendStringTo(typeSample, output, headerLength); output(newLine); // It can omit the in keyword.
                output("TSpan with footerLength: "); span.ItemsAppendStringTo(typeSample, output, headerLength, footerLength); output(newLine);
                output("TSpan with footerLength and stringFlags: "); span.ItemsAppendStringTo(typeSample, output, headerLength, footerLength, null, stringFlags); output(newLine);
            }
#endif // ALLOWS_REF_STRUCT
        }

        [Fact]
        public void ItemsAppendStringTest() {
            const ItemsToStringFlags stringFlagsDefault = ItemsToStringFlags.Default;
            const ItemsToStringFlags stringFlags = ItemsToStringFlags.HideType;
            const TypeNameFlags nameFlags = TypeNameFlags.ShowNamespace | TypeNameFlags.SubShowNamespace;
            TSize headerLength = (TSize)3;
            TSize footerLength = (TSize)4;
            StringBuilder output = new StringBuilder();
            Span<int> sourceSpan = stackalloc int[bufferSize];
            ExSpan<int> span = sourceSpan; // Implicit conversion Span to ExSpan.
            span.Fill(1);
            span[(TSize)0] = 0;
            span[span.Length - 2] = 2;

            // Output - Span.
            CallExSpan(output, span);

            // Output - Empty.
            CallExSpan(output, ExSpan<long>.Empty);

            // Output - TSpan.
#if ALLOWS_REF_STRUCT
            CallTSpan(output, span, span.GetPinnableReference());
            output.Append("TSpan-ExSpan with typeSample: "); span.ItemsAppendString(span.GetPinnableReference(), output); output.AppendLine();
            output.Append("TSpan-ExSpan with typeSample and stringFlags: "); span.ItemsAppendString(span.GetPinnableReference(), output, null, stringFlags); output.AppendLine();
            output.Append("TSpan-ExSpan with typeSample and nameFlags: "); span.ItemsAppendString(span.GetPinnableReference(), output, null, stringFlagsDefault, nameFlags); output.AppendLine();
            output.Append("TSpan-ExSpan with footerLength: "); span.ItemsAppendString(span.GetPinnableReference(), output, headerLength, footerLength); output.AppendLine();
            output.Append("TSpan-ExSpan with footerLength and stringFlags-HideType: "); span.ItemsAppendString(span.GetPinnableReference(), output, headerLength, footerLength, null, stringFlags); output.AppendLine();
            output.Append("TSpan-ExSpan with footerLength and stringFlags-HideLength: "); span.ItemsAppendString(span.GetPinnableReference(), output, headerLength, footerLength, null, ItemsToStringFlags.HideLength); output.AppendLine();
            output.Append("TSpan-ExSpan with footerLength and stringFlags-HideBrace: "); span.ItemsAppendString(span.GetPinnableReference(), output, headerLength, footerLength, null, ItemsToStringFlags.HideBrace); output.AppendLine();
            output.Append("TSpan-ExSpan with footerLength and stringFlags-All: "); span.ItemsAppendString(span.GetPinnableReference(), output, headerLength, footerLength, null, ItemsToStringFlagsUtil.All); output.AppendLine();
            output.Append("TSpan-ExSpan with footerLength and itemFormater: "); span.ItemsAppendString(span.GetPinnableReference(), output, headerLength, footerLength, ItemFormaters.Hex); output.AppendLine();
            output.Append("TSpan-ExSpan with headerLength and footerLength less: "); span.ItemsAppendString(span.GetPinnableReference(), output, 5, 10); output.AppendLine();
            output.Append("TSpan-ExSpan with headerLength and footerLength equal: "); span.ItemsAppendString(span.GetPinnableReference(), output, 5, 11); output.AppendLine();
            output.Append("TSpan-ExSpan with headerLength and footerLength greater: "); span.ItemsAppendString(span.GetPinnableReference(), output, 5, 12); output.AppendLine();
#endif // ALLOWS_REF_STRUCT

            // done.
            Output.WriteLine(output.ToString());

            void CallExSpan<T>(StringBuilder output, ExSpan<T> span) {
                // Output - ExSpan.
                output.Append("ExSpan: "); span.ItemsAppendString(output); output.AppendLine();
                output.Append("ExSpan with stringFlags: "); span.ItemsAppendString(output, null, stringFlags); output.AppendLine();
                output.Append("ExSpan with footerLength: "); span.ItemsAppendString(output, headerLength, footerLength); output.AppendLine();
                output.Append("ExSpan with footerLength and stringFlags: "); span.ItemsAppendString(output, headerLength, footerLength, null, stringFlags); output.AppendLine();

                // Output - ReadOnlyExSpan.
                ReadOnlyExSpan<T> spanReadOnly = span;
                output.Append("ReadOnlyExSpan: "); spanReadOnly.ItemsAppendString(output); output.AppendLine();
                output.Append("ReadOnlyExSpan with stringFlags: "); spanReadOnly.ItemsAppendString(output, null, stringFlags); output.AppendLine();
                output.Append("ReadOnlyExSpan with nameFlags: "); spanReadOnly.ItemsAppendString(output, null, stringFlagsDefault, nameFlags); output.AppendLine();
                output.Append("ReadOnlyExSpan with footerLength: "); spanReadOnly.ItemsAppendString(output, headerLength, footerLength); output.AppendLine();
                output.Append("ReadOnlyExSpan with footerLength and stringFlags: "); spanReadOnly.ItemsAppendString(output, headerLength, footerLength, null, stringFlags); output.AppendLine();
            }

#if ALLOWS_REF_STRUCT
            void CallTSpan<T, TSpan>(StringBuilder output, TSpan span, in T typeSample)
                    where TSpan : IReadOnlyExSpanBase<T>, allows ref struct {
                output.Append("TSpan with typeSample: "); span.ItemsAppendString(in span.GetPinnableReadOnlyReference(), output); output.AppendLine(); // Can work with GetPinnableReadOnlyReference without use typeSample parameter.
                output.Append("TSpan with typeSample and stringFlags: "); span.ItemsAppendString(in typeSample, output, null, stringFlags); output.AppendLine();
                output.Append("TSpan with headerLength: "); span.ItemsAppendString(typeSample, output, headerLength); output.AppendLine(); // It can omit the in keyword.
                output.Append("TSpan with footerLength: "); span.ItemsAppendString(typeSample, output, headerLength, footerLength); output.AppendLine();
                output.Append("TSpan with footerLength and stringFlags: "); span.ItemsAppendString(typeSample, output, headerLength, footerLength, null, stringFlags); output.AppendLine();
            }
#endif // ALLOWS_REF_STRUCT
        }

        [Fact]
        public void ItemsToStringTest() {
            const ItemsToStringFlags stringFlagsDefault = ItemsToStringFlags.Default;
            const ItemsToStringFlags stringFlags = ItemsToStringFlags.HideType;
            const TypeNameFlags nameFlags = TypeNameFlags.ShowNamespace | TypeNameFlags.SubShowNamespace;
            TSize headerLength = (TSize)3;
            TSize footerLength = (TSize)4;
            Span<int> sourceSpan = stackalloc int[bufferSize];
            ExSpan<int> span = sourceSpan; // Implicit conversion Span to ExSpan.
            span.Fill(1);
            span[(TSize)0] = 0;
            span[span.Length - 2] = 2;

            // Output - Span.
            CallExSpan(span);

            // Output - Empty.
            CallExSpan(ExSpan<long>.Empty);

            // Output - TSpan.
#if ALLOWS_REF_STRUCT
            CallTSpan(span, span.GetPinnableReference());
            Output.WriteLine("TSpan-ExSpan with typeSample: {0}", span.ItemsToString(span.GetPinnableReference()));
            Output.WriteLine("TSpan-ExSpan with typeSample and stringFlags: {0}", span.ItemsToString(span.GetPinnableReference(), null, stringFlags));
            Output.WriteLine("TSpan-ExSpan with typeSample and nameFlags: {0}", span.ItemsToString(span.GetPinnableReference(), null, stringFlagsDefault, nameFlags));
            Output.WriteLine("TSpan-ExSpan with footerLength: {0}", span.ItemsToString(span.GetPinnableReference(), headerLength, footerLength));
            Output.WriteLine("TSpan-ExSpan with footerLength and stringFlags-HideType: {0}", span.ItemsToString(span.GetPinnableReference(), headerLength, footerLength, null, stringFlags));
            Output.WriteLine("TSpan-ExSpan with footerLength and stringFlags-HideLength: {0}", span.ItemsToString(span.GetPinnableReference(), headerLength, footerLength, null, ItemsToStringFlags.HideLength));
            Output.WriteLine("TSpan-ExSpan with footerLength and stringFlags-HideBrace: {0}", span.ItemsToString(span.GetPinnableReference(), headerLength, footerLength, null, ItemsToStringFlags.HideBrace));
            Output.WriteLine("TSpan-ExSpan with footerLength and stringFlags-All: {0}", span.ItemsToString(span.GetPinnableReference(), headerLength, footerLength, null, ItemsToStringFlagsUtil.All));
            Output.WriteLine("TSpan-ExSpan with footerLength and itemFormater: {0}", span.ItemsToString(span.GetPinnableReference(), headerLength, footerLength, ItemFormaters.Hex));
            Output.WriteLine("TSpan-ExSpan with headerLength and footerLength less: {0}", span.ItemsToString(span.GetPinnableReference(), 5, 10));
            Output.WriteLine("TSpan-ExSpan with headerLength and footerLength equal: {0}", span.ItemsToString(span.GetPinnableReference(), 5, 11));
            Output.WriteLine("TSpan-ExSpan with headerLength and footerLength greater: {0}", span.ItemsToString(span.GetPinnableReference(), 5, 12));
#endif // ALLOWS_REF_STRUCT

            void CallExSpan<T>(ExSpan<T> span) {
                // Output - ExSpan.
                Output.WriteLine("ExSpan: {0}", span.ItemsToString());
                Output.WriteLine("ExSpan with stringFlags: {0}", span.ItemsToString(null, stringFlags));
                Output.WriteLine("ExSpan with footerLength: {0}", span.ItemsToString(headerLength, footerLength));
                Output.WriteLine("ExSpan with footerLength and stringFlags: {0}", span.ItemsToString(headerLength, footerLength, null, stringFlags));

                // Output - ReadOnlyExSpan.
                ReadOnlyExSpan<T> spanReadOnly = span;
                Output.WriteLine("ReadOnlyExSpan: {0}", spanReadOnly.ItemsToString());
                Output.WriteLine("ReadOnlyExSpan with stringFlags: {0}", spanReadOnly.ItemsToString(null, stringFlags));
                Output.WriteLine("ReadOnlyExSpan with nameFlags: {0}", spanReadOnly.ItemsToString(null, stringFlagsDefault, nameFlags));
                Output.WriteLine("ReadOnlyExSpan with footerLength: {0}", spanReadOnly.ItemsToString(headerLength, footerLength));
                Output.WriteLine("ReadOnlyExSpan with footerLength and stringFlags: {0}", spanReadOnly.ItemsToString(headerLength, footerLength, null, stringFlags));
            }

#if ALLOWS_REF_STRUCT
            void CallTSpan<T, TSpan>(TSpan span, in T typeSample)
                    where TSpan : IReadOnlyExSpanBase<T>, allows ref struct {
                // Try without typeSample.
                //Output.WriteLine("TSpan without typeSample: {0}", span.ItemsToString()); // CS0411 The type arguments for method 'ExSpanExtensions.ItemsToString<T, TSpan>(TSpan, bool)' cannot be inferred from the usage. Try specifying the type arguments explicitly.
                //Output.WriteLine("TSpan without typeSample: {0}", span.ItemsToString<int, ExSpan<int>>()); // OK. But the code is too long. So it was decided to disable it.
                //Output.WriteLine("TSpan use itemFormater: {0}", span.ItemsToString(ItemFormaters.Hex)); // CS0411 The type arguments for method 'ExSpanExtensions.ItemsToString<T, TSpan>(TSpan, Func<TSize, T, string>?, bool)' cannot be inferred from the usage. Try specifying the type arguments explicitly.
                // OK.
                Output.WriteLine("TSpan with typeSample: {0}", span.ItemsToString(in span.GetPinnableReadOnlyReference())); // Can work with GetPinnableReadOnlyReference without use typeSample parameter.
                Output.WriteLine("TSpan with typeSample and stringFlags: {0}", span.ItemsToString(in typeSample, null, stringFlags));
                Output.WriteLine("TSpan with headerLength: {0}", span.ItemsToString(typeSample, headerLength)); // It can omit the in keyword.
                Output.WriteLine("TSpan with footerLength: {0}", span.ItemsToString(typeSample, headerLength, footerLength));
                Output.WriteLine("TSpan with footerLength and stringFlags: {0}", span.ItemsToString(typeSample, headerLength, footerLength, null, stringFlags));
            }
#endif // ALLOWS_REF_STRUCT
        }

    }
}
