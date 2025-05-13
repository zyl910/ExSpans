#if NETCOREAPP3_0_OR_GREATER

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace Zyl.ExSpans.Text {
#if TODO
    /// <summary>
    /// Enumerates the lines of a <see cref="ReadOnlyExSpan{Char}"/>.
    /// </summary>
    /// <remarks>
    /// To get an instance of this type, use <see cref="ExMemoryExtensions.EnumerateLines(ReadOnlyExSpan{char})"/>.
    /// </remarks>
    public ref struct ExSpanLineEnumerator {
        private ReadOnlyExSpan<char> _remaining;
        private ReadOnlyExSpan<char> _current;
        private bool _isEnumeratorActive;

        internal ExSpanLineEnumerator(ReadOnlyExSpan<char> buffer) {
            _remaining = buffer;
            _current = default;
            _isEnumeratorActive = true;
        }

        /// <summary>
        /// Gets the line at the current position of the enumerator.
        /// </summary>
        public ReadOnlyExSpan<char> Current => _current;

        /// <summary>
        /// Returns this instance as an enumerator.
        /// </summary>
        public ExSpanLineEnumerator GetEnumerator() => this;

        /// <summary>
        /// Advances the enumerator to the next line of the ExSpan.
        /// </summary>
        /// <returns>
        /// True if the enumerator successfully advanced to the next line; false if
        /// the enumerator has advanced past the end of the ExSpan.
        /// </returns>
        public bool MoveNext() {
            if (!_isEnumeratorActive) {
                return false; // EOF previously reached or enumerator was never initialized
            }

            ReadOnlyExSpan<char> remaining = _remaining;

            int idx = remaining.IndexOfAny(string.SearchValuesStorage.NewLineChars);

            if ((uint)idx < (uint)remaining.Length) {
                int stride = 1;

                if (remaining[idx] == '\r' && (uint)(idx + 1) < (uint)remaining.Length && remaining[idx + 1] == '\n') {
                    stride = 2;
                }

                _current = remaining.Slice(0, idx);
                _remaining = remaining.Slice(idx + stride);
            } else {
                // We've reached EOF, but we still need to return 'true' for this final
                // iteration so that the caller can query the Current property once more.

                _current = remaining;
                _remaining = default;
                _isEnumeratorActive = false;
            }

            return true;
        }
    }
#endif // TODO
}
#endif // NETCOREAPP3_0_OR_GREATER
