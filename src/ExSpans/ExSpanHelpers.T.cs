﻿#if NET7_0_OR_GREATER
#define GENERIC_MATH // C# 11 - Generic math support. https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-11#generic-math-support
#endif // NET7_0_OR_GREATER

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#if NETCOREAPP3_0_OR_GREATER
using System.Runtime.Intrinsics;
#endif // NETCOREAPP3_0_OR_GREATER
using System.Text;
using Zyl.ExSpans.Extensions;
using Zyl.ExSpans.Impl;
using Zyl.ExSpans.Reflection;
using Zyl.VectorTraits;
using Zyl.VectorTraits.Extensions;
using Zyl.VectorTraits.Numerics;

namespace Zyl.ExSpans {
    partial class ExSpanHelpers {

        // Unrolled for small sizes
        internal static void Fill<T>(ref T refData, nuint numElements, T value) {
            // Early checks to see if it's even possible to vectorize - JIT will turn these checks into consts.
            // - T cannot contain references (GC can't track references in vectors)
            // - Vectorization must be hardware-accelerated
            // - T's size must not exceed the vector's size
            // - T's size must be a whole power of 2

            if (!TypeHelper.IsBlittable<T>()) { goto CannotVectorize; }
            if (!Vector.IsHardwareAccelerated) { goto CannotVectorize; }
            if (Unsafe.SizeOf<T>() > Vector<byte>.Count) { goto CannotVectorize; }
            if (!MathBitOperations.IsPow2(Unsafe.SizeOf<T>())) { goto CannotVectorize; }

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
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
                } else if (Unsafe.SizeOf<T>() <= Vector<byte>.Count) {
                    ReadOnlySpan<byte> spanTmp = MemoryMarshal.CreateReadOnlySpan<byte>(ref Unsafe.As<T, byte>(ref tmp), Unsafe.SizeOf<T>());
                    vector = Vectors.CreateRotate(spanTmp);
#endif // NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
                } else {
                    DebugHelper.Fail(string.Format("Vector<{0}> is greater than 512 bits in size?", typeof(T).FullName));
                    //Debug.WriteLine(string.Format("Vector<{0}> is greater than 512 bits in size?", typeof(T).FullName));
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
                        Unsafe.WriteUnaligned(ref ExUnsafe.AddByteOffset(ref refDataAsBytes, offset), vector);
                        Unsafe.WriteUnaligned(ref ExUnsafe.AddByteOffset(ref refDataAsBytes, offset + (nuint)Vector<byte>.Count), vector);
                        offset += (uint)(2 * Vector<byte>.Count);
                    } while (offset < stopLoopAtOffset);
                }

                // At this point, if any data remains to be written, it's strictly less than
                // 2 * sizeof(Vector) bytes. The loop above had us write an even number of vectors.
                // If the total byte length instead involves us writing an odd number of vectors, write
                // one additional vector now. The bit check below tells us if we're in an "odd vector
                // count" situation.

                if ((totalByteLength & (nuint)Vector<byte>.Count) != 0) {
                    Unsafe.WriteUnaligned(ref ExUnsafe.AddByteOffset(ref refDataAsBytes, offset), vector);
                }

                // It's possible that some small buffer remains to be populated - something that won't
                // fit an entire vector's worth of data. Instead of falling back to a loop, we'll write
                // a vector at the very end of the buffer. This may involve overwriting previously
                // populated data, which is fine since we're splatting the same value for all entries.
                // There's no need to perform a length check here because we already performed this
                // check before entering the vectorized code path.

