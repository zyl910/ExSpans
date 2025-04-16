using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
#if NETCOREAPP3_0_OR_GREATER
using System.Runtime.Intrinsics;
#endif // NETCOREAPP3_0_OR_GREATER
using System.Text;
using Zyl.SizableSpans.Impl;
using Zyl.VectorTraits.Numerics;

namespace Zyl.SizableSpans {
    partial class SizableSpanHelpers {

        // Optimized byte-based SequenceEquals. The "length" parameter for this one is declared a nuint rather than int as we also use it for types other than byte
        // where the length can exceed 2Gb once scaled by sizeof(T).
        // [Intrinsic] // Unrolled for constant length
        public static unsafe bool SequenceEqual(ref byte first, ref byte second, nuint length) {
            bool result;
            // Use nint for arithmetic to avoid unnecessary 64->32->64 truncations
            if (length >= (nuint)sizeof(nuint)) {
                // Conditional jmp forward to favor shorter lengths. (See comment at "Equal:" label)
                // The longer lengths can make back the time due to branch misprediction
                // better than shorter lengths.
                goto Longer;
            }

            if (length < sizeof(uint)) {
                uint differentBits = 0;
                nuint offset = (length & 2);
                if (offset != 0) {
                    differentBits = LoadUShort(ref first);
                    differentBits -= LoadUShort(ref second);
                }
                if ((length & 1) != 0) {
                    differentBits |= (uint)SizableUnsafe.AddByteOffset(ref first, offset) - (uint)SizableUnsafe.AddByteOffset(ref second, offset);
                }
                result = (differentBits == 0);
                goto Result;
            } else {
                nuint offset = length - sizeof(uint);
                uint differentBits = LoadUInt(ref first) - LoadUInt(ref second);
                differentBits |= LoadUInt(ref first, offset) - LoadUInt(ref second, offset);
                result = (differentBits == 0);
                goto Result;
            }
        Longer:
            // Only check that the ref is the same if buffers are large,
            // and hence its worth avoiding doing unnecessary comparisons
            if (!Unsafe.AreSame(ref first, ref second)) {
                // C# compiler inverts this test, making the outer goto the conditional jmp.
                goto Vector;
            }

            // This becomes a conditional jmp forward to not favor it.
            goto Equal;

        Result:
            return result;
        // When the sequence is equal; which is the longest execution, we want it to determine that
        // as fast as possible so we do not want the early outs to be "predicted not taken" branches.
        Equal:
            return true;

        Vector:
            if (false) {
                // See else.
#if NET7_0_OR_GREATER
            } else if (Vector128.IsHardwareAccelerated) {
                if (false) {
                    // See else.
#if NET8_0_OR_GREATER
                } else if (Vector512.IsHardwareAccelerated && length >= (nuint)Vector512<byte>.Count) {
                    nuint offset = 0;
                    nuint lengthToExamine = length - (nuint)Vector512<byte>.Count;
                    // Unsigned, so it shouldn't have overflowed larger than length (rather than negative)
                    Debug.Assert(lengthToExamine < length);
                    if (lengthToExamine != 0) {
                        do {
                            if (Vector512.LoadUnsafe(ref first, offset) !=
                                Vector512.LoadUnsafe(ref second, offset)) {
                                goto NotEqual;
                            }
                            offset += (nuint)Vector512<byte>.Count;
                        } while (lengthToExamine > offset);
                    }

                    // Do final compare as Vector512<byte>.Count from end rather than start
                    if (Vector512.LoadUnsafe(ref first, lengthToExamine) ==
                        Vector512.LoadUnsafe(ref second, lengthToExamine)) {
                        // C# compiler inverts this test, making the outer goto the conditional jmp.
                        goto Equal;
                    }

                    // This becomes a conditional jmp forward to not favor it.
                    goto NotEqual;
#endif // NET8_0_OR_GREATER
                } else if (Vector256.IsHardwareAccelerated && length >= (nuint)Vector256<byte>.Count) {
                    nuint offset = 0;
                    nuint lengthToExamine = length - (nuint)Vector256<byte>.Count;
                    // Unsigned, so it shouldn't have overflowed larger than length (rather than negative)
                    Debug.Assert(lengthToExamine < length);
                    if (lengthToExamine != 0) {
                        do {
                            if (Vector256.LoadUnsafe(ref first, offset) !=
                                Vector256.LoadUnsafe(ref second, offset)) {
                                goto NotEqual;
                            }
                            offset += (nuint)Vector256<byte>.Count;
                        } while (lengthToExamine > offset);
                    }

                    // Do final compare as Vector256<byte>.Count from end rather than start
                    if (Vector256.LoadUnsafe(ref first, lengthToExamine) ==
                        Vector256.LoadUnsafe(ref second, lengthToExamine)) {
                        // C# compiler inverts this test, making the outer goto the conditional jmp.
                        goto Equal;
                    }

                    // This becomes a conditional jmp forward to not favor it.
                    goto NotEqual;
                } else if (length >= (nuint)Vector128<byte>.Count) {
                    nuint offset = 0;
                    nuint lengthToExamine = length - (nuint)Vector128<byte>.Count;
                    // Unsigned, so it shouldn't have overflowed larger than length (rather than negative)
                    Debug.Assert(lengthToExamine < length);
                    if (lengthToExamine != 0) {
                        do {
                            if (Vector128.LoadUnsafe(ref first, offset) !=
                                Vector128.LoadUnsafe(ref second, offset)) {
                                goto NotEqual;
                            }
                            offset += (nuint)Vector128<byte>.Count;
                        } while (lengthToExamine > offset);
                    }

                    // Do final compare as Vector128<byte>.Count from end rather than start
                    if (Vector128.LoadUnsafe(ref first, lengthToExamine) ==
                        Vector128.LoadUnsafe(ref second, lengthToExamine)) {
                        // C# compiler inverts this test, making the outer goto the conditional jmp.
                        goto Equal;
                    }

                    // This becomes a conditional jmp forward to not favor it.
                    goto NotEqual;
                }
#endif // NET7_0_OR_GREATER
            }

            if (false) {
                // See else.
#if NET7_0_OR_GREATER
            } else if (Vector128.IsHardwareAccelerated && 8 == sizeof(nuint)) { // old: Vector128.IsHardwareAccelerated
                Debug.Assert(length <= (nuint)sizeof(nuint) * 2);

                nuint offset = length - (nuint)sizeof(nuint);
                nuint differentBits = LoadNUInt(ref first) - LoadNUInt(ref second);
                differentBits |= LoadNUInt(ref first, offset) - LoadNUInt(ref second, offset);
                result = (differentBits == 0);
                goto Result;
#endif // NET7_0_OR_GREATER
            } else {
                Debug.Assert(length >= (nuint)sizeof(nuint));
                {
                    nuint offset = 0;
                    nuint lengthToExamine = length - (nuint)sizeof(nuint);
                    // Unsigned, so it shouldn't have overflowed larger than length (rather than negative)
                    Debug.Assert(lengthToExamine < length);
                    if (lengthToExamine > 0) {
                        do {
                            // Compare unsigned so not do a sign extend mov on 64 bit
                            if (LoadNUInt(ref first, offset) != LoadNUInt(ref second, offset)) {
                                goto NotEqual;
                            }
                            offset += (nuint)sizeof(nuint);
                        } while (lengthToExamine > offset);
                    }

                    // Do final compare as sizeof(nuint) from end rather than start
                    result = (LoadNUInt(ref first, lengthToExamine) == LoadNUInt(ref second, lengthToExamine));
                    goto Result;
                }
            }

        // As there are so many true/false exit points the Jit will coalesce them to one location.
        // We want them at the end so the conditional early exit jmps are all jmp forwards so the
        // branch predictor in a uninitialized state will not take them e.g.
        // - loops are conditional jmps backwards and predicted
        // - exceptions are conditional forwards jmps and not predicted
        NotEqual:
            return false;
        }

