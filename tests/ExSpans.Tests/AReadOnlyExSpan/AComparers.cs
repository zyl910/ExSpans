using System.Collections.Generic;
using Xunit;

namespace Zyl.ExSpans.Tests.AReadOnlyExSpan {
    public static partial class AComparers {
        internal static IEnumerable<IEqualityComparer<T>?> GetDefaultEqualityComparers<T>() {
            yield return null;

            yield return EqualityComparer<T>.Default;

#if NET8_0_OR_GREATER
            yield return EqualityComparer<T>.Create((i, j) => EqualityComparer<T>.Default.Equals(i, j));
#endif // NET8_0_OR_GREATER

            if (typeof(T) == typeof(string)) {
                yield return (IEqualityComparer<T>)(object)StringComparer.Ordinal;
            }
        }

        internal static IEnumerable<IComparer<T>?> GetDefaultComparers<T>() {
            yield return null;

            yield return Comparer<T>.Default;

            yield return Comparer<T>.Create((i, j) => Comparer<T>.Default.Compare(i, j));

            if (typeof(T) == typeof(string)) {
                yield return (IComparer<T>)(object)StringComparer.Ordinal;
            }
        }

#if NET8_0_OR_GREATER
        internal static IEqualityComparer<T> GetFalseEqualityComparer<T>() =>
            EqualityComparer<T>.Create((i, j) => false);
#endif // NET8_0_OR_GREATER
    }
}
