using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Zyl.ExSpans.Extensions;

namespace Zyl.ExSpans {
    partial class ExMemoryExtensions {

        /// <summary>
        /// Removes all leading and trailing occurrences of a specified element from the memory.
        /// </summary>
        /// <param name="memory">The source memory from which the element is removed.</param>
        /// <param name="trimElement">The specified element to look for and remove.</param>
        public static ExMemory<T> Trim<T>(this ExMemory<T> memory, T trimElement) where T : IEquatable<T>? {
            ReadOnlyExSpan<T> span = memory.ExSpan;
            TSize start = ClampStart(span, trimElement);
            TSize length = ClampEnd(span, start, trimElement);
            return memory.Slice(start, length);
        }

        /// <summary>
        /// Removes all leading occurrences of a specified element from the memory.
        /// </summary>
        /// <param name="memory">The source memory from which the element is removed.</param>
        /// <param name="trimElement">The specified element to look for and remove.</param>
        public static ExMemory<T> TrimStart<T>(this ExMemory<T> memory, T trimElement) where T : IEquatable<T>?
            => memory.Slice(ClampStart(memory.ExSpan, trimElement));

        /// <summary>
        /// Removes all trailing occurrences of a specified element from the memory.
        /// </summary>
        /// <param name="memory">The source memory from which the element is removed.</param>
        /// <param name="trimElement">The specified element to look for and remove.</param>
        public static ExMemory<T> TrimEnd<T>(this ExMemory<T> memory, T trimElement) where T : IEquatable<T>?
            => memory.Slice(0, ClampEnd(memory.ExSpan, 0, trimElement));

        /// <summary>
        /// Removes all leading and trailing occurrences of a specified element from the memory.
        /// </summary>
        /// <param name="memory">The source memory from which the element is removed.</param>
        /// <param name="trimElement">The specified element to look for and remove.</param>
        public static ReadOnlyExMemory<T> Trim<T>(this ReadOnlyExMemory<T> memory, T trimElement) where T : IEquatable<T>? {
            ReadOnlyExSpan<T> span = memory.ExSpan;
            TSize start = ClampStart(span, trimElement);
            TSize length = ClampEnd(span, start, trimElement);
            return memory.Slice(start, length);
        }

        /// <summary>
        /// Removes all leading occurrences of a specified element from the memory.
        /// </summary>
        /// <param name="memory">The source memory from which the element is removed.</param>
        /// <param name="trimElement">The specified element to look for and remove.</param>
        public static ReadOnlyExMemory<T> TrimStart<T>(this ReadOnlyExMemory<T> memory, T trimElement) where T : IEquatable<T>?
            => memory.Slice(ClampStart(memory.ExSpan, trimElement));

        /// <summary>
        /// Removes all trailing occurrences of a specified element from the memory.
        /// </summary>
        /// <param name="memory">The source memory from which the element is removed.</param>
        /// <param name="trimElement">The specified element to look for and remove.</param>
        public static ReadOnlyExMemory<T> TrimEnd<T>(this ReadOnlyExMemory<T> memory, T trimElement) where T : IEquatable<T>?
            => memory.Slice(0, ClampEnd(memory.ExSpan, 0, trimElement));

        /// <summary>
        /// Removes all leading and trailing occurrences of a specified element from the span.
        /// </summary>
        /// <param name="span">The source span from which the element is removed.</param>
        /// <param name="trimElement">The specified element to look for and remove.</param>
        public static ExSpan<T> Trim<T>(this ExSpan<T> span, T trimElement) where T : IEquatable<T>? {
            TSize start = ClampStart(span, trimElement);
            TSize length = ClampEnd(span, start, trimElement);
            return span.Slice(start, length);
        }

        /// <summary>
        /// Removes all leading occurrences of a specified element from the span.
        /// </summary>
        /// <param name="span">The source span from which the element is removed.</param>
        /// <param name="trimElement">The specified element to look for and remove.</param>
        public static ExSpan<T> TrimStart<T>(this ExSpan<T> span, T trimElement) where T : IEquatable<T>?
            => span.Slice(ClampStart(span, trimElement));

        /// <summary>
        /// Removes all trailing occurrences of a specified element from the span.
        /// </summary>
        /// <param name="span">The source span from which the element is removed.</param>
        /// <param name="trimElement">The specified element to look for and remove.</param>
        public static ExSpan<T> TrimEnd<T>(this ExSpan<T> span, T trimElement) where T : IEquatable<T>?
            => span.Slice((TSize)0, ClampEnd(span, (TSize)0, trimElement));