        public static unsafe int SequenceCompareTo(ref byte first, int firstLength, ref byte second, int secondLength) {
            Debug.Assert(firstLength >= 0);
            Debug.Assert(secondLength >= 0);

            if (Unsafe.AreSame(ref first, ref second))
                goto Equal;

            nuint minLength = (nuint)(((uint)firstLength < (uint)secondLength) ? (uint)firstLength : (uint)secondLength);

            nuint offset = 0; // Use nuint for arithmetic to avoid unnecessary 64->32->64 truncations
            nuint lengthToExamine = minLength;

            if (false) {
                // See else.
#if NET7_0_OR_GREATER
            } else if (Vector256.IsHardwareAccelerated) {
                if (false) {
                    // See else.
#if NET8_0_OR_GREATER
                } else if (Vector512.IsHardwareAccelerated && (lengthToExamine >= (nuint)Vector512<byte>.Count)) {
                    lengthToExamine -= (nuint)Vector512<byte>.Count;
                    ulong matches;
                    while (lengthToExamine > offset) {
                        matches = Vector512.Equals(Vector512.LoadUnsafe(ref first, offset), Vector512.LoadUnsafe(ref second, offset)).ExtractMostSignificantBits();
                        // Note that MoveMask has converted the equal vector elements into a set of bit flags,
                        // So the bit position in 'matches' corresponds to the element offset.

                        // 32 elements in Vector256<byte> so we compare to uint.MaxValue to check if everything matched
                        if (matches == ulong.MaxValue) {
                            // All matched
                            offset += (nuint)Vector512<byte>.Count;
                            continue;
                        }

                        goto Difference;
                    }
                    // Move to Vector length from end for final compare
                    offset = lengthToExamine;
                    // Same as method as above
                    matches = Vector512.Equals(Vector512.LoadUnsafe(ref first, offset), Vector512.LoadUnsafe(ref second, offset)).ExtractMostSignificantBits();
                    if (matches == ulong.MaxValue) {
                        // All matched
                        goto Equal;
                    }
                Difference:
                    // Invert matches to find differences
                    ulong differences = ~matches;
                    // Find bitflag offset of first difference and add to current offset
                    offset += (uint)BitOperations.TrailingZeroCount(differences);

                    int result = Unsafe.AddByteOffset(ref first, offset).CompareTo(Unsafe.AddByteOffset(ref second, offset));
                    Debug.Assert(result != 0);

                    return result;
#endif // NET8_0_OR_GREATER
                }

                if (lengthToExamine >= (nuint)Vector256<byte>.Count) {
                    lengthToExamine -= (nuint)Vector256<byte>.Count;
                    uint matches;
                    while (lengthToExamine > offset) {
                        matches = Vector256.Equals(Vector256.LoadUnsafe(ref first, offset), Vector256.LoadUnsafe(ref second, offset)).ExtractMostSignificantBits();
                        // Note that MoveMask has converted the equal vector elements into a set of bit flags,
                        // So the bit position in 'matches' corresponds to the element offset.

                        // 32 elements in Vector256<byte> so we compare to uint.MaxValue to check if everything matched
                        if (matches == uint.MaxValue) {
                            // All matched
                            offset += (nuint)Vector256<byte>.Count;
                            continue;
                        }

                        goto Difference;
                    }
                    // Move to Vector length from end for final compare
                    offset = lengthToExamine;
                    // Same as method as above
                    matches = Vector256.Equals(Vector256.LoadUnsafe(ref first, offset), Vector256.LoadUnsafe(ref second, offset)).ExtractMostSignificantBits();
                    if (matches == uint.MaxValue) {
                        // All matched
                        goto Equal;
                    }
                Difference:
                    // Invert matches to find differences
                    uint differences = ~matches;
                    // Find bitflag offset of first difference and add to current offset
                    offset += (uint)BitOperations.TrailingZeroCount(differences);

                    int result = Unsafe.AddByteOffset(ref first, offset).CompareTo(Unsafe.AddByteOffset(ref second, offset));
                    Debug.Assert(result != 0);

                    return result;
                }

                if (lengthToExamine >= (nuint)Vector128<byte>.Count) {
                    lengthToExamine -= (nuint)Vector128<byte>.Count;
                    uint matches;
                    if (lengthToExamine > offset) {
                        matches = Vector128.Equals(Vector128.LoadUnsafe(ref first, offset), Vector128.LoadUnsafe(ref second, offset)).ExtractMostSignificantBits();
                        // Note that MoveMask has converted the equal vector elements into a set of bit flags,
                        // So the bit position in 'matches' corresponds to the element offset.

                        // 16 elements in Vector128<byte> so we compare to ushort.MaxValue to check if everything matched
                        if (matches != ushort.MaxValue) {
                            goto Difference;
                        }
                    }
                    // Move to Vector length from end for final compare
                    offset = lengthToExamine;
                    // Same as method as above
                    matches = Vector128.Equals(Vector128.LoadUnsafe(ref first, offset), Vector128.LoadUnsafe(ref second, offset)).ExtractMostSignificantBits();
                    if (matches == ushort.MaxValue) {
                        // All matched
                        goto Equal;
                    }
                Difference:
                    // Invert matches to find differences
                    uint differences = ~matches;
                    // Find bitflag offset of first difference and add to current offset
                    offset += (uint)BitOperations.TrailingZeroCount(differences);

                    int result = Unsafe.AddByteOffset(ref first, offset).CompareTo(Unsafe.AddByteOffset(ref second, offset));
                    Debug.Assert(result != 0);

                    return result;
                }
            } else if (Vector128.IsHardwareAccelerated) {
                if (lengthToExamine >= (nuint)Vector128<byte>.Count) {
                    lengthToExamine -= (nuint)Vector128<byte>.Count;
                    while (lengthToExamine > offset) {
                        if (Vector128.LoadUnsafe(ref first, offset) == Vector128.LoadUnsafe(ref second, offset)) {
                            // All matched
                            offset += (nuint)Vector128<byte>.Count;
                            continue;
                        }

                        goto BytewiseCheck;
                    }
                    // Move to Vector length from end for final compare
                    offset = lengthToExamine;
                    if (Vector128.LoadUnsafe(ref first, offset) == Vector128.LoadUnsafe(ref second, offset)) {
                        // All matched
                        goto Equal;
                    }
                    goto BytewiseCheck;
                }
#endif // NET7_0_OR_GREATER
            }

            if (lengthToExamine > (nuint)sizeof(nuint)) {
                lengthToExamine -= (nuint)sizeof(nuint);
                while (lengthToExamine > offset) {
                    if (LoadNUInt(ref first, offset) != LoadNUInt(ref second, offset)) {
                        goto BytewiseCheck;
                    }
                    offset += (nuint)sizeof(nuint);
                }
            }

        BytewiseCheck:  // Workaround for https://github.com/dotnet/runtime/issues/8795
            while (minLength > offset) {
                int result = SizableUnsafe.AddByteOffset(ref first, offset).CompareTo(SizableUnsafe.AddByteOffset(ref second, offset));
                if (result != 0)
                    return result;
                offset += 1;
            }

        Equal:
            return firstLength - secondLength;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int LocateFirstFoundByte(ulong match)
            => MathBitOperations.TrailingZeroCount(match) >> 3;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int LocateLastFoundByte(ulong match)
            => MathBitOperations.Log2(match) >> 3;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ushort LoadUShort(ref byte start)
            => Unsafe.ReadUnaligned<ushort>(ref start);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint LoadUInt(ref byte start)
            => Unsafe.ReadUnaligned<uint>(ref start);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint LoadUInt(ref byte start, nuint offset)
            => Unsafe.ReadUnaligned<uint>(ref SizableUnsafe.AddByteOffset(ref start, offset));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static nuint LoadNUInt(ref byte start)
            => Unsafe.ReadUnaligned<nuint>(ref start);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static nuint LoadNUInt(ref byte start, nuint offset)
            => Unsafe.ReadUnaligned<nuint>(ref SizableUnsafe.AddByteOffset(ref start, offset));

#if NETCOREAPP3_0_OR_GREATER

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector128<byte> LoadVector128(ref byte start, nuint offset)
            => Unsafe.ReadUnaligned<Vector128<byte>>(ref SizableUnsafe.AddByteOffset(ref start, offset));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector256<byte> LoadVector256(ref byte start, nuint offset)
            => Unsafe.ReadUnaligned<Vector256<byte>>(ref SizableUnsafe.AddByteOffset(ref start, offset));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static nuint GetByteVector128SpanLength(nuint offset, int length)
            => (nuint)(uint)((length - (int)offset) & ~(Vector128<byte>.Count - 1));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static nuint GetByteVector256SpanLength(nuint offset, int length)
            => (nuint)(uint)((length - (int)offset) & ~(Vector256<byte>.Count - 1));

#if NET8_0_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static nuint GetByteVector512SpanLength(nuint offset, int length)
            => (nuint)(uint)((length - (int)offset) & ~(Vector512<byte>.Count - 1));
#endif // NET8_0_OR_GREATER

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe nuint UnalignedCountVector128(byte* searchSpace) {
            nint unaligned = (nint)searchSpace & (Vector128<byte>.Count - 1);
            return (nuint)(uint)((Vector128<byte>.Count - unaligned) & (Vector128<byte>.Count - 1));
        }

#endif // NETCOREAPP3_0_OR_GREATER

    }
}
