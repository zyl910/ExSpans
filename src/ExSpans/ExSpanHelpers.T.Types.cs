#if NET7_0_OR_GREATER
#define GENERIC_MATH // C# 11 - Generic math support. https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-11#generic-math-support
#endif // NET7_0_OR_GREATER

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
#if NETCOREAPP3_0_OR_GREATER
using System.Runtime.Intrinsics;
#endif // NETCOREAPP3_0_OR_GREATER
using System.Text;
using Zyl.ExSpans.Impl;
using Zyl.VectorTraits;

namespace Zyl.ExSpans {
    partial class ExSpanHelpers {

        internal interface INegator<T> where T : struct {
#if GENERIC_MATH
            static abstract
#endif // GENERIC_MATH
            bool NegateIfNeeded(bool equals);
#if NETCOREAPP3_0_OR_GREATER
#if GENERIC_MATH
            static abstract
#endif // GENERIC_MATH
            Vector128<T> NegateIfNeeded(Vector128<T> equals);
#if GENERIC_MATH
            static abstract
#endif // GENERIC_MATH
            Vector256<T> NegateIfNeeded(Vector256<T> equals);
#endif // NETCOREAPP3_0_OR_GREATER
#if NET8_0_OR_GREATER
            static abstract Vector512<T> NegateIfNeeded(Vector512<T> equals);
#endif // NET8_0_OR_GREATER
#if GENERIC_MATH
            static abstract
#endif // GENERIC_MATH
            Vector<T> NegateIfNeeded(Vector<T> equals);

            // The generic vector APIs assume use for IndexOf where `DontNegate` is
            // for `IndexOfAny` and `Negate` is for `IndexOfAnyExcept`

            //static abstract bool HasMatch<TVector>(TVector left, TVector right)
            //    where TVector : struct, ISimdVector<TVector, T>;
#if NETCOREAPP3_0_OR_GREATER
#if GENERIC_MATH
            static abstract
#endif // GENERIC_MATH
            bool HasMatch(Vector128<T> left, Vector128<T> right);
#if GENERIC_MATH
            static abstract
#endif // GENERIC_MATH
            bool HasMatch(Vector256<T> left, Vector256<T> right);
#endif // NETCOREAPP3_0_OR_GREATER
#if NET8_0_OR_GREATER
#if GENERIC_MATH
            static abstract
#endif // GENERIC_MATH
            bool HasMatch(Vector512<T> left, Vector512<T> right);
#endif // NET8_0_OR_GREATER
#if GENERIC_MATH
            static abstract
#endif // GENERIC_MATH
            bool HasMatch(Vector<T> left, Vector<T> right);

            //static abstract TVector GetMatchMask<TVector>(TVector left, TVector right)
            //    where TVector : struct, ISimdVector<TVector, T>;
#if NETCOREAPP3_0_OR_GREATER
#if GENERIC_MATH
            static abstract
#endif // GENERIC_MATH
            Vector128<T> GetMatchMask(Vector128<T> left, Vector128<T> right);
#if GENERIC_MATH
            static abstract
#endif // GENERIC_MATH
            Vector256<T> GetMatchMask(Vector256<T> left, Vector256<T> right);
#endif // NETCOREAPP3_0_OR_GREATER
#if NET8_0_OR_GREATER
#if GENERIC_MATH
            static abstract
#endif // GENERIC_MATH
            Vector512<T> GetMatchMask(Vector512<T> left, Vector512<T> right);
#endif // NET8_0_OR_GREATER
#if GENERIC_MATH
            static abstract
#endif // GENERIC_MATH
            Vector<T> GetMatchMask(Vector<T> left, Vector<T> right);
        }

