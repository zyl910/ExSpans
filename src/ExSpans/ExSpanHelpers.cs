using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
#if NETCOREAPP3_0_OR_GREATER
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
#endif // NETCOREAPP3_0_OR_GREATER
using Zyl.ExSpans.Impl;
using Zyl.ExSpans.Reflection;
using Zyl.VectorTraits;

namespace Zyl.ExSpans {
    /// <summary>
    /// Helper methods of ExSpan (ExSpan 的帮助方法).
    /// </summary>
    internal static partial class ExSpanHelpers {

#if TODO
        public static void ClearWithReferences(ref IntPtr ip, nuint pointerSizeLength) {
            Debug.Assert(Unsafe.IsOpportunisticallyAligned(ref ip, (uint)sizeof(IntPtr)), "Should've been aligned on natural word boundary.");

            // First write backward 8 natural words at a time.
            // Writing backward allows us to get away with only simple modifications to the
            // mov instruction's base and index registers between loop iterations.

            for (; pointerSizeLength >= 8; pointerSizeLength -= 8) {
                Unsafe.Add(ref Unsafe.Add(ref ip, (nint)pointerSizeLength), -1) = default;
                Unsafe.Add(ref Unsafe.Add(ref ip, (nint)pointerSizeLength), -2) = default;
                Unsafe.Add(ref Unsafe.Add(ref ip, (nint)pointerSizeLength), -3) = default;
                Unsafe.Add(ref Unsafe.Add(ref ip, (nint)pointerSizeLength), -4) = default;
                Unsafe.Add(ref Unsafe.Add(ref ip, (nint)pointerSizeLength), -5) = default;
                Unsafe.Add(ref Unsafe.Add(ref ip, (nint)pointerSizeLength), -6) = default;
                Unsafe.Add(ref Unsafe.Add(ref ip, (nint)pointerSizeLength), -7) = default;
                Unsafe.Add(ref Unsafe.Add(ref ip, (nint)pointerSizeLength), -8) = default;
            }

            Debug.Assert(pointerSizeLength <= 7);

            // The logic below works by trying to minimize the number of branches taken for any
            // given range of lengths. For example, the lengths [ 4 .. 7 ] are handled by a single
            // branch, [ 2 .. 3 ] are handled by a single branch, and [ 1 ] is handled by a single
            // branch.
            //
            // We can write both forward and backward as a perf improvement. For example,
            // the lengths [ 4 .. 7 ] can be handled by zeroing out the first four natural
            // words and the last 3 natural words. In the best case (length = 7), there are
            // no overlapping writes. In the worst case (length = 4), there are three
            // overlapping writes near the middle of the buffer. In perf testing, the
            // penalty for performing duplicate writes is less expensive than the penalty
            // for complex branching.

            if (pointerSizeLength >= 4) {
                goto Write4To7;
            } else if (pointerSizeLength >= 2) {
                goto Write2To3;
            } else if (pointerSizeLength > 0) {
                goto Write1;
            } else {
                return; // nothing to write
            }

        Write4To7:
            Debug.Assert(pointerSizeLength >= 4);

            // Write first four and last three.
            Unsafe.Add(ref ip, 2) = default;
            Unsafe.Add(ref ip, 3) = default;
            Unsafe.Add(ref Unsafe.Add(ref ip, (nint)pointerSizeLength), -3) = default;
            Unsafe.Add(ref Unsafe.Add(ref ip, (nint)pointerSizeLength), -2) = default;

        Write2To3:
            Debug.Assert(pointerSizeLength >= 2);

            // Write first two and last one.
            Unsafe.Add(ref ip, 1) = default;
            Unsafe.Add(ref Unsafe.Add(ref ip, (nint)pointerSizeLength), -1) = default;

        Write1:
            Debug.Assert(pointerSizeLength >= 1);

            // Write only element.
            ip = default;
        }
#endif // (TODO)