        /// <summary>
        /// Removes all leading and trailing occurrences of a specified element from the span.
        /// </summary>
        /// <param name="span">The source span from which the element is removed.</param>
        /// <param name="trimElement">The specified element to look for and remove.</param>
        public static ReadOnlyExSpan<T> Trim<T>(this ReadOnlyExSpan<T> span, T trimElement) where T : IEquatable<T>? {
            TSize start = ClampStart(span, trimElement);
            TSize length = ClampEnd(span, start, trimElement);
            return span.Slice(start, length);
        }

        /// <summary>
        /// Removes all leading occurrences of a specified element from the span.
        /// </summary>
        /// <param name="span">The source span from which the element is removed.</param>
        /// <param name="trimElement">The specified element to look for and remove.</param>
        public static ReadOnlyExSpan<T> TrimStart<T>(this ReadOnlyExSpan<T> span, T trimElement) where T : IEquatable<T>?
            => span.Slice(ClampStart(span, trimElement));

        /// <summary>
        /// Removes all trailing occurrences of a specified element from the span.
        /// </summary>
        /// <param name="span">The source span from which the element is removed.</param>
        /// <param name="trimElement">The specified element to look for and remove.</param>
        public static ReadOnlyExSpan<T> TrimEnd<T>(this ReadOnlyExSpan<T> span, T trimElement) where T : IEquatable<T>?
            => span.Slice((TSize)0, ClampEnd(span, (TSize)0, trimElement));

        /// <summary>
        /// Delimits all leading occurrences of a specified element from the span.
        /// </summary>
        /// <param name="span">The source span from which the element is removed.</param>
        /// <param name="trimElement">The specified element to look for and remove.</param>
        private static TSize ClampStart<T>(ReadOnlyExSpan<T> span, T trimElement) where T : IEquatable<T>? {
            TSize start = (TSize)0;

            if (trimElement != null) {
                for (; start < span.Length; start += 1) {
                    if (!trimElement.Equals(span[start])) {
                        break;
                    }
                }
            } else {
                for (; start < span.Length; start += 1) {
                    if (span[start] != null) {
                        break;
                    }
                }
            }

            return start;
        }

        /// <summary>
        /// Delimits all trailing occurrences of a specified element from the span.
        /// </summary>
        /// <param name="span">The source span from which the element is removed.</param>
        /// <param name="start">The start index from which to being searching.</param>
        /// <param name="trimElement">The specified element to look for and remove.</param>
        private static TSize ClampEnd<T>(ReadOnlyExSpan<T> span, TSize start, T trimElement) where T : IEquatable<T>? {
            // Initially, start==len==0. If ClampStart trims all, start==len
            Debug.Assert(start.ToUIntPtr() <= span.Length.ToUIntPtr());

            TSize end = span.Length - 1;

            if (trimElement != null) {
                for (; end >= start; end -= 1) {
                    if (!trimElement.Equals(span[end])) {
                        break;
                    }
                }
            } else {
                for (; end >= start; end -= 1) {
                    if (span[end] != null) {
                        break;
                    }
                }
            }

            return end - start + 1;
        }

        /// <summary>
        /// Removes all leading and trailing occurrences of a set of elements specified
        /// in a readonly span from the memory.
        /// </summary>
        /// <param name="memory">The source memory from which the elements are removed.</param>
        /// <param name="trimElements">The span which contains the set of elements to remove.</param>
        /// <remarks>If <paramref name="trimElements"/> is empty, the memory is returned unaltered.</remarks>
        public static ExMemory<T> Trim<T>(this ExMemory<T> memory, ReadOnlyExSpan<T> trimElements) where T : IEquatable<T>? {
            if (trimElements.Length > 1) {
                ReadOnlyExSpan<T> span = memory.ExSpan;
                TSize start = ClampStart(span, trimElements);
                TSize length = ClampEnd(span, start, trimElements);
                return memory.Slice(start, length);
            }

            if (trimElements.Length == 1) {
                return Trim(memory, trimElements[0]);
            }

            return memory;
        }

