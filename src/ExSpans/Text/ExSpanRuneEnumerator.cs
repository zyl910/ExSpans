#if NETCOREAPP3_0_OR_GREATER

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace Zyl.ExSpans.Text {
    /// <summary>
    /// Provides an enumerator for the <see cref="Rune"/> values represented by a span containing UTF-16 text (为由包含 UTF-16 文本的跨度表示的 <see cref="Rune"/> 值提供枚举器).
    /// </summary>
    public ref struct ExSpanRuneEnumerator : IEnumerator<Rune> {
        private ReadOnlyExSpan<char> _remaining;
        private Rune _current;

        internal ExSpanRuneEnumerator(ReadOnlyExSpan<char> buffer) {
            _remaining = buffer;
            _current = default;
        }

        /// <inheritdoc cref="IEnumerator{T}.Current"/>
        public readonly Rune Current {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _current; }
        }

        readonly object? IEnumerator.Current {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _current; }
        }

        /// <inheritdoc cref="IDisposable.Dispose"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Dispose() {
        }

        /// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
        public readonly ExSpanRuneEnumerator GetEnumerator() => this;

        /// <inheritdoc cref="IEnumerator{T}.MoveNext"/>
        public bool MoveNext() {
            if (_remaining.IsEmpty) {
                // reached the end of the buffer
                _current = default;
                return false;
            }

            Rune.DecodeFromUtf16(_remaining.AsReadOnlySpan(), out _current, out int charsConsumed);
            _remaining = _remaining.Slice(charsConsumed);
            return true;
        }

        /// <inheritdoc cref="IEnumerator.Reset"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Reset() {
            throw new NotImplementedException();
        }
    }
}
#endif // NETCOREAPP3_0_OR_GREATER
