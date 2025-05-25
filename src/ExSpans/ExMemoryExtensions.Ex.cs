using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Zyl.ExSpans.Extensions;

namespace Zyl.ExSpans {
    partial class ExMemoryExtensions {

        /// <summary>
        /// Fills the contents of this span with the given value (用指定的值填充此跨度的内容).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="span">Target span (目标跨度).</param>
        /// <param name="value">The given value (指定的值).</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Fill<T>(this ExSpan<T> span, T value) {
            ExSpanHelpers.Fill(ref span.GetPinnableReference(), span.Length.ToUIntPtr(), value);
        }

        /// <summary>
        /// Determines the relative order of the sequences being compared by comparing the elements using IComparable{T}.CompareTo(T).
        /// </summary>
        public static int SequenceCompareTo<T>(this ExSpan<T> span, ReadOnlyExSpan<T> other, IComparer<T>? comparer = null) {
            return SequenceCompareTo(span.AsReadOnlyExSpan(), other, comparer);
        }

        /// <summary>
        /// Copies the contents of this string into the destination span.
        /// </summary>
        /// <param name="source">The source string.</param>
        /// <param name="destination">The span into which to copy this string's contents.</param>
        /// <returns>true if the data was copied; false if the destination was too short to fit the contents of the string.</returns>
        public static bool TryCopyTo(this string source, ExSpan<char> destination) {
            return source.AsExSpan().TryCopyTo(destination);
        }

    }
}