        /// <summary>
        /// Removes all leading occurrences of a set of elements specified
        /// in a readonly span from the memory.
        /// </summary>
        /// <param name="memory">The source memory from which the elements are removed.</param>
        /// <param name="trimElements">The span which contains the set of elements to remove.</param>
        /// <remarks>If <paramref name="trimElements"/> is empty, the memory is returned unaltered.</remarks>
        public static ExMemory<T> TrimStart<T>(this ExMemory<T> memory, ReadOnlyExSpan<T> trimElements) where T : IEquatable<T>? {
            if (trimElements.Length > 1) {
                return memory.Slice(ClampStart(memory.ExSpan, trimElements));
            }

            if (trimElements.Length == 1) {
                return TrimStart(memory, trimElements[0]);
            }

            return memory;
        }

        /// <summary>
        /// Removes all trailing occurrences of a set of elements specified
        /// in a readonly span from the memory.
        /// </summary>
        /// <param name="memory">The source memory from which the elements are removed.</param>
        /// <param name="trimElements">The span which contains the set of elements to remove.</param>
        /// <remarks>If <paramref name="trimElements"/> is empty, the memory is returned unaltered.</remarks>
        public static ExMemory<T> TrimEnd<T>(this ExMemory<T> memory, ReadOnlyExSpan<T> trimElements) where T : IEquatable<T>? {
            if (trimElements.Length > 1) {
                return memory.Slice(0, ClampEnd(memory.ExSpan, 0, trimElements));
            }

            if (trimElements.Length == 1) {
                return TrimEnd(memory, trimElements[0]);
            }

            return memory;
        }

        /// <summary>
        /// Removes all leading and trailing occurrences of a set of elements specified
        /// in a readonly span from the memory.
        /// </summary>
        /// <param name="memory">The source memory from which the elements are removed.</param>
        /// <param name="trimElements">The span which contains the set of elements to remove.</param>
        /// <remarks>If <paramref name="trimElements"/> is empty, the memory is returned unaltered.</remarks>
        public static ReadOnlyExMemory<T> Trim<T>(this ReadOnlyExMemory<T> memory, ReadOnlyExSpan<T> trimElements) where T : IEquatable<T>? {
            if (trimElements.Length > 1) {
                ReadOnlyExSpan<T> span = memory.ExSpan;
                TSize start = ClampStart(span, trimElements);
                TSize length = ClampEnd(span, start, trimElements);
                return memory.Slice(start, length);
            }

            if (trimElements.Length == 1) {
                return Trim(memory, trimElements[0]);
            }

            return memory;
        }

        /// <summary>
        /// Removes all leading occurrences of a set of elements specified
        /// in a readonly span from the memory.
        /// </summary>
        /// <param name="memory">The source memory from which the elements are removed.</param>
        /// <param name="trimElements">The span which contains the set of elements to remove.</param>
        /// <remarks>If <paramref name="trimElements"/> is empty, the memory is returned unaltered.</remarks>
        public static ReadOnlyExMemory<T> TrimStart<T>(this ReadOnlyExMemory<T> memory, ReadOnlyExSpan<T> trimElements) where T : IEquatable<T>? {
            if (trimElements.Length > 1) {
                return memory.Slice(ClampStart(memory.ExSpan, trimElements));
            }

            if (trimElements.Length == 1) {
                return TrimStart(memory, trimElements[0]);
            }

            return memory;
        }

        /// <summary>
        /// Removes all trailing occurrences of a set of elements specified
        /// in a readonly span from the memory.
        /// </summary>
        /// <param name="memory">The source memory from which the elements are removed.</param>
        /// <param name="trimElements">The span which contains the set of elements to remove.</param>
        /// <remarks>If <paramref name="trimElements"/> is empty, the memory is returned unaltered.</remarks>
        public static ReadOnlyExMemory<T> TrimEnd<T>(this ReadOnlyExMemory<T> memory, ReadOnlyExSpan<T> trimElements) where T : IEquatable<T>? {
            if (trimElements.Length > 1) {
                return memory.Slice(0, ClampEnd(memory.ExSpan, 0, trimElements));
            }

            if (trimElements.Length == 1) {
                return TrimEnd(memory, trimElements[0]);
            }

            return memory;
        }