                Unsafe.WriteUnaligned(ref ExUnsafe.AddByteOffset(ref refDataAsBytes, totalByteLength - (nuint)Vector<byte>.Count), vector);

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
                    ExUnsafe.Add(ref refData, (nint)i + 0) = value;
                    ExUnsafe.Add(ref refData, (nint)i + 1) = value;
                    ExUnsafe.Add(ref refData, (nint)i + 2) = value;
                    ExUnsafe.Add(ref refData, (nint)i + 3) = value;
                    ExUnsafe.Add(ref refData, (nint)i + 4) = value;
                    ExUnsafe.Add(ref refData, (nint)i + 5) = value;
                    ExUnsafe.Add(ref refData, (nint)i + 6) = value;
                    ExUnsafe.Add(ref refData, (nint)i + 7) = value;
                } while ((i += 8) < stopLoopAtOffset);
            }

            // Write next 4 elements if needed

            if ((numElements & 4) != 0) {
                ExUnsafe.Add(ref refData, (nint)i + 0) = value;
                ExUnsafe.Add(ref refData, (nint)i + 1) = value;
                ExUnsafe.Add(ref refData, (nint)i + 2) = value;
                ExUnsafe.Add(ref refData, (nint)i + 3) = value;
                i += 4;
            }

            // Write next 2 elements if needed

            if ((numElements & 2) != 0) {
                ExUnsafe.Add(ref refData, (nint)i + 0) = value;
                ExUnsafe.Add(ref refData, (nint)i + 1) = value;
                i += 2;
            }

            // Write final element if needed

            if ((numElements & 1) != 0) {
                ExUnsafe.Add(ref refData, (nint)i) = value;
            }
        }

        public static nint IndexOf<T>(ref T searchSpace, TSize searchSpaceLength, ref T value, nint valueLength) where T : IEquatable<T>? {
            Debug.Assert(searchSpaceLength >= 0);
            Debug.Assert(valueLength >= 0);

            if (valueLength == 0)
                return 0;  // A zero-length sequence is always treated as "found" at the start of the search space.

            T valueHead = value;
            ref T valueTail = ref ExUnsafe.Add(ref value, 1);
            nint valueTailLength = valueLength - 1;

            nint index = 0;
            while (true) {
                Debug.Assert(0 <= index && index <= searchSpaceLength); // Ensures no deceptive underflows in the computation of "remainingSearchSpaceLength".
                nint remainingSearchSpaceLength = searchSpaceLength - index - valueTailLength;
                if (remainingSearchSpaceLength <= 0)
                    break;  // The unsearched portion is now shorter than the sequence we're looking for. So it can't be there.

                // Do a quick search for the first element of "value".
                nint relativeIndex = IndexOf(ref ExUnsafe.Add(ref searchSpace, index), valueHead, remainingSearchSpaceLength);
                if (relativeIndex < 0)
                    break;
                index += relativeIndex;

                // Found the first element of "value". See if the tail matches.
                if (SequenceEqual(ref ExUnsafe.Add(ref searchSpace, index + 1), ref valueTail, valueTailLength.ToUIntPtr()))
                    return index;  // The tail matched. Return a successful find.

                index++;
            }
            return -1;
        }

        // Adapted from IndexOf(...)
        public static bool Contains<T>(ref T searchSpace, T value, TSize length) where T : IEquatable<T>? {
            Debug.Assert(length >= 0);

            nint index = 0; // Use nint for arithmetic to avoid unnecessary 64->32->64 truncations

            if (default(T) != null || (object?)value != null) {
                Debug.Assert(value is not null);

                while (length >= 8) {
                    length -= 8;

                    if (value!.Equals(ExUnsafe.Add(ref searchSpace, index + 0)) ||
                        value.Equals(ExUnsafe.Add(ref searchSpace, index + 1)) ||
                        value.Equals(ExUnsafe.Add(ref searchSpace, index + 2)) ||
                        value.Equals(ExUnsafe.Add(ref searchSpace, index + 3)) ||
                        value.Equals(ExUnsafe.Add(ref searchSpace, index + 4)) ||
                        value.Equals(ExUnsafe.Add(ref searchSpace, index + 5)) ||
                        value.Equals(ExUnsafe.Add(ref searchSpace, index + 6)) ||
                        value.Equals(ExUnsafe.Add(ref searchSpace, index + 7))) {
                        goto Found;
                    }

                    index += 8;
                }

                if (length >= 4) {
                    length -= 4;

                    if (value!.Equals(ExUnsafe.Add(ref searchSpace, index + 0)) ||
                        value.Equals(ExUnsafe.Add(ref searchSpace, index + 1)) ||
                        value.Equals(ExUnsafe.Add(ref searchSpace, index + 2)) ||
                        value.Equals(ExUnsafe.Add(ref searchSpace, index + 3))) {
                        goto Found;
                    }

                    index += 4;
                }

                while (length > 0) {
                    length--;

                    if (value!.Equals(ExUnsafe.Add(ref searchSpace, index)))
                        goto Found;

                    index += 1;
                }
            } else {
                nint len = length;
                for (index = 0; index < len; index++) {
                    if ((object?)ExUnsafe.Add(ref searchSpace, index) is null) {
                        goto Found;
                    }
                }
            }

            return false;

        Found:
            return true;
        }

        public static nint IndexOf<T>(ref T searchSpace, T value, TSize length) where T : IEquatable<T>? {
            Debug.Assert(length >= 0);

            nint index = 0; // Use nint for arithmetic to avoid unnecessary 64->32->64 truncations
            if (default(T) != null || (object?)value != null) {
                Debug.Assert(value is not null);

                while (length >= 8) {
                    length -= 8;

                    if (value!.Equals(ExUnsafe.Add(ref searchSpace, index)))
                        goto Found;
                    if (value.Equals(ExUnsafe.Add(ref searchSpace, index + 1)))
                        goto Found1;
                    if (value.Equals(ExUnsafe.Add(ref searchSpace, index + 2)))
                        goto Found2;
                    if (value.Equals(ExUnsafe.Add(ref searchSpace, index + 3)))
                        goto Found3;
                    if (value.Equals(ExUnsafe.Add(ref searchSpace, index + 4)))
                        goto Found4;
                    if (value.Equals(ExUnsafe.Add(ref searchSpace, index + 5)))
                        goto Found5;
                    if (value.Equals(ExUnsafe.Add(ref searchSpace, index + 6)))
                        goto Found6;
                    if (value.Equals(ExUnsafe.Add(ref searchSpace, index + 7)))
                        goto Found7;

                    index += 8;
                }

                if (length >= 4) {
                    length -= 4;

                    if (value!.Equals(ExUnsafe.Add(ref searchSpace, index)))
                        goto Found;
                    if (value.Equals(ExUnsafe.Add(ref searchSpace, index + 1)))
                        goto Found1;
                    if (value.Equals(ExUnsafe.Add(ref searchSpace, index + 2)))
                        goto Found2;
                    if (value.Equals(ExUnsafe.Add(ref searchSpace, index + 3)))
                        goto Found3;

                    index += 4;
                }

                while (length > 0) {
                    if (value!.Equals(ExUnsafe.Add(ref searchSpace, index)))
                        goto Found;

                    index += 1;
                    length--;
                }
            } else {
                nint len = (nint)length;
                for (index = 0; index < len; index++) {
                    if ((object?)ExUnsafe.Add(ref searchSpace, index) is null) {
                        goto Found;
                    }
                }
            }
            return -1;

        Found: // Workaround for https://github.com/dotnet/runtime/issues/8795
            return index;
        Found1:
            return index + 1;
        Found2:
            return index + 2;
        Found3:
            return index + 3;
        Found4:
            return index + 4;
        Found5:
            return index + 5;
        Found6:
            return index + 6;
        Found7:
            return index + 7;
        }

        public static nint IndexOfAny<T>(ref T searchSpace, T value0, T value1, TSize length) where T : IEquatable<T>? {
            Debug.Assert(length >= 0);

            T lookUp;
            nint index = 0;
            if (default(T) != null || ((object?)value0 != null && (object?)value1 != null)) {
                Debug.Assert(value0 is not null && value1 is not null);
                if (value0 is null) throw new ArgumentNullException(nameof(value0));
                if (value1 is null) throw new ArgumentNullException(nameof(value1));

                while ((length - index) >= 8) {
                    lookUp = ExUnsafe.Add(ref searchSpace, index);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp))
                        goto Found;
                    lookUp = ExUnsafe.Add(ref searchSpace, index + 1);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp))
                        goto Found1;
                    lookUp = ExUnsafe.Add(ref searchSpace, index + 2);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp))
                        goto Found2;
                    lookUp = ExUnsafe.Add(ref searchSpace, index + 3);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp))
                        goto Found3;
                    lookUp = ExUnsafe.Add(ref searchSpace, index + 4);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp))
                        goto Found4;
                    lookUp = ExUnsafe.Add(ref searchSpace, index + 5);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp))
                        goto Found5;
                    lookUp = ExUnsafe.Add(ref searchSpace, index + 6);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp))
                        goto Found6;
                    lookUp = ExUnsafe.Add(ref searchSpace, index + 7);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp))
                        goto Found7;

                    index += 8;
                }

                if ((length - index) >= 4) {
                    lookUp = ExUnsafe.Add(ref searchSpace, index);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp))
                        goto Found;
                    lookUp = ExUnsafe.Add(ref searchSpace, index + 1);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp))
                        goto Found1;
                    lookUp = ExUnsafe.Add(ref searchSpace, index + 2);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp))
                        goto Found2;
                    lookUp = ExUnsafe.Add(ref searchSpace, index + 3);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp))
                        goto Found3;

                    index += 4;
                }

                while (index < length) {
                    lookUp = ExUnsafe.Add(ref searchSpace, index);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp))
                        goto Found;

                    index++;
                }
            } else {
                for (index = 0; index < length; index++) {
                    lookUp = ExUnsafe.Add(ref searchSpace, index);
                    if ((object?)lookUp is null) {
                        if ((object?)value0 is null || (object?)value1 is null) {
                            goto Found;
                        }
                    } else if (lookUp.Equals(value0) || lookUp.Equals(value1)) {
                        goto Found;
                    }
                }
            }

            return -1;

        Found: // Workaround for https://github.com/dotnet/runtime/issues/8795
            return index;
        Found1:
            return index + 1;
        Found2:
            return index + 2;
        Found3:
            return index + 3;
        Found4:
            return index + 4;
        Found5:
            return index + 5;
        Found6:
            return index + 6;
        Found7:
            return index + 7;
        }

        public static nint IndexOfAny<T>(ref T searchSpace, T value0, T value1, T value2, TSize length) where T : IEquatable<T>? {
            Debug.Assert(length >= 0);

            T lookUp;
            nint index = 0;
            if (default(T) != null || ((object?)value0 != null && (object?)value1 != null && (object?)value2 != null)) {
                Debug.Assert(value0 is not null && value1 is not null && value2 is not null);
                if (value0 is null) throw new ArgumentNullException(nameof(value0));
                if (value1 is null) throw new ArgumentNullException(nameof(value1));
                if (value2 is null) throw new ArgumentNullException(nameof(value2));

                while ((length - index) >= 8) {
                    lookUp = ExUnsafe.Add(ref searchSpace, index);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp) || value2.Equals(lookUp))
                        goto Found;
                    lookUp = ExUnsafe.Add(ref searchSpace, index + 1);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp) || value2.Equals(lookUp))
                        goto Found1;
                    lookUp = ExUnsafe.Add(ref searchSpace, index + 2);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp) || value2.Equals(lookUp))
                        goto Found2;
                    lookUp = ExUnsafe.Add(ref searchSpace, index + 3);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp) || value2.Equals(lookUp))
                        goto Found3;
                    lookUp = ExUnsafe.Add(ref searchSpace, index + 4);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp) || value2.Equals(lookUp))
                        goto Found4;
                    lookUp = ExUnsafe.Add(ref searchSpace, index + 5);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp) || value2.Equals(lookUp))
                        goto Found5;
                    lookUp = ExUnsafe.Add(ref searchSpace, index + 6);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp) || value2.Equals(lookUp))
                        goto Found6;
                    lookUp = ExUnsafe.Add(ref searchSpace, index + 7);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp) || value2.Equals(lookUp))
                        goto Found7;

                    index += 8;
                }

                if ((length - index) >= 4) {
                    lookUp = ExUnsafe.Add(ref searchSpace, index);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp) || value2.Equals(lookUp))
                        goto Found;
                    lookUp = ExUnsafe.Add(ref searchSpace, index + 1);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp) || value2.Equals(lookUp))
                        goto Found1;
                    lookUp = ExUnsafe.Add(ref searchSpace, index + 2);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp) || value2.Equals(lookUp))
                        goto Found2;
                    lookUp = ExUnsafe.Add(ref searchSpace, index + 3);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp) || value2.Equals(lookUp))
                        goto Found3;

                    index += 4;
                }

                while (index < length) {
                    lookUp = ExUnsafe.Add(ref searchSpace, index);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp) || value2.Equals(lookUp))
                        goto Found;

                    index++;
                }
            } else {
                for (index = 0; index < length; index++) {
                    lookUp = ExUnsafe.Add(ref searchSpace, index);
                    if ((object?)lookUp is null) {
                        if ((object?)value0 is null || (object?)value1 is null || (object?)value2 is null) {
                            goto Found;
                        }
                    } else if (lookUp.Equals(value0) || lookUp.Equals(value1) || lookUp.Equals(value2)) {
                        goto Found;
                    }
                }
            }
            return -1;

        Found: // Workaround for https://github.com/dotnet/runtime/issues/8795
            return index;
        Found1:
            return index + 1;
        Found2:
            return index + 2;
        Found3:
            return index + 3;
        Found4:
            return index + 4;
        Found5:
            return index + 5;
        Found6:
            return index + 6;
        Found7:
            return index + 7;
        }

        public static nint IndexOfAny<T>(ref T searchSpace, TSize searchSpaceLength, ref T value, nint valueLength) where T : IEquatable<T>? {
            Debug.Assert(searchSpaceLength >= 0);
            Debug.Assert(valueLength >= 0);

            if (valueLength == 0)
                return -1;  // A zero-length set of values is always treated as "not found".

            // For the following paragraph, let:
            //   n := length of haystack
            //   i := index of first occurrence of any needle within haystack
            //   l := length of needle array
            //
            // We use a naive non-vectorized search because we want to bound the complexity of IndexOfAny
            // to O(i * l) rather than O(n * l), or just O(n * l) if no needle is found. The reason for
            // this is that it's common for callers to invoke IndexOfAny immediately before slicing,
            // and when this is called in a loop, we want the entire loop to be bounded by O(n * l)
            // rather than O(n^2 * l).

            if (TypeHelper.IsValueType<T>()) {
                // Calling ValueType.Equals (devirtualized), which takes 'this' byref. We'll make
                // a byval copy of the candidate from the search space in the outer loop, then in
                // the inner loop we'll pass a ref (as 'this') to each element in the needle.

                for (nint i = 0; i < searchSpaceLength; i++) {
                    T candidate = ExUnsafe.Add(ref searchSpace, i);
                    for (nint j = 0; j < valueLength; j++) {
                        if (ExUnsafe.Add(ref value, j)!.Equals(candidate)) {
                            return i;
                        }
                    }
                }
            } else {
                // Calling IEquatable<T>.Equals (virtual dispatch). We'll perform the null check
                // in the outer loop instead of in the inner loop to save some branching.

                for (nint i = 0; i < searchSpaceLength; i++) {
                    T candidate = ExUnsafe.Add(ref searchSpace, i);
                    if (candidate is not null) {
                        for (nint j = 0; j < valueLength; j++) {
                            if (candidate.Equals(ExUnsafe.Add(ref value, j))) {
                                return i;
                            }
                        }
                    } else {
                        for (nint j = 0; j < valueLength; j++) {
                            if (ExUnsafe.Add(ref value, j) is null) {
                                return i;
                            }
                        }
                    }
                }
            }

            return -1; // not found
        }

        public static TSize LastIndexOf<T>(ref T searchSpace, TSize searchSpaceLength, ref T value, TSize valueLength) where T : IEquatable<T>? {
            Debug.Assert(searchSpaceLength >= 0);
            Debug.Assert(valueLength >= 0);

            if (valueLength == 0)
                return searchSpaceLength;  // A zero-length sequence is always treated as "found" at the end of the search space.

            TSize valueTailLength = valueLength - 1;
            if (valueTailLength == 0) {
                return LastIndexOf(ref searchSpace, value, searchSpaceLength);
            }

            TSize index = 0;

            T valueHead = value;
            ref T valueTail = ref ExUnsafe.Add(ref value, 1);

            while (true) {
                Debug.Assert(0 <= index && index <= searchSpaceLength); // Ensures no deceptive underflows in the computation of "remainingSearchSpaceLength".
                TSize remainingSearchSpaceLength = searchSpaceLength - index - valueTailLength;
                if (remainingSearchSpaceLength <= 0)
                    break;  // The unsearched portion is now shorter than the sequence we're looking for. So it can't be there.

                // Do a quick search for the first element of "value".
                TSize relativeIndex = LastIndexOf(ref searchSpace, valueHead, remainingSearchSpaceLength);
                if (relativeIndex < 0)
                    break;

                // Found the first element of "value". See if the tail matches.
                if (SequenceEqual(ref ExUnsafe.Add(ref searchSpace, relativeIndex + 1), ref valueTail, valueTailLength.ToUIntPtr()))
                    return relativeIndex;  // The tail matched. Return a successful find.

                index += remainingSearchSpaceLength - relativeIndex;
            }
            return -1;
        }

        public static TSize LastIndexOf<T>(ref T searchSpace, T value, TSize length) where T : IEquatable<T>? {
            Debug.Assert(length >= 0);

            if (default(T) != null || (object?)value != null) {
                Debug.Assert(value is not null);
                if (value is null) throw new ArgumentNullException(nameof(value));

                while (length >= 8) {
                    length -= 8;

                    if (value.Equals(ExUnsafe.Add(ref searchSpace, length + 7)))
                        goto Found7;
                    if (value.Equals(ExUnsafe.Add(ref searchSpace, length + 6)))
                        goto Found6;
                    if (value.Equals(ExUnsafe.Add(ref searchSpace, length + 5)))
                        goto Found5;
                    if (value.Equals(ExUnsafe.Add(ref searchSpace, length + 4)))
                        goto Found4;
                    if (value.Equals(ExUnsafe.Add(ref searchSpace, length + 3)))
                        goto Found3;
                    if (value.Equals(ExUnsafe.Add(ref searchSpace, length + 2)))
                        goto Found2;
                    if (value.Equals(ExUnsafe.Add(ref searchSpace, length + 1)))
                        goto Found1;
                    if (value.Equals(ExUnsafe.Add(ref searchSpace, length)))
                        goto Found;
                }

                if (length >= 4) {
                    length -= 4;

                    if (value.Equals(ExUnsafe.Add(ref searchSpace, length + 3)))
                        goto Found3;
                    if (value.Equals(ExUnsafe.Add(ref searchSpace, length + 2)))
                        goto Found2;
                    if (value.Equals(ExUnsafe.Add(ref searchSpace, length + 1)))
                        goto Found1;
                    if (value.Equals(ExUnsafe.Add(ref searchSpace, length)))
                        goto Found;
                }

                while (length > 0) {
                    length--;

                    if (value.Equals(ExUnsafe.Add(ref searchSpace, length)))
                        goto Found;
                }
            } else {
                for (length--; length >= 0; length--) {
                    if ((object?)ExUnsafe.Add(ref searchSpace, length) is null) {
                        goto Found;
                    }
                }
            }

            return -1;

        Found: // Workaround for https://github.com/dotnet/runtime/issues/8795
            return length;
        Found1:
            return length + 1;
        Found2:
            return length + 2;
        Found3:
            return length + 3;
        Found4:
            return length + 4;
        Found5:
            return length + 5;
        Found6:
            return length + 6;
        Found7:
            return length + 7;
        }

        public static TSize LastIndexOfAny<T>(ref T searchSpace, T value0, T value1, TSize length) where T : IEquatable<T>? {
            Debug.Assert(length >= 0);

            T lookUp;
            if (default(T) != null || ((object?)value0 != null && (object?)value1 != null)) {
                Debug.Assert(value0 is not null && value1 is not null);
                if (value0 is null) throw new ArgumentNullException(nameof(value0));
                if (value1 is null) throw new ArgumentNullException(nameof(value1));

                while (length >= 8) {
                    length -= 8;

                    lookUp = ExUnsafe.Add(ref searchSpace, length + 7);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp))
                        goto Found7;
                    lookUp = ExUnsafe.Add(ref searchSpace, length + 6);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp))
                        goto Found6;
                    lookUp = ExUnsafe.Add(ref searchSpace, length + 5);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp))
                        goto Found5;
                    lookUp = ExUnsafe.Add(ref searchSpace, length + 4);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp))
                        goto Found4;
                    lookUp = ExUnsafe.Add(ref searchSpace, length + 3);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp))
                        goto Found3;
                    lookUp = ExUnsafe.Add(ref searchSpace, length + 2);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp))
                        goto Found2;
                    lookUp = ExUnsafe.Add(ref searchSpace, length + 1);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp))
                        goto Found1;
                    lookUp = ExUnsafe.Add(ref searchSpace, length);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp))
                        goto Found;
                }

                if (length >= 4) {
                    length -= 4;

                    lookUp = ExUnsafe.Add(ref searchSpace, length + 3);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp))
                        goto Found3;
                    lookUp = ExUnsafe.Add(ref searchSpace, length + 2);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp))
                        goto Found2;
                    lookUp = ExUnsafe.Add(ref searchSpace, length + 1);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp))
                        goto Found1;
                    lookUp = ExUnsafe.Add(ref searchSpace, length);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp))
                        goto Found;
                }

                while (length > 0) {
                    length--;

                    lookUp = ExUnsafe.Add(ref searchSpace, length);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp))
                        goto Found;
                }
            } else {
                for (length--; length >= 0; length--) {
                    lookUp = ExUnsafe.Add(ref searchSpace, length);
                    if ((object?)lookUp is null) {
                        if ((object?)value0 is null || (object?)value1 is null) {
                            goto Found;
                        }
                    } else if (lookUp.Equals(value0) || lookUp.Equals(value1)) {
                        goto Found;
                    }
                }
            }

            return -1;

        Found: // Workaround for https://github.com/dotnet/runtime/issues/8795
            return length;
        Found1:
            return length + 1;
        Found2:
            return length + 2;
        Found3:
            return length + 3;
        Found4:
            return length + 4;
        Found5:
            return length + 5;
        Found6:
            return length + 6;
        Found7:
            return length + 7;
        }

        public static TSize LastIndexOfAny<T>(ref T searchSpace, T value0, T value1, T value2, TSize length) where T : IEquatable<T>? {
            Debug.Assert(length >= 0);

            T lookUp;
            if (default(T) != null || ((object?)value0 != null && (object?)value1 != null && (object?)value2 != null)) {
                Debug.Assert(value0 is not null && value1 is not null && value2 is not null);
                if (value0 is null) throw new ArgumentNullException(nameof(value0));
                if (value1 is null) throw new ArgumentNullException(nameof(value1));
                if (value2 is null) throw new ArgumentNullException(nameof(value2));

                while (length >= 8) {
                    length -= 8;

                    lookUp = ExUnsafe.Add(ref searchSpace, length + 7);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp) || value2.Equals(lookUp))
                        goto Found7;
                    lookUp = ExUnsafe.Add(ref searchSpace, length + 6);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp) || value2.Equals(lookUp))
                        goto Found6;
                    lookUp = ExUnsafe.Add(ref searchSpace, length + 5);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp) || value2.Equals(lookUp))
                        goto Found5;
                    lookUp = ExUnsafe.Add(ref searchSpace, length + 4);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp) || value2.Equals(lookUp))
                        goto Found4;
                    lookUp = ExUnsafe.Add(ref searchSpace, length + 3);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp) || value2.Equals(lookUp))
                        goto Found3;
                    lookUp = ExUnsafe.Add(ref searchSpace, length + 2);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp) || value2.Equals(lookUp))
                        goto Found2;
                    lookUp = ExUnsafe.Add(ref searchSpace, length + 1);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp) || value2.Equals(lookUp))
                        goto Found1;
                    lookUp = ExUnsafe.Add(ref searchSpace, length);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp) || value2.Equals(lookUp))
                        goto Found;
                }

                if (length >= 4) {
                    length -= 4;

                    lookUp = ExUnsafe.Add(ref searchSpace, length + 3);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp) || value2.Equals(lookUp))
                        goto Found3;
                    lookUp = ExUnsafe.Add(ref searchSpace, length + 2);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp) || value2.Equals(lookUp))
                        goto Found2;
                    lookUp = ExUnsafe.Add(ref searchSpace, length + 1);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp) || value2.Equals(lookUp))
                        goto Found1;
                    lookUp = ExUnsafe.Add(ref searchSpace, length);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp) || value2.Equals(lookUp))
                        goto Found;
                }

                while (length > 0) {
                    length--;

                    lookUp = ExUnsafe.Add(ref searchSpace, length);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp) || value2.Equals(lookUp))
                        goto Found;
                }
            } else {
                for (length--; length >= 0; length--) {
                    lookUp = ExUnsafe.Add(ref searchSpace, length);
                    if ((object?)lookUp is null) {
                        if ((object?)value0 is null || (object?)value1 is null || (object?)value2 is null) {
                            goto Found;
                        }
                    } else if (lookUp.Equals(value0) || lookUp.Equals(value1) || lookUp.Equals(value2)) {
                        goto Found;
                    }
                }
            }

            return -1;

        Found: // Workaround for https://github.com/dotnet/runtime/issues/8795
            return length;
        Found1:
            return length + 1;
        Found2:
            return length + 2;
        Found3:
            return length + 3;
        Found4:
            return length + 4;
        Found5:
            return length + 5;
        Found6:
            return length + 6;
        Found7:
            return length + 7;
        }

        public static TSize LastIndexOfAny<T>(ref T searchSpace, TSize searchSpaceLength, ref T value, TSize valueLength) where T : IEquatable<T>? {
            Debug.Assert(searchSpaceLength >= 0);
            Debug.Assert(valueLength >= 0);

            if (valueLength == 0)
                return -1;  // A zero-length set of values is always treated as "not found".

            // See comments in IndexOfAny(ref T, int, ref T, int) above regarding algorithmic complexity concerns.
            // This logic is similar, but it runs backward.

            if (TypeHelper.IsValueType<T>()) {
                for (TSize i = searchSpaceLength - 1; i >= 0; i--) {
                    T candidate = ExUnsafe.Add(ref searchSpace, i);
                    for (TSize j = 0; j < valueLength; j++) {
                        if (ExUnsafe.Add(ref value, j)!.Equals(candidate)) {
                            return i;
                        }
                    }
                }
            } else {
                for (TSize i = searchSpaceLength - 1; i >= 0; i--) {
                    T candidate = ExUnsafe.Add(ref searchSpace, i);
                    if (candidate is not null) {
                        for (TSize j = 0; j < valueLength; j++) {
                            if (candidate.Equals(ExUnsafe.Add(ref value, j))) {
                                return i;
                            }
                        }
                    } else {
                        for (TSize j = 0; j < valueLength; j++) {
                            if (ExUnsafe.Add(ref value, j) is null) {
                                return i;
                            }
                        }
                    }
                }
            }

            return -1; // not found
        }

        internal static TSize IndexOfAnyExcept<T>(ref T searchSpace, T value0, TSize length) {
            Debug.Assert(length >= 0, "Expected non-negative length");

            for (TSize i = 0; i < length; i++) {
                if (!EqualityComparer<T>.Default.Equals(ExUnsafe.Add(ref searchSpace, i), value0)) {
                    return i;
                }
            }

            return -1;
        }

        internal static TSize LastIndexOfAnyExcept<T>(ref T searchSpace, T value0, TSize length) {
            Debug.Assert(length >= 0, "Expected non-negative length");

            for (TSize i = length - 1; i >= 0; i--) {
                if (!EqualityComparer<T>.Default.Equals(ExUnsafe.Add(ref searchSpace, i), value0)) {
                    return i;
                }
            }

            return -1;
        }

        internal static TSize IndexOfAnyExcept<T>(ref T searchSpace, T value0, T value1, TSize length) {
            Debug.Assert(length >= 0, "Expected non-negative length");

            for (TSize i = 0; i < length; i++) {
                ref T current = ref ExUnsafe.Add(ref searchSpace, i);
                if (!EqualityComparer<T>.Default.Equals(current, value0) && !EqualityComparer<T>.Default.Equals(current, value1)) {
                    return i;
                }
            }

            return -1;
        }

        internal static TSize LastIndexOfAnyExcept<T>(ref T searchSpace, T value0, T value1, TSize length) {
            Debug.Assert(length >= 0, "Expected non-negative length");

            for (TSize i = length - 1; i >= 0; i--) {
                ref T current = ref ExUnsafe.Add(ref searchSpace, i);
                if (!EqualityComparer<T>.Default.Equals(current, value0) && !EqualityComparer<T>.Default.Equals(current, value1)) {
                    return i;
                }
            }

            return -1;
        }

        internal static TSize IndexOfAnyExcept<T>(ref T searchSpace, T value0, T value1, T value2, TSize length) {
            Debug.Assert(length >= 0, "Expected non-negative length");

            for (TSize i = 0; i < length; i++) {
                ref T current = ref ExUnsafe.Add(ref searchSpace, i);
                if (!EqualityComparer<T>.Default.Equals(current, value0)
                    && !EqualityComparer<T>.Default.Equals(current, value1)
                    && !EqualityComparer<T>.Default.Equals(current, value2)) {
                    return i;
                }
            }

            return -1;
        }

        internal static TSize LastIndexOfAnyExcept<T>(ref T searchSpace, T value0, T value1, T value2, TSize length) {
            Debug.Assert(length >= 0, "Expected non-negative length");

            for (TSize i = length - 1; i >= 0; i--) {
                ref T current = ref ExUnsafe.Add(ref searchSpace, i);
                if (!EqualityComparer<T>.Default.Equals(current, value0)
                    && !EqualityComparer<T>.Default.Equals(current, value1)
                    && !EqualityComparer<T>.Default.Equals(current, value2)) {
                    return i;
                }
            }

            return -1;
        }

        internal static TSize IndexOfAnyExcept<T>(ref T searchSpace, T value0, T value1, T value2, T value3, TSize length) {
            Debug.Assert(length >= 0, "Expected non-negative length");

            for (TSize i = 0; i < length; i++) {
                ref T current = ref ExUnsafe.Add(ref searchSpace, i);
                if (!EqualityComparer<T>.Default.Equals(current, value0)
                    && !EqualityComparer<T>.Default.Equals(current, value1)
                    && !EqualityComparer<T>.Default.Equals(current, value2)
                    && !EqualityComparer<T>.Default.Equals(current, value3)) {
                    return i;
                }
            }

            return -1;
        }

        internal static TSize LastIndexOfAnyExcept<T>(ref T searchSpace, T value0, T value1, T value2, T value3, TSize length) {
            Debug.Assert(length >= 0, "Expected non-negative length");

            for (TSize i = length - 1; i >= 0; i--) {
                ref T current = ref ExUnsafe.Add(ref searchSpace, i);
                if (!EqualityComparer<T>.Default.Equals(current, value0)
                    && !EqualityComparer<T>.Default.Equals(current, value1)
                    && !EqualityComparer<T>.Default.Equals(current, value2)
                    && !EqualityComparer<T>.Default.Equals(current, value3)) {
                    return i;
                }
            }

            return -1;
        }

        public static bool SequenceEqual<T>(ref T first, ref T second, nuint length) where T : IEquatable<T>? {
            Debug.Assert(length >= 0);

            if (Unsafe.AreSame(ref first, ref second))
                goto Equal;

            nint index = 0; // Use nint for arithmetic to avoid unnecessary 64->32->64 truncations
            T lookUp0;
            T lookUp1;
            while (length >= 8) {
                length -= 8;

                lookUp0 = ExUnsafe.Add(ref first, index);
                lookUp1 = ExUnsafe.Add(ref second, index);
                if (!(lookUp0?.Equals(lookUp1) ?? (object?)lookUp1 is null))
                    goto NotEqual;
                lookUp0 = ExUnsafe.Add(ref first, index + 1);
                lookUp1 = ExUnsafe.Add(ref second, index + 1);
                if (!(lookUp0?.Equals(lookUp1) ?? (object?)lookUp1 is null))
                    goto NotEqual;
                lookUp0 = ExUnsafe.Add(ref first, index + 2);
                lookUp1 = ExUnsafe.Add(ref second, index + 2);
                if (!(lookUp0?.Equals(lookUp1) ?? (object?)lookUp1 is null))
                    goto NotEqual;
                lookUp0 = ExUnsafe.Add(ref first, index + 3);
                lookUp1 = ExUnsafe.Add(ref second, index + 3);
                if (!(lookUp0?.Equals(lookUp1) ?? (object?)lookUp1 is null))
                    goto NotEqual;
                lookUp0 = ExUnsafe.Add(ref first, index + 4);
                lookUp1 = ExUnsafe.Add(ref second, index + 4);
                if (!(lookUp0?.Equals(lookUp1) ?? (object?)lookUp1 is null))
                    goto NotEqual;
                lookUp0 = ExUnsafe.Add(ref first, index + 5);
                lookUp1 = ExUnsafe.Add(ref second, index + 5);
                if (!(lookUp0?.Equals(lookUp1) ?? (object?)lookUp1 is null))
                    goto NotEqual;
                lookUp0 = ExUnsafe.Add(ref first, index + 6);
                lookUp1 = ExUnsafe.Add(ref second, index + 6);
                if (!(lookUp0?.Equals(lookUp1) ?? (object?)lookUp1 is null))
                    goto NotEqual;
                lookUp0 = ExUnsafe.Add(ref first, index + 7);
                lookUp1 = ExUnsafe.Add(ref second, index + 7);
                if (!(lookUp0?.Equals(lookUp1) ?? (object?)lookUp1 is null))
                    goto NotEqual;

                index += 8;
            }

            if (length >= 4) {
                length -= 4;

                lookUp0 = ExUnsafe.Add(ref first, index);
                lookUp1 = ExUnsafe.Add(ref second, index);
                if (!(lookUp0?.Equals(lookUp1) ?? (object?)lookUp1 is null))
                    goto NotEqual;
                lookUp0 = ExUnsafe.Add(ref first, index + 1);
                lookUp1 = ExUnsafe.Add(ref second, index + 1);
                if (!(lookUp0?.Equals(lookUp1) ?? (object?)lookUp1 is null))
                    goto NotEqual;
                lookUp0 = ExUnsafe.Add(ref first, index + 2);
                lookUp1 = ExUnsafe.Add(ref second, index + 2);
                if (!(lookUp0?.Equals(lookUp1) ?? (object?)lookUp1 is null))
                    goto NotEqual;
                lookUp0 = ExUnsafe.Add(ref first, index + 3);
                lookUp1 = ExUnsafe.Add(ref second, index + 3);
                if (!(lookUp0?.Equals(lookUp1) ?? (object?)lookUp1 is null))
                    goto NotEqual;

                index += 4;
            }

            while (length > 0) {
                lookUp0 = ExUnsafe.Add(ref first, index);
                lookUp1 = ExUnsafe.Add(ref second, index);
                if (!(lookUp0?.Equals(lookUp1) ?? (object?)lookUp1 is null))
                    goto NotEqual;
                index += 1;
                length--;
            }

        Equal:
            return true;

        NotEqual: // Workaround for https://github.com/dotnet/runtime/issues/8795
            return false;
        }

        public static int SequenceCompareTo<T>(ref T first, TSize firstLength, ref T second, TSize secondLength)
            where T : IComparable<T>? {
            Debug.Assert(firstLength >= 0);
            Debug.Assert(secondLength >= 0);

            TSize minLength = firstLength;
            if (minLength > secondLength)
                minLength = secondLength;
            for (TSize i = 0; i < minLength; i++) {
                T lookUp = ExUnsafe.Add(ref second, i);
                int result = (ExUnsafe.Add(ref first, i)?.CompareTo(lookUp) ?? (((object?)lookUp is null) ? 0 : -1));
                result.CompareTo(0);
                if (result != 0)
                    return result;
            }
            return firstLength.CompareTo(secondLength);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool ContainsValueType<T>(ref T searchSpace, T value, TSize length) where T : struct, IEquatable<T>
#if GENERIC_MATH
                , INumber<T>
#endif // GENERIC_MATH
                {
            //if (PackedSpanHelpers.PackedIndexOfIsSupported && typeof(T) == typeof(short) && PackedSpanHelpers.CanUsePackedIndexOf(value)) {
            //    return PackedSpanHelpers.Contains(ref Unsafe.As<T, short>(ref searchSpace), Unsafe.BitCast<T, short>(value), length);
            //}

            return NonPackedContainsValueType(ref searchSpace, value, length);
        }

        internal static bool NonPackedContainsValueType<T>(ref T searchSpace, T value, TSize length) where T : struct, IEquatable<T>
#if GENERIC_MATH
                , INumber<T>
#endif // GENERIC_MATH
                {
            Debug.Assert(length >= 0, "Expected non-negative length");
            Debug.Assert(value is byte or short or int or long, "Expected caller to normalize to one of these types");

            if (false) {
#if NET8_0_OR_GREATER
            } else if (Vector512.IsHardwareAccelerated && length >= Vector512<T>.Count && Vector512<byte>.Count >= Vector<byte>.Count) {
                Vector512<T> current, values = Vector512.Create(value);
                ref T currentSearchSpace = ref searchSpace;
                ref T oneVectorAwayFromEnd = ref ExUnsafe.Add(ref searchSpace, length - Vector512<T>.Count);

                // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
                do {
                    current = Vector512.LoadUnsafe(ref currentSearchSpace);

                    if (Vector512.EqualsAny(values, current)) {
                        return true;
                    }

                    currentSearchSpace = ref ExUnsafe.Add(ref currentSearchSpace, Vector512<T>.Count);
                }
                while (!Unsafe.IsAddressGreaterThan(ref currentSearchSpace, ref oneVectorAwayFromEnd));

                // If any elements remain, process the last vector in the search space.
                if ((nuint)length % (nuint)Vector512<T>.Count != 0) {
                    current = Vector512.LoadUnsafe(ref oneVectorAwayFromEnd);

                    if (Vector512.EqualsAny(values, current)) {
                        return true;
                    }
                }
#endif // NET8_0_OR_GREATER
            } else if (Vector.IsHardwareAccelerated && length >= Vector<T>.Count) {
                Vector<T> current, values = Vectors.Create(value);
                ref T currentSearchSpace = ref searchSpace;
                ref T oneVectorAwayFromEnd = ref ExUnsafe.Add(ref searchSpace, length - Vector<T>.Count);

                // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
                do {
                    current = VectorHelper.LoadUnsafe(ref currentSearchSpace);

                    if (Vector.EqualsAny(values, current)) {
                        return true;
                    }

                    currentSearchSpace = ref ExUnsafe.Add(ref currentSearchSpace, Vector<T>.Count);
                }
                while (!Unsafe.IsAddressGreaterThan(ref currentSearchSpace, ref oneVectorAwayFromEnd));

                // If any elements remain, process the last vector in the search space.
                if ((nuint)length % (nuint)Vector<T>.Count != 0) {
                    current = VectorHelper.LoadUnsafe(ref oneVectorAwayFromEnd);

                    if (Vector.EqualsAny(values, current)) {
                        return true;
                    }
                }
#if NET7_0_OR_GREATER
            } else if (Vector256.IsHardwareAccelerated && length >= Vector256<T>.Count) {
                Vector256<T> equals, values = Vector256.Create(value);
                ref T currentSearchSpace = ref searchSpace;
                ref T oneVectorAwayFromEnd = ref ExUnsafe.Add(ref searchSpace, length - Vector256<T>.Count);

                // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
                do {
                    equals = Vector256.Equals(values, Vector256.LoadUnsafe(ref currentSearchSpace));
                    if (equals == Vector256<T>.Zero) {
                        currentSearchSpace = ref ExUnsafe.Add(ref currentSearchSpace, Vector256<T>.Count);
                        continue;
                    }

                    return true;
                }
                while (!Unsafe.IsAddressGreaterThan(ref currentSearchSpace, ref oneVectorAwayFromEnd));

                // If any elements remain, process the last vector in the search space.
                if ((nuint)length % (nuint)Vector256<T>.Count != 0) {
                    equals = Vector256.Equals(values, Vector256.LoadUnsafe(ref oneVectorAwayFromEnd));
                    if (equals != Vector256<T>.Zero) {
                        return true;
                    }
                }
            } else if (Vector128.IsHardwareAccelerated && length >= Vector128<T>.Count) {
                Vector128<T> equals, values = Vector128.Create(value);
                ref T currentSearchSpace = ref searchSpace;
                ref T oneVectorAwayFromEnd = ref ExUnsafe.Add(ref searchSpace, length - Vector128<T>.Count);

                // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
                do {
                    equals = Vector128.Equals(values, Vector128.LoadUnsafe(ref currentSearchSpace));
                    if (equals == Vector128<T>.Zero) {
                        currentSearchSpace = ref ExUnsafe.Add(ref currentSearchSpace, Vector128<T>.Count);
                        continue;
                    }

                    return true;
                }
                while (!Unsafe.IsAddressGreaterThan(ref currentSearchSpace, ref oneVectorAwayFromEnd));

                // If any elements remain, process the first vector in the search space.
                if ((nuint)length % (nuint)Vector128<T>.Count != 0) {
                    equals = Vector128.Equals(values, Vector128.LoadUnsafe(ref oneVectorAwayFromEnd));
                    if (equals != Vector128<T>.Zero) {
                        return true;
                    }
                }
#endif // NET7_0_OR_GREATER
            }

            if (true) {
                nuint offset = 0;

                while (length >= 8) {
                    length -= 8;

                    if (ExUnsafe.Add(ref searchSpace, offset).Equals(value)
                     || ExUnsafe.Add(ref searchSpace, offset + 1).Equals(value)
                     || ExUnsafe.Add(ref searchSpace, offset + 2).Equals(value)
                     || ExUnsafe.Add(ref searchSpace, offset + 3).Equals(value)
                     || ExUnsafe.Add(ref searchSpace, offset + 4).Equals(value)
                     || ExUnsafe.Add(ref searchSpace, offset + 5).Equals(value)
                     || ExUnsafe.Add(ref searchSpace, offset + 6).Equals(value)
                     || ExUnsafe.Add(ref searchSpace, offset + 7).Equals(value)) {
                        return true;
                    }

                    offset += 8;
                }

                if (length >= 4) {
                    length -= 4;

                    if (ExUnsafe.Add(ref searchSpace, offset).Equals(value)
                     || ExUnsafe.Add(ref searchSpace, offset + 1).Equals(value)
                     || ExUnsafe.Add(ref searchSpace, offset + 2).Equals(value)
                     || ExUnsafe.Add(ref searchSpace, offset + 3).Equals(value)) {
                        return true;
                    }

                    offset += 4;
                }

                while (length > 0) {
                    length -= 1;

                    if (ExUnsafe.Add(ref searchSpace, offset).Equals(value)) return true;

                    offset += 1;
                }
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static nint IndexOfChar(ref char searchSpace, char value, TSize length)
            => IndexOfValueType(ref Unsafe.As<char, short>(ref searchSpace), (short)value, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static nint NonPackedIndexOfChar(ref char searchSpace, char value, TSize length) =>
            NonPackedIndexOfValueType<short, DontNegate<short>>(ref Unsafe.As<char, short>(ref searchSpace), (short)value, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static nint IndexOfValueType<T>(ref T searchSpace, T value, TSize length) where T : struct, IEquatable<T>
#if GENERIC_MATH
            , INumber<T>
#endif // GENERIC_MATH
            => IndexOfValueType<T, DontNegate<T>>(ref searchSpace, value, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static nint IndexOfAnyExceptValueType<T>(ref T searchSpace, T value, TSize length) where T : struct, IEquatable<T>
#if GENERIC_MATH
            , INumber<T>
#endif // GENERIC_MATH
            => IndexOfValueType<T, Negate<T>>(ref searchSpace, value, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static nint IndexOfValueType<TValue, TNegator>(ref TValue searchSpace, TValue value, TSize length)
            where TValue : struct, IEquatable<TValue>
#if GENERIC_MATH
            , INumber<TValue>
#endif // GENERIC_MATH
            where TNegator : struct, INegator<TValue> {
            //if (PackedSpanHelpers.PackedIndexOfIsSupported && typeof(TValue) == typeof(short) && PackedSpanHelpers.CanUsePackedIndexOf(value)) {
            //    return typeof(TNegator) == typeof(DontNegate<short>)
            //        ? PackedSpanHelpers.IndexOf(ref Unsafe.As<TValue, char>(ref searchSpace), Unsafe.BitCast<TValue, char>(value), length)
            //        : PackedSpanHelpers.IndexOfAnyExcept(ref Unsafe.As<TValue, char>(ref searchSpace), Unsafe.BitCast<TValue, char>(value), length);
            //}

            return NonPackedIndexOfValueType<TValue, TNegator>(ref searchSpace, value, length);
        }

        internal static nint NonPackedIndexOfValueType<TValue,
#if GENERIC_MATH
            TNegator
#else
            TNegatorType
#endif // GENERIC_MATH
            >(ref TValue searchSpace, TValue value, TSize length)
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
            Debug.Assert(value is byte or short or int or long, "Expected caller to normalize to one of these types");

            if (false) {
#if NET8_0_OR_GREATER
            } else if (Vector512.IsHardwareAccelerated && length >= Vector512<TValue>.Count && Vector512<byte>.Count >= Vector<byte>.Count) {
                Vector512<TValue> current, values = Vector512.Create(value);
                ref TValue currentSearchSpace = ref searchSpace;
                ref TValue oneVectorAwayFromEnd = ref ExUnsafe.Add(ref searchSpace, length - Vector512<TValue>.Count);

                // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
                do {
                    current = Vector512.LoadUnsafe(ref currentSearchSpace);

                    if (TNegator.HasMatch(values, current)) {
                        return ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, TNegator.GetMatchMask(values, current));
                    }

                    currentSearchSpace = ref ExUnsafe.Add(ref currentSearchSpace, Vector512<TValue>.Count);
                }
                while (!Unsafe.IsAddressGreaterThan(ref currentSearchSpace, ref oneVectorAwayFromEnd));

                // If any elements remain, process the last vector in the search space.
                if ((nuint)length % (nuint)Vector512<TValue>.Count != 0) {
                    current = Vector512.LoadUnsafe(ref oneVectorAwayFromEnd);

                    if (TNegator.HasMatch(values, current)) {
                        return ComputeFirstIndex(ref searchSpace, ref oneVectorAwayFromEnd, TNegator.GetMatchMask(values, current));
                    }
                }
#endif // NET8_0_OR_GREATER
            } else if (Vector.IsHardwareAccelerated && length >= Vector<TValue>.Count) {
                Vector<TValue> current, values = Vectors.Create(value);
                ref TValue currentSearchSpace = ref searchSpace;
                ref TValue oneVectorAwayFromEnd = ref ExUnsafe.Add(ref searchSpace, length - Vector<TValue>.Count);

                // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
                do {
                    current = VectorHelper.LoadUnsafe(ref currentSearchSpace);

                    if (TNegator.HasMatch(values, current)) {
                        return ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, TNegator.GetMatchMask(values, current));
                    }

                    currentSearchSpace = ref ExUnsafe.Add(ref currentSearchSpace, Vector<TValue>.Count);
                }
                while (!Unsafe.IsAddressGreaterThan(ref currentSearchSpace, ref oneVectorAwayFromEnd));

                // If any elements remain, process the last vector in the search space.
                if ((nuint)length % (nuint)Vector<TValue>.Count != 0) {
                    current = VectorHelper.LoadUnsafe(ref oneVectorAwayFromEnd);

                    if (TNegator.HasMatch(values, current)) {
                        return ComputeFirstIndex(ref searchSpace, ref oneVectorAwayFromEnd, TNegator.GetMatchMask(values, current));
                    }
                }
#if NET7_0_OR_GREATER
            } else if (Vector256.IsHardwareAccelerated && length >= Vector256<TValue>.Count) {
                Vector256<TValue> equals, values = Vector256.Create(value);
                ref TValue currentSearchSpace = ref searchSpace;
                ref TValue oneVectorAwayFromEnd = ref ExUnsafe.Add(ref searchSpace, length - Vector256<TValue>.Count);

                // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
                do {
                    equals = TNegator.NegateIfNeeded(Vector256.Equals(values, Vector256.LoadUnsafe(ref currentSearchSpace)));
                    if (equals == Vector256<TValue>.Zero) {
                        currentSearchSpace = ref ExUnsafe.Add(ref currentSearchSpace, Vector256<TValue>.Count);
                        continue;
                    }

                    return ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, equals);
                }
                while (!Unsafe.IsAddressGreaterThan(ref currentSearchSpace, ref oneVectorAwayFromEnd));

                // If any elements remain, process the last vector in the search space.
                if ((nuint)length % (nuint)Vector256<TValue>.Count != 0) {
                    equals = TNegator.NegateIfNeeded(Vector256.Equals(values, Vector256.LoadUnsafe(ref oneVectorAwayFromEnd)));
                    if (equals != Vector256<TValue>.Zero) {
                        return ComputeFirstIndex(ref searchSpace, ref oneVectorAwayFromEnd, equals);
                    }
                }
            } else if (Vector128.IsHardwareAccelerated && length >= Vector128<TValue>.Count) {
                Vector128<TValue> equals, values = Vector128.Create(value);
                ref TValue currentSearchSpace = ref searchSpace;
                ref TValue oneVectorAwayFromEnd = ref ExUnsafe.Add(ref searchSpace, length - Vector128<TValue>.Count);

                // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
                do {
                    equals = TNegator.NegateIfNeeded(Vector128.Equals(values, Vector128.LoadUnsafe(ref currentSearchSpace)));
                    if (equals == Vector128<TValue>.Zero) {
                        currentSearchSpace = ref ExUnsafe.Add(ref currentSearchSpace, Vector128<TValue>.Count);
                        continue;
                    }

                    return ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, equals);
                }
                while (!Unsafe.IsAddressGreaterThan(ref currentSearchSpace, ref oneVectorAwayFromEnd));

                // If any elements remain, process the first vector in the search space.
                if ((nuint)length % (nuint)Vector128<TValue>.Count != 0) {
                    equals = TNegator.NegateIfNeeded(Vector128.Equals(values, Vector128.LoadUnsafe(ref oneVectorAwayFromEnd)));
                    if (equals != Vector128<TValue>.Zero) {
                        return ComputeFirstIndex(ref searchSpace, ref oneVectorAwayFromEnd, equals);
                    }
                }
#endif // NET7_0_OR_GREATER
            }

            if (true) {
                nint offset = 0;

                while (length >= 8) {
                    length -= 8;

                    if (TNegator.NegateIfNeeded(ExUnsafe.Add(ref searchSpace, offset).Equals(value))) goto Found;
                    if (TNegator.NegateIfNeeded(ExUnsafe.Add(ref searchSpace, offset + 1).Equals(value))) goto Found1;
                    if (TNegator.NegateIfNeeded(ExUnsafe.Add(ref searchSpace, offset + 2).Equals(value))) goto Found2;
                    if (TNegator.NegateIfNeeded(ExUnsafe.Add(ref searchSpace, offset + 3).Equals(value))) goto Found3;
                    if (TNegator.NegateIfNeeded(ExUnsafe.Add(ref searchSpace, offset + 4).Equals(value))) goto Found4;
                    if (TNegator.NegateIfNeeded(ExUnsafe.Add(ref searchSpace, offset + 5).Equals(value))) goto Found5;
                    if (TNegator.NegateIfNeeded(ExUnsafe.Add(ref searchSpace, offset + 6).Equals(value))) goto Found6;
                    if (TNegator.NegateIfNeeded(ExUnsafe.Add(ref searchSpace, offset + 7).Equals(value))) goto Found7;

                    offset += 8;
                }

                if (length >= 4) {
                    length -= 4;

                    if (TNegator.NegateIfNeeded(ExUnsafe.Add(ref searchSpace, offset).Equals(value))) goto Found;
                    if (TNegator.NegateIfNeeded(ExUnsafe.Add(ref searchSpace, offset + 1).Equals(value))) goto Found1;
                    if (TNegator.NegateIfNeeded(ExUnsafe.Add(ref searchSpace, offset + 2).Equals(value))) goto Found2;
                    if (TNegator.NegateIfNeeded(ExUnsafe.Add(ref searchSpace, offset + 3).Equals(value))) goto Found3;

                    offset += 4;
                }

                while (length > 0) {
                    length -= 1;

                    if (TNegator.NegateIfNeeded(ExUnsafe.Add(ref searchSpace, offset).Equals(value))) goto Found;

                    offset += 1;
                }
                return -1;
            Found7:
                return (offset + 7);
            Found6:
                return (offset + 6);
            Found5:
                return (offset + 5);
            Found4:
                return (offset + 4);
            Found3:
                return (offset + 3);
            Found2:
                return (offset + 2);
            Found1:
                return (offset + 1);
            Found:
                return (offset);
            }
            //return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static nint IndexOfAnyChar(ref char searchSpace, char value0, char value1, TSize length)
            => IndexOfAnyValueType(ref Unsafe.As<char, short>(ref searchSpace), (short)value0, (short)value1, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static nint IndexOfAnyValueType<T>(ref T searchSpace, T value0, T value1, TSize length) where T : struct, IEquatable<T>
#if GENERIC_MATH
                , INumber<T>
#endif // GENERIC_MATH
            => IndexOfAnyValueType<T, DontNegate<T>>(ref searchSpace, value0, value1, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static nint IndexOfAnyExceptValueType<T>(ref T searchSpace, T value0, T value1, TSize length) where T : struct, IEquatable<T>
#if GENERIC_MATH
                , INumber<T>
#endif // GENERIC_MATH
            => IndexOfAnyValueType<T, Negate<T>>(ref searchSpace, value0, value1, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static nint IndexOfAnyValueType<TValue, TNegator>(ref TValue searchSpace, TValue value0, TValue value1, TSize length)
            where TValue : struct, IEquatable<TValue>
#if GENERIC_MATH
                , INumber<TValue>
#endif // GENERIC_MATH
            where TNegator : struct, INegator<TValue> {
            //if (PackedSpanHelpers.PackedIndexOfIsSupported && typeof(TValue) == typeof(short) && PackedSpanHelpers.CanUsePackedIndexOf(value0) && PackedSpanHelpers.CanUsePackedIndexOf(value1)) {
            //    char char0 = Unsafe.BitCast<TValue, char>(value0);
            //    char char1 = Unsafe.BitCast<TValue, char>(value1);

            //    if (RuntimeHelpers.IsKnownConstant(value0) && RuntimeHelpers.IsKnownConstant(value1)) {
            //        // If the values differ only in the 0x20 bit, we can optimize the search by reducing the number of comparisons.
            //        // This optimization only applies to a small subset of values and the throughput difference is not too significant.
            //        // We avoid introducing per-call overhead for non-constant values by guarding this optimization behind RuntimeHelpers.IsKnownConstant.
            //        if ((char0 ^ char1) == 0x20) {
            //            char lowerCase = (char)Math.Max(char0, char1);

            //            return typeof(TNegator) == typeof(DontNegate<short>)
            //                ? PackedSpanHelpers.IndexOfAnyIgnoreCase(ref Unsafe.As<TValue, char>(ref searchSpace), lowerCase, length)
            //                : PackedSpanHelpers.IndexOfAnyExceptIgnoreCase(ref Unsafe.As<TValue, char>(ref searchSpace), lowerCase, length);
            //        }
            //    }

            //    return typeof(TNegator) == typeof(DontNegate<short>)
            //        ? PackedSpanHelpers.IndexOfAny(ref Unsafe.As<TValue, char>(ref searchSpace), char0, char1, length)
            //        : PackedSpanHelpers.IndexOfAnyExcept(ref Unsafe.As<TValue, char>(ref searchSpace), char0, char1, length);
            //}

            return NonPackedIndexOfAnyValueType<TValue, TNegator>(ref searchSpace, value0, value1, length);
        }

        // having INumber<T> constraint here allows to use == operator and get better perf compared to .Equals
        internal static nint NonPackedIndexOfAnyValueType<TValue,
#if GENERIC_MATH
                TNegator
#else
                TNegatorType
#endif // GENERIC_MATH
            >(ref TValue searchSpace, TValue value0, TValue value1, TSize length)
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
                Vector512<TValue> equals, current, values0 = Vector512.Create(value0), values1 = Vector512.Create(value1);
                ref TValue currentSearchSpace = ref searchSpace;
                ref TValue oneVectorAwayFromEnd = ref ExUnsafe.Add(ref searchSpace, length - Vector512<TValue>.Count);

                // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
                do {
                    current = Vector512.LoadUnsafe(ref currentSearchSpace);
                    equals = TNegator.NegateIfNeeded(Vector512.Equals(values0, current) | Vector512.Equals(values1, current));
                    if (equals == Vector512<TValue>.Zero) {
                        currentSearchSpace = ref ExUnsafe.Add(ref currentSearchSpace, Vector512<TValue>.Count);
                        continue;
                    }

                    return ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, equals);
                }
                while (!Unsafe.IsAddressGreaterThan(ref currentSearchSpace, ref oneVectorAwayFromEnd));

                // If any elements remain, process the last vector in the search space.
                if (length % Vector512<TValue>.Count != 0) {
                    current = Vector512.LoadUnsafe(ref oneVectorAwayFromEnd);
                    equals = TNegator.NegateIfNeeded(Vector512.Equals(values0, current) | Vector512.Equals(values1, current));
                    if (equals != Vector512<TValue>.Zero) {
                        return ComputeFirstIndex(ref searchSpace, ref oneVectorAwayFromEnd, equals);
                    }
                }
#endif // NET8_0_OR_GREATER
            } else if (Vector.IsHardwareAccelerated && length >= Vector<TValue>.Count) {
                Vector<TValue> equals, current, values0 = Vectors.Create(value0), values1 = Vectors.Create(value1);
                ref TValue currentSearchSpace = ref searchSpace;
                ref TValue oneVectorAwayFromEnd = ref ExUnsafe.Add(ref searchSpace, length - Vector<TValue>.Count);

                // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
                do {
                    current = VectorHelper.LoadUnsafe(ref currentSearchSpace);
                    equals = TNegator.NegateIfNeeded(Vector.Equals(values0, current) | Vector.Equals(values1, current));
                    if (equals == Vector<TValue>.Zero) {
                        currentSearchSpace = ref ExUnsafe.Add(ref currentSearchSpace, Vector<TValue>.Count);
                        continue;
                    }

                    return ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, equals);
                }
                while (!Unsafe.IsAddressGreaterThan(ref currentSearchSpace, ref oneVectorAwayFromEnd));

                // If any elements remain, process the last vector in the search space.
                if (length % Vector<TValue>.Count != 0) {
                    current = VectorHelper.LoadUnsafe(ref oneVectorAwayFromEnd);
                    equals = TNegator.NegateIfNeeded(Vector.Equals(values0, current) | Vector.Equals(values1, current));
                    if (equals != Vector<TValue>.Zero) {
                        return ComputeFirstIndex(ref searchSpace, ref oneVectorAwayFromEnd, equals);
                    }
                }
#if NET7_0_OR_GREATER
            } else if (Vector256.IsHardwareAccelerated && length >= Vector256<TValue>.Count) {
                Vector256<TValue> equals, current, values0 = Vector256.Create(value0), values1 = Vector256.Create(value1);
                ref TValue currentSearchSpace = ref searchSpace;
                ref TValue oneVectorAwayFromEnd = ref ExUnsafe.Add(ref searchSpace, length - Vector256<TValue>.Count);

                // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
                do {
                    current = Vector256.LoadUnsafe(ref currentSearchSpace);
                    equals = TNegator.NegateIfNeeded(Vector256.Equals(values0, current) | Vector256.Equals(values1, current));
                    if (equals == Vector256<TValue>.Zero) {
                        currentSearchSpace = ref ExUnsafe.Add(ref currentSearchSpace, Vector256<TValue>.Count);
                        continue;
                    }

                    return ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, equals);
                }
                while (!Unsafe.IsAddressGreaterThan(ref currentSearchSpace, ref oneVectorAwayFromEnd));

                // If any elements remain, process the last vector in the search space.
                if (length % Vector256<TValue>.Count != 0) {
                    current = Vector256.LoadUnsafe(ref oneVectorAwayFromEnd);
                    equals = TNegator.NegateIfNeeded(Vector256.Equals(values0, current) | Vector256.Equals(values1, current));
                    if (equals != Vector256<TValue>.Zero) {
                        return ComputeFirstIndex(ref searchSpace, ref oneVectorAwayFromEnd, equals);
                    }
                }
            } else if (Vector128.IsHardwareAccelerated && length >= Vector128<TValue>.Count) {
                Vector128<TValue> equals, current, values0 = Vector128.Create(value0), values1 = Vector128.Create(value1);
                ref TValue currentSearchSpace = ref searchSpace;
                ref TValue oneVectorAwayFromEnd = ref ExUnsafe.Add(ref searchSpace, length - Vector128<TValue>.Count);

                // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
                do {
                    current = Vector128.LoadUnsafe(ref currentSearchSpace);
                    equals = TNegator.NegateIfNeeded(Vector128.Equals(values0, current) | Vector128.Equals(values1, current));
                    if (equals == Vector128<TValue>.Zero) {
                        currentSearchSpace = ref ExUnsafe.Add(ref currentSearchSpace, Vector128<TValue>.Count);
                        continue;
                    }

                    return ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, equals);
                }
                while (!Unsafe.IsAddressGreaterThan(ref currentSearchSpace, ref oneVectorAwayFromEnd));

                // If any elements remain, process the first vector in the search space.
                if (length % Vector128<TValue>.Count != 0) {
                    current = Vector128.LoadUnsafe(ref oneVectorAwayFromEnd);
                    equals = TNegator.NegateIfNeeded(Vector128.Equals(values0, current) | Vector128.Equals(values1, current));
                    if (equals != Vector128<TValue>.Zero) {
                        return ComputeFirstIndex(ref searchSpace, ref oneVectorAwayFromEnd, equals);
                    }
                }
#endif // NET7_0_OR_GREATER
            }

            if (true) {
                nint offset = 0;
                TValue lookUp;

                if (typeof(TValue) == typeof(byte)) // this optimization is beneficial only to byte
                {
                    while (length >= 8) {
                        length -= 8;

                        ref TValue current = ref ExUnsafe.Add(ref searchSpace, offset);
                        lookUp = current;
                        if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1))) goto Found;
                        lookUp = ExUnsafe.Add(ref current, 1);
                        if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1))) goto Found1;
                        lookUp = ExUnsafe.Add(ref current, 2);
                        if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1))) goto Found2;
                        lookUp = ExUnsafe.Add(ref current, 3);
                        if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1))) goto Found3;
                        lookUp = ExUnsafe.Add(ref current, 4);
                        if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1))) goto Found4;
                        lookUp = ExUnsafe.Add(ref current, 5);
                        if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1))) goto Found5;
                        lookUp = ExUnsafe.Add(ref current, 6);
                        if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1))) goto Found6;
                        lookUp = ExUnsafe.Add(ref current, 7);
                        if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1))) goto Found7;

                        offset += 8;
                    }
                }

                while (length >= 4) {
                    length -= 4;

                    ref TValue current = ref ExUnsafe.Add(ref searchSpace, offset);
                    lookUp = current;
                    if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1))) goto Found;
                    lookUp = ExUnsafe.Add(ref current, 1);
                    if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1))) goto Found1;
                    lookUp = ExUnsafe.Add(ref current, 2);
                    if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1))) goto Found2;
                    lookUp = ExUnsafe.Add(ref current, 3);
                    if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1))) goto Found3;

                    offset += 4;
                }

                while (length > 0) {
                    length -= 1;

                    lookUp = ExUnsafe.Add(ref searchSpace, offset);
                    if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1))) goto Found;

                    offset += 1;
                }
                return -1;
            Found7:
                return offset + 7;
            Found6:
                return offset + 6;
            Found5:
                return offset + 5;
            Found4:
                return offset + 4;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static nint IndexOfAnyValueType<T>(ref T searchSpace, T value0, T value1, T value2, TSize length) where T : struct, IEquatable<T>