        public static void Reverse(ref int buf, nuint length) {
            Debug.Assert(length > 1);

            nint remainder = (nint)length;
            nint offset = 0;

            if (false) {
#if NET8_0_OR_GREATER
            } else if (Vector512.IsHardwareAccelerated && remainder >= Vector512<int>.Count * 2 && Vector512<byte>.Count > Vector<byte>.Count) {
                nint lastOffset = remainder - Vector512<int>.Count;
                do {
                    // Load in values from beginning and end of the array.
                    Vector512<int> tempFirst = Vector512.LoadUnsafe(ref buf, (nuint)offset);
                    Vector512<int> tempLast = Vector512.LoadUnsafe(ref buf, (nuint)lastOffset);

                    // Shuffle to reverse each vector:
                    //     +---------------+
                    //     | A | B | C | D |
                    //     +---------------+
                    //          --->
                    //     +---------------+
                    //     | D | C | B | A |
                    //     +---------------+
                    tempFirst = Vector512.Shuffle(tempFirst, Vector512.Create(15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0));
                    tempLast = Vector512.Shuffle(tempLast, Vector512.Create(15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0));

                    // Store the reversed vectors
                    tempLast.StoreUnsafe(ref buf, (nuint)offset);
                    tempFirst.StoreUnsafe(ref buf, (nuint)lastOffset);

                    offset += Vector512<int>.Count;
                    lastOffset -= Vector512<int>.Count;
                } while (lastOffset >= offset);

                remainder = lastOffset + Vector512<int>.Count - offset;
#endif // NET8_0_OR_GREATER
            } else if (Vector.IsHardwareAccelerated && remainder > Vector<int>.Count && Vectors.YShuffleKernel_AcceleratedTypes.HasFlag(TypeCodeFlags.Int32)) {
                int vectorSize = Vector<int>.Count;
                nint lastOffset = remainder - vectorSize;
                Vector<int> reverseMask = Vectors<int>.SerialDesc;
                Vectors.YShuffleKernel_Args(reverseMask, out var args0, out var args1);
                do {
                    // Load the values into vectors
                    Vector<int> tempFirst = VectorHelper.LoadUnsafe(ref buf, (nuint)offset);
                    Vector<int> tempLast = VectorHelper.LoadUnsafe(ref buf, (nuint)lastOffset);
                    // Shuffle to reverse each vector.
                    //tempFirst = Vectors.YShuffleKernel(tempFirst, reverseMask);
                    //tempLast = Vectors.YShuffleKernel(tempLast, reverseMask);
                    tempFirst = Vectors.YShuffleKernel_Core(tempFirst, args0, args1);
                    tempLast = Vectors.YShuffleKernel_Core(tempLast, args0, args1);
                    // Store the reversed vectors
                    tempLast.StoreUnsafe(ref buf, (nuint)offset);
                    tempFirst.StoreUnsafe(ref buf, (nuint)lastOffset);
                    // Next
                    offset += vectorSize;
                    lastOffset -= vectorSize;
                } while (lastOffset >= offset);
                remainder = lastOffset + vectorSize - offset;
#if NET7_0_OR_GREATER
            } else if (Avx2.IsSupported && remainder >= Vector256<int>.Count * 2) {
                nint lastOffset = remainder - Vector256<int>.Count;
                do {
                    // Load the values into vectors
                    Vector256<int> tempFirst = Vector256.LoadUnsafe(ref buf, (nuint)offset);
                    Vector256<int> tempLast = Vector256.LoadUnsafe(ref buf, (nuint)lastOffset);

                    // Permute to reverse each vector:
                    //     +-------------------------------+
                    //     | A | B | C | D | E | F | G | H |
                    //     +-------------------------------+
                    //         --->
                    //     +-------------------------------+
                    //     | H | G | F | E | D | C | B | A |
                    //     +-------------------------------+
                    tempFirst = Avx2.PermuteVar8x32(tempFirst, Vector256.Create(7, 6, 5, 4, 3, 2, 1, 0));
                    tempLast = Avx2.PermuteVar8x32(tempLast, Vector256.Create(7, 6, 5, 4, 3, 2, 1, 0));

                    // Store the reversed vectors
                    tempLast.StoreUnsafe(ref buf, (nuint)offset);
                    tempFirst.StoreUnsafe(ref buf, (nuint)lastOffset);

                    offset += Vector256<int>.Count;
                    lastOffset -= Vector256<int>.Count;
                } while (lastOffset >= offset);

                remainder = lastOffset + Vector256<int>.Count - offset;
            } else if (Vector128.IsHardwareAccelerated && remainder >= Vector128<int>.Count * 2) {
                nint lastOffset = remainder - Vector128<int>.Count;
                do {
                    // Load in values from beginning and end of the array.
                    Vector128<int> tempFirst = Vector128.LoadUnsafe(ref buf, (nuint)offset);
                    Vector128<int> tempLast = Vector128.LoadUnsafe(ref buf, (nuint)lastOffset);

                    // Shuffle to reverse each vector:
                    //     +---------------+
                    //     | A | B | C | D |
                    //     +---------------+
                    //          --->
                    //     +---------------+
                    //     | D | C | B | A |
                    //     +---------------+
                    tempFirst = Vector128.Shuffle(tempFirst, Vector128.Create(3, 2, 1, 0));
                    tempLast = Vector128.Shuffle(tempLast, Vector128.Create(3, 2, 1, 0));

                    // Store the reversed vectors
                    tempLast.StoreUnsafe(ref buf, (nuint)offset);
                    tempFirst.StoreUnsafe(ref buf, (nuint)lastOffset);

                    offset += Vector128<int>.Count;
                    lastOffset -= Vector128<int>.Count;
                } while (lastOffset >= offset);

                remainder = lastOffset + Vector128<int>.Count - offset;
#endif // NET7_0_OR_GREATER
            }

            // Store any remaining values one-by-one
            if (remainder > 1) {
                ReverseInner(ref Unsafe.Add(ref buf, offset), (nuint)remainder);
            }
        }

