using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

namespace Zyl.ExSpans.Impl {

#if TODO
    internal interface IExArraySortHelper<TKey> {
        void Sort(Span<TKey> keys, IComparer<TKey>? comparer);
        int BinarySearch(TKey[] keys, int index, int length, TKey value, IComparer<TKey>? comparer);
    }

    internal sealed partial class ExArraySortHelper<T>
        : IExArraySortHelper<T> {
        private static readonly IExArraySortHelper<T> s_defaultExArraySortHelper = CreateExArraySortHelper();

        public static IExArraySortHelper<T> Default => s_defaultExArraySortHelper;

        private static IExArraySortHelper<T> CreateExArraySortHelper() {
            IExArraySortHelper<T> defaultExArraySortHelper;

            if (typeof(IComparable<T>).IsAssignableFrom(typeof(T))) {
                defaultExArraySortHelper = (IExArraySortHelper<T>)RuntimeTypeHandle.CreateInstanceForAnotherGenericParameter((RuntimeType)typeof(GenericExArraySortHelper<string>), (RuntimeType)typeof(T));
            } else {
                defaultExArraySortHelper = new ExArraySortHelper<T>();
            }
            return defaultExArraySortHelper;
        }
    }

    internal sealed partial class GenericExArraySortHelper<T>
        : IExArraySortHelper<T> {
    }

    internal interface IExArraySortHelper<TKey, TValue> {
        void Sort(Span<TKey> keys, Span<TValue> values, IComparer<TKey>? comparer);
    }

    internal sealed partial class ExArraySortHelper<TKey, TValue>
        : IExArraySortHelper<TKey, TValue> {
        private static readonly IExArraySortHelper<TKey, TValue> s_defaultExArraySortHelper = CreateExArraySortHelper();

        public static IExArraySortHelper<TKey, TValue> Default => s_defaultExArraySortHelper;

        private static IExArraySortHelper<TKey, TValue> CreateExArraySortHelper() {
            IExArraySortHelper<TKey, TValue> defaultExArraySortHelper;

            if (typeof(IComparable<TKey>).IsAssignableFrom(typeof(TKey))) {
                defaultExArraySortHelper = (IExArraySortHelper<TKey, TValue>)RuntimeTypeHandle.CreateInstanceForAnotherGenericParameter((RuntimeType)typeof(GenericExArraySortHelper<string, string>), (RuntimeType)typeof(TKey), (RuntimeType)typeof(TValue));
            } else {
                defaultExArraySortHelper = new ExArraySortHelper<TKey, TValue>();
            }
            return defaultExArraySortHelper;
        }
    }

    internal sealed partial class GenericExArraySortHelper<TKey, TValue>
        : IExArraySortHelper<TKey, TValue> {
    }
#endif // TODO

}