        /// <summary>
        /// Removes all leading and trailing occurrences of a set of elements specified
        /// in a readonly span from the span.
        /// </summary>
        /// <param name="span">The source span from which the elements are removed.</param>
        /// <param name="trimElements">The span which contains the set of elements to remove.</param>
        /// <remarks>If <paramref name="trimElements"/> is empty, the span is returned unaltered.</remarks>
        public static ExSpan<T> Trim<T>(this ExSpan<T> span, ReadOnlyExSpan<T> trimElements) where T : IEquatable<T>? {
            if (trimElements.Length > 1) {
                TSize start = ClampStart(span, trimElements);
                TSize length = ClampEnd(span, start, trimElements);
                return span.Slice(start, length);
            }

            if (trimElements.Length == (TSize)1) {
                return Trim(span, trimElements[(TSize)0]);
            }

            return span;
        }

        /// <summary>
        /// Removes all leading occurrences of a set of elements specified
        /// in a readonly span from the span.
        /// </summary>
        /// <param name="span">The source span from which the elements are removed.</param>
        /// <param name="trimElements">The span which contains the set of elements to remove.</param>
        /// <remarks>If <paramref name="trimElements"/> is empty, the span is returned unaltered.</remarks>
        public static ExSpan<T> TrimStart<T>(this ExSpan<T> span, ReadOnlyExSpan<T> trimElements) where T : IEquatable<T>? {
            if (trimElements.Length > 1) {
                return span.Slice(ClampStart(span, trimElements));
            }

            if (trimElements.Length == (TSize)1) {
                return TrimStart(span, trimElements[(TSize)0]);
            }

            return span;
        }

        /// <summary>
        /// Removes all trailing occurrences of a set of elements specified
        /// in a readonly span from the span.
        /// </summary>
        /// <param name="span">The source span from which the elements are removed.</param>
        /// <param name="trimElements">The span which contains the set of elements to remove.</param>
        /// <remarks>If <paramref name="trimElements"/> is empty, the span is returned unaltered.</remarks>
        public static ExSpan<T> TrimEnd<T>(this ExSpan<T> span, ReadOnlyExSpan<T> trimElements) where T : IEquatable<T>? {
            if (trimElements.Length > 1) {
                return span.Slice((TSize)0, ClampEnd(span, (TSize)0, trimElements));
            }

            if (trimElements.Length == (TSize)1) {
                return TrimEnd(span, trimElements[(TSize)0]);
            }

            return span;
        }

        /// <summary>
        /// Removes all leading and trailing occurrences of a set of elements specified
        /// in a readonly span from the span.
        /// </summary>
        /// <param name="span">The source span from which the elements are removed.</param>
        /// <param name="trimElements">The span which contains the set of elements to remove.</param>
        /// <remarks>If <paramref name="trimElements"/> is empty, the span is returned unaltered.</remarks>
        public static ReadOnlyExSpan<T> Trim<T>(this ReadOnlyExSpan<T> span, ReadOnlyExSpan<T> trimElements) where T : IEquatable<T>? {
            if (trimElements.Length > 1) {
                TSize start = ClampStart(span, trimElements);
                TSize length = ClampEnd(span, start, trimElements);
                return span.Slice(start, length);
            }

            if (trimElements.Length == (TSize)1) {
                return Trim(span, trimElements[(TSize)0]);
            }

            return span;
        }

        /// <summary>
        /// Removes all leading occurrences of a set of elements specified
        /// in a readonly span from the span.
        /// </summary>
        /// <param name="span">The source span from which the elements are removed.</param>
        /// <param name="trimElements">The span which contains the set of elements to remove.</param>
        /// <remarks>If <paramref name="trimElements"/> is empty, the span is returned unaltered.</remarks>
        public static ReadOnlyExSpan<T> TrimStart<T>(this ReadOnlyExSpan<T> span, ReadOnlyExSpan<T> trimElements) where T : IEquatable<T>? {
            if (trimElements.Length > 1) {
                return span.Slice(ClampStart(span, trimElements));
            }

            if (trimElements.Length == (TSize)1) {
                return TrimStart(span, trimElements[(TSize)0]);
            }

            return span;
        }

        /// <summary>
        /// Removes all trailing occurrences of a set of elements specified
        /// in a readonly span from the span.
        /// </summary>
        /// <param name="span">The source span from which the elements are removed.</param>
        /// <param name="trimElements">The span which contains the set of elements to remove.</param>
        /// <remarks>If <paramref name="trimElements"/> is empty, the span is returned unaltered.</remarks>
        public static ReadOnlyExSpan<T> TrimEnd<T>(this ReadOnlyExSpan<T> span, ReadOnlyExSpan<T> trimElements) where T : IEquatable<T>? {
            if (trimElements.Length > 1) {
                return span.Slice((TSize)0, ClampEnd(span, (TSize)0, trimElements));
            }

            if (trimElements.Length == (TSize)1) {
                return TrimEnd(span, trimElements[(TSize)0]);
            }

            return span;
        }