#if GENERIC_MATH
                , INumber<T>
#endif // GENERIC_MATH
            => IndexOfAnyValueType<T, DontNegate<T>>(ref searchSpace, value0, value1, value2, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static nint IndexOfAnyExceptValueType<T>(ref T searchSpace, T value0, T value1, T value2, TSize length) where T : struct, IEquatable<T>
#if GENERIC_MATH
                , INumber<T>
#endif // GENERIC_MATH
            => IndexOfAnyValueType<T, Negate<T>>(ref searchSpace, value0, value1, value2, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static nint IndexOfAnyValueType<TValue, TNegator>(ref TValue searchSpace, TValue value0, TValue value1, TValue value2, TSize length)
            where TValue : struct, IEquatable<TValue>
#if GENERIC_MATH
                , INumber<TValue>
#endif // GENERIC_MATH
            where TNegator : struct, INegator<TValue> {
            //if (PackedSpanHelpers.PackedIndexOfIsSupported && typeof(TValue) == typeof(short) && PackedSpanHelpers.CanUsePackedIndexOf(value0) && PackedSpanHelpers.CanUsePackedIndexOf(value1) && PackedSpanHelpers.CanUsePackedIndexOf(value2)) {
            //    return typeof(TNegator) == typeof(DontNegate<short>)
            //        ? PackedSpanHelpers.IndexOfAny(ref Unsafe.As<TValue, char>(ref searchSpace), Unsafe.BitCast<TValue, char>(value0), Unsafe.BitCast<TValue, char>(value1), Unsafe.BitCast<TValue, char>(value2), length)
            //        : PackedSpanHelpers.IndexOfAnyExcept(ref Unsafe.As<TValue, char>(ref searchSpace), Unsafe.BitCast<TValue, char>(value0), Unsafe.BitCast<TValue, char>(value1), Unsafe.BitCast<TValue, char>(value2), length);
            //}

            return NonPackedIndexOfAnyValueType<TValue, TNegator>(ref searchSpace, value0, value1, value2, length);
        }

        internal static nint NonPackedIndexOfAnyValueType<TValue,
#if GENERIC_MATH
                TNegator
#else
                TNegatorType
#endif // GENERIC_MATH
            >(ref TValue searchSpace, TValue value0, TValue value1, TValue value2, TSize length)
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
                Vector512<TValue> equals, current, values0 = Vector512.Create(value0), values1 = Vector512.Create(value1), values2 = Vector512.Create(value2);
                ref TValue currentSearchSpace = ref searchSpace;
                ref TValue oneVectorAwayFromEnd = ref ExUnsafe.Add(ref searchSpace, length - Vector512<TValue>.Count);

                // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
                do {
                    current = Vector512.LoadUnsafe(ref currentSearchSpace);
                    equals = TNegator.NegateIfNeeded(Vector512.Equals(values0, current) | Vector512.Equals(values1, current) | Vector512.Equals(values2, current));
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
                    equals = TNegator.NegateIfNeeded(Vector512.Equals(values0, current) | Vector512.Equals(values1, current) | Vector512.Equals(values2, current));
                    if (equals != Vector512<TValue>.Zero) {
                        return ComputeFirstIndex(ref searchSpace, ref oneVectorAwayFromEnd, equals);
                    }
                }
#endif // NET8_0_OR_GREATER
            } else if (Vector.IsHardwareAccelerated && length >= Vector<TValue>.Count) {
                Vector<TValue> equals, current, values0 = Vectors.Create(value0), values1 = Vectors.Create(value1), values2 = Vectors.Create(value2);
                ref TValue currentSearchSpace = ref searchSpace;
                ref TValue oneVectorAwayFromEnd = ref ExUnsafe.Add(ref searchSpace, length - Vector<TValue>.Count);

                // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
                do {
                    current = VectorHelper.LoadUnsafe(ref currentSearchSpace);
                    equals = TNegator.NegateIfNeeded(Vector.Equals(values0, current) | Vector.Equals(values1, current) | Vector.Equals(values2, current));
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
                    equals = TNegator.NegateIfNeeded(Vector.Equals(values0, current) | Vector.Equals(values1, current) | Vector.Equals(values2, current));
                    if (equals != Vector<TValue>.Zero) {
                        return ComputeFirstIndex(ref searchSpace, ref oneVectorAwayFromEnd, equals);
                    }
                }
#if NET7_0_OR_GREATER
            } else if (Vector256.IsHardwareAccelerated && length >= Vector256<TValue>.Count) {
                Vector256<TValue> equals, current, values0 = Vector256.Create(value0), values1 = Vector256.Create(value1), values2 = Vector256.Create(value2);
                ref TValue currentSearchSpace = ref searchSpace;
                ref TValue oneVectorAwayFromEnd = ref ExUnsafe.Add(ref searchSpace, length - Vector256<TValue>.Count);

                // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
                do {
                    current = Vector256.LoadUnsafe(ref currentSearchSpace);
                    equals = TNegator.NegateIfNeeded(Vector256.Equals(values0, current) | Vector256.Equals(values1, current) | Vector256.Equals(values2, current));
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
                    equals = TNegator.NegateIfNeeded(Vector256.Equals(values0, current) | Vector256.Equals(values1, current) | Vector256.Equals(values2, current));
                    if (equals != Vector256<TValue>.Zero) {
                        return ComputeFirstIndex(ref searchSpace, ref oneVectorAwayFromEnd, equals);
                    }
                }
            } else if (Vector128.IsHardwareAccelerated && length >= Vector128<TValue>.Count) {
                Vector128<TValue> equals, current, values0 = Vector128.Create(value0), values1 = Vector128.Create(value1), values2 = Vector128.Create(value2);
                ref TValue currentSearchSpace = ref searchSpace;
                ref TValue oneVectorAwayFromEnd = ref ExUnsafe.Add(ref searchSpace, length - Vector128<TValue>.Count);

                // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
                do {
                    current = Vector128.LoadUnsafe(ref currentSearchSpace);
                    equals = TNegator.NegateIfNeeded(Vector128.Equals(values0, current) | Vector128.Equals(values1, current) | Vector128.Equals(values2, current));
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
                    equals = TNegator.NegateIfNeeded(Vector128.Equals(values0, current) | Vector128.Equals(values1, current) | Vector128.Equals(values2, current));
                    if (equals != Vector128<TValue>.Zero) {
                        return ComputeFirstIndex(ref searchSpace, ref oneVectorAwayFromEnd, equals);
                    }
                }
#endif // NET7_0_OR_GREATER
            }

            if (true) {
                nint offset = 0;
                TValue lookUp;

                if (typeof(TValue) == typeof(byte)) // this optimization is beneficial only to byte
                {
                    while (length >= 8) {
                        length -= 8;

                        ref TValue current = ref ExUnsafe.Add(ref searchSpace, offset);
                        lookUp = current;
                        if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1) || lookUp.Equals(value2))) goto Found;
                        lookUp = ExUnsafe.Add(ref current, 1);
                        if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1) || lookUp.Equals(value2))) goto Found1;
                        lookUp = ExUnsafe.Add(ref current, 2);
                        if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1) || lookUp.Equals(value2))) goto Found2;
                        lookUp = ExUnsafe.Add(ref current, 3);
                        if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1) || lookUp.Equals(value2))) goto Found3;
                        lookUp = ExUnsafe.Add(ref current, 4);
                        if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1) || lookUp.Equals(value2))) goto Found4;
                        lookUp = ExUnsafe.Add(ref current, 5);
                        if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1) || lookUp.Equals(value2))) goto Found5;
                        lookUp = ExUnsafe.Add(ref current, 6);
                        if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1) || lookUp.Equals(value2))) goto Found6;
                        lookUp = ExUnsafe.Add(ref current, 7);
                        if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1) || lookUp.Equals(value2))) goto Found7;

                        offset += 8;
                    }
                }

                while (length >= 4) {
                    length -= 4;

                    ref TValue current = ref ExUnsafe.Add(ref searchSpace, offset);
                    lookUp = current;
                    if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1) || lookUp.Equals(value2))) goto Found;
                    lookUp = ExUnsafe.Add(ref current, 1);
                    if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1) || lookUp.Equals(value2))) goto Found1;
                    lookUp = ExUnsafe.Add(ref current, 2);
                    if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1) || lookUp.Equals(value2))) goto Found2;
                    lookUp = ExUnsafe.Add(ref current, 3);
                    if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1) || lookUp.Equals(value2))) goto Found3;

                    offset += 4;
                }

                while (length > 0) {
                    length -= 1;

                    lookUp = ExUnsafe.Add(ref searchSpace, offset);
                    if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1) || lookUp.Equals(value2))) goto Found;

                    offset += 1;
                }
                return -1;
            Found7:
                return offset + 7;
            Found6:
                return offset + 6;
            Found5:
                return offset + 5;
            Found4:
                return offset + 4;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static nint IndexOfAnyValueType<T>(ref T searchSpace, T value0, T value1, T value2, T value3, TSize length) where T : struct, IEquatable<T>
