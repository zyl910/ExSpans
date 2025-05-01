using System;
using System.Collections.Generic;
using System.Text;

namespace Zyl.ExSpans {
    partial class SpanHelpers {

#if TODO

        internal interface INegator<T> where T : struct {
            static abstract bool NegateIfNeeded(bool equals);
            static abstract Vector128<T> NegateIfNeeded(Vector128<T> equals);
            static abstract Vector256<T> NegateIfNeeded(Vector256<T> equals);
            static abstract Vector512<T> NegateIfNeeded(Vector512<T> equals);

            // The generic vector APIs assume use for IndexOf where `DontNegate` is
            // for `IndexOfAny` and `Negate` is for `IndexOfAnyExcept`

            static abstract bool HasMatch<TVector>(TVector left, TVector right)
                where TVector : struct, ISimdVector<TVector, T>;

            static abstract TVector GetMatchMask<TVector>(TVector left, TVector right)
                where TVector : struct, ISimdVector<TVector, T>;
        }

        internal readonly struct DontNegate<T> : INegator<T>
            where T : struct {
            public static bool NegateIfNeeded(bool equals) => equals;
            public static Vector128<T> NegateIfNeeded(Vector128<T> equals) => equals;
            public static Vector256<T> NegateIfNeeded(Vector256<T> equals) => equals;
            public static Vector512<T> NegateIfNeeded(Vector512<T> equals) => equals;

            // The generic vector APIs assume use for `IndexOfAny` where we
            // want "HasMatch" to mean any of the two elements match.

            public static bool HasMatch<TVector>(TVector left, TVector right)
                where TVector : struct, ISimdVector<TVector, T> {
                return TVector.EqualsAny(left, right);
            }

            public static TVector GetMatchMask<TVector>(TVector left, TVector right)
                where TVector : struct, ISimdVector<TVector, T> {
                return TVector.Equals(left, right);
            }
        }

        internal readonly struct Negate<T> : INegator<T>
            where T : struct {
            public static bool NegateIfNeeded(bool equals) => !equals;
            public static Vector128<T> NegateIfNeeded(Vector128<T> equals) => ~equals;
            public static Vector256<T> NegateIfNeeded(Vector256<T> equals) => ~equals;
            public static Vector512<T> NegateIfNeeded(Vector512<T> equals) => ~equals;

            // The generic vector APIs assume use for `IndexOfAnyExcept` where we
            // want "HasMatch" to mean any of the two elements don't match

            public static bool HasMatch<TVector>(TVector left, TVector right)
                where TVector : struct, ISimdVector<TVector, T> {
                return !TVector.EqualsAll(left, right);
            }

            public static TVector GetMatchMask<TVector>(TVector left, TVector right)
                where TVector : struct, ISimdVector<TVector, T> {
                return ~TVector.Equals(left, right);
            }
        }

#endif // TODO

    }
}