        internal readonly struct DontNegate<T> : INegator<T>
            where T : struct {
            public
#if GENERIC_MATH
                static
#endif // GENERIC_MATH
                bool NegateIfNeeded(bool equals) => equals;
#if NETCOREAPP3_0_OR_GREATER
            public
#if GENERIC_MATH
                static
#endif // GENERIC_MATH
                Vector128<T> NegateIfNeeded(Vector128<T> equals) => equals;
            public
#if GENERIC_MATH
                static
#endif // GENERIC_MATH
                Vector256<T> NegateIfNeeded(Vector256<T> equals) => equals;
#endif // NETCOREAPP3_0_OR_GREATER
#if NET8_0_OR_GREATER
            public static Vector512<T> NegateIfNeeded(Vector512<T> equals) => equals;
#endif // NET8_0_OR_GREATER
            public
#if GENERIC_MATH
                static
#endif // GENERIC_MATH
                Vector<T> NegateIfNeeded(Vector<T> equals) => equals;

            // The generic vector APIs assume use for `IndexOfAny` where we
            // want "HasMatch" to mean any of the two elements match.

            //public static bool HasMatch<TVector>(TVector left, TVector right)
            //    where TVector : struct, ISimdVector<TVector, T> {
            //    return TVector.EqualsAny(left, right);
            //}
#if NETCOREAPP3_0_OR_GREATER
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public
#if GENERIC_MATH
                    static
#endif // GENERIC_MATH
                bool HasMatch(Vector128<T> left, Vector128<T> right) {
                return Vector128Helper.EqualsAny(left, right);
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public
#if GENERIC_MATH
                    static
#endif // GENERIC_MATH
                bool HasMatch(Vector256<T> left, Vector256<T> right) {
                return Vector256Helper.EqualsAny(left, right);
            }
#endif // NETCOREAPP3_0_OR_GREATER
#if NET8_0_OR_GREATER
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool HasMatch(Vector512<T> left, Vector512<T> right) {
                return Vector512.EqualsAny(left, right);
            }
#endif // NET8_0_OR_GREATER
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public
#if GENERIC_MATH
                    static
#endif // GENERIC_MATH
                bool HasMatch(Vector<T> left, Vector<T> right) {
                return Vector.EqualsAny(left, right);
            }

            //public static TVector GetMatchMask<TVector>(TVector left, TVector right)
            //    where TVector : struct, ISimdVector<TVector, T> {
            //    return TVector.Equals(left, right);
            //}
#if NETCOREAPP3_0_OR_GREATER
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public
#if GENERIC_MATH
                    static
#endif // GENERIC_MATH
                Vector128<T> GetMatchMask(Vector128<T> left, Vector128<T> right) {
                return Vector128Helper.Equals(left, right);
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public
#if GENERIC_MATH
                    static
#endif // GENERIC_MATH
                Vector256<T> GetMatchMask(Vector256<T> left, Vector256<T> right) {
                return Vector256Helper.Equals(left, right);
            }
#endif // NETCOREAPP3_0_OR_GREATER
#if NET8_0_OR_GREATER
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static Vector512<T> GetMatchMask(Vector512<T> left, Vector512<T> right) {
                return Vector512.Equals(left, right);
            }
#endif // NET8_0_OR_GREATER
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public
#if GENERIC_MATH
                    static
#endif // GENERIC_MATH
                Vector<T> GetMatchMask(Vector<T> left, Vector<T> right) {
                return Vector.Equals(left, right);
            }
        }
        
        internal readonly struct Negate<T> : INegator<T>
            where T : struct {
            public
#if GENERIC_MATH
                static
#endif // GENERIC_MATH
                bool NegateIfNeeded(bool equals) => !equals;
            //public static Vector128<T> NegateIfNeeded(Vector128<T> equals) => ~equals;
            //public static Vector256<T> NegateIfNeeded(Vector256<T> equals) => ~equals;
#if NETCOREAPP3_0_OR_GREATER
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public
#if GENERIC_MATH
                    static
#endif // GENERIC_MATH
                Vector128<T> NegateIfNeeded(Vector128<T> equals) => Vector128s.OnesComplement(equals);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public
#if GENERIC_MATH
                    static
#endif // GENERIC_MATH
                Vector256<T> NegateIfNeeded(Vector256<T> equals) => Vector256s.OnesComplement(equals);
#endif // NETCOREAPP3_0_OR_GREATER
#if NET8_0_OR_GREATER
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static Vector512<T> NegateIfNeeded(Vector512<T> equals) => ~equals;
#endif // NET8_0_OR_GREATER
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public
#if GENERIC_MATH
                    static
#endif // GENERIC_MATH
                Vector<T> NegateIfNeeded(Vector<T> equals) => ~equals;

            // The generic vector APIs assume use for `IndexOfAnyExcept` where we
            // want "HasMatch" to mean any of the two elements don't match

            //public static bool HasMatch<TVector>(TVector left, TVector right)
            //    where TVector : struct, ISimdVector<TVector, T> {
            //    return !TVector.EqualsAll(left, right);
            //}
#if NETCOREAPP3_0_OR_GREATER
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public
#if GENERIC_MATH
                    static
#endif // GENERIC_MATH
                bool HasMatch(Vector128<T> left, Vector128<T> right) {
                return !Vector128Helper.EqualsAll(left, right);
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public
#if GENERIC_MATH
                    static
#endif // GENERIC_MATH
                bool HasMatch(Vector256<T> left, Vector256<T> right) {
                return !Vector256Helper.EqualsAll(left, right);
            }
#endif // NETCOREAPP3_0_OR_GREATER
#if NET8_0_OR_GREATER
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool HasMatch(Vector512<T> left, Vector512<T> right) {
                return !Vector512.EqualsAll(left, right);
            }
#endif // NET8_0_OR_GREATER
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public
#if GENERIC_MATH
                    static
#endif // GENERIC_MATH
                bool HasMatch(Vector<T> left, Vector<T> right) {
                return !Vector.EqualsAll(left, right);
            }

            //public static TVector GetMatchMask<TVector>(TVector left, TVector right)
            //    where TVector : struct, ISimdVector<TVector, T> {
            //    return ~TVector.Equals(left, right);
            //}
#if NETCOREAPP3_0_OR_GREATER
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public
#if GENERIC_MATH
                    static
#endif // GENERIC_MATH
                Vector128<T> GetMatchMask(Vector128<T> left, Vector128<T> right) {
                return Vector128s.OnesComplement(Vector128Helper.Equals(left, right));
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public
#if GENERIC_MATH
                    static
#endif // GENERIC_MATH
                Vector256<T> GetMatchMask(Vector256<T> left, Vector256<T> right) {
                return Vector256s.OnesComplement(Vector256Helper.Equals(left, right));
            }
#endif // NETCOREAPP3_0_OR_GREATER
#if NET8_0_OR_GREATER
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static Vector512<T> GetMatchMask(Vector512<T> left, Vector512<T> right) {
                return ~Vector512.Equals(left, right);
            }
#endif // NET8_0_OR_GREATER
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public
#if GENERIC_MATH
                    static
#endif // GENERIC_MATH
                Vector<T> GetMatchMask(Vector<T> left, Vector<T> right) {
                return ~Vector.Equals(left, right);
            }
        }

    }
}