#if GENERIC_MATH
                , INumber<T>
#endif // GENERIC_MATH
            => IndexOfAnyValueType<T, DontNegate<T>>(ref searchSpace, value0, value1, value2, value3, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static nint IndexOfAnyExceptValueType<T>(ref T searchSpace, T value0, T value1, T value2, T value3, TSize length) where T : struct, IEquatable<T>
#if GENERIC_MATH
                , INumber<T>
#endif // GENERIC_MATH
            => IndexOfAnyValueType<T, Negate<T>>(ref searchSpace, value0, value1, value2, value3, length);

        private static nint IndexOfAnyValueType<TValue,
#if GENERIC_MATH
                TNegator
#else
                TNegatorType
#endif // GENERIC_MATH
                >(ref TValue searchSpace, TValue value0, TValue value1, TValue value2, TValue value3, TSize length)
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
                Vector512<TValue> equals, current, values0 = Vector512.Create(value0), values1 = Vector512.Create(value1), values2 = Vector512.Create(value2), values3 = Vector512.Create(value3);
                ref TValue currentSearchSpace = ref searchSpace;
                ref TValue oneVectorAwayFromEnd = ref ExUnsafe.Add(ref searchSpace, length - Vector512<TValue>.Count);

                // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
                do {
                    current = Vector512.LoadUnsafe(ref currentSearchSpace);
                    equals = TNegator.NegateIfNeeded(Vector512.Equals(values0, current) | Vector512.Equals(values1, current)
                        | Vector512.Equals(values2, current) | Vector512.Equals(values3, current));
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
                    equals = TNegator.NegateIfNeeded(Vector512.Equals(values0, current) | Vector512.Equals(values1, current)
                        | Vector512.Equals(values2, current) | Vector512.Equals(values3, current));
                    if (equals != Vector512<TValue>.Zero) {
                        return ComputeFirstIndex(ref searchSpace, ref oneVectorAwayFromEnd, equals);
                    }
                }
#endif // NET8_0_OR_GREATER
            } else if (Vector.IsHardwareAccelerated && length >= Vector<TValue>.Count) {
                Vector<TValue> equals, current, values0 = Vectors.Create(value0), values1 = Vectors.Create(value1), values2 = Vectors.Create(value2), values3 = Vectors.Create(value3);
                ref TValue currentSearchSpace = ref searchSpace;
                ref TValue oneVectorAwayFromEnd = ref ExUnsafe.Add(ref searchSpace, length - Vector<TValue>.Count);

                // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
                do {
                    current = VectorHelper.LoadUnsafe(ref currentSearchSpace);
                    equals = TNegator.NegateIfNeeded(Vector.Equals(values0, current) | Vector.Equals(values1, current)
                        | Vector.Equals(values2, current) | Vector.Equals(values3, current));
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
                    equals = TNegator.NegateIfNeeded(Vector.Equals(values0, current) | Vector.Equals(values1, current)
                        | Vector.Equals(values2, current) | Vector.Equals(values3, current));
                    if (equals != Vector<TValue>.Zero) {
                        return ComputeFirstIndex(ref searchSpace, ref oneVectorAwayFromEnd, equals);
                    }
                }
#if NET7_0_OR_GREATER
            } else if (Vector256.IsHardwareAccelerated && length >= Vector256<TValue>.Count) {
                Vector256<TValue> equals, current, values0 = Vector256.Create(value0), values1 = Vector256.Create(value1), values2 = Vector256.Create(value2), values3 = Vector256.Create(value3);
                ref TValue currentSearchSpace = ref searchSpace;
                ref TValue oneVectorAwayFromEnd = ref ExUnsafe.Add(ref searchSpace, length - Vector256<TValue>.Count);

                // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
                do {
                    current = Vector256.LoadUnsafe(ref currentSearchSpace);
                    equals = TNegator.NegateIfNeeded(Vector256.Equals(values0, current) | Vector256.Equals(values1, current)
                        | Vector256.Equals(values2, current) | Vector256.Equals(values3, current));
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
                    equals = TNegator.NegateIfNeeded(Vector256.Equals(values0, current) | Vector256.Equals(values1, current)
                        | Vector256.Equals(values2, current) | Vector256.Equals(values3, current));
                    if (equals != Vector256<TValue>.Zero) {
                        return ComputeFirstIndex(ref searchSpace, ref oneVectorAwayFromEnd, equals);
                    }
                }
            } else if (Vector128.IsHardwareAccelerated && length >= Vector128<TValue>.Count) {
                Vector128<TValue> equals, current, values0 = Vector128.Create(value0), values1 = Vector128.Create(value1), values2 = Vector128.Create(value2), values3 = Vector128.Create(value3);
                ref TValue currentSearchSpace = ref searchSpace;
                ref TValue oneVectorAwayFromEnd = ref ExUnsafe.Add(ref searchSpace, length - Vector128<TValue>.Count);

                // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
                do {
                    current = Vector128.LoadUnsafe(ref currentSearchSpace);
                    equals = TNegator.NegateIfNeeded(Vector128.Equals(values0, current) | Vector128.Equals(values1, current)
                        | Vector128.Equals(values2, current) | Vector128.Equals(values3, current));
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
                    equals = TNegator.NegateIfNeeded(Vector128.Equals(values0, current) | Vector128.Equals(values1, current)
                        | Vector128.Equals(values2, current) | Vector128.Equals(values3, current));
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
                    if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1) || lookUp.Equals(value2) || lookUp.Equals(value3))) goto Found;
                    lookUp = ExUnsafe.Add(ref current, 1);
                    if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1) || lookUp.Equals(value2) || lookUp.Equals(value3))) goto Found1;
                    lookUp = ExUnsafe.Add(ref current, 2);
                    if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1) || lookUp.Equals(value2) || lookUp.Equals(value3))) goto Found2;
                    lookUp = ExUnsafe.Add(ref current, 3);
                    if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1) || lookUp.Equals(value2) || lookUp.Equals(value3))) goto Found3;

                    offset += 4;
                }

                while (length > 0) {
                    length -= 1;

                    lookUp = ExUnsafe.Add(ref searchSpace, offset);
                    if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1) || lookUp.Equals(value2) || lookUp.Equals(value3))) goto Found;

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static nint IndexOfAnyValueType<T>(ref T searchSpace, T value0, T value1, T value2, T value3, T value4, TSize length) where T : struct, IEquatable<T>
#if GENERIC_MATH
                , INumber<T>
