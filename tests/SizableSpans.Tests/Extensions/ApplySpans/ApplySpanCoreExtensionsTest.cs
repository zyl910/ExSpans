using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Zyl.SizableSpans.Extensions.ApplySpans;
using Zyl.SizableSpans.Reflection;

namespace Zyl.SizableSpans.Tests.Extensions.ApplySpans {
    public class ApplySpanCoreExtensionsTest {
        const int bufferSize = 16;

        private readonly ITestOutputHelper _output;

        public ApplySpanCoreExtensionsTest(ITestOutputHelper output) {
            _output = output;
        }

        private ITestOutputHelper Output => _output;

        [Fact]
        public void ItemsAppendStringToTest() {
            const ItemsToStringFlags stringFlagsDefault = ItemsToStringFlags.Default;
            const ItemsToStringFlags stringFlags = ItemsToStringFlags.HideType;
            const TypeNameFlags nameFlags = TypeNameFlags.ShowNamespace | TypeNameFlags.SubShowNamespace;
            const nuint headerLength = (nuint)3;
            const nuint footerLength = (nuint)4;
            string newLine = Environment.NewLine;
            StringBuilder stringBuilder = new StringBuilder();
            Action<string> output = delegate (string str) {
                stringBuilder.Append(str);
            };
            Span<int> span = stackalloc int[bufferSize];
            span.Fill(1);
            span[0] = 0;
            span[span.Length - 2] = 2;

            // Output - Span.
            CallSpan(output, span);

            // Output - Empty.
            CallSpan(output, Span<long>.Empty);

            // done.
            Output.WriteLine(stringBuilder.ToString());

            void CallSpan<T>(Action<string> output, Span<T> span) {
                // Output - Span.
                output("Span: "); span.ItemsAppendStringTo(output); output(newLine);
                output("Span with stringFlags: "); span.ItemsAppendStringTo(output, null, stringFlags); output(newLine);
                output("Span with footerLength: "); span.ItemsAppendStringTo(output, headerLength, footerLength); output(newLine);
                output("Span with footerLength and stringFlags: "); span.ItemsAppendStringTo(output, headerLength, footerLength, null, stringFlags); output(newLine);

                // Output - ReadOnlySpan.
                ReadOnlySpan<T> spanReadOnly = span;
                output("ReadOnlySpan: "); spanReadOnly.ItemsAppendStringTo(output); output(newLine);
                output("ReadOnlySpan with stringFlags: "); spanReadOnly.ItemsAppendStringTo(output, null, stringFlags); output(newLine);
                output("ReadOnlySpan with nameFlags: "); spanReadOnly.ItemsAppendStringTo(output, null, stringFlagsDefault, nameFlags); output(newLine);
                output("ReadOnlySpan with footerLength: "); spanReadOnly.ItemsAppendStringTo(output, headerLength, footerLength); output(newLine);
                output("ReadOnlySpan with footerLength and stringFlags: "); spanReadOnly.ItemsAppendStringTo(output, headerLength, footerLength, null, stringFlags); output(newLine);
            }
        }

        [Fact]
        public void ItemsAppendStringTest() {
            const ItemsToStringFlags stringFlagsDefault = ItemsToStringFlags.Default;
            const ItemsToStringFlags stringFlags = ItemsToStringFlags.HideType;
            const TypeNameFlags nameFlags = TypeNameFlags.ShowNamespace | TypeNameFlags.SubShowNamespace;
            const nuint headerLength = (nuint)3;
            const nuint footerLength = (nuint)4;
            StringBuilder output = new StringBuilder();
            Span<int> span = stackalloc int[bufferSize];
            span.Fill(1);
            span[0] = 0;
            span[span.Length - 2] = 2;

            // Output - Span.
            CallSpan(output, span);

            // Output - Empty.
            CallSpan(output, Span<long>.Empty);

            // done.
            Output.WriteLine(output.ToString());

            static void CallSpan<T>(StringBuilder output, Span<T> span) {
                // Output - Span.
                output.Append("Span: "); span.ItemsAppendString(output); output.AppendLine();
                output.Append("Span with stringFlags: "); span.ItemsAppendString(output, null, stringFlags); output.AppendLine();
                output.Append("Span with footerLength: "); span.ItemsAppendString(output, headerLength, footerLength); output.AppendLine();
                output.Append("Span with footerLength and stringFlags: "); span.ItemsAppendString(output, headerLength, footerLength, null, stringFlags); output.AppendLine();

                // Output - ReadOnlySpan.
                ReadOnlySpan<T> spanReadOnly = span;
                output.Append("ReadOnlySpan: "); spanReadOnly.ItemsAppendString(output); output.AppendLine();
                output.Append("ReadOnlySpan with stringFlags: "); spanReadOnly.ItemsAppendString(output, null, stringFlags); output.AppendLine();
                output.Append("ReadOnlySpan with nameFlags: "); spanReadOnly.ItemsAppendString(output, null, stringFlagsDefault, nameFlags); output.AppendLine();
                output.Append("ReadOnlySpan with footerLength: "); spanReadOnly.ItemsAppendString(output, headerLength, footerLength); output.AppendLine();
                output.Append("ReadOnlySpan with footerLength and stringFlags: "); spanReadOnly.ItemsAppendString(output, headerLength, footerLength, null, stringFlags); output.AppendLine();
            }
        }

        [Fact]
        public void ItemsToStringTest() {
            const ItemsToStringFlags stringFlagsDefault = ItemsToStringFlags.Default;
            const ItemsToStringFlags stringFlags = ItemsToStringFlags.HideType;
            const TypeNameFlags nameFlags = TypeNameFlags.ShowNamespace | TypeNameFlags.SubShowNamespace;
            const nuint headerLength = (nuint)3;
            const nuint footerLength = (nuint)4;
            Span<int> span = stackalloc int[bufferSize];
            span.Fill(1);
            span[0] = 0;
            span[span.Length - 2] = 2;

            // Output - Span.
            CallSpan(span);

            // Output - Empty.
            CallSpan(Span<long>.Empty);

            void CallSpan<T>(Span<T> span) {
                // Output - Span.
                Output.WriteLine("Span: {0}", span.ItemsToString());
                Output.WriteLine("Span with stringFlags: {0}", span.ItemsToString(null, stringFlags));
                Output.WriteLine("Span with footerLength: {0}", span.ItemsToString(headerLength, footerLength));
                Output.WriteLine("Span with footerLength and stringFlags: {0}", span.ItemsToString(headerLength, footerLength, null, stringFlags));

                // Output - ReadOnlySpan.
                ReadOnlySpan<T> spanReadOnly = span;
                Output.WriteLine("ReadOnlySpan: {0}", spanReadOnly.ItemsToString());
                Output.WriteLine("ReadOnlySpan with stringFlags: {0}", spanReadOnly.ItemsToString(null, stringFlags));
                Output.WriteLine("ReadOnlySpan with nameFlags: {0}", spanReadOnly.ItemsToString(null, stringFlagsDefault, nameFlags));
                Output.WriteLine("ReadOnlySpan with footerLength: {0}", spanReadOnly.ItemsToString(headerLength, footerLength));
                Output.WriteLine("ReadOnlySpan with footerLength and stringFlags: {0}", spanReadOnly.ItemsToString(headerLength, footerLength, null, stringFlags));
            }
        }

    }
}
