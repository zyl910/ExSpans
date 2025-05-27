#if NET7_0_OR_GREATER
#define GENERIC_MATH // C# 11 - Generic math support. https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-11#generic-math-support
#endif // NET7_0_OR_GREATER

using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static nint IndexOfAnyValueType(ref char searchSpace, char value0, char value1, char value2, char value3, char value4, char value5, TSize length) {
            return IndexOfAnyValueType<short>(ref Unsafe.As<char, short>(ref searchSpace), (short)value0, (short)value1, (short)value2, (short)value3, (short)value4, (short)value5, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static nint IndexOfAnyValueType<T>(ref T searchSpace, T value0, T value1, T value2, T value3, T value4, T value5, TSize length) where T : struct, IEquatable<T>
#if GENERIC_MATH
                , INumber<T>
#endif // GENERIC_MATH
            => IndexOfAnyValueType<T, DontNegate<T>>(ref searchSpace, value0, value1, value2, value3, value4, value5, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static nint IndexOfAnyExceptValueType<T>(ref T searchSpace, T value0, T value1, T value2, T value3, T value4, T value5, TSize length) where T : struct, IEquatable<T>
#if GENERIC_MATH
                , INumber<T>
#endif // GENERIC_MATH
            => IndexOfAnyValueType<T, Negate<T>>(ref searchSpace, value0, value1, value2, value3, value4, value5, length);

        private static nint IndexOfAnyValueType<TValue,
#if GENERIC_MATH
                TNegator
#else
                TNegatorType
#endif // GENERIC_MATH
                >(ref TValue searchSpace, TValue value0, TValue value1, TValue value2, TValue value3, TValue value4, TValue value5, TSize length)
            where TValue : struct, IEquatable<TValue>
#if GENERIC_MATH
                , INumber<TValue>
#endif // GENERIC_MATH
                where
#if GENERIC_MATH
                TNegator
#else
                TNegatorType
#endif // GENERIC_MATH
                : struct, INegator<TValue> {
#if GENERIC_MATH
#else
            TNegatorType TNegator = default;
#endif // GENERIC_MATH
            Debug.Assert(length >= 0, "Expected non-negative length");
            Debug.Assert(value0 is byte or short or int or long, "Expected caller to normalize to one of these types");

            if (false) {
#if NET8_0_OR_GREATER
            } else if (Vector512.IsHardwareAccelerated && length >= Vector512<TValue>.Count && Vector512<byte>.Count >= Vector<byte>.Count) {
                Vector512<TValue> equals, current, values0 = Vector512.Create(value0), values1 = Vector512.Create(value1),
                    values2 = Vector512.Create(value2), values3 = Vector512.Create(value3), values4 = Vector512.Create(value4),
                    values5 = Vector512.Create(value5);
                ref TValue currentSearchSpace = ref searchSpace;
                ref TValue oneVectorAwayFromEnd = ref ExUnsafe.Add(ref searchSpace, length - Vector512<TValue>.Count);

                // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
                do {
                    current = Vector512.LoadUnsafe(ref currentSearchSpace);
                    equals = TNegator.NegateIfNeeded(Vector512.Equals(values0, current) | Vector512.Equals(values1, current) | Vector512.Equals(values2, current)
                           | Vector512.Equals(values3, current) | Vector512.Equals(values4, current) | Vector512.Equals(values5, current));
                    if (equals == Vector512<TValue>.Zero) {
                        currentSearchSpace = ref ExUnsafe.Add(ref currentSearchSpace, Vector512<TValue>.Count);
                        continue;
                    }

                    return ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, equals);
                }
                while (!Unsafe.IsAddressGreaterThan(ref currentSearchSpace, ref oneVectorAwayFromEnd));

                // If any elements remain, process the last vector in the search space.
                if ((nuint)length % (nuint)Vector512<TValue>.Count != 0) {
                    current = Vector512.LoadUnsafe(ref oneVectorAwayFromEnd);
                    equals = TNegator.NegateIfNeeded(Vector512.Equals(values0, current) | Vector512.Equals(values1, current) | Vector512.Equals(values2, current)
                           | Vector512.Equals(values3, current) | Vector512.Equals(values4, current) | Vector512.Equals(values5, current));
                    if (equals != Vector512<TValue>.Zero) {
                        return ComputeFirstIndex(ref searchSpace, ref oneVectorAwayFromEnd, equals);
                    }
                }
#endif // NET8_0_OR_GREATER
            } else if (Vector.IsHardwareAccelerated && length >= Vector<TValue>.Count) {
                Vector<TValue> equals, current, values0 = Vectors.Create(value0), values1 = Vectors.Create(value1),
                    values2 = Vectors.Create(value2), values3 = Vectors.Create(value3), values4 = Vectors.Create(value4),
                    values5 = Vectors.Create(value5);
                ref TValue currentSearchSpace = ref searchSpace;
                ref TValue oneVectorAwayFromEnd = ref ExUnsafe.Add(ref searchSpace, length - Vector<TValue>.Count);

                // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
                do {
                    current = VectorHelper.LoadUnsafe(ref currentSearchSpace);
                    equals = TNegator.NegateIfNeeded(Vector.Equals(values0, current) | Vector.Equals(values1, current) | Vector.Equals(values2, current)
                           | Vector.Equals(values3, current) | Vector.Equals(values4, current) | Vector.Equals(values5, current));
                    if (equals == Vector<TValue>.Zero) {
                        currentSearchSpace = ref ExUnsafe.Add(ref currentSearchSpace, Vector<TValue>.Count);
                        continue;
                    }

                    return ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, equals);
                }
                while (!Unsafe.IsAddressGreaterThan(ref currentSearchSpace, ref oneVectorAwayFromEnd));

                // If any elements remain, process the last vector in the search space.
                if ((nuint)length % (nuint)Vector<TValue>.Count != 0) {
                    current = VectorHelper.LoadUnsafe(ref oneVectorAwayFromEnd);
                    equals = TNegator.NegateIfNeeded(Vector.Equals(values0, current) | Vector.Equals(values1, current) | Vector.Equals(values2, current)
                           | Vector.Equals(values3, current) | Vector.Equals(values4, current) | Vector.Equals(values5, current));
                    if (equals != Vector<TValue>.Zero) {
                        return ComputeFirstIndex(ref searchSpace, ref oneVectorAwayFromEnd, equals);
                    }
                }
#if NET7_0_OR_GREATER
            } else if (Vector256.IsHardwareAccelerated && length >= Vector256<TValue>.Count) {
                Vector256<TValue> equals, current, values0 = Vector256.Create(value0), values1 = Vector256.Create(value1),
                    values2 = Vector256.Create(value2), values3 = Vector256.Create(value3), values4 = Vector256.Create(value4),
                    values5 = Vector256.Create(value5);
                ref TValue currentSearchSpace = ref searchSpace;
                ref TValue oneVectorAwayFromEnd = ref ExUnsafe.Add(ref searchSpace, length - Vector256<TValue>.Count);

                // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
                do {
                    current = Vector256.LoadUnsafe(ref currentSearchSpace);
                    equals = TNegator.NegateIfNeeded(Vector256.Equals(values0, current) | Vector256.Equals(values1, current) | Vector256.Equals(values2, current)
                           | Vector256.Equals(values3, current) | Vector256.Equals(values4, current) | Vector256.Equals(values5, current));
                    if (equals == Vector256<TValue>.Zero) {
                        currentSearchSpace = ref ExUnsafe.Add(ref currentSearchSpace, Vector256<TValue>.Count);
                        continue;
                    }

                    return ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, equals);
                }
                while (!Unsafe.IsAddressGreaterThan(ref currentSearchSpace, ref oneVectorAwayFromEnd));

                // If any elements remain, process the last vector in the search space.
                if ((nuint)length % (nuint)Vector256<TValue>.Count != 0) {
                    current = Vector256.LoadUnsafe(ref oneVectorAwayFromEnd);
                    equals = TNegator.NegateIfNeeded(Vector256.Equals(values0, current) | Vector256.Equals(values1, current) | Vector256.Equals(values2, current)
                           | Vector256.Equals(values3, current) | Vector256.Equals(values4, current) | Vector256.Equals(values5, current));
                    if (equals != Vector256<TValue>.Zero) {
                        return ComputeFirstIndex(ref searchSpace, ref oneVectorAwayFromEnd, equals);
                    }
                }
            } else if (Vector128.IsHardwareAccelerated && length >= Vector128<TValue>.Count) {
                Vector128<TValue> equals, current, values0 = Vector128.Create(value0), values1 = Vector128.Create(value1),
                    values2 = Vector128.Create(value2), values3 = Vector128.Create(value3), values4 = Vector128.Create(value4),
                    values5 = Vector128.Create(value5);
                ref TValue currentSearchSpace = ref searchSpace;
                ref TValue oneVectorAwayFromEnd = ref ExUnsafe.Add(ref searchSpace, length - Vector128<TValue>.Count);

                // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
                do {
                    current = Vector128.LoadUnsafe(ref currentSearchSpace);
                    equals = TNegator.NegateIfNeeded(Vector128.Equals(values0, current) | Vector128.Equals(values1, current) | Vector128.Equals(values2, current)
                           | Vector128.Equals(values3, current) | Vector128.Equals(values4, current) | Vector128.Equals(values5, current));
                    if (equals == Vector128<TValue>.Zero) {
                        currentSearchSpace = ref ExUnsafe.Add(ref currentSearchSpace, Vector128<TValue>.Count);
                        continue;
                    }

                    return ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, equals);
                }
                while (!Unsafe.IsAddressGreaterThan(ref currentSearchSpace, ref oneVectorAwayFromEnd));

                // If any elements remain, process the first vector in the search space.
                if ((nuint)length % (nuint)Vector128<TValue>.Count != 0) {
                    current = Vector128.LoadUnsafe(ref oneVectorAwayFromEnd);
                    equals = TNegator.NegateIfNeeded(Vector128.Equals(values0, current) | Vector128.Equals(values1, current) | Vector128.Equals(values2, current)
                           | Vector128.Equals(values3, current) | Vector128.Equals(values4, current) | Vector128.Equals(values5, current));
                    if (equals != Vector128<TValue>.Zero) {
                        return ComputeFirstIndex(ref searchSpace, ref oneVectorAwayFromEnd, equals);
                    }
                }
#endif // NET7_0_OR_GREATER
            }

            if (true) {
                nint offset = 0;
                TValue lookUp;

                while (length >= 4) {
                    length -= 4;

                    ref TValue current = ref ExUnsafe.Add(ref searchSpace, offset);
                    lookUp = current;
                    if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1) || lookUp.Equals(value2) || lookUp.Equals(value3) || lookUp.Equals(value4) || lookUp.Equals(value5))) goto Found;
                    lookUp = ExUnsafe.Add(ref current, 1);
                    if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1) || lookUp.Equals(value2) || lookUp.Equals(value3) || lookUp.Equals(value4) || lookUp.Equals(value5))) goto Found1;
                    lookUp = ExUnsafe.Add(ref current, 2);
                    if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1) || lookUp.Equals(value2) || lookUp.Equals(value3) || lookUp.Equals(value4) || lookUp.Equals(value5))) goto Found2;
                    lookUp = ExUnsafe.Add(ref current, 3);
                    if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1) || lookUp.Equals(value2) || lookUp.Equals(value3) || lookUp.Equals(value4) || lookUp.Equals(value5))) goto Found3;

                    offset += 4;
                }

                while (length > 0) {
                    length -= 1;

                    lookUp = ExUnsafe.Add(ref searchSpace, offset);
                    if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1) || lookUp.Equals(value2) || lookUp.Equals(value3) || lookUp.Equals(value4) || lookUp.Equals(value5))) goto Found;

                    offset += 1;
                }

                return -1;
            Found3:
                return offset + 3;
            Found2:
                return offset + 2;
            Found1:
                return offset + 1;
            Found:
                return offset;
            }
            //return -1;
        }

    }
}
