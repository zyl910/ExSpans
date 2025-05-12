#if NETCOREAPP3_0_OR_GREATER

using System;
using System.Collections.Generic;
using System.Text;

namespace Zyl.ExSpans.Text {
#if TODO
    /// <summary>
    /// Provides an enumerator for the Rune values represented by a span containing UTF-16 text (为由包含 UTF-16 文本的跨度表示的 Rune 值提供枚举器).
    /// </summary>
    public ref struct ExSpanRuneEnumerator {
        private ReadOnlyExSpan<char> _remaining;
        private Rune _current;

        internal ExSpanRuneEnumerator(ReadOnlyExSpan<char> buffer) {
            _remaining = buffer;
            _current = default;
        }

        public Rune Current => _current;

        public ExSpanRuneEnumerator GetEnumerator() => this;

        public bool MoveNext() {
            if (_remaining.IsEmpty) {
                // reached the end of the buffer
                _current = default;
                return false;
            }

            int scalarValue = Rune.ReadFirstRuneFromUtf16Buffer(_remaining);
            if (scalarValue < 0) {
                // replace invalid sequences with U+FFFD
                scalarValue = Rune.ReplacementChar.Value;
            }

            // In UTF-16 specifically, invalid sequences always have length 1, which is the same
            // length as the replacement character U+FFFD. This means that we can always bump the
            // next index by the current scalar's UTF-16 sequence length. This optimization is not
            // generally applicable; for example, enumerating scalars from UTF-8 cannot utilize
            // this same trick.

            _current = Rune.UnsafeCreate((uint)scalarValue);
            _remaining = _remaining.Slice(_current.Utf16SequenceLength);
            return true;
        }
    }
#endif // TODO
}
#endif // NETCOREAPP3_0_OR_GREATER
