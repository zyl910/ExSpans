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

namespace Zyl.SizableSpans {
    partial class SizableSpanHelpers {

        // Unrolled for small sizes
        internal static unsafe void Fill<T>(ref T refData, nuint numElements, T value) {
            // Early checks to see if it's even possible to vectorize - JIT will turn these checks into consts.
            // - T cannot contain references (GC can't track references in vectors)
            // - Vectorization must be hardware-accelerated
            // - T's size must not exceed the vector's size
            // - T's size must be a whole power of 2

            if (!TypeHelper.IsBlittable<T>()) { goto CannotVectorize; }
            if (!Vector.IsHardwareAccelerated) { goto CannotVectorize; }
            if (Unsafe.SizeOf<T>() > Vector<byte>.Count) { goto CannotVectorize; }
            if (!BitOperationsHelper.IsPow2(Unsafe.SizeOf<T>())) { goto CannotVectorize; }

            if (numElements >= (uint)(Vector<byte>.Count / Unsafe.SizeOf<T>())) {
                // We have enough data for at least one vectorized write.

                T tmp = value; // Avoid taking address of the "value" argument. It would regress performance of the loops below.
                Vector<byte> vector;

                if (Unsafe.SizeOf<T>() == 1) {
                    vector = new Vector<byte>(Unsafe.As<T, byte>(ref tmp));
                } else if (Unsafe.SizeOf<T>() == 2) {
                    vector = (Vector<byte>)(new Vector<ushort>(Unsafe.As<T, ushort>(ref tmp)));
                } else if (Unsafe.SizeOf<T>() == 4) {
                    // special-case float since it's already passed in a SIMD reg
                    vector = (typeof(T) == typeof(float))
                        ? (Vector<byte>)(new Vector<float>((float)(object)tmp!))
                        : (Vector<byte>)(new Vector<uint>(Unsafe.As<T, uint>(ref tmp)));
                } else if (Unsafe.SizeOf<T>() == 8) {
                    // special-case double since it's already passed in a SIMD reg
                    vector = (typeof(T) == typeof(double))
                        ? (Vector<byte>)(new Vector<double>((double)(object)tmp!))
                        : (Vector<byte>)(new Vector<ulong>(Unsafe.As<T, ulong>(ref tmp)));
#if NETCOREAPP3_0_OR_GREATER
                } else if (Unsafe.SizeOf<T>() == 16) {
                    Vector128<byte> vec128 = Unsafe.As<T, Vector128<byte>>(ref tmp);
                    if (Vector<byte>.Count == 16) {
                        vector = vec128.AsVector();
                    } else if (Vector<byte>.Count == 32) {
                        vector = Vector256.Create(vec128, vec128).AsVector();
#if NET8_0_OR_GREATER
                    } else if (Vector<byte>.Count == 64) {
                        var vec256 = Vector256.Create(vec128, vec128);
                        vector = Vector512.Create(vec256, vec256).AsVector();
#endif // NET8_0_OR_GREATER
                    } else {
                        Debug.Fail("Vector<T> is unexpected size.");
                        goto CannotVectorize;
                    }
                } else if (Unsafe.SizeOf<T>() == 32) {
                    Vector256<byte> vec256 = Unsafe.As<T, Vector256<byte>>(ref tmp);
                    if (Vector<byte>.Count == 32) {
                        vector = vec256.AsVector();
#if NET8_0_OR_GREATER
                    } else if (Vector<byte>.Count == 64) {
                        vector = Vector512.Create(vec256, vec256).AsVector();
#endif // NET8_0_OR_GREATER
                    } else {
                        Debug.Fail("Vector<T> is unexpected size.");
                        goto CannotVectorize;
                    }
#if NET8_0_OR_GREATER
                } else if (Unsafe.SizeOf<T>() == 64) {
                    if (Vector<byte>.Count == 64) {
                        vector = Unsafe.As<T, Vector512<byte>>(ref tmp).AsVector();
                    } else {
                        Debug.Fail("Vector<T> is unexpected size.");
                        goto CannotVectorize;
                    }
#endif // NET8_0_OR_GREATER
#else
                } else if (Unsafe.SizeOf<T>() == Vector<byte>.Count) {
                    vector = Unsafe.As<T, Vector<byte>>(ref tmp);
#endif // NETCOREAPP3_0_OR_GREATER
                } else {
                    DebugHelper.Fail("Vector<T> is greater than 512 bits in size?");
                    goto CannotVectorize;
                }

                ref byte refDataAsBytes = ref Unsafe.As<T, byte>(ref refData);
                nuint totalByteLength = numElements * (nuint)Unsafe.SizeOf<T>(); // get this calculation ready ahead of time
                nuint stopLoopAtOffset = totalByteLength & (nuint)(nint)(2 * (int)-Vector<byte>.Count); // intentional sign extension carries the negative bit
                nuint offset = 0;

                // Loop, writing 2 vectors at a time.
                // Compare 'numElements' rather than 'stopLoopAtOffset' because we don't want a dependency
                // on the very recently calculated 'stopLoopAtOffset' value.

                if (numElements >= (uint)(2 * Vector<byte>.Count / Unsafe.SizeOf<T>())) {
                    do {
                        Unsafe.WriteUnaligned(ref SizableUnsafe.AddByteOffset(ref refDataAsBytes, offset), vector);
                        Unsafe.WriteUnaligned(ref SizableUnsafe.AddByteOffset(ref refDataAsBytes, offset + (nuint)Vector<byte>.Count), vector);
                        offset += (uint)(2 * Vector<byte>.Count);
                    } while (offset < stopLoopAtOffset);
                }

                // At this point, if any data remains to be written, it's strictly less than
                // 2 * sizeof(Vector) bytes. The loop above had us write an even number of vectors.
                // If the total byte length instead involves us writing an odd number of vectors, write
                // one additional vector now. The bit check below tells us if we're in an "odd vector
                // count" situation.

                if ((totalByteLength & (nuint)Vector<byte>.Count) != 0) {
                    Unsafe.WriteUnaligned(ref SizableUnsafe.AddByteOffset(ref refDataAsBytes, offset), vector);
                }

                // It's possible that some small buffer remains to be populated - something that won't
                // fit an entire vector's worth of data. Instead of falling back to a loop, we'll write
                // a vector at the very end of the buffer. This may involve overwriting previously
                // populated data, which is fine since we're splatting the same value for all entries.
                // There's no need to perform a length check here because we already performed this
                // check before entering the vectorized code path.

                Unsafe.WriteUnaligned(ref SizableUnsafe.AddByteOffset(ref refDataAsBytes, totalByteLength - (nuint)Vector<byte>.Count), vector);

                // And we're done!

                return;
            }

        CannotVectorize:

            // If we reached this point, we cannot vectorize this T, or there are too few
            // elements for us to vectorize. Fall back to an unrolled loop.

            nuint i = 0;

            // Write 8 elements at a time

            if (numElements >= 8) {
                nuint stopLoopAtOffset = numElements & ~(nuint)7;
                do {
                    Unsafe.Add(ref refData, (nint)i + 0) = value;
                    Unsafe.Add(ref refData, (nint)i + 1) = value;
                    Unsafe.Add(ref refData, (nint)i + 2) = value;
                    Unsafe.Add(ref refData, (nint)i + 3) = value;
                    Unsafe.Add(ref refData, (nint)i + 4) = value;
                    Unsafe.Add(ref refData, (nint)i + 5) = value;
                    Unsafe.Add(ref refData, (nint)i + 6) = value;
                    Unsafe.Add(ref refData, (nint)i + 7) = value;
                } while ((i += 8) < stopLoopAtOffset);
            }

            // Write next 4 elements if needed

            if ((numElements & 4) != 0) {
                Unsafe.Add(ref refData, (nint)i + 0) = value;
                Unsafe.Add(ref refData, (nint)i + 1) = value;
                Unsafe.Add(ref refData, (nint)i + 2) = value;
                Unsafe.Add(ref refData, (nint)i + 3) = value;
                i += 4;
            }

            // Write next 2 elements if needed

            if ((numElements & 2) != 0) {
                Unsafe.Add(ref refData, (nint)i + 0) = value;
                Unsafe.Add(ref refData, (nint)i + 1) = value;
                i += 2;
            }

            // Write final element if needed

            if ((numElements & 1) != 0) {
                Unsafe.Add(ref refData, (nint)i) = value;
            }
        }

    }
}