#endif // GENERIC_MATH
            => IndexOfAnyValueType<T, DontNegate<T>>(ref searchSpace, value0, value1, value2, value3, value4, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static nint IndexOfAnyExceptValueType<T>(ref T searchSpace, T value0, T value1, T value2, T value3, T value4, TSize length) where T : struct, IEquatable<T>
#if GENERIC_MATH
                , INumber<T>
#endif // GENERIC_MATH
            => IndexOfAnyValueType<T, Negate<T>>(ref searchSpace, value0, value1, value2, value3, value4, length);

        private static nint IndexOfAnyValueType<TValue,
#if GENERIC_MATH
                TNegator
#else
                TNegatorType
#endif // GENERIC_MATH
                >(ref TValue searchSpace, TValue value0, TValue value1, TValue value2, TValue value3, TValue value4, TSize length)
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
                    values2 = Vector512.Create(value2), values3 = Vector512.Create(value3), values4 = Vector512.Create(value4);
                ref TValue currentSearchSpace = ref searchSpace;
                ref TValue oneVectorAwayFromEnd = ref ExUnsafe.Add(ref searchSpace, length - Vector512<TValue>.Count);

                // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
                do {
                    current = Vector512.LoadUnsafe(ref currentSearchSpace);
                    equals = TNegator.NegateIfNeeded(Vector512.Equals(values0, current) | Vector512.Equals(values1, current) | Vector512.Equals(values2, current)
                           | Vector512.Equals(values3, current) | Vector512.Equals(values4, current));
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
                           | Vector512.Equals(values3, current) | Vector512.Equals(values4, current));
                    if (equals != Vector512<TValue>.Zero) {
                        return ComputeFirstIndex(ref searchSpace, ref oneVectorAwayFromEnd, equals);
                    }
                }
#endif // NET8_0_OR_GREATER
            } else if (Vector.IsHardwareAccelerated && length >= Vector<TValue>.Count) {
                Vector<TValue> equals, current, values0 = Vectors.Create(value0), values1 = Vectors.Create(value1),
                    values2 = Vectors.Create(value2), values3 = Vectors.Create(value3), values4 = Vectors.Create(value4);
                ref TValue currentSearchSpace = ref searchSpace;
                ref TValue oneVectorAwayFromEnd = ref ExUnsafe.Add(ref searchSpace, length - Vector<TValue>.Count);

                // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
                do {
                    current = VectorHelper.LoadUnsafe(ref currentSearchSpace);
                    equals = TNegator.NegateIfNeeded(Vector.Equals(values0, current) | Vector.Equals(values1, current) | Vector.Equals(values2, current)
                           | Vector.Equals(values3, current) | Vector.Equals(values4, current));
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
                           | Vector.Equals(values3, current) | Vector.Equals(values4, current));
                    if (equals != Vector<TValue>.Zero) {
                        return ComputeFirstIndex(ref searchSpace, ref oneVectorAwayFromEnd, equals);
                    }
                }
#if NET7_0_OR_GREATER
            } else if (Vector256.IsHardwareAccelerated && length >= Vector256<TValue>.Count) {
                Vector256<TValue> equals, current, values0 = Vector256.Create(value0), values1 = Vector256.Create(value1),
                    values2 = Vector256.Create(value2), values3 = Vector256.Create(value3), values4 = Vector256.Create(value4);
                ref TValue currentSearchSpace = ref searchSpace;
                ref TValue oneVectorAwayFromEnd = ref ExUnsafe.Add(ref searchSpace, length - Vector256<TValue>.Count);

                // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
                do {
                    current = Vector256.LoadUnsafe(ref currentSearchSpace);
                    equals = TNegator.NegateIfNeeded(Vector256.Equals(values0, current) | Vector256.Equals(values1, current) | Vector256.Equals(values2, current)
                           | Vector256.Equals(values3, current) | Vector256.Equals(values4, current));
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
                           | Vector256.Equals(values3, current) | Vector256.Equals(values4, current));
                    if (equals != Vector256<TValue>.Zero) {
                        return ComputeFirstIndex(ref searchSpace, ref oneVectorAwayFromEnd, equals);
                    }
                }
            } else if (Vector128.IsHardwareAccelerated && length >= Vector128<TValue>.Count) {
                Vector128<TValue> equals, current, values0 = Vector128.Create(value0), values1 = Vector128.Create(value1),
                    values2 = Vector128.Create(value2), values3 = Vector128.Create(value3), values4 = Vector128.Create(value4);
                ref TValue currentSearchSpace = ref searchSpace;
                ref TValue oneVectorAwayFromEnd = ref ExUnsafe.Add(ref searchSpace, length - Vector128<TValue>.Count);

                // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
                do {
                    current = Vector128.LoadUnsafe(ref currentSearchSpace);
                    equals = TNegator.NegateIfNeeded(Vector128.Equals(values0, current) | Vector128.Equals(values1, current) | Vector128.Equals(values2, current)
                           | Vector128.Equals(values3, current) | Vector128.Equals(values4, current));
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
                           | Vector128.Equals(values3, current) | Vector128.Equals(values4, current));
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
                    if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1) || lookUp.Equals(value2) || lookUp.Equals(value3) || lookUp.Equals(value4))) goto Found;
                    lookUp = ExUnsafe.Add(ref current, 1);
                    if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1) || lookUp.Equals(value2) || lookUp.Equals(value3) || lookUp.Equals(value4))) goto Found1;
                    lookUp = ExUnsafe.Add(ref current, 2);
                    if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1) || lookUp.Equals(value2) || lookUp.Equals(value3) || lookUp.Equals(value4))) goto Found2;
                    lookUp = ExUnsafe.Add(ref current, 3);
                    if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1) || lookUp.Equals(value2) || lookUp.Equals(value3) || lookUp.Equals(value4))) goto Found3;

                    offset += 4;
                }

                while (length > 0) {
                    length -= 1;

                    lookUp = ExUnsafe.Add(ref searchSpace, offset);
                    if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1) || lookUp.Equals(value2) || lookUp.Equals(value3) || lookUp.Equals(value4))) goto Found;

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static TSize LastIndexOfValueType<T>(ref T searchSpace, T value, TSize length) where T : struct, IEquatable<T>
#if GENERIC_MATH
            , INumber<T>
#endif // GENERIC_MATH
            => LastIndexOfValueType<T, DontNegate<T>>(ref searchSpace, value, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static TSize LastIndexOfAnyExceptValueType<T>(ref T searchSpace, T value, TSize length) where T : struct, IEquatable<T>
#if GENERIC_MATH
            , INumber<T>
#endif // GENERIC_MATH
            => LastIndexOfValueType<T, Negate<T>>(ref searchSpace, value, length);

        private static TSize LastIndexOfValueType<TValue,
#if GENERIC_MATH
            TNegator
#else
            TNegatorType
#endif // GENERIC_MATH
            >(ref TValue searchSpace, TValue value, TSize length)
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
            Debug.Assert(value is byte or short or int or long, "Expected caller to normalize to one of these types");

            //if (false) {
            //} else if (Vector512.IsHardwareAccelerated && length >= Vector512<TValue>.Count && Vector512<byte>.Count >= Vector<byte>.Count) {
            //    return SimdImpl<Vector512<TValue>>(ref searchSpace, value, length);
            //} else if (Vector256.IsHardwareAccelerated && length >= Vector256<TValue>.Count) {
            //    return SimdImpl<Vector256<TValue>>(ref searchSpace, value, length);
            //} else {
            //    return SimdImpl<Vector128<TValue>>(ref searchSpace, value, length);
            //}
            //static int SimdImpl<TVector>(ref TValue searchSpace, TValue value, TSize length)
            //    where TVector : struct, ISimdVector<TVector, TValue> {
            //    TVector current;
            //    TVector values = TVector.Create(value);
            //    int offset = length - TVector.ElementCount;
            //    // Loop until either we've finished all elements -or- there's one or less than a vector's-worth remaining.
            //    while (offset > 0) {
            //        current = TVectorHelper.LoadUnsafe(ref searchSpace, (uint)(offset));
            //        if (TNegator.HasMatch(values, current)) {
            //            return offset + TVector.LastIndexOfWhereAllBitsSet(TNegator.GetMatchMask(values, current));
            //        }
            //        offset -= TVector.ElementCount;
            //    }
            //    // Process the first vector in the search space.
            //    current = TVectorHelper.LoadUnsafe(ref searchSpace);
            //    if (TNegator.HasMatch(values, current)) {
            //        return TVector.LastIndexOfWhereAllBitsSet(TNegator.GetMatchMask(values, current));
            //    }
            //    return -1;
            //}
            if (false) {
#if NET8_0_OR_GREATER
            } else if (Vector512.IsHardwareAccelerated && length >= Vector512<TValue>.Count && Vector512<byte>.Count >= Vector<byte>.Count) {
                Vector512<TValue> current;
                Vector512<TValue> values = Vector512.Create(value);
                TSize offset = length - Vector512<TValue>.Count;
                // Loop until either we've finished all elements -or- there's one or less than a vector's-worth remaining.
                while (offset > 0) {
                    current = Vector512.LoadUnsafe(ref searchSpace, offset.ToUIntPtr());
                    if (TNegator.HasMatch(values, current)) {
                        return offset + Vector512Helper.LastIndexOfWhereAllBitsSet(TNegator.GetMatchMask(values, current));
                    }
                    offset -= Vector512<TValue>.Count;
                }
                // Process the first vector in the search space.
                current = Vector512.LoadUnsafe(ref searchSpace);
                if (TNegator.HasMatch(values, current)) {
                    return Vector512Helper.LastIndexOfWhereAllBitsSet(TNegator.GetMatchMask(values, current));
                }
                return -1;
#endif // NET8_0_OR_GREATER
            } else if (Vector.IsHardwareAccelerated && length >= Vector<TValue>.Count) {
                Vector<TValue> current;
                Vector<TValue> values = Vectors.Create(value);
                TSize offset = length - Vector<TValue>.Count;
                // Loop until either we've finished all elements -or- there's one or less than a vector's-worth remaining.
                while (offset > 0) {
                    current = VectorHelper.LoadUnsafe(ref searchSpace, offset.ToUIntPtr());
                    if (TNegator.HasMatch(values, current)) {
                        return offset + VectorHelper.LastIndexOfWhereAllBitsSet(TNegator.GetMatchMask(values, current));
                    }
                    offset -= Vector<TValue>.Count;
                }
                // Process the first vector in the search space.
                current = VectorHelper.LoadUnsafe(ref searchSpace);
                if (TNegator.HasMatch(values, current)) {
                    return VectorHelper.LastIndexOfWhereAllBitsSet(TNegator.GetMatchMask(values, current));
                }
                return -1;
#if NET7_0_OR_GREATER
            } else if (Vector256.IsHardwareAccelerated && length >= Vector256<TValue>.Count) {
                Vector256<TValue> current;
                Vector256<TValue> values = Vector256.Create(value);
                TSize offset = length - Vector256<TValue>.Count;
                // Loop until either we've finished all elements -or- there's one or less than a vector's-worth remaining.
                while (offset > 0) {
                    current = Vector256.LoadUnsafe(ref searchSpace, offset.ToUIntPtr());
                    if (TNegator.HasMatch(values, current)) {
                        return offset + Vector256Helper.LastIndexOfWhereAllBitsSet(TNegator.GetMatchMask(values, current));
                    }
                    offset -= Vector256<TValue>.Count;
                }
                // Process the first vector in the search space.
                current = Vector256.LoadUnsafe(ref searchSpace);
                if (TNegator.HasMatch(values, current)) {
                    return Vector256Helper.LastIndexOfWhereAllBitsSet(TNegator.GetMatchMask(values, current));
                }
                return -1;
            } else if (Vector128.IsHardwareAccelerated && length >= Vector128<TValue>.Count) {
                Vector128<TValue> current;
                Vector128<TValue> values = Vector128.Create(value);
                TSize offset = length - Vector128<TValue>.Count;
                // Loop until either we've finished all elements -or- there's one or less than a vector's-worth remaining.
                while (offset > 0) {
                    current = Vector128.LoadUnsafe(ref searchSpace, offset.ToUIntPtr());
                    if (TNegator.HasMatch(values, current)) {
                        return offset + Vector128Helper.LastIndexOfWhereAllBitsSet(TNegator.GetMatchMask(values, current));
                    }
                    offset -= Vector128<TValue>.Count;
                }
                // Process the first vector in the search space.
                current = Vector128.LoadUnsafe(ref searchSpace);
                if (TNegator.HasMatch(values, current)) {
                    return Vector128Helper.LastIndexOfWhereAllBitsSet(TNegator.GetMatchMask(values, current));
                }
                return -1;
#endif // NET7_0_OR_GREATER
            }

            if (true) {
                TSize offset = length - 1;

                while (length >= 8) {
                    length -= 8;

                    if (TNegator.NegateIfNeeded(ExUnsafe.Add(ref searchSpace, offset).Equals(value))) goto Found;
                    if (TNegator.NegateIfNeeded(ExUnsafe.Add(ref searchSpace, offset - 1).Equals(value))) goto FoundM1;
                    if (TNegator.NegateIfNeeded(ExUnsafe.Add(ref searchSpace, offset - 2).Equals(value))) goto FoundM2;
                    if (TNegator.NegateIfNeeded(ExUnsafe.Add(ref searchSpace, offset - 3).Equals(value))) goto FoundM3;
                    if (TNegator.NegateIfNeeded(ExUnsafe.Add(ref searchSpace, offset - 4).Equals(value))) goto FoundM4;
                    if (TNegator.NegateIfNeeded(ExUnsafe.Add(ref searchSpace, offset - 5).Equals(value))) goto FoundM5;
                    if (TNegator.NegateIfNeeded(ExUnsafe.Add(ref searchSpace, offset - 6).Equals(value))) goto FoundM6;
                    if (TNegator.NegateIfNeeded(ExUnsafe.Add(ref searchSpace, offset - 7).Equals(value))) goto FoundM7;

                    offset -= 8;
                }

                if (length >= 4) {
                    length -= 4;

                    if (TNegator.NegateIfNeeded(ExUnsafe.Add(ref searchSpace, offset).Equals(value))) goto Found;
                    if (TNegator.NegateIfNeeded(ExUnsafe.Add(ref searchSpace, offset - 1).Equals(value))) goto FoundM1;
                    if (TNegator.NegateIfNeeded(ExUnsafe.Add(ref searchSpace, offset - 2).Equals(value))) goto FoundM2;
                    if (TNegator.NegateIfNeeded(ExUnsafe.Add(ref searchSpace, offset - 3).Equals(value))) goto FoundM3;

                    offset -= 4;
                }

                while (length > 0) {
                    length -= 1;

                    if (TNegator.NegateIfNeeded(ExUnsafe.Add(ref searchSpace, offset).Equals(value))) goto Found;

                    offset -= 1;
                }
                return -1;
            FoundM7:
                return offset - 7;
            FoundM6:
                return offset - 6;
            FoundM5:
                return offset - 5;
            FoundM4:
                return offset - 4;
            FoundM3:
                return offset - 3;
            FoundM2:
                return offset - 2;
            FoundM1:
                return offset - 1;
            Found:
                return offset;
            }

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static TSize LastIndexOfAnyValueType<T>(ref T searchSpace, T value0, T value1, TSize length) where T : struct, IEquatable<T>
#if GENERIC_MATH
            , INumber<T>
#endif // GENERIC_MATH
            => LastIndexOfAnyValueType<T, DontNegate<T>>(ref searchSpace, value0, value1, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static TSize LastIndexOfAnyExceptValueType<T>(ref T searchSpace, T value0, T value1, TSize length) where T : struct, IEquatable<T>
#if GENERIC_MATH
            , INumber<T>
#endif // GENERIC_MATH
            => LastIndexOfAnyValueType<T, Negate<T>>(ref searchSpace, value0, value1, length);

        private static TSize LastIndexOfAnyValueType<TValue,
#if GENERIC_MATH
            TNegator
#else
            TNegatorType
#endif // GENERIC_MATH
            >(ref TValue searchSpace, TValue value0, TValue value1, TSize length)
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
                Vector512<TValue> equals, current, values0 = Vector512.Create(value0), values1 = Vector512.Create(value1);
                TSize offset = length - Vector512<TValue>.Count;

                // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
                while (offset > 0) {
                    current = Vector512.LoadUnsafe(ref searchSpace, offset.ToUIntPtr());
                    equals = TNegator.NegateIfNeeded(Vector512.Equals(current, values0) | Vector512.Equals(current, values1));

                    if (equals == Vector512<TValue>.Zero) {
                        offset -= Vector512<TValue>.Count;
                        continue;
                    }

                    return ComputeLastIndex(offset, equals);
                }

                // Process the first vector in the search space.

                current = Vector512.LoadUnsafe(ref searchSpace);
                equals = TNegator.NegateIfNeeded(Vector512.Equals(current, values0) | Vector512.Equals(current, values1));

                if (equals != Vector512<TValue>.Zero) {
                    return ComputeLastIndex(offset: 0, equals);
                }
#endif // NET8_0_OR_GREATER
            } else if (Vector.IsHardwareAccelerated && length >= Vector<TValue>.Count) {
                Vector<TValue> equals, current, values0 = Vectors.Create(value0), values1 = Vectors.Create(value1);
                TSize offset = length - Vector<TValue>.Count;

                // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
                while (offset > 0) {
                    current = VectorHelper.LoadUnsafe(ref searchSpace, offset.ToUIntPtr());
                    equals = TNegator.NegateIfNeeded(Vector.Equals(current, values0) | Vector.Equals(current, values1));

                    if (equals == Vector<TValue>.Zero) {
                        offset -= Vector<TValue>.Count;
                        continue;
                    }

                    return ComputeLastIndex(offset, equals);
                }

                // Process the first vector in the search space.

                current = VectorHelper.LoadUnsafe(ref searchSpace);
                equals = TNegator.NegateIfNeeded(Vector.Equals(current, values0) | Vector.Equals(current, values1));

                if (equals != Vector<TValue>.Zero) {
                    return ComputeLastIndex(offset: 0, equals);
                }
#if NET7_0_OR_GREATER
            } else if (Vector256.IsHardwareAccelerated && length >= Vector256<TValue>.Count) {
                Vector256<TValue> equals, current, values0 = Vector256.Create(value0), values1 = Vector256.Create(value1);
                TSize offset = length - Vector256<TValue>.Count;

                // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
                while (offset > 0) {
                    current = Vector256.LoadUnsafe(ref searchSpace, offset.ToUIntPtr());
                    equals = TNegator.NegateIfNeeded(Vector256.Equals(current, values0) | Vector256.Equals(current, values1));

                    if (equals == Vector256<TValue>.Zero) {
                        offset -= Vector256<TValue>.Count;
                        continue;
                    }

                    return ComputeLastIndex(offset, equals);
                }

                // Process the first vector in the search space.

                current = Vector256.LoadUnsafe(ref searchSpace);
                equals = TNegator.NegateIfNeeded(Vector256.Equals(current, values0) | Vector256.Equals(current, values1));

                if (equals != Vector256<TValue>.Zero) {
                    return ComputeLastIndex(offset: 0, equals);
                }
            } else if (Vector128.IsHardwareAccelerated && length >= Vector128<TValue>.Count) {
                Vector128<TValue> equals, current, values0 = Vector128.Create(value0), values1 = Vector128.Create(value1);
                TSize offset = length - Vector128<TValue>.Count;

                // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
                while (offset > 0) {
                    current = Vector128.LoadUnsafe(ref searchSpace, offset.ToUIntPtr());
                    equals = TNegator.NegateIfNeeded(Vector128.Equals(current, values0) | Vector128.Equals(current, values1));
                    if (equals == Vector128<TValue>.Zero) {
                        offset -= Vector128<TValue>.Count;
                        continue;
                    }

                    return ComputeLastIndex(offset, equals);
                }

                // Process the first vector in the search space.

                current = Vector128.LoadUnsafe(ref searchSpace);
                equals = TNegator.NegateIfNeeded(Vector128.Equals(current, values0) | Vector128.Equals(current, values1));

                if (equals != Vector128<TValue>.Zero) {
                    return ComputeLastIndex(offset: 0, equals);
                }
#endif // NET7_0_OR_GREATER
            }

            if (true) {
                TSize offset = length - 1;
                TValue lookUp;

                if (typeof(TValue) == typeof(byte)) // this optimization is beneficial only to byte
                {
                    while (length >= 8) {
                        length -= 8;

                        ref TValue current = ref ExUnsafe.Add(ref searchSpace, offset);
                        lookUp = current;
                        if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1))) goto Found;
                        lookUp = ExUnsafe.Add(ref current, -1);
                        if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1))) goto FoundM1;
                        lookUp = ExUnsafe.Add(ref current, -2);
                        if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1))) goto FoundM2;
                        lookUp = ExUnsafe.Add(ref current, -3);
                        if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1))) goto FoundM3;
                        lookUp = ExUnsafe.Add(ref current, -4);
                        if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1))) goto FoundM4;
                        lookUp = ExUnsafe.Add(ref current, -5);
                        if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1))) goto FoundM5;
                        lookUp = ExUnsafe.Add(ref current, -6);
                        if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1))) goto FoundM6;
                        lookUp = ExUnsafe.Add(ref current, -7);
                        if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1))) goto FoundM7;

                        offset -= 8;
                    }
                }

                while (length >= 4) {
                    length -= 4;

                    ref TValue current = ref ExUnsafe.Add(ref searchSpace, offset);
                    lookUp = current;
                    if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1))) goto Found;
                    lookUp = ExUnsafe.Add(ref current, -1);
                    if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1))) goto FoundM1;
                    lookUp = ExUnsafe.Add(ref current, -2);
                    if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1))) goto FoundM2;
                    lookUp = ExUnsafe.Add(ref current, -3);
                    if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1))) goto FoundM3;

                    offset -= 4;
                }

                while (length > 0) {
                    length -= 1;

                    lookUp = ExUnsafe.Add(ref searchSpace, offset);
                    if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1))) goto Found;

                    offset -= 1;
                }
                return -1;
            FoundM7:
                return offset - 7;
            FoundM6:
                return offset - 6;
            FoundM5:
                return offset - 5;
            FoundM4:
                return offset - 4;
            FoundM3:
                return offset - 3;
            FoundM2:
                return offset - 2;
            FoundM1:
                return offset - 1;
            Found:
                return offset;
            }
            //return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static TSize LastIndexOfAnyValueType<T>(ref T searchSpace, T value0, T value1, T value2, TSize length) where T : struct, IEquatable<T>
