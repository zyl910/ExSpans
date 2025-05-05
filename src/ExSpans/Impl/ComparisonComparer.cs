using System;
using System.Collections.Generic;
using System.Text;

namespace Zyl.ExSpans.Impl {

    internal sealed class ComparisonComparer<T> : Comparer<T> {
        private readonly Comparison<T> _comparison;

        public ComparisonComparer(Comparison<T> comparison) {
            _comparison = comparison;
        }

        public override int Compare(T? x, T? y) => _comparison(x!, y!);
    }

}
