using System;
using System.Collections.Generic;
using System.Text;

namespace Zyl.ExSpans {

#if NET8_0_OR_GREATER && TODO // [TODO why] SearchValues.IndexOfAny is internal
    /// <summary>
    /// Enables enumerating each split within a <see cref="ReadOnlyExSpan{T}"/> that has been divided using one or more separators.
    /// </summary>
    /// <typeparam name="T">The type of items in the <see cref="ExSpanSplitEnumerator{T}"/>.</typeparam>
    public ref struct ExSpanSplitEnumerator<T> where T : IEquatable<T> {
            /// <summary>The input ExSpan being split.</summary>
            private readonly ReadOnlyExSpan<T> _source;

            /// <summary>A single separator to use when <see cref="_splitMode"/> is <see cref="ExSpanSplitEnumeratorMode.SingleElement"/>.</summary>
            private readonly T _separator = default!;
            /// <summary>
            /// A separator ExSpan to use when <see cref="_splitMode"/> is <see cref="ExSpanSplitEnumeratorMode.Sequence"/> (in which case
            /// it's treated as a single separator) or <see cref="ExSpanSplitEnumeratorMode.Any"/> (in which case it's treated as a set of separators).
            /// </summary>
            private readonly ReadOnlyExSpan<T> _separatorBuffer;
            /// <summary>A set of separators to use when <see cref="_splitMode"/> is <see cref="ExSpanSplitEnumeratorMode.SearchValues"/>.</summary>
            private readonly SearchValues<T> _searchValues = default!;

            /// <summary>Mode that dictates how the instance was configured and how its fields should be used in <see cref="MoveNext"/>.</summary>
            private ExSpanSplitEnumeratorMode _splitMode;
            /// <summary>The inclusive starting index in <see cref="_source"/> of the current range.</summary>
            private int _startCurrent = 0;
            /// <summary>The exclusive ending index in <see cref="_source"/> of the current range.</summary>
            private int _endCurrent = 0;
            /// <summary>The index in <see cref="_source"/> from which the next separator search should start.</summary>
            private int _startNext = 0;

            /// <summary>Gets an enumerator that allows for iteration over the split span.</summary>
            /// <returns>Returns a <see cref="ExSpanSplitEnumerator{T}"/> that can be used to iterate over the split span.</returns>
            public ExSpanSplitEnumerator<T> GetEnumerator() => this;

            /// <summary>Gets the source span being enumerated.</summary>
            /// <returns>Returns the <see cref="ReadOnlyExSpan{T}"/> that was provided when creating this enumerator.</returns>
            public readonly ReadOnlyExSpan<T> Source => _source;

            /// <summary>Gets the current element of the enumeration.</summary>
            /// <returns>Returns a <see cref="Range"/> instance that indicates the bounds of the current element withing the source span.</returns>
            public Range Current => new Range(_startCurrent, _endCurrent);

            /// <summary>Initializes the enumerator for <see cref="ExSpanSplitEnumeratorMode.SearchValues"/>.</summary>
            internal ExSpanSplitEnumerator(ReadOnlyExSpan<T> source, SearchValues<T> searchValues) {
                _source = source;
                _splitMode = ExSpanSplitEnumeratorMode.SearchValues;
                _searchValues = searchValues;
            }

            /// <summary>Initializes the enumerator for <see cref="ExSpanSplitEnumeratorMode.Any"/>.</summary>
            /// <remarks>
            /// If <paramref name="separators"/> is empty and <typeparamref name="T"/> is <see cref="char"/>, as an optimization
            /// it will instead use <see cref="ExSpanSplitEnumeratorMode.SearchValues"/> with a cached <see cref="SearchValues{Char}"/>
            /// for all whitespace characters.
            /// </remarks>
            internal ExSpanSplitEnumerator(ReadOnlyExSpan<T> source, ReadOnlyExSpan<T> separators) {
                _source = source;
                if (typeof(T) == typeof(char) && separators.Length == 0) {
                    _searchValues = Unsafe.As<SearchValues<T>>(string.SearchValuesStorage.WhiteSpaceChars);
                    _splitMode = ExSpanSplitEnumeratorMode.SearchValues;
                } else {
                    _separatorBuffer = separators;
                    _splitMode = ExSpanSplitEnumeratorMode.Any;
                }
            }

            /// <summary>Initializes the enumerator for <see cref="ExSpanSplitEnumeratorMode.Sequence"/> (or <see cref="ExSpanSplitEnumeratorMode.EmptySequence"/> if the separator is empty).</summary>
            /// <remarks><paramref name="treatAsSingleSeparator"/> must be true.</remarks>
            internal ExSpanSplitEnumerator(ReadOnlyExSpan<T> source, ReadOnlyExSpan<T> separator, bool treatAsSingleSeparator) {
                Debug.Assert(treatAsSingleSeparator, "Should only ever be called as true; exists to differentiate from separators overload");

                _source = source;
                _separatorBuffer = separator;
                _splitMode = separator.Length == 0 ?
                    ExSpanSplitEnumeratorMode.EmptySequence :
                    ExSpanSplitEnumeratorMode.Sequence;
            }

            /// <summary>Initializes the enumerator for <see cref="ExSpanSplitEnumeratorMode.SingleElement"/>.</summary>
            internal ExSpanSplitEnumerator(ReadOnlyExSpan<T> source, T separator) {
                _source = source;
                _separator = separator;
                _splitMode = ExSpanSplitEnumeratorMode.SingleElement;
            }

            /// <summary>
            /// Advances the enumerator to the next element of the enumeration.
            /// </summary>
            /// <returns><see langword="true"/> if the enumerator was successfully advanced to the next element; <see langword="false"/> if the enumerator has passed the end of the enumeration.</returns>
            public bool MoveNext() {
                // Search for the next separator index.
                int separatorIndex, separatorLength;
                switch (_splitMode) {
                    case ExSpanSplitEnumeratorMode.None:
                        return false;

                    case ExSpanSplitEnumeratorMode.SingleElement:
                        separatorIndex = _source.Slice(_startNext).IndexOf(_separator);
                        separatorLength = 1;
                        break;

                    case ExSpanSplitEnumeratorMode.Any:
                        separatorIndex = _source.Slice(_startNext).IndexOfAny(_separatorBuffer);
                        separatorLength = 1;
                        break;

                    case ExSpanSplitEnumeratorMode.Sequence:
                        separatorIndex = _source.Slice(_startNext).IndexOf(_separatorBuffer);
                        separatorLength = _separatorBuffer.Length;
                        break;

                    case ExSpanSplitEnumeratorMode.EmptySequence:
                        separatorIndex = -1;
                        separatorLength = 1;
                        break;

                    default:
                        Debug.Assert(_splitMode == ExSpanSplitEnumeratorMode.SearchValues, $"Unknown split mode: {_splitMode}");
                        separatorIndex = _source.Slice(_startNext).IndexOfAny(_searchValues);
                        separatorLength = 1;
                        break;
                }

                _startCurrent = _startNext;
                if (separatorIndex >= 0) {
                    _endCurrent = _startCurrent + separatorIndex;
                    _startNext = _endCurrent + separatorLength;
                } else {
                    _startNext = _endCurrent = _source.Length;

                    // Set _splitMode to None so that subsequent MoveNext calls will return false.
                    _splitMode = ExSpanSplitEnumeratorMode.None;
                }

                return true;
            }
        }

        /// <summary>Indicates in which mode <see cref="ExSpanSplitEnumerator{T}"/> is operating, with regards to how it should interpret its state.</summary>
        internal enum ExSpanSplitEnumeratorMode {
            /// <summary>Either a default <see cref="ExSpanSplitEnumerator{T}"/> was used, or the enumerator has finished enumerating and there's no more work to do.</summary>
            None = 0,

            /// <summary>A single T separator was provided.</summary>
            SingleElement,

            /// <summary>A ExSpan of separators was provided, each of which should be treated independently.</summary>
            Any,

            /// <summary>The separator is a ExSpan of elements to be treated as a single sequence.</summary>
            Sequence,

            /// <summary>The separator is an empty sequence, such that no splits should be performed.</summary>
            EmptySequence,

            /// <summary>
            /// A <see cref="SearchValues{Char}"/> was provided and should behave the same as with <see cref="Any"/> but with the separators in the <see cref="SearchValues"/>
            /// instance instead of in a <see cref="ReadOnlyExSpan{Char}"/>.
            /// </summary>
            SearchValues
        }
#endif // NET8_0_OR_GREATER

}
