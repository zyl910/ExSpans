using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zyl.ExSpans.Tests.Fake {
    internal sealed class DelegateEqualityComparer<T> : EqualityComparer<T> {
        private readonly Func<T?, T?, bool> _equals;
        private readonly Func<T, int> _getHashCode;

        public DelegateEqualityComparer(Func<T?, T?, bool> equals, Func<T, int> getHashCode) {
            _equals = equals;
            _getHashCode = getHashCode;
        }

        public override bool Equals(T? x, T? y) =>
            _equals(x, y);

        public override int GetHashCode(
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
            [DisallowNull]
#endif
        T obj) =>
            _getHashCode(obj);

        public override bool Equals(object? obj) =>
            obj is DelegateEqualityComparer<T> other &&
            _equals == other._equals &&
            _getHashCode == other._getHashCode;

        public override int GetHashCode() {
            //return HashCode.Combine(_equals.GetHashCode(), _getHashCode.GetHashCode());
            return _equals.GetHashCode() | _getHashCode.GetHashCode();
        }

#if NET8_0_OR_GREATER
#else
        /// <summary>
        /// Creates an <see cref="EqualityComparer{T}"/> by using the specified delegates as the implementation of the comparer's
        /// <see cref="EqualityComparer{T}.Equals"/> and <see cref="EqualityComparer{T}.GetHashCode"/> methods.
        /// </summary>
        /// <param name="equals">The delegate to use to implement the <see cref="EqualityComparer{T}.Equals"/> method.</param>
        /// <param name="getHashCode">
        /// The delegate to use to implement the <see cref="EqualityComparer{T}.GetHashCode"/> method.
        /// If no delegate is supplied, calls to the resulting comparer's <see cref="EqualityComparer{T}.GetHashCode"/>
        /// will throw <see cref="NotSupportedException"/>.
        /// </param>
        /// <returns>The new comparer.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="equals"/> delegate was null.</exception>
        public static EqualityComparer<T> Create(Func<T?, T?, bool> equals, Func<T, int>? getHashCode = null) {
#if NET8_0_OR_GREATER
            return EqualityComparer<T>.Create(equals, getHashCode);
#else
            if (null == equals) throw new ArgumentNullException(nameof(equals));
            getHashCode ??= _ => throw new NotSupportedException();
            return new DelegateEqualityComparer<T>(equals, getHashCode);
#endif // NET8_0_OR_GREATER
        }
#endif // NET8_0_OR_GREATER

    }
}