#if GENERIC_MATH
            , INumber<T>
#endif // GENERIC_MATH
            => LastIndexOfAnyValueType<T, DontNegate<T>>(ref searchSpace, value0, value1, value2, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static TSize LastIndexOfAnyExceptValueType<T>(ref T searchSpace, T value0, T value1, T value2, TSize length) where T : struct, IEquatable<T>
#if GENERIC_MATH
            , INumber<T>
#endif // GENERIC_MATH
            => LastIndexOfAnyValueType<T, Negate<T>>(ref searchSpace, value0, value1, value2, length);

        private static TSize LastIndexOfAnyValueType<TValue,
#if GENERIC_MATH
            TNegator
#else
            TNegatorType
#endif // GENERIC_MATH
            >(ref TValue searchSpace, TValue value0, TValue value1, TValue value2, TSize length)
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
                Vector512<TValue> equals, current, values0 = Vector512.Create(value0), values1 = Vector512.Create(value1), values2 = Vector512.Create(value2);
                nint offset = length - Vector512<TValue>.Count;

                // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
                while (offset > 0) {
                    current = Vector512.LoadUnsafe(ref searchSpace, offset.ToUIntPtr());
                    equals = TNegator.NegateIfNeeded(Vector512.Equals(current, values0) | Vector512.Equals(current, values1) | Vector512.Equals(current, values2));

                    if (equals == Vector512<TValue>.Zero) {
                        offset -= Vector512<TValue>.Count;
                        continue;
                    }

                    return ComputeLastIndex(offset, equals);
                }

                // Process the first vector in the search space.

                current = Vector512.LoadUnsafe(ref searchSpace);
                equals = TNegator.NegateIfNeeded(Vector512.Equals(current, values0) | Vector512.Equals(current, values1) | Vector512.Equals(current, values2));

                if (equals != Vector512<TValue>.Zero) {
                    return ComputeLastIndex(offset: 0, equals);
                }
#endif // NET8_0_OR_GREATER
            } else if (Vector.IsHardwareAccelerated && length >= Vector<TValue>.Count) {
                Vector<TValue> equals, current, values0 = Vectors.Create(value0), values1 = Vectors.Create(value1), values2 = Vectors.Create(value2);
                nint offset = length - Vector<TValue>.Count;

                // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
                while (offset > 0) {
                    current = VectorHelper.LoadUnsafe(ref searchSpace, offset.ToUIntPtr());
                    equals = TNegator.NegateIfNeeded(Vector.Equals(current, values0) | Vector.Equals(current, values1) | Vector.Equals(current, values2));

                    if (equals == Vector<TValue>.Zero) {
                        offset -= Vector<TValue>.Count;
                        continue;
                    }

                    return ComputeLastIndex(offset, equals);
                }

                // Process the first vector in the search space.

                current = VectorHelper.LoadUnsafe(ref searchSpace);
                equals = TNegator.NegateIfNeeded(Vector.Equals(current, values0) | Vector.Equals(current, values1) | Vector.Equals(current, values2));

                if (equals != Vector<TValue>.Zero) {
                    return ComputeLastIndex(offset: 0, equals);
                }
#if NET7_0_OR_GREATER
            } else if (Vector256.IsHardwareAccelerated && length >= Vector256<TValue>.Count) {
                Vector256<TValue> equals, current, values0 = Vector256.Create(value0), values1 = Vector256.Create(value1), values2 = Vector256.Create(value2);
                nint offset = length - Vector256<TValue>.Count;

                // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
                while (offset > 0) {
                    current = Vector256.LoadUnsafe(ref searchSpace, offset.ToUIntPtr());
                    equals = TNegator.NegateIfNeeded(Vector256.Equals(current, values0) | Vector256.Equals(current, values1) | Vector256.Equals(current, values2));

                    if (equals == Vector256<TValue>.Zero) {
                        offset -= Vector256<TValue>.Count;
                        continue;
                    }

                    return ComputeLastIndex(offset, equals);
                }

                // Process the first vector in the search space.

                current = Vector256.LoadUnsafe(ref searchSpace);
                equals = TNegator.NegateIfNeeded(Vector256.Equals(current, values0) | Vector256.Equals(current, values1) | Vector256.Equals(current, values2));

                if (equals != Vector256<TValue>.Zero) {
                    return ComputeLastIndex(offset: 0, equals);
                }
            } else if (Vector128.IsHardwareAccelerated && length >= Vector128<TValue>.Count) {
                Vector128<TValue> equals, current, values0 = Vector128.Create(value0), values1 = Vector128.Create(value1), values2 = Vector128.Create(value2);
                nint offset = length - Vector128<TValue>.Count;

                // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
                while (offset > 0) {
                    current = Vector128.LoadUnsafe(ref searchSpace, offset.ToUIntPtr());
                    equals = TNegator.NegateIfNeeded(Vector128.Equals(current, values0) | Vector128.Equals(current, values1) | Vector128.Equals(current, values2));

                    if (equals == Vector128<TValue>.Zero) {
                        offset -= Vector128<TValue>.Count;
                        continue;
                    }

                    return ComputeLastIndex(offset, equals);
                }

                // Process the first vector in the search space.

                current = Vector128.LoadUnsafe(ref searchSpace);
                equals = TNegator.NegateIfNeeded(Vector128.Equals(current, values0) | Vector128.Equals(current, values1) | Vector128.Equals(current, values2));

                if (equals != Vector128<TValue>.Zero) {
                    return ComputeLastIndex(offset: 0, equals);
                }
#endif // NET7_0_OR_GREATER
            }

            if (true) {
                TSize offset = length - 1;
                TValue lookUp;

                if (typeof(TValue) == typeof(byte)) // this optimization is beneficial only to byte
                {
                    while (length >= 8) {
                        length -= 8;

                        ref TValue current = ref ExUnsafe.Add(ref searchSpace, offset);
                        lookUp = current;
                        if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1) || lookUp.Equals(value2))) goto Found;
                        lookUp = ExUnsafe.Add(ref current, -1);
                        if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1) || lookUp.Equals(value2))) goto FoundM1;
                        lookUp = ExUnsafe.Add(ref current, -2);
                        if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1) || lookUp.Equals(value2))) goto FoundM2;
                        lookUp = ExUnsafe.Add(ref current, -3);
                        if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1) || lookUp.Equals(value2))) goto FoundM3;
                        lookUp = ExUnsafe.Add(ref current, -4);
                        if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1) || lookUp.Equals(value2))) goto FoundM4;
                        lookUp = ExUnsafe.Add(ref current, -5);
                        if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1) || lookUp.Equals(value2))) goto FoundM5;
                        lookUp = ExUnsafe.Add(ref current, -6);
                        if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1) || lookUp.Equals(value2))) goto FoundM6;
                        lookUp = ExUnsafe.Add(ref current, -7);
                        if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1) || lookUp.Equals(value2))) goto FoundM7;

                        offset -= 8;
                    }
                }

                while (length >= 4) {
                    length -= 4;

                    ref TValue current = ref ExUnsafe.Add(ref searchSpace, offset);
                    lookUp = current;
                    if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1) || lookUp.Equals(value2))) goto Found;
                    lookUp = ExUnsafe.Add(ref current, -1);
                    if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1) || lookUp.Equals(value2))) goto FoundM1;
                    lookUp = ExUnsafe.Add(ref current, -2);
                    if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1) || lookUp.Equals(value2))) goto FoundM2;
                    lookUp = ExUnsafe.Add(ref current, -3);
                    if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1) || lookUp.Equals(value2))) goto FoundM3;

                    offset -= 4;
                }

                while (length > 0) {
                    length -= 1;

                    lookUp = ExUnsafe.Add(ref searchSpace, offset);
                    if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1) || lookUp.Equals(value2))) goto Found;

                    offset -= 1;
                }
                return -1;
            FoundM7:
                return offset - 7;
            FoundM6:
                return offset - 6;
            FoundM5:
                return offset - 5;
            FoundM4:
                return offset - 4;
            FoundM3:
                return offset - 3;
            FoundM2:
                return offset - 2;
            FoundM1:
                return offset - 1;
            Found:
                return offset;
            }
            //return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static TSize LastIndexOfAnyValueType<T>(ref T searchSpace, T value0, T value1, T value2, T value3, TSize length) where T : struct, IEquatable<T>
#if GENERIC_MATH
            , INumber<T>