        /// <summary>
        /// Delimits all leading occurrences of a set of elements specified
        /// in a readonly span from the span.
        /// </summary>
        /// <param name="span">The source span from which the elements are removed.</param>
        /// <param name="trimElements">The span which contains the set of elements to remove.</param>
        private static TSize ClampStart<T>(ReadOnlyExSpan<T> span, ReadOnlyExSpan<T> trimElements) where T : IEquatable<T>? {
            TSize start = (TSize)0;
            for (; start < span.Length; start+=1) {
                if (!trimElements.Contains(span[start])) {
                    break;
                }
            }

            return start;
        }

        /// <summary>
        /// Delimits all trailing occurrences of a set of elements specified
        /// in a readonly span from the span.
        /// </summary>
        /// <param name="span">The source span from which the elements are removed.</param>
        /// <param name="start">The start index from which to being searching.</param>
        /// <param name="trimElements">The span which contains the set of elements to remove.</param>
        private static TSize ClampEnd<T>(ReadOnlyExSpan<T> span, TSize start, ReadOnlyExSpan<T> trimElements) where T : IEquatable<T>? {
            // Initially, start==len==0. If ClampStart trims all, start==len
            Debug.Assert(start.ToUIntPtr() <= span.Length.ToUIntPtr());

            TSize end = span.Length - 1;
            for (; end >= start; end -= 1) {
                if (!trimElements.Contains(span[end])) {
                    break;
                }
            }

            return end - start + 1;
        }

        /// <summary>
        /// Removes all leading and trailing white-space characters from the memory.
        /// </summary>
        /// <param name="memory">The source memory from which the characters are removed.</param>
        public static ExMemory<char> Trim(this ExMemory<char> memory) {
            ReadOnlyExSpan<char> span = memory.ExSpan;
            TSize start = ClampStart(span);
            TSize length = ClampEnd(span, start);
            return memory.Slice(start, length);
        }

        /// <summary>
        /// Removes all leading white-space characters from the memory.
        /// </summary>
        /// <param name="memory">The source memory from which the characters are removed.</param>
        public static ExMemory<char> TrimStart(this ExMemory<char> memory)
            => memory.Slice(ClampStart(memory.ExSpan));

        /// <summary>
        /// Removes all trailing white-space characters from the memory.
        /// </summary>
        /// <param name="memory">The source memory from which the characters are removed.</param>
        public static ExMemory<char> TrimEnd(this ExMemory<char> memory)
            => memory.Slice(0, ClampEnd(memory.ExSpan, 0));

        /// <summary>
        /// Removes all leading and trailing white-space characters from the memory.
        /// </summary>
        /// <param name="memory">The source memory from which the characters are removed.</param>
        public static ReadOnlyExMemory<char> Trim(this ReadOnlyExMemory<char> memory) {
            ReadOnlyExSpan<char> span = memory.ExSpan;
            TSize start = ClampStart(span);
            TSize length = ClampEnd(span, start);
            return memory.Slice(start, length);
        }

        /// <summary>
        /// Removes all leading white-space characters from the memory.
        /// </summary>
        /// <param name="memory">The source memory from which the characters are removed.</param>
        public static ReadOnlyExMemory<char> TrimStart(this ReadOnlyExMemory<char> memory)
            => memory.Slice(ClampStart(memory.ExSpan));

        /// <summary>
        /// Removes all trailing white-space characters from the memory.
        /// </summary>
        /// <param name="memory">The source memory from which the characters are removed.</param>
        public static ReadOnlyExMemory<char> TrimEnd(this ReadOnlyExMemory<char> memory)
            => memory.Slice(0, ClampEnd(memory.ExSpan, 0));

        /// <summary>
        /// Removes all leading and trailing white-space characters from the span.
        /// </summary>
        /// <param name="span">The source span from which the characters are removed.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyExSpan<char> Trim(this ReadOnlyExSpan<char> span) {
            // Assume that in most cases input doesn't need trimming
            if (span.Length == (TSize)0 ||
                (!char.IsWhiteSpace(span[(TSize)0]) && !char.IsWhiteSpace(span[span.Length - 1]))) {
                return span;
            }
            return TrimFallback(span);

