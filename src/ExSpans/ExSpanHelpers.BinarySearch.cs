#if NET9_0_OR_GREATER
#define ALLOWS_REF_STRUCT // C# 13 - ref struct interface; allows ref struct. https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-13#ref-struct-interfaces
#endif // NET9_0_OR_GREATER

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Zyl.ExSpans {
    partial class ExSpanHelpers {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSize BinarySearch<T, TComparable>(
            ReadOnlyExSpan<T> span, TComparable comparable)
            where TComparable : IComparable<T>
#if ALLOWS_REF_STRUCT
            , allows ref struct
#endif
            {
            if (comparable == null)
                throw new ArgumentNullException(nameof(comparable));

            return BinarySearch(ref ExMemoryMarshal.GetReference(span), span.Length, comparable);
        }

        public static TSize BinarySearch<T, TComparable>(
            ref T spanStart, TSize length, TComparable comparable)
            where TComparable : IComparable<T>
#if ALLOWS_REF_STRUCT
            , allows ref struct
#endif
            {
            TSize lo = 0;
            TSize hi = length - 1;
            // If length == 0, hi == -1, and loop will not be entered
            while (lo <= hi) {
                // PERF: `lo` or `hi` will never be negative inside the loop,
                //       so computing median using uints is safe since we know
                //       `length <= int.MaxValue`, and indices are >= 0
                //       and thus cannot overflow an uint.
                //       Saves one subtraction per loop compared to
                //       `int i = lo + ((hi - lo) >> 1);`
                TSize i = (TSize)(((TUSize)hi + (TUSize)lo) >> 1);

                int c = comparable.CompareTo(Unsafe.Add(ref spanStart, i));
                if (c == 0) {
                    return i;
                } else if (c > 0) {
                    lo = i + 1;
                } else {
                    hi = i - 1;
                }
            }
            // If none found, then a negative number that is the bitwise complement
            // of the index of the next element that is larger than or, if there is
            // no larger element, the bitwise complement of `length`, which
            // is `lo` at this point.
            return ~lo;
        }

        // Helper to allow sharing all code via IComparable<T> inlineable
        internal readonly
#if ALLOWS_REF_STRUCT
            ref
#endif
            struct ComparerComparable<T, TComparer> : IComparable<T>
            where TComparer : IComparer<T>
#if ALLOWS_REF_STRUCT
            , allows ref struct
#endif
            {
            private readonly T _value;
            private readonly TComparer _comparer;

            public ComparerComparable(T value, TComparer comparer) {
                _value = value;
                _comparer = comparer;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int CompareTo(T? other) => _comparer.Compare(_value, other!);
        }

    }
}