#endif // GENERIC_MATH
            => LastIndexOfAnyValueType<T, DontNegate<T>>(ref searchSpace, value0, value1, value2, value3, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static TSize LastIndexOfAnyExceptValueType<T>(ref T searchSpace, T value0, T value1, T value2, T value3, TSize length) where T : struct, IEquatable<T>
#if GENERIC_MATH
            , INumber<T>
#endif // GENERIC_MATH
            => LastIndexOfAnyValueType<T, Negate<T>>(ref searchSpace, value0, value1, value2, value3, length);

        private static TSize LastIndexOfAnyValueType<TValue,
#if GENERIC_MATH
            TNegator
#else
            TNegatorType
#endif // GENERIC_MATH
            >(ref TValue searchSpace, TValue value0, TValue value1, TValue value2, TValue value3, TSize length)
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
                Vector512<TValue> equals, current, values0 = Vector512.Create(value0), values1 = Vector512.Create(value1), values2 = Vector512.Create(value2), values3 = Vector512.Create(value3);
                TSize offset = length - Vector512<TValue>.Count;

                // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
                while (offset > 0) {
                    current = Vector512.LoadUnsafe(ref searchSpace, offset.ToUIntPtr());
                    equals = TNegator.NegateIfNeeded(Vector512.Equals(current, values0) | Vector512.Equals(current, values1)
                                            | Vector512.Equals(current, values2) | Vector512.Equals(current, values3));
                    if (equals == Vector512<TValue>.Zero) {
                        offset -= Vector512<TValue>.Count;
                        continue;
                    }

                    return ComputeLastIndex(offset, equals);
                }

                // Process the first vector in the search space.

                current = Vector512.LoadUnsafe(ref searchSpace);
                equals = TNegator.NegateIfNeeded(Vector512.Equals(current, values0) | Vector512.Equals(current, values1) | Vector512.Equals(current, values2) | Vector512.Equals(current, values3));

                if (equals != Vector512<TValue>.Zero) {
                    return ComputeLastIndex(offset: 0, equals);
                }
#endif // NET8_0_OR_GREATER
            } else if (Vector.IsHardwareAccelerated && length >= Vector<TValue>.Count) {
                Vector<TValue> equals, current, values0 = Vectors.Create(value0), values1 = Vectors.Create(value1), values2 = Vectors.Create(value2), values3 = Vectors.Create(value3);
                TSize offset = length - Vector<TValue>.Count;

                // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
                while (offset > 0) {
                    current = VectorHelper.LoadUnsafe(ref searchSpace, offset.ToUIntPtr());
                    equals = TNegator.NegateIfNeeded(Vector.Equals(current, values0) | Vector.Equals(current, values1)
                                            | Vector.Equals(current, values2) | Vector.Equals(current, values3));
                    if (equals == Vector<TValue>.Zero) {
                        offset -= Vector<TValue>.Count;
                        continue;
                    }

                    return ComputeLastIndex(offset, equals);
                }

                // Process the first vector in the search space.

                current = VectorHelper.LoadUnsafe(ref searchSpace);
                equals = TNegator.NegateIfNeeded(Vector.Equals(current, values0) | Vector.Equals(current, values1) | Vector.Equals(current, values2) | Vector.Equals(current, values3));

                if (equals != Vector<TValue>.Zero) {
                    return ComputeLastIndex(offset: 0, equals);
                }
#if NET7_0_OR_GREATER
            } else if (Vector256.IsHardwareAccelerated && length >= Vector256<TValue>.Count) {
                Vector256<TValue> equals, current, values0 = Vector256.Create(value0), values1 = Vector256.Create(value1), values2 = Vector256.Create(value2), values3 = Vector256.Create(value3);
                TSize offset = length - Vector256<TValue>.Count;

                // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
                while (offset > 0) {
                    current = Vector256.LoadUnsafe(ref searchSpace, offset.ToUIntPtr());
                    equals = TNegator.NegateIfNeeded(Vector256.Equals(current, values0) | Vector256.Equals(current, values1)
                                            | Vector256.Equals(current, values2) | Vector256.Equals(current, values3));
                    if (equals == Vector256<TValue>.Zero) {
                        offset -= Vector256<TValue>.Count;
                        continue;
                    }

                    return ComputeLastIndex(offset, equals);
                }

                // Process the first vector in the search space.

                current = Vector256.LoadUnsafe(ref searchSpace);
                equals = TNegator.NegateIfNeeded(Vector256.Equals(current, values0) | Vector256.Equals(current, values1) | Vector256.Equals(current, values2) | Vector256.Equals(current, values3));

                if (equals != Vector256<TValue>.Zero) {
                    return ComputeLastIndex(offset: 0, equals);
                }
            } else if (Vector128.IsHardwareAccelerated && length >= Vector128<TValue>.Count) {
                Vector128<TValue> equals, current, values0 = Vector128.Create(value0), values1 = Vector128.Create(value1), values2 = Vector128.Create(value2), values3 = Vector128.Create(value3);
                TSize offset = length - Vector128<TValue>.Count;

                // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
                while (offset > 0) {
                    current = Vector128.LoadUnsafe(ref searchSpace, offset.ToUIntPtr());
                    equals = TNegator.NegateIfNeeded(Vector128.Equals(current, values0) | Vector128.Equals(current, values1) | Vector128.Equals(current, values2) | Vector128.Equals(current, values3));

                    if (equals == Vector128<TValue>.Zero) {
                        offset -= Vector128<TValue>.Count;
                        continue;
                    }

                    return ComputeLastIndex(offset, equals);
                }

                // Process the first vector in the search space.

                current = Vector128.LoadUnsafe(ref searchSpace);
                equals = TNegator.NegateIfNeeded(Vector128.Equals(current, values0) | Vector128.Equals(current, values1) | Vector128.Equals(current, values2) | Vector128.Equals(current, values3));

                if (equals != Vector128<TValue>.Zero) {
                    return ComputeLastIndex(offset: 0, equals);
                }
#endif // NET7_0_OR_GREATER
            }

            if (true) {
                TSize offset = length - 1;
                TValue lookUp;

                while (length >= 4) {
                    length -= 4;

                    ref TValue current = ref ExUnsafe.Add(ref searchSpace, offset);
                    lookUp = current;
                    if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1) || lookUp.Equals(value2) || lookUp.Equals(value3))) goto Found;
                    lookUp = ExUnsafe.Add(ref current, -1);
                    if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1) || lookUp.Equals(value2) || lookUp.Equals(value3))) goto FoundM1;
                    lookUp = ExUnsafe.Add(ref current, -2);
                    if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1) || lookUp.Equals(value2) || lookUp.Equals(value3))) goto FoundM2;
                    lookUp = ExUnsafe.Add(ref current, -3);
                    if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1) || lookUp.Equals(value2) || lookUp.Equals(value3))) goto FoundM3;

                    offset -= 4;
                }

                while (length > 0) {
                    length -= 1;

                    lookUp = ExUnsafe.Add(ref searchSpace, offset);
                    if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1) || lookUp.Equals(value2) || lookUp.Equals(value3))) goto Found;

                    offset -= 1;
                }
                return -1;
            FoundM3:
                return offset - 3;
            FoundM2:
                return offset - 2;
            FoundM1:
                return offset - 1;
            Found:
                return offset;
            }
            //return -1;
        }

        public static void Replace<T>(ref T src, ref T dst, T oldValue, T newValue, nuint length) where T : IEquatable<T>? {
            if (default(T) is not null || oldValue is not null) {
                Debug.Assert(oldValue is not null);

                for (nuint idx = 0; idx < length; ++idx) {
                    T original = ExUnsafe.Add(ref src, idx);
                    ExUnsafe.Add(ref dst, idx) = oldValue!.Equals(original) ? newValue : original;
                }
            } else {
                for (nuint idx = 0; idx < length; ++idx) {
                    T original = ExUnsafe.Add(ref src, idx);
                    ExUnsafe.Add(ref dst, idx) = original is null ? newValue : original;
                }
            }
        }

        public static void ReplaceValueType<T>(ref T src, ref T dst, T oldValue, T newValue, nuint length) where T : struct {
            nuint idx = 0;
            if (false) {
#if NET8_0_OR_GREATER
            } else if (Vector512.IsHardwareAccelerated && length >= (uint)Vector512<T>.Count && Vector512<byte>.Count >= Vector<byte>.Count) {
                Debug.Assert(Vector512.IsHardwareAccelerated && Vector512<T>.IsSupported, "Vector512 is not HW-accelerated or not supported");

                nuint lastVectorIndex = length - (uint)Vector512<T>.Count;
                Vector512<T> oldValues = Vector512.Create(oldValue);
                Vector512<T> newValues = Vector512.Create(newValue);
                Vector512<T> original, mask, result;

                do {
                    original = Vector512.LoadUnsafe(ref src, idx);
                    mask = Vector512.Equals(oldValues, original);
                    result = Vector512.ConditionalSelect(mask, newValues, original);
                    result.StoreUnsafe(ref dst, idx);

                    idx += (uint)Vector512<T>.Count;
                }
                while (idx < lastVectorIndex);

                original = Vector512.LoadUnsafe(ref src, lastVectorIndex);
                mask = Vector512.Equals(oldValues, original);
                result = Vector512.ConditionalSelect(mask, newValues, original);
                result.StoreUnsafe(ref dst, lastVectorIndex);
#endif // NET8_0_OR_GREATER
            } else if (Vector.IsHardwareAccelerated && length >= (uint)Vector<T>.Count) {
                nuint lastVectorIndex = length - (uint)Vector<T>.Count;
                Vector<T> oldValues = Vectors.Create(oldValue);
                Vector<T> newValues = Vectors.Create(newValue);
                Vector<T> original, mask, result;

                do {
                    original = VectorHelper.LoadUnsafe(ref src, idx);
                    mask = Vector.Equals(oldValues, original);
                    result = Vector.ConditionalSelect(mask, newValues, original);
                    result.StoreUnsafe(ref dst, idx);

                    idx += (uint)Vector<T>.Count;
                }
                while (idx < lastVectorIndex);

                original = VectorHelper.LoadUnsafe(ref src, lastVectorIndex);
                mask = Vector.Equals(oldValues, original);
                result = Vector.ConditionalSelect(mask, newValues, original);
                result.StoreUnsafe(ref dst, lastVectorIndex);
#if NET7_0_OR_GREATER
            } else if (Vector256.IsHardwareAccelerated && length >= (uint)Vector256<T>.Count) {
                nuint lastVectorIndex = length - (uint)Vector256<T>.Count;
                Vector256<T> oldValues = Vector256.Create(oldValue);
                Vector256<T> newValues = Vector256.Create(newValue);
                Vector256<T> original, mask, result;

                do {
                    original = Vector256.LoadUnsafe(ref src, idx);
                    mask = Vector256.Equals(oldValues, original);
                    result = Vector256.ConditionalSelect(mask, newValues, original);
                    result.StoreUnsafe(ref dst, idx);

                    idx += (uint)Vector256<T>.Count;
                }
                while (idx < lastVectorIndex);

                original = Vector256.LoadUnsafe(ref src, lastVectorIndex);
                mask = Vector256.Equals(oldValues, original);
                result = Vector256.ConditionalSelect(mask, newValues, original);
                result.StoreUnsafe(ref dst, lastVectorIndex);
            } else if (Vector128.IsHardwareAccelerated && length >= (uint)Vector128<T>.Count) {
                nuint lastVectorIndex = length - (uint)Vector128<T>.Count;
                Vector128<T> oldValues = Vector128.Create(oldValue);
                Vector128<T> newValues = Vector128.Create(newValue);
                Vector128<T> original, mask, result;

                do {
                    original = Vector128.LoadUnsafe(ref src, idx);
                    mask = Vector128.Equals(oldValues, original);
                    result = Vector128.ConditionalSelect(mask, newValues, original);
                    result.StoreUnsafe(ref dst, idx);

                    idx += (uint)Vector128<T>.Count;
                }
                while (idx < lastVectorIndex);

                // There are (0, Vector128<T>.Count] elements remaining now.
                // As the operation is idempotent, and we know that in total there are at least Vector128<T>.Count
                // elements available, we read a vector from the very end, perform the replace and write to the
                // the resulting vector at the very end.
                // Thus we can eliminate the scalar processing of the remaining elements.
                original = Vector128.LoadUnsafe(ref src, lastVectorIndex);
                mask = Vector128.Equals(oldValues, original);
                result = Vector128.ConditionalSelect(mask, newValues, original);
                result.StoreUnsafe(ref dst, lastVectorIndex);
#endif // NET7_0_OR_GREATER
            }
            if (true) {
                for (idx = 0; idx < length; ++idx) {
                    T original = ExUnsafe.Add(ref src, idx);
                    ExUnsafe.Add(ref dst, idx) = EqualityComparer<T>.Default.Equals(original, oldValue) ? newValue : original;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static TSize LastIndexOfAnyValueType<T>(ref T searchSpace, T value0, T value1, T value2, T value3, T value4, TSize length) where T : struct, IEquatable<T>
#if GENERIC_MATH
            , INumber<T>
#endif // GENERIC_MATH
            => LastIndexOfAnyValueType<T, DontNegate<T>>(ref searchSpace, value0, value1, value2, value3, value4, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static TSize LastIndexOfAnyExceptValueType<T>(ref T searchSpace, T value0, T value1, T value2, T value3, T value4, TSize length) where T : struct, IEquatable<T>
#if GENERIC_MATH
            , INumber<T>
#endif // GENERIC_MATH
            => LastIndexOfAnyValueType<T, Negate<T>>(ref searchSpace, value0, value1, value2, value3, value4, length);

        private static TSize LastIndexOfAnyValueType<TValue,
#if GENERIC_MATH
            TNegator
#else
            TNegatorType
#endif // GENERIC_MATH
            >(ref TValue searchSpace, TValue value0, TValue value1, TValue value2, TValue value3, TValue value4, TSize length)
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
                    values2 = Vector512.Create(value2), values3 = Vector512.Create(value3), values4 = Vector512.Create(value4);
                TSize offset = length - Vector512<TValue>.Count;

                // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
                while (offset > 0) {
                    current = Vector512.LoadUnsafe(ref searchSpace, offset.ToUIntPtr());
                    equals = TNegator.NegateIfNeeded(Vector512.Equals(current, values0) | Vector512.Equals(current, values1) | Vector512.Equals(current, values2)
                        | Vector512.Equals(current, values3) | Vector512.Equals(current, values4));
                    if (equals == Vector512<TValue>.Zero) {
                        offset -= Vector512<TValue>.Count;
                        continue;
                    }

                    return ComputeLastIndex(offset, equals);
                }

                // Process the first vector in the search space.

                current = Vector512.LoadUnsafe(ref searchSpace);
                equals = TNegator.NegateIfNeeded(Vector512.Equals(current, values0) | Vector512.Equals(current, values1) | Vector512.Equals(current, values2)
                    | Vector512.Equals(current, values3) | Vector512.Equals(current, values4));

                if (equals != Vector512<TValue>.Zero) {
                    return ComputeLastIndex(offset: 0, equals);
                }
#endif // NET8_0_OR_GREATER
            } else if (Vector.IsHardwareAccelerated && length >= Vector<TValue>.Count) {
                Vector<TValue> equals, current, values0 = Vectors.Create(value0), values1 = Vectors.Create(value1),
                    values2 = Vectors.Create(value2), values3 = Vectors.Create(value3), values4 = Vectors.Create(value4);
                TSize offset = length - Vector<TValue>.Count;

                // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
                while (offset > 0) {
                    current = VectorHelper.LoadUnsafe(ref searchSpace, offset.ToUIntPtr());
                    equals = TNegator.NegateIfNeeded(Vector.Equals(current, values0) | Vector.Equals(current, values1) | Vector.Equals(current, values2)
                        | Vector.Equals(current, values3) | Vector.Equals(current, values4));
                    if (equals == Vector<TValue>.Zero) {
                        offset -= Vector<TValue>.Count;
                        continue;
                    }

                    return ComputeLastIndex(offset, equals);
                }

                // Process the first vector in the search space.

                current = VectorHelper.LoadUnsafe(ref searchSpace);
                equals = TNegator.NegateIfNeeded(Vector.Equals(current, values0) | Vector.Equals(current, values1) | Vector.Equals(current, values2)
                    | Vector.Equals(current, values3) | Vector.Equals(current, values4));

                if (equals != Vector<TValue>.Zero) {
                    return ComputeLastIndex(offset: 0, equals);
                }
#if NET7_0_OR_GREATER
            } else if (Vector256.IsHardwareAccelerated && length >= Vector256<TValue>.Count) {
                Vector256<TValue> equals, current, values0 = Vector256.Create(value0), values1 = Vector256.Create(value1),
                    values2 = Vector256.Create(value2), values3 = Vector256.Create(value3), values4 = Vector256.Create(value4);
                TSize offset = length - Vector256<TValue>.Count;

                // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
                while (offset > 0) {
                    current = Vector256.LoadUnsafe(ref searchSpace, offset.ToUIntPtr());
                    equals = TNegator.NegateIfNeeded(Vector256.Equals(current, values0) | Vector256.Equals(current, values1) | Vector256.Equals(current, values2)
                        | Vector256.Equals(current, values3) | Vector256.Equals(current, values4));
                    if (equals == Vector256<TValue>.Zero) {
                        offset -= Vector256<TValue>.Count;
                        continue;
                    }

                    return ComputeLastIndex(offset, equals);
                }

                // Process the first vector in the search space.

                current = Vector256.LoadUnsafe(ref searchSpace);
                equals = TNegator.NegateIfNeeded(Vector256.Equals(current, values0) | Vector256.Equals(current, values1) | Vector256.Equals(current, values2)
                    | Vector256.Equals(current, values3) | Vector256.Equals(current, values4));

                if (equals != Vector256<TValue>.Zero) {
                    return ComputeLastIndex(offset: 0, equals);
                }
            } else if (Vector128.IsHardwareAccelerated && length >= Vector128<TValue>.Count) {
                Vector128<TValue> equals, current, values0 = Vector128.Create(value0), values1 = Vector128.Create(value1),
                    values2 = Vector128.Create(value2), values3 = Vector128.Create(value3), values4 = Vector128.Create(value4);
                TSize offset = length - Vector128<TValue>.Count;

                // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
                while (offset > 0) {
                    current = Vector128.LoadUnsafe(ref searchSpace, offset.ToUIntPtr());
                    equals = TNegator.NegateIfNeeded(Vector128.Equals(current, values0) | Vector128.Equals(current, values1) | Vector128.Equals(current, values2)
                        | Vector128.Equals(current, values3) | Vector128.Equals(current, values4));

                    if (equals == Vector128<TValue>.Zero) {
                        offset -= Vector128<TValue>.Count;
                        continue;
                    }

                    return ComputeLastIndex(offset, equals);
                }

                // Process the first vector in the search space.

                current = Vector128.LoadUnsafe(ref searchSpace);
                equals = TNegator.NegateIfNeeded(Vector128.Equals(current, values0) | Vector128.Equals(current, values1) | Vector128.Equals(current, values2)
                    | Vector128.Equals(current, values3) | Vector128.Equals(current, values4));

                if (equals != Vector128<TValue>.Zero) {
                    return ComputeLastIndex(offset: 0, equals);
                }
#endif // NET7_0_OR_GREATER
            }

            if (true) {
                TSize offset = length - 1;
                TValue lookUp;

                while (length >= 4) {
                    length -= 4;

                    ref TValue current = ref ExUnsafe.Add(ref searchSpace, offset);
                    lookUp = current;
                    if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1) || lookUp.Equals(value2) || lookUp.Equals(value3) || lookUp.Equals(value4))) return offset;
                    lookUp = ExUnsafe.Add(ref current, -1);
                    if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1) || lookUp.Equals(value2) || lookUp.Equals(value3) || lookUp.Equals(value4))) return offset - 1;
                    lookUp = ExUnsafe.Add(ref current, -2);
                    if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1) || lookUp.Equals(value2) || lookUp.Equals(value3) || lookUp.Equals(value4))) return offset - 2;
                    lookUp = ExUnsafe.Add(ref current, -3);
                    if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1) || lookUp.Equals(value2) || lookUp.Equals(value3) || lookUp.Equals(value4))) return offset - 3;

                    offset -= 4;
                }

                while (length > 0) {
                    length -= 1;

                    lookUp = ExUnsafe.Add(ref searchSpace, offset);
                    if (TNegator.NegateIfNeeded(lookUp.Equals(value0) || lookUp.Equals(value1) || lookUp.Equals(value2) || lookUp.Equals(value3) || lookUp.Equals(value4))) return offset;

                    offset -= 1;
                }
            }
            return -1;
        }

#if NET7_0_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static nint ComputeFirstIndex<T>(ref T searchSpace, ref T current, Vector128<T> equals) where T : struct {
            uint notEqualsElements = equals.ExtractMostSignificantBits();
            int index = BitOperations.TrailingZeroCount(notEqualsElements);
            return index + Unsafe.ByteOffset(ref searchSpace, ref current) / Unsafe.SizeOf<T>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static nint ComputeFirstIndex<T>(ref T searchSpace, ref T current, Vector256<T> equals) where T : struct {
            uint notEqualsElements = equals.ExtractMostSignificantBits();
            int index = BitOperations.TrailingZeroCount(notEqualsElements);
            return index + Unsafe.ByteOffset(ref searchSpace, ref current) / Unsafe.SizeOf<T>();
        }
#endif // NET7_0_OR_GREATER

#if NET8_0_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static nint ComputeFirstIndex<T>(ref T searchSpace, ref T current, Vector512<T> equals) where T : struct {
            ulong notEqualsElements = equals.ExtractMostSignificantBits();
            int index = BitOperations.TrailingZeroCount(notEqualsElements);
            return index + Unsafe.ByteOffset(ref searchSpace, ref current) / Unsafe.SizeOf<T>();
        }
#endif // NET8_0_OR_GREATER

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static nint ComputeFirstIndex<T>(ref T searchSpace, ref T current, Vector<T> equals) where T : struct {
            ulong notEqualsElements = equals.ExtractMostSignificantBits();
            int index = MathBitOperations.TrailingZeroCount(notEqualsElements);
            return index + Unsafe.ByteOffset(ref searchSpace, ref current) / (nint)Unsafe.SizeOf<T>();
        }

#if NET7_0_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static nint ComputeLastIndex<T>(nint offset, Vector128<T> equals) where T : struct {
            uint notEqualsElements = equals.ExtractMostSignificantBits();
            int index = 31 - BitOperations.LeadingZeroCount(notEqualsElements); // 31 = 32 (bits in Int32) - 1 (indexing from zero)
            return offset + index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static nint ComputeLastIndex<T>(nint offset, Vector256<T> equals) where T : struct {
            uint notEqualsElements = equals.ExtractMostSignificantBits();
            int index = 31 - BitOperations.LeadingZeroCount(notEqualsElements); // 31 = 32 (bits in Int32) - 1 (indexing from zero)
            return offset + index;
        }
#endif // NET7_0_OR_GREATER

#if NET8_0_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static nint ComputeLastIndex<T>(nint offset, Vector512<T> equals) where T : struct {
            ulong notEqualsElements = equals.ExtractMostSignificantBits();
            int index = 63 - BitOperations.LeadingZeroCount(notEqualsElements); // 31 = 32 (bits in Int32) - 1 (indexing from zero)
            return offset + index;
        }
#endif // NET8_0_OR_GREATER

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static TSize ComputeLastIndex<T>(TSize offset, Vector<T> equals) where T : struct {
            ulong notEqualsElements = equals.ExtractMostSignificantBits();
            int index = 63 - MathBitOperations.LeadingZeroCount(notEqualsElements); // 31 = 32 (bits in Int32) - 1 (indexing from zero)
            return offset + index;
        }

        internal static TSize IndexOfAnyInRange<T>(ref T searchSpace, T lowInclusive, T highInclusive, TSize length)
            where T : IComparable<T> {
            for (TSize i = 0; i < length; i++) {
                ref T current = ref ExUnsafe.Add(ref searchSpace, i);
                if ((lowInclusive.CompareTo(current) <= 0) && (highInclusive.CompareTo(current) >= 0)) {
                    return i;
                }
            }

            return -1;
        }

        internal static TSize IndexOfAnyExceptInRange<T>(ref T searchSpace, T lowInclusive, T highInclusive, TSize length)
            where T : IComparable<T> {
            for (TSize i = 0; i < length; i++) {
                ref T current = ref ExUnsafe.Add(ref searchSpace, i);
                if ((lowInclusive.CompareTo(current) > 0) || (highInclusive.CompareTo(current) < 0)) {
                    return i;
                }
            }

            return -1;
        }