        public static void Reverse(ref long buf, nuint length) {
            Debug.Assert(length > 1);

            nint remainder = (nint)length;
            nint offset = 0;

            if (false) {
#if NET8_0_OR_GREATER
            } else if (Vector512.IsHardwareAccelerated && remainder >= Vector512<long>.Count * 2 && Vector512<byte>.Count > Vector<byte>.Count) {
                nint lastOffset = remainder - Vector512<long>.Count;
                do {
                    // Load in values from beginning and end of the array.
                    Vector512<long> tempFirst = Vector512.LoadUnsafe(ref buf, (nuint)offset);
                    Vector512<long> tempLast = Vector512.LoadUnsafe(ref buf, (nuint)lastOffset);

                    // Shuffle to reverse each vector:
                    //     +-------+
                    //     | A | B |
                    //     +-------+
                    //          --->
                    //     +-------+
                    //     | B | A |
                    //     +-------+
                    tempFirst = Vector512.Shuffle(tempFirst, Vector512.Create(7, 6, 5, 4, 3, 2, 1, 0));
                    tempLast = Vector512.Shuffle(tempLast, Vector512.Create(7, 6, 5, 4, 3, 2, 1, 0));

                    // Store the reversed vectors
                    tempLast.StoreUnsafe(ref buf, (nuint)offset);
                    tempFirst.StoreUnsafe(ref buf, (nuint)lastOffset);

                    offset += Vector512<long>.Count;
                    lastOffset -= Vector512<long>.Count;
                } while (lastOffset >= offset);

                remainder = lastOffset + Vector512<long>.Count - offset;
#endif // NET8_0_OR_GREATER
            } else if (Vector.IsHardwareAccelerated && remainder > Vector<long>.Count && Vectors.YShuffleKernel_AcceleratedTypes.HasFlag(TypeCodeFlags.Int64)) {
                int vectorSize = Vector<long>.Count;
                nint lastOffset = remainder - vectorSize;
                Vector<long> reverseMask = Vectors<long>.SerialDesc;
                Vectors.YShuffleKernel_Args(reverseMask, out var args0, out var args1);
                do {
                    // Load the values into vectors
                    Vector<long> tempFirst = VectorHelper.LoadUnsafe(ref buf, (nuint)offset);
                    Vector<long> tempLast = VectorHelper.LoadUnsafe(ref buf, (nuint)lastOffset);
                    // Shuffle to reverse each vector.
                    //tempFirst = Vectors.YShuffleKernel(tempFirst, reverseMask);
                    //tempLast = Vectors.YShuffleKernel(tempLast, reverseMask);
                    tempFirst = Vectors.YShuffleKernel_Core(tempFirst, args0, args1);
                    tempLast = Vectors.YShuffleKernel_Core(tempLast, args0, args1);
                    // Store the reversed vectors
                    tempLast.StoreUnsafe(ref buf, (nuint)offset);
                    tempFirst.StoreUnsafe(ref buf, (nuint)lastOffset);
                    // Next
                    offset += vectorSize;
                    lastOffset -= vectorSize;
                } while (lastOffset >= offset);
                remainder = lastOffset + vectorSize - offset;
#if NET7_0_OR_GREATER
            } else if (Avx2.IsSupported && remainder >= Vector256<long>.Count * 2) {
                nint lastOffset = remainder - Vector256<long>.Count;
                do {
                    // Load the values into vectors
                    Vector256<long> tempFirst = Vector256.LoadUnsafe(ref buf, (nuint)offset);
                    Vector256<long> tempLast = Vector256.LoadUnsafe(ref buf, (nuint)lastOffset);

                    // Permute to reverse each vector:
                    //     +---------------+
                    //     | A | B | C | D |
                    //     +---------------+
                    //         --->
                    //     +---------------+
                    //     | D | C | B | A |
                    //     +---------------+
                    tempFirst = Avx2.Permute4x64(tempFirst, 0b00_01_10_11);
                    tempLast = Avx2.Permute4x64(tempLast, 0b00_01_10_11);

                    // Store the reversed vectors
                    tempLast.StoreUnsafe(ref buf, (nuint)offset);
                    tempFirst.StoreUnsafe(ref buf, (nuint)lastOffset);

                    offset += Vector256<long>.Count;
                    lastOffset -= Vector256<long>.Count;
                } while (lastOffset >= offset);

                remainder = lastOffset + Vector256<long>.Count - offset;
            } else if (Vector128.IsHardwareAccelerated && remainder >= Vector128<long>.Count * 2) {
                nint lastOffset = remainder - Vector128<long>.Count;
                do {
                    // Load in values from beginning and end of the array.
                    Vector128<long> tempFirst = Vector128.LoadUnsafe(ref buf, (nuint)offset);
                    Vector128<long> tempLast = Vector128.LoadUnsafe(ref buf, (nuint)lastOffset);

                    // Shuffle to reverse each vector:
                    //     +-------+
                    //     | A | B |
                    //     +-------+
                    //          --->
                    //     +-------+
                    //     | B | A |
                    //     +-------+
                    tempFirst = Vector128.Shuffle(tempFirst, Vector128.Create(1, 0));
                    tempLast = Vector128.Shuffle(tempLast, Vector128.Create(1, 0));

                    // Store the reversed vectors
                    tempLast.StoreUnsafe(ref buf, (nuint)offset);
                    tempFirst.StoreUnsafe(ref buf, (nuint)lastOffset);

                    offset += Vector128<long>.Count;
                    lastOffset -= Vector128<long>.Count;
                } while (lastOffset >= offset);

                remainder = lastOffset + Vector128<long>.Count - offset;
#endif // NET7_0_OR_GREATER
            }

            // Store any remaining values one-by-one
            if (remainder > 1) {
                ReverseInner(ref Unsafe.Add(ref buf, offset), (nuint)remainder);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Reverse<T>(ref T elements, nuint length) {
            Debug.Assert(length > 1);

            if (!TypeHelper.IsReferenceOrContainsReferences<T>()) {
                if (Unsafe.SizeOf<T>() == sizeof(byte)) {
                    Reverse(ref Unsafe.As<T, byte>(ref elements), length);
                    return;
                } else if (Unsafe.SizeOf<T>() == sizeof(char)) {
                    Reverse(ref Unsafe.As<T, char>(ref elements), length);
                    return;
                } else if (Unsafe.SizeOf<T>() == sizeof(int)) {
                    Reverse(ref Unsafe.As<T, int>(ref elements), length);
                    return;
                } else if (Unsafe.SizeOf<T>() == sizeof(long)) {
                    Reverse(ref Unsafe.As<T, long>(ref elements), length);
                    return;
                }
            }

            ReverseInner(ref elements, length);
        }
#if TODO
#endif // (TODO)

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ReverseInner<T>(ref T elements, nuint length) {
            Debug.Assert(length > 1);

            ref T first = ref elements;
            ref T last = ref Unsafe.Subtract(ref ExUnsafe.Add(ref first, length), 1);
            do {
                T temp = first;
                first = last;
                last = temp;
                first = ref Unsafe.Add(ref first, 1);
                last = ref Unsafe.Subtract(ref last, 1);
            } while (Unsafe.IsAddressLessThan(ref first, ref last));
        }

    }
}
