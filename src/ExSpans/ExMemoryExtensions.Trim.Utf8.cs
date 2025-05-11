using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Zyl.ExSpans {
    partial class ExMemoryExtensions {

#if NETCOREAPP3_0_OR_GREATER
        internal static ReadOnlyExSpan<byte> TrimUtf8(this ReadOnlyExSpan<byte> span) {
            // Assume that in most cases input doesn't need trimming
            //
            // Since `DecodeFromUtf8` and `DecodeLastFromUtf8` return `Rune.ReplacementChar`
            // on failure and that is not whitespace, we can safely treat it as no trimming
            // and leave failure handling up to the caller instead

            Debug.Assert(!Rune.IsWhiteSpace(Rune.ReplacementChar));

            if (span.Length == 0) {
                return span;
            }

            _ = Rune.DecodeFromUtf8(span.AsReadOnlySpan(), out Rune first, out int firstBytesConsumed);

            if (Rune.IsWhiteSpace(first)) {
                span = span.Slice(firstBytesConsumed);
                return TrimFallback(span);
            }

            _ = Rune.DecodeLastFromUtf8(span.LastAsReadOnlySpan(), out Rune last, out int lastBytesConsumed);

            if (Rune.IsWhiteSpace(last)) {
                span = span.LastSlice(lastBytesConsumed);
                return TrimFallback(span);
            }

            return span;

            [MethodImpl(MethodImplOptions.NoInlining)]
            static ReadOnlyExSpan<byte> TrimFallback(ReadOnlyExSpan<byte> span) {
                while (span.Length != 0) {
                    _ = Rune.DecodeFromUtf8(span.AsReadOnlySpan(), out Rune current, out int bytesConsumed);

                    if (!Rune.IsWhiteSpace(current)) {
                        break;
                    }

                    span = span.Slice(bytesConsumed);
                }

                while (span.Length != 0) {
                    _ = Rune.DecodeLastFromUtf8(span.LastAsReadOnlySpan(), out Rune current, out int bytesConsumed);

                    if (!Rune.IsWhiteSpace(current)) {
                        break;
                    }

                    span = span.LastSlice(bytesConsumed);
                }

                return span;
            }
        }
#endif // NETCOREAPP3_0_OR_GREATER

    }
}