#if GENERIC_MATH
        internal static TSize IndexOfAnyInRangeUnsignedNumber<T>(ref T searchSpace, T lowInclusive, T highInclusive, TSize length)
            where T : struct, IUnsignedNumber<T>, IComparisonOperators<T, T, bool> =>
            IndexOfAnyInRangeUnsignedNumber<T, DontNegate<T>>(ref searchSpace, lowInclusive, highInclusive, length);

        internal static TSize IndexOfAnyExceptInRangeUnsignedNumber<T>(ref T searchSpace, T lowInclusive, T highInclusive, TSize length)
            where T : struct, IUnsignedNumber<T>, IComparisonOperators<T, T, bool> =>
            IndexOfAnyInRangeUnsignedNumber<T, Negate<T>>(ref searchSpace, lowInclusive, highInclusive, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static TSize IndexOfAnyInRangeUnsignedNumber<T, TNegator>(ref T searchSpace, T lowInclusive, T highInclusive, TSize length)
            where T : struct, IUnsignedNumber<T>, IComparisonOperators<T, T, bool>
            where TNegator : struct, INegator<T> {
            //if (PackedSpanHelpers.PackedIndexOfIsSupported && typeof(T) == typeof(ushort) && PackedSpanHelpers.CanUsePackedIndexOf(lowInclusive) && PackedSpanHelpers.CanUsePackedIndexOf(highInclusive) && highInclusive >= lowInclusive) {
            //    ref char charSearchSpace = ref Unsafe.As<T, char>(ref searchSpace);
            //    char charLowInclusive = Unsafe.BitCast<T, char>(lowInclusive);
            //    char charRange = (char)(Unsafe.BitCast<T, char>(highInclusive) - charLowInclusive);

            //    return typeof(TNegator) == typeof(DontNegate<ushort>)
            //        ? PackedSpanHelpers.IndexOfAnyInRange(ref charSearchSpace, charLowInclusive, charRange, length)
            //        : PackedSpanHelpers.IndexOfAnyExceptInRange(ref charSearchSpace, charLowInclusive, charRange, length);
            //}

            return NonPackedIndexOfAnyInRangeUnsignedNumber<T, TNegator>(ref searchSpace, lowInclusive, highInclusive, length);
        }

        internal static TSize NonPackedIndexOfAnyInRangeUnsignedNumber<T, TNegator>(ref T searchSpace, T lowInclusive, T highInclusive, TSize length)
            where T : struct, IUnsignedNumber<T>, IComparisonOperators<T, T, bool>
            where TNegator : struct, INegator<T> {
            // T must be a type whose comparison operator semantics match that of Vector128/256.

            if (false) {
#if NET8_0_OR_GREATER
            } else if (Vector512.IsHardwareAccelerated && length >= Vector512<T>.Count && Vector512<byte>.Count >= Vector<byte>.Count) {
                Vector512<T> lowVector = Vector512.Create(lowInclusive);
                Vector512<T> rangeVector = Vector512.Create(highInclusive - lowInclusive);
                Vector512<T> inRangeVector;

                ref T current = ref searchSpace;
                ref T oneVectorAwayFromEnd = ref ExUnsafe.Add(ref searchSpace, length - Vector512<T>.Count);

                // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
                do {
                    inRangeVector = TNegator.NegateIfNeeded(Vector512.LessThanOrEqual(Vector512.LoadUnsafe(ref current) - lowVector, rangeVector));
                    if (inRangeVector != Vector512<T>.Zero) {
                        return ComputeFirstIndex(ref searchSpace, ref current, inRangeVector);
                    }

                    current = ref ExUnsafe.Add(ref current, Vector256<T>.Count);
                }
                while (Unsafe.IsAddressLessThan(ref current, ref oneVectorAwayFromEnd));

                // Process the last vector in the search space (which might overlap with already processed elements).
                inRangeVector = TNegator.NegateIfNeeded(Vector512.LessThanOrEqual(Vector512.LoadUnsafe(ref oneVectorAwayFromEnd) - lowVector, rangeVector));
                if (inRangeVector != Vector512<T>.Zero) {
                    return ComputeFirstIndex(ref searchSpace, ref oneVectorAwayFromEnd, inRangeVector);
                }
#endif // NET8_0_OR_GREATER
            } else if (Vector.IsHardwareAccelerated && length >= Vector<T>.Count) {
                Vector<T> lowVector = Vectors.Create(lowInclusive);
                Vector<T> rangeVector = Vectors.Create(highInclusive - lowInclusive);
                Vector<T> inRangeVector;

                ref T current = ref searchSpace;
                ref T oneVectorAwayFromEnd = ref ExUnsafe.Add(ref searchSpace, length - Vector<T>.Count);

                // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
                do {
                    inRangeVector = TNegator.NegateIfNeeded(Vector.LessThanOrEqual(VectorHelper.LoadUnsafe(ref current) - lowVector, rangeVector));
                    if (inRangeVector != Vector<T>.Zero) {
                        return ComputeFirstIndex(ref searchSpace, ref current, inRangeVector);
                    }

                    current = ref ExUnsafe.Add(ref current, Vector<T>.Count);
                }
                while (Unsafe.IsAddressLessThan(ref current, ref oneVectorAwayFromEnd));

                // Process the last vector in the search space (which might overlap with already processed elements).
                inRangeVector = TNegator.NegateIfNeeded(Vector.LessThanOrEqual(VectorHelper.LoadUnsafe(ref oneVectorAwayFromEnd) - lowVector, rangeVector));
                if (inRangeVector != Vector<T>.Zero) {
                    return ComputeFirstIndex(ref searchSpace, ref oneVectorAwayFromEnd, inRangeVector);
                }
            } else if (Vector256.IsHardwareAccelerated && length >= Vector256<T>.Count) {
                Vector256<T> lowVector = Vector256.Create(lowInclusive);
                Vector256<T> rangeVector = Vector256.Create(highInclusive - lowInclusive);
                Vector256<T> inRangeVector;

                ref T current = ref searchSpace;
                ref T oneVectorAwayFromEnd = ref ExUnsafe.Add(ref searchSpace, length - Vector256<T>.Count);

                // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
                do {
                    inRangeVector = TNegator.NegateIfNeeded(Vector256.LessThanOrEqual(Vector256.LoadUnsafe(ref current) - lowVector, rangeVector));
                    if (inRangeVector != Vector256<T>.Zero) {
                        return ComputeFirstIndex(ref searchSpace, ref current, inRangeVector);
                    }

                    current = ref ExUnsafe.Add(ref current, Vector256<T>.Count);
                }
                while (Unsafe.IsAddressLessThan(ref current, ref oneVectorAwayFromEnd));

                // Process the last vector in the search space (which might overlap with already processed elements).
                inRangeVector = TNegator.NegateIfNeeded(Vector256.LessThanOrEqual(Vector256.LoadUnsafe(ref oneVectorAwayFromEnd) - lowVector, rangeVector));
                if (inRangeVector != Vector256<T>.Zero) {
                    return ComputeFirstIndex(ref searchSpace, ref oneVectorAwayFromEnd, inRangeVector);
                }
            } else if (Vector128.IsHardwareAccelerated && length >= Vector128<T>.Count) {
                Vector128<T> lowVector = Vector128.Create(lowInclusive);
                Vector128<T> rangeVector = Vector128.Create(highInclusive - lowInclusive);
                Vector128<T> inRangeVector;

                ref T current = ref searchSpace;
                ref T oneVectorAwayFromEnd = ref ExUnsafe.Add(ref searchSpace, length - Vector128<T>.Count);

                // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
                do {
                    inRangeVector = TNegator.NegateIfNeeded(Vector128.LessThanOrEqual(Vector128.LoadUnsafe(ref current) - lowVector, rangeVector));
                    if (inRangeVector != Vector128<T>.Zero) {
                        return ComputeFirstIndex(ref searchSpace, ref current, inRangeVector);
                    }

                    current = ref ExUnsafe.Add(ref current, Vector128<T>.Count);
                }
                while (Unsafe.IsAddressLessThan(ref current, ref oneVectorAwayFromEnd));

                // Process the last vector in the search space (which might overlap with already processed elements).
                inRangeVector = TNegator.NegateIfNeeded(Vector128.LessThanOrEqual(Vector128.LoadUnsafe(ref oneVectorAwayFromEnd) - lowVector, rangeVector));
                if (inRangeVector != Vector128<T>.Zero) {
                    return ComputeFirstIndex(ref searchSpace, ref oneVectorAwayFromEnd, inRangeVector);
                }
            }

            if (true) {
                T rangeInclusive = highInclusive - lowInclusive;
                for (TSize i = 0; i < length; i++) {
                    ref T current = ref ExUnsafe.Add(ref searchSpace, i);
                    if (TNegator.NegateIfNeeded((current - lowInclusive) <= rangeInclusive)) {
                        return i;
                    }
                }
            }
            return -1;
        }
#endif // GENERIC_MATH

        internal static TSize LastIndexOfAnyInRange<T>(ref T searchSpace, T lowInclusive, T highInclusive, TSize length)
            where T : IComparable<T> {
            for (TSize i = length - 1; i >= 0; i--) {
                ref T current = ref ExUnsafe.Add(ref searchSpace, i);
                if ((lowInclusive.CompareTo(current) <= 0) && (highInclusive.CompareTo(current) >= 0)) {
                    return i;
                }
            }

            return -1;
        }

        internal static TSize LastIndexOfAnyExceptInRange<T>(ref T searchSpace, T lowInclusive, T highInclusive, TSize length)
            where T : IComparable<T> {
            for (TSize i = length - 1; i >= 0; i--) {
                ref T current = ref ExUnsafe.Add(ref searchSpace, i);
                if ((lowInclusive.CompareTo(current) > 0) || (highInclusive.CompareTo(current) < 0)) {
                    return i;
                }
            }

            return -1;
        }

#if GENERIC_MATH
        internal static TSize LastIndexOfAnyInRangeUnsignedNumber<T>(ref T searchSpace, T lowInclusive, T highInclusive, TSize length)
            where T : struct, IUnsignedNumber<T>, IComparisonOperators<T, T, bool> =>
            LastIndexOfAnyInRangeUnsignedNumber<T, DontNegate<T>>(ref searchSpace, lowInclusive, highInclusive, length);

        internal static TSize LastIndexOfAnyExceptInRangeUnsignedNumber<T>(ref T searchSpace, T lowInclusive, T highInclusive, TSize length)
            where T : struct, IUnsignedNumber<T>, IComparisonOperators<T, T, bool> =>
            LastIndexOfAnyInRangeUnsignedNumber<T, Negate<T>>(ref searchSpace, lowInclusive, highInclusive, length);

        private static TSize LastIndexOfAnyInRangeUnsignedNumber<T, TNegator>(ref T searchSpace, T lowInclusive, T highInclusive, TSize length)
            where T : struct, IUnsignedNumber<T>, IComparisonOperators<T, T, bool>
            where TNegator : struct, INegator<T> {
            // T must be a type whose comparison operator semantics match that of Vector128/256.

            if (false) {
#if NET8_0_OR_GREATER
            } else if (Vector512.IsHardwareAccelerated && length >= Vector512<T>.Count && Vector512<byte>.Count >= Vector<byte>.Count) {
                Vector512<T> lowVector = Vector512.Create(lowInclusive);
                Vector512<T> rangeVector = Vector512.Create(highInclusive - lowInclusive);
                Vector512<T> inRangeVector;

                TSize offset = length - Vector512<T>.Count;

                // Loop until either we've finished all elements or there's a vector's-worth or less remaining.
                while (offset > 0) {
                    inRangeVector = TNegator.NegateIfNeeded(Vector512.LessThanOrEqual(Vector512.LoadUnsafe(ref searchSpace, (nuint)offset) - lowVector, rangeVector));
                    if (inRangeVector != Vector512<T>.Zero) {
                        return ComputeLastIndex(offset, inRangeVector);
                    }

                    offset -= Vector512<T>.Count;
                }

                // Process the first vector in the search space.
                inRangeVector = TNegator.NegateIfNeeded(Vector512.LessThanOrEqual(Vector512.LoadUnsafe(ref searchSpace) - lowVector, rangeVector));
                if (inRangeVector != Vector512<T>.Zero) {
                    return ComputeLastIndex(offset: 0, inRangeVector);
                }
#endif // NET8_0_OR_GREATER
            } else if (Vector.IsHardwareAccelerated && length >= Vector<T>.Count) {
                Vector<T> lowVector = Vectors.Create(lowInclusive);
                Vector<T> rangeVector = Vectors.Create(highInclusive - lowInclusive);
                Vector<T> inRangeVector;

                TSize offset = length - Vector<T>.Count;

                // Loop until either we've finished all elements or there's a vector's-worth or less remaining.
                while (offset > 0) {
                    inRangeVector = TNegator.NegateIfNeeded(Vector.LessThanOrEqual(VectorHelper.LoadUnsafe(ref searchSpace, (nuint)offset) - lowVector, rangeVector));
                    if (inRangeVector != Vector<T>.Zero) {
                        return ComputeLastIndex(offset, inRangeVector);
                    }

                    offset -= Vector<T>.Count;
                }

                // Process the first vector in the search space.
                inRangeVector = TNegator.NegateIfNeeded(Vector.LessThanOrEqual(VectorHelper.LoadUnsafe(ref searchSpace) - lowVector, rangeVector));
                if (inRangeVector != Vector<T>.Zero) {
                    return ComputeLastIndex(offset: 0, inRangeVector);
                }
#if NET7_0_OR_GREATER
            } else if (Vector256.IsHardwareAccelerated && length >= Vector256<T>.Count) {
                Vector256<T> lowVector = Vector256.Create(lowInclusive);
                Vector256<T> rangeVector = Vector256.Create(highInclusive - lowInclusive);
                Vector256<T> inRangeVector;

                TSize offset = length - Vector256<T>.Count;

                // Loop until either we've finished all elements or there's a vector's-worth or less remaining.
                while (offset > 0) {
                    inRangeVector = TNegator.NegateIfNeeded(Vector256.LessThanOrEqual(Vector256.LoadUnsafe(ref searchSpace, (nuint)offset) - lowVector, rangeVector));
                    if (inRangeVector != Vector256<T>.Zero) {
                        return ComputeLastIndex(offset, inRangeVector);
                    }

                    offset -= Vector256<T>.Count;
                }

                // Process the first vector in the search space.
                inRangeVector = TNegator.NegateIfNeeded(Vector256.LessThanOrEqual(Vector256.LoadUnsafe(ref searchSpace) - lowVector, rangeVector));
                if (inRangeVector != Vector256<T>.Zero) {
                    return ComputeLastIndex(offset: 0, inRangeVector);
                }
            } else if (Vector128.IsHardwareAccelerated && length >= Vector128<T>.Count) {
                Vector128<T> lowVector = Vector128.Create(lowInclusive);
                Vector128<T> rangeVector = Vector128.Create(highInclusive - lowInclusive);
                Vector128<T> inRangeVector;

                TSize offset = length - Vector128<T>.Count;

                // Loop until either we've finished all elements or there's a vector's-worth or less remaining.
                while (offset > 0) {
                    inRangeVector = TNegator.NegateIfNeeded(Vector128.LessThanOrEqual(Vector128.LoadUnsafe(ref searchSpace, (nuint)offset) - lowVector, rangeVector));
                    if (inRangeVector != Vector128<T>.Zero) {
                        return ComputeLastIndex(offset, inRangeVector);
                    }

                    offset -= Vector128<T>.Count;
                }

                // Process the first vector in the search space.
                inRangeVector = TNegator.NegateIfNeeded(Vector128.LessThanOrEqual(Vector128.LoadUnsafe(ref searchSpace) - lowVector, rangeVector));
                if (inRangeVector != Vector128<T>.Zero) {
                    return ComputeLastIndex(offset: 0, inRangeVector);
                }
#endif // NET7_0_OR_GREATER
            }

            if (true) {
                T rangeInclusive = highInclusive - lowInclusive;
                for (TSize i = length - 1; i >= 0; i--) {
                    ref T current = ref ExUnsafe.Add(ref searchSpace, i);
                    if (TNegator.NegateIfNeeded((current - lowInclusive) <= rangeInclusive)) {
                        return i;
                    }
                }
            }
            return -1;
        }
#endif // GENERIC_MATH

        public static TSize Count<T>(ref T current, T value, TSize length) where T : IEquatable<T>? {
            TSize count = 0;

            ref T end = ref ExUnsafe.Add(ref current, length);
            if (value is not null) {
                while (Unsafe.IsAddressLessThan(ref current, ref end)) {
                    if (value.Equals(current)) {
                        count++;
                    }

                    current = ref ExUnsafe.Add(ref current, 1);
                }
            } else {
                while (Unsafe.IsAddressLessThan(ref current, ref end)) {
                    if (current is null) {
                        count++;
                    }

                    current = ref ExUnsafe.Add(ref current, 1);
                }
            }

            return count;
        }

        public static TSize CountValueType<T>(ref T current, T value, TSize length) where T : struct, IEquatable<T>? {
            TSize count = 0;
            ref T end = ref ExUnsafe.Add(ref current, length);

            if (false) {
#if NET8_0_OR_GREATER
            } else if (Vector512.IsHardwareAccelerated && length >= Vector512<T>.Count && Vector512<byte>.Count >= Vector<byte>.Count) {
                Vector512<T> targetVector = Vector512.Create(value);
                ref T oneVectorAwayFromEnd = ref Unsafe.Subtract(ref end, Vector512<T>.Count);
                while (Unsafe.IsAddressLessThan(ref current, ref oneVectorAwayFromEnd)) {
                    count += BitOperations.PopCount(Vector512.Equals(Vector512.LoadUnsafe(ref current), targetVector).ExtractMostSignificantBits());
                    current = ref ExUnsafe.Add(ref current, Vector512<T>.Count);
                }

                // Count the last vector and mask off the elements that were already counted (number of elements between oneVectorAwayFromEnd and current).
                ulong mask = Vector512.Equals(Vector512.LoadUnsafe(ref oneVectorAwayFromEnd), targetVector).ExtractMostSignificantBits();
                mask >>= (int)((nuint)Unsafe.ByteOffset(ref oneVectorAwayFromEnd, ref current) / (uint)Unsafe.SizeOf<T>());
                count += BitOperations.PopCount(mask);
                return count;
#endif // NET8_0_OR_GREATER
            } else if (Vector.IsHardwareAccelerated && length >= Vector<T>.Count) {
                Vector<T> targetVector = Vectors.Create(value);
                ref T oneVectorAwayFromEnd = ref Unsafe.Subtract(ref end, Vector<T>.Count);
                while (Unsafe.IsAddressLessThan(ref current, ref oneVectorAwayFromEnd)) {
                    count += MathBitOperations.PopCount(Vector.Equals(VectorHelper.LoadUnsafe(ref current), targetVector).ExtractMostSignificantBits());
                    current = ref ExUnsafe.Add(ref current, Vector<T>.Count);
                }

                // Count the last vector and mask off the elements that were already counted (number of elements between oneVectorAwayFromEnd and current).
                ulong mask = Vector.Equals(VectorHelper.LoadUnsafe(ref oneVectorAwayFromEnd), targetVector).ExtractMostSignificantBits();
                mask >>= (int)(Unsafe.ByteOffset(ref oneVectorAwayFromEnd, ref current).ToUIntPtr() / (uint)Unsafe.SizeOf<T>());
                count += MathBitOperations.PopCount(mask);
                return count;
#if NET7_0_OR_GREATER
            } else if (Vector256.IsHardwareAccelerated && length >= Vector256<T>.Count) {
                Vector256<T> targetVector = Vector256.Create(value);
                ref T oneVectorAwayFromEnd = ref Unsafe.Subtract(ref end, Vector256<T>.Count);
                while (Unsafe.IsAddressLessThan(ref current, ref oneVectorAwayFromEnd)) {
                    count += BitOperations.PopCount(Vector256.Equals(Vector256.LoadUnsafe(ref current), targetVector).ExtractMostSignificantBits());
                    current = ref ExUnsafe.Add(ref current, Vector256<T>.Count);
                }

                // Count the last vector and mask off the elements that were already counted (number of elements between oneVectorAwayFromEnd and current).
                uint mask = Vector256.Equals(Vector256.LoadUnsafe(ref oneVectorAwayFromEnd), targetVector).ExtractMostSignificantBits();
                mask >>= (int)((nuint)Unsafe.ByteOffset(ref oneVectorAwayFromEnd, ref current) / (uint)Unsafe.SizeOf<T>());
                count += BitOperations.PopCount(mask);
                return count;
            } else if (Vector128.IsHardwareAccelerated && length >= Vector128<T>.Count) {
                Vector128<T> targetVector = Vector128.Create(value);
                ref T oneVectorAwayFromEnd = ref Unsafe.Subtract(ref end, Vector128<T>.Count);
                while (Unsafe.IsAddressLessThan(ref current, ref oneVectorAwayFromEnd)) {
                    count += BitOperations.PopCount(Vector128.Equals(Vector128.LoadUnsafe(ref current), targetVector).ExtractMostSignificantBits());
                    current = ref ExUnsafe.Add(ref current, Vector128<T>.Count);
                }

                // Count the last vector and mask off the elements that were already counted (number of elements between oneVectorAwayFromEnd and current).
                uint mask = Vector128.Equals(Vector128.LoadUnsafe(ref oneVectorAwayFromEnd), targetVector).ExtractMostSignificantBits();
                mask >>= (int)((nuint)Unsafe.ByteOffset(ref oneVectorAwayFromEnd, ref current) / (uint)Unsafe.SizeOf<T>());
                count += BitOperations.PopCount(mask);
                return count;
#endif // NET7_0_OR_GREATER
            }
            if (true) {
                while (Unsafe.IsAddressLessThan(ref current, ref end)) {
                    if (current.Equals(value)) {
                        count++;
                    }

                    current = ref ExUnsafe.Add(ref current, 1);
                }
            }
            return count;
        }

    }
}