            [MethodImpl(MethodImplOptions.NoInlining)]
            static ReadOnlyExSpan<char> TrimFallback(ReadOnlyExSpan<char> span) {
                TSize start = (TSize)0;
                for (; start < span.Length; start += 1) {
                    if (!char.IsWhiteSpace(span[start])) {
                        break;
                    }
                }

                TSize end = span.Length - 1;
                for (; end > start; end -= 1) {
                    if (!char.IsWhiteSpace(span[end])) {
                        break;
                    }
                }
                return span.Slice(start, end - start + 1);
            }
        }

        /// <summary>
        /// Removes all leading white-space characters from the span.
        /// </summary>
        /// <param name="span">The source span from which the characters are removed.</param>
        public static ReadOnlyExSpan<char> TrimStart(this ReadOnlyExSpan<char> span) {
            TSize start = (TSize)0;
            for (; start < span.Length; start += 1) {
                if (!char.IsWhiteSpace(span[start])) {
                    break;
                }
            }

            return span.Slice(start);
        }

        /// <summary>
        /// Removes all trailing white-space characters from the span.
        /// </summary>
        /// <param name="span">The source span from which the characters are removed.</param>
        public static ReadOnlyExSpan<char> TrimEnd(this ReadOnlyExSpan<char> span) {
            TSize end = span.Length - 1;
            for (; end >= (TSize)0; end -= 1) {
                if (!char.IsWhiteSpace(span[end])) {
                    break;
                }
            }

            return span.Slice((TSize)0, end + 1);
        }

        /// <summary>
        /// Removes all leading and trailing occurrences of a specified character from the span.
        /// </summary>
        /// <param name="span">The source span from which the character is removed.</param>
        /// <param name="trimChar">The specified character to look for and remove.</param>
        public static ReadOnlyExSpan<char> Trim(this ReadOnlyExSpan<char> span, char trimChar) {
            TSize start = (TSize)0;
            for (; start < span.Length  ; start += 1) {
                if (span[start] != trimChar) {
                    break;
                }
            }

            TSize end = span.Length - 1;
            for (; end > start; end -= 1) {
                if (span[end] != trimChar) {
                    break;
                }
            }

            return span.Slice(start, end - start + 1);
        }

        /// <summary>
        /// Removes all leading occurrences of a specified character from the span.
        /// </summary>
        /// <param name="span">The source span from which the character is removed.</param>
        /// <param name="trimChar">The specified character to look for and remove.</param>
        public static ReadOnlyExSpan<char> TrimStart(this ReadOnlyExSpan<char> span, char trimChar) {
            TSize start = (TSize)0;
            for (; start < span.Length; start += 1) {
                if (span[start] != trimChar) {
                    break;
                }
            }

            return span.Slice(start);
        }

        /// <summary>
        /// Removes all trailing occurrences of a specified character from the span.
        /// </summary>
        /// <param name="span">The source span from which the character is removed.</param>
        /// <param name="trimChar">The specified character to look for and remove.</param>
        public static ReadOnlyExSpan<char> TrimEnd(this ReadOnlyExSpan<char> span, char trimChar) {
            TSize end = span.Length - 1;
            for (; end >= (TSize)0; end -= 1) {
                if (span[end] != trimChar) {
                    break;
                }
            }

            return span.Slice((TSize)0, end + 1);
        }

        /// <summary>
        /// Removes all leading and trailing occurrences of a set of characters specified
        /// in a readonly span from the span.
        /// </summary>
        /// <param name="span">The source span from which the characters are removed.</param>
        /// <param name="trimChars">The span which contains the set of characters to remove.</param>
        /// <remarks>If <paramref name="trimChars"/> is empty, white-space characters are removed instead.</remarks>
        public static ReadOnlyExSpan<char> Trim(this ReadOnlyExSpan<char> span, ReadOnlyExSpan<char> trimChars)
            => span.TrimStart(trimChars).TrimEnd(trimChars);

        /// <summary>
        /// Removes all leading occurrences of a set of characters specified
        /// in a readonly span from the span.
        /// </summary>
        /// <param name="span">The source span from which the characters are removed.</param>
        /// <param name="trimChars">The span which contains the set of characters to remove.</param>
        /// <remarks>If <paramref name="trimChars"/> is empty, white-space characters are removed instead.</remarks>
        public static ReadOnlyExSpan<char> TrimStart(this ReadOnlyExSpan<char> span, ReadOnlyExSpan<char> trimChars) {
            if (trimChars.IsEmpty) {
                return span.TrimStart();
            }

            TSize start = (TSize)0;
            for (; start < span.Length; start += 1) {
                for (TSize i = (TSize)0; i < trimChars.Length; i += 1) {
                    if (span[start] == trimChars[i]) {
                        goto Next;
                    }
                }

                break;
            Next:
                ;
            }

            return span.Slice(start);
        }

        /// <summary>
        /// Removes all trailing occurrences of a set of characters specified
        /// in a readonly span from the span.
        /// </summary>
        /// <param name="span">The source span from which the characters are removed.</param>
        /// <param name="trimChars">The span which contains the set of characters to remove.</param>
        /// <remarks>If <paramref name="trimChars"/> is empty, white-space characters are removed instead.</remarks>
        public static ReadOnlyExSpan<char> TrimEnd(this ReadOnlyExSpan<char> span, ReadOnlyExSpan<char> trimChars) {
            if (trimChars.IsEmpty) {
                return span.TrimEnd();
            }

            TSize end = span.Length - 1;
            for (; end >= (TSize)0; end -= 1) {
                for (TSize i = (TSize)0; i < trimChars.Length; i += 1) {
                    if (span[end] == trimChars[i]) {
                        goto Next;
                    }
                }
                    
                break;
            Next:
                ;
            }

            return span.Slice((TSize)0, end + 1);
        }

        /// <summary>
        /// Removes all leading and trailing white-space characters from the span.
        /// </summary>
        /// <param name="span">The source span from which the characters are removed.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ExSpan<char> Trim(this ExSpan<char> span) {
            // Assume that in most cases input doesn't need trimming
            if (span.Length == (TSize)0 ||
                (!char.IsWhiteSpace(span[(TSize)0]) && !char.IsWhiteSpace(span[span.Length - 1]))) {
                return span;
            }
            return TrimFallback(span);

            [MethodImpl(MethodImplOptions.NoInlining)]
            static ExSpan<char> TrimFallback(ExSpan<char> span) {
                TSize start = (TSize)0;
                for (; start < span.Length; start += 1) {
                    if (!char.IsWhiteSpace(span[start])) {
                        break;
                    }
                }

                TSize end = span.Length - 1;
                for (; end > start; end -= 1) {
                    if (!char.IsWhiteSpace(span[end])) {
                        break;
                    }
                }
                return span.Slice(start, end - start + 1);
            }
        }

        /// <summary>
        /// Removes all leading white-space characters from the span.
        /// </summary>
        /// <param name="span">The source span from which the characters are removed.</param>
        public static ExSpan<char> TrimStart(this ExSpan<char> span)
            => span.Slice(ClampStart(span));

        /// <summary>
        /// Removes all trailing white-space characters from the span.
        /// </summary>
        /// <param name="span">The source span from which the characters are removed.</param>
        public static ExSpan<char> TrimEnd(this ExSpan<char> span)
            => span.Slice((TSize)0, ClampEnd(span, (TSize)0));

        /// <summary>
        /// Delimits all leading occurrences of whitespace charecters from the span.
        /// </summary>
        /// <param name="span">The source span from which the characters are removed.</param>
        private static TSize ClampStart(ReadOnlyExSpan<char> span) {
            TSize start = (TSize)0;

            for (; start < span.Length; start += 1) {
                if (!char.IsWhiteSpace(span[start])) {
                    break;
                }
            }

            return start;
        }

        /// <summary>
        /// Delimits all trailing occurrences of whitespace charecters from the span.
        /// </summary>
        /// <param name="span">The source span from which the characters are removed.</param>
        /// <param name="start">The start index from which to being searching.</param>
        private static TSize ClampEnd(ReadOnlyExSpan<char> span, TSize start) {
            // Initially, start==len==0. If ClampStart trims all, start==len
            Debug.Assert(start.ToUIntPtr() <= span.Length.ToUIntPtr());

            TSize end = span.Length - 1;

            for (; end >= start; end -= 1) {
                if (!char.IsWhiteSpace(span[end])) {
                    break;
                }
            }

            return end - start + 1;
        }

    }
}
