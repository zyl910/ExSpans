﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Zyl.VectorTraits.Numerics;

#pragma warning disable CA1822

namespace Zyl.ExSpans.Impl {
#if NET5_0_OR_GREATER
#else
    using Half = float;
#endif // NET5_0_OR_GREATER

    #region ExArraySortHelper for single arrays

    internal sealed partial class ExArraySortHelper<T> {
        #region IExArraySortHelper<T> Members

        public void Sort(ExSpan<T> keys, IComparer<T>? comparer) {
            // Add a try block here to detect IComparers (or their
            // underlying IComparables, etc) that are bogus.
            try {
                comparer ??= Comparer<T>.Default;
                IntrospectiveSort(keys, comparer.Compare);
            } catch (IndexOutOfRangeException) {
                ThrowHelper.ThrowArgumentException_BadComparer(comparer);
            } catch (Exception e) {
                ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_IComparerFailed, e);
            }
        }

        public TSize BinarySearch(T[] array, TSize index, TSize length, T value, IComparer<T>? comparer) {
            try {
                comparer ??= Comparer<T>.Default;
                return InternalBinarySearch(array, index, length, value, comparer);
            } catch (Exception e) {
                ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_IComparerFailed, e);
                return 0;
            }
        }

        #endregion

        internal static void Sort(ExSpan<T> keys, Comparison<T> comparer) {
            Debug.Assert(comparer != null, "Check the arguments in the caller!");

            // Add a try block here to detect bogus comparisons
            try {
                IntrospectiveSort(keys, comparer!);
            } catch (IndexOutOfRangeException) {
                ThrowHelper.ThrowArgumentException_BadComparer(comparer);
            } catch (Exception e) {
                ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_IComparerFailed, e);
            }
        }

        internal static TSize InternalBinarySearch(T[] array, TSize index, TSize length, T value, IComparer<T> comparer) {
            Debug.Assert(array != null, "Check the arguments in the caller!");
            if (null==array) throw new ArgumentNullException(nameof(array));
            Debug.Assert(index >= 0 && length >= 0 && (array.Length - index >= length), "Check the arguments in the caller!");

            TSize lo = index;
            TSize hi = index + length - 1;
            while (lo <= hi) {
                TSize i = lo + ((hi - lo) >> 1);
                int order = comparer.Compare(array[i], value);

                if (order == 0) return i;
                if (order < 0) {
                    lo = i + 1;
                } else {
                    hi = i - 1;
                }
            }

            return ~lo;
        }

        private static void SwapIfGreater(ExSpan<T> keys, Comparison<T> comparer, TSize i, TSize j) {
            Debug.Assert(i != j);

            if (comparer(keys[i], keys[j]) > 0) {
                T key = keys[i];
                keys[i] = keys[j];
                keys[j] = key;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Swap(ExSpan<T> a, TSize i, TSize j) {
            Debug.Assert(i != j);

            T t = a[i];
            a[i] = a[j];
            a[j] = t;
        }

        internal static void IntrospectiveSort(ExSpan<T> keys, Comparison<T> comparer) {
            Debug.Assert(comparer != null);

            if (keys.Length > 1) {
                IntroSort(keys, 2 * (MathBitOperations.Log2((nuint)keys.Length) + 1), comparer!);
            }
        }

        // IntroSort is recursive; block it from being inlined into itself as
        // this is currenly not profitable.
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void IntroSort(ExSpan<T> keys, TSize depthLimit, Comparison<T> comparer) {
            Debug.Assert(!keys.IsEmpty);
            Debug.Assert(depthLimit >= 0);
            Debug.Assert(comparer != null);
            if (null == comparer) throw new ArgumentNullException(nameof(comparer));

            TSize partitionSize = keys.Length;
            while (partitionSize > 1) {
                if (partitionSize <= ArrayHelper.IntrosortSizeThreshold) {

                    if (partitionSize == 2) {
                        SwapIfGreater(keys, comparer, 0, 1);
                        return;
                    }

                    if (partitionSize == 3) {
                        SwapIfGreater(keys, comparer, 0, 1);
                        SwapIfGreater(keys, comparer, 0, 2);
                        SwapIfGreater(keys, comparer, 1, 2);
                        return;
                    }

                    InsertionSort(keys.Slice(0, partitionSize), comparer);
                    return;
                }

                if (depthLimit == 0) {
                    HeapSort(keys.Slice(0, partitionSize), comparer);
                    return;
                }
                depthLimit--;

                TSize p = PickPivotAndPartition(keys.Slice(0, partitionSize), comparer);

                // Note we've already partitioned around the pivot and do not have to move the pivot again.
                IntroSort(keys.Slice(p + 1, partitionSize), depthLimit, comparer);
                partitionSize = p;
            }
        }

        private static TSize PickPivotAndPartition(ExSpan<T> keys, Comparison<T> comparer) {
            Debug.Assert(keys.Length >= ArrayHelper.IntrosortSizeThreshold);
            Debug.Assert(comparer != null);
            if (null == comparer) throw new ArgumentNullException(nameof(comparer));

            TSize hi = keys.Length - 1;

            // Compute median-of-three.  But also partition them, since we've done the comparison.
            TSize middle = hi >> 1;

            // Sort lo, mid and hi appropriately, then pick mid as the pivot.
            SwapIfGreater(keys, comparer, 0, middle);  // swap the low with the mid point
            SwapIfGreater(keys, comparer, 0, hi);   // swap the low with the high
            SwapIfGreater(keys, comparer, middle, hi); // swap the middle with the high

            T pivot = keys[middle];
            Swap(keys, middle, hi - 1);
            TSize left = 0, right = hi - 1;  // We already partitioned lo and hi and put the pivot in hi - 1.  And we pre-increment & decrement below.

            while (left < right) {
                while (comparer(keys[++left], pivot) < 0) ;
                while (comparer(pivot, keys[--right]) < 0) ;

                if (left >= right)
                    break;

                Swap(keys, left, right);
            }

            // Put pivot in the right location.
            if (left != hi - 1) {
                Swap(keys, left, hi - 1);
            }
            return left;
        }

        private static void HeapSort(ExSpan<T> keys, Comparison<T> comparer) {
            Debug.Assert(comparer != null);
            if (null == comparer) throw new ArgumentNullException(nameof(comparer));
            Debug.Assert(!keys.IsEmpty);

            TSize n = keys.Length;
            for (TSize i = n >> 1; i >= 1; i--) {
                DownHeap(keys, i, n, comparer);
            }

            for (TSize i = n; i > 1; i--) {
                Swap(keys, 0, i - 1);
                DownHeap(keys, 1, i - 1, comparer);
            }
        }

        private static void DownHeap(ExSpan<T> keys, TSize i, TSize n, Comparison<T> comparer) {
            Debug.Assert(comparer != null);
            if (null == comparer) throw new ArgumentNullException(nameof(comparer));

            T d = keys[i - 1];
            while (i <= n >> 1) {
                TSize child = 2 * i;
                if (child < n && comparer(keys[child - 1], keys[child]) < 0) {
                    child++;
                }

                if (!(comparer(d, keys[child - 1]) < 0))
                    break;

                keys[i - 1] = keys[child - 1];
                i = child;
            }

            keys[i - 1] = d;
        }

        private static void InsertionSort(ExSpan<T> keys, Comparison<T> comparer) {
            for (TSize i = 0; i < keys.Length - 1; i++) {
                T t = keys[i + 1];

                TSize j = i;
                while (j >= 0 && comparer(t, keys[j]) < 0) {
                    keys[j + 1] = keys[j];
                    j--;
                }

                keys[j + 1] = t;
            }
        }
    }

    internal sealed partial class GenericExArraySortHelper<T>
        where T : IComparable<T> {
        // Do not add a constructor to this class because ExArraySortHelper<T>.CreateSortHelper will not execute it

        #region IExArraySortHelper<T> Members

        public void Sort(ExSpan<T> keys, IComparer<T>? comparer) {
            try {
                if (comparer == null || comparer == Comparer<T>.Default) {
                    if (keys.Length > 1) {
                        // For floating-point, do a pre-pass to move all NaNs to the beginning
                        // so that we can do an optimized comparison as part of the actual sort
                        // on the remainder of the values.
                        if (typeof(T) == typeof(double) ||
                            typeof(T) == typeof(float) ||
                            typeof(T) == typeof(Half)) {
                            TSize nanLeft = SortUtils.MoveNansToFront(keys, default(ExSpan<byte>));
                            if (nanLeft == keys.Length) {
                                return;
                            }
                            keys = keys.Slice(nanLeft);
                        }

                        IntroSort(keys, 2 * (MathBitOperations.Log2((nuint)keys.Length) + 1));
                    }
                } else {
                    ExArraySortHelper<T>.IntrospectiveSort(keys, comparer.Compare);
                }
            } catch (IndexOutOfRangeException) {
                ThrowHelper.ThrowArgumentException_BadComparer(comparer);
            } catch (Exception e) {
                ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_IComparerFailed, e);
            }
        }

        public TSize BinarySearch(T[] array, TSize index, TSize length, T value, IComparer<T>? comparer) {
            Debug.Assert(array != null, "Check the arguments in the caller!");
            if (null == array) throw new ArgumentNullException(nameof(array));
            Debug.Assert(index >= 0 && length >= 0 && (array.Length - index >= length), "Check the arguments in the caller!");

            try {
                if (comparer == null || comparer == Comparer<T>.Default) {
                    return BinarySearch(array, index, length, value);
                } else {
                    return ExArraySortHelper<T>.InternalBinarySearch(array, index, length, value, comparer);
                }
            } catch (Exception e) {
                ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_IComparerFailed, e);
                return 0;
            }
        }

        #endregion

        // This function is called when the user doesn't specify any comparer.
        // Since T is constrained here, we can call IComparable<T>.CompareTo here.
        // We can avoid boxing for value type and casting for reference types.
        private static TSize BinarySearch(T[] array, TSize index, TSize length, T value) {
            TSize lo = index;
            TSize hi = index + length - 1;
            while (lo <= hi) {
                TSize i = lo + ((hi - lo) >> 1);
                TSize order;
                if (array[i] == null) {
                    order = (value == null) ? 0 : -1;
                } else {
                    order = array[i].CompareTo(value!);
                }

                if (order == 0) {
                    return i;
                }

                if (order < 0) {
                    lo = i + 1;
                } else {
                    hi = i - 1;
                }
            }

            return ~lo;
        }

        /// <summary>Swaps the values in the two references if the first is greater than the second.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SwapIfGreater(ref T i, ref T j) {
            if (i != null && GreaterThan(ref i, ref j)) {
                Swap(ref i, ref j);
            }
        }

        /// <summary>Swaps the values in the two references, regardless of whether the two references are the same.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Swap(ref T i, ref T j) {
            Debug.Assert(!Unsafe.AreSame(ref i, ref j));

            T t = i;
            i = j;
            j = t;
        }

        // IntroSort is recursive; block it from being inlined into itself as
        // this is currenly not profitable.
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void IntroSort(ExSpan<T> keys, TSize depthLimit) {
            Debug.Assert(!keys.IsEmpty);
            Debug.Assert(depthLimit >= 0);

            TSize partitionSize = keys.Length;
            while (partitionSize > 1) {
                if (partitionSize <= ArrayHelper.IntrosortSizeThreshold) {
                    if (partitionSize == 2) {
                        SwapIfGreater(ref keys[0], ref keys[1]);
                        return;
                    }

                    if (partitionSize == 3) {
                        ref T hiRef = ref keys[2];
                        ref T him1Ref = ref keys[1];
                        ref T loRef = ref keys[0];

                        SwapIfGreater(ref loRef, ref him1Ref);
                        SwapIfGreater(ref loRef, ref hiRef);
                        SwapIfGreater(ref him1Ref, ref hiRef);
                        return;
                    }

                    InsertionSort(keys.Slice(0, partitionSize));
                    return;
                }

                if (depthLimit == 0) {
                    HeapSort(keys.Slice(0, partitionSize));
                    return;
                }
                depthLimit--;

                TSize p = PickPivotAndPartition(keys.Slice(0, partitionSize));

                // Note we've already partitioned around the pivot and do not have to move the pivot again.
                IntroSort(keys.Slice(p + 1, partitionSize), depthLimit);
                partitionSize = p;
            }
        }

        private static TSize PickPivotAndPartition(ExSpan<T> keys) {
            Debug.Assert(keys.Length >= ArrayHelper.IntrosortSizeThreshold);

            // Use median-of-three to select a pivot. Grab a reference to the 0th, Length-1th, and Length/2th elements, and sort them.
            ref T zeroRef = ref ExMemoryMarshal.GetReference(keys);
            ref T lastRef = ref Unsafe.Add(ref zeroRef, keys.Length - 1);
            ref T middleRef = ref Unsafe.Add(ref zeroRef, (keys.Length - 1) >> 1);
            SwapIfGreater(ref zeroRef, ref middleRef);
            SwapIfGreater(ref zeroRef, ref lastRef);
            SwapIfGreater(ref middleRef, ref lastRef);

            // Select the middle value as the pivot, and move it to be just before the last element.
            ref T nextToLastRef = ref Unsafe.Add(ref zeroRef, keys.Length - 2);
            T pivot = middleRef;
            Swap(ref middleRef, ref nextToLastRef);

            // Walk the left and right pointers, swapping elements as necessary, until they cross.
            ref T leftRef = ref zeroRef, rightRef = ref nextToLastRef;
            while (Unsafe.IsAddressLessThan(ref leftRef, ref rightRef)) {
                if (pivot == null) {
                    while (Unsafe.IsAddressLessThan(ref leftRef, ref nextToLastRef) && (leftRef = ref Unsafe.Add(ref leftRef, 1)) == null) ;
                    while (Unsafe.IsAddressGreaterThan(ref rightRef, ref zeroRef) && (rightRef = ref Unsafe.Add(ref rightRef, -1)) != null) ;
                } else {
                    while (Unsafe.IsAddressLessThan(ref leftRef, ref nextToLastRef) && GreaterThan(ref pivot, ref leftRef = ref Unsafe.Add(ref leftRef, 1))) ;
                    while (Unsafe.IsAddressGreaterThan(ref rightRef, ref zeroRef) && LessThan(ref pivot, ref rightRef = ref Unsafe.Add(ref rightRef, -1))) ;
                }

                if (!Unsafe.IsAddressLessThan(ref leftRef, ref rightRef)) {
                    break;
                }

                Swap(ref leftRef, ref rightRef);
            }

            // Put the pivot in the correct location.
            if (!Unsafe.AreSame(ref leftRef, ref nextToLastRef)) {
                Swap(ref leftRef, ref nextToLastRef);
            }

            return (TSize)((nint)Unsafe.ByteOffset(ref zeroRef, ref leftRef) / Unsafe.SizeOf<T>());
        }

        private static void HeapSort(ExSpan<T> keys) {
            Debug.Assert(!keys.IsEmpty);

            TSize n = keys.Length;
            for (TSize i = n >> 1; i >= 1; i--) {
                DownHeap(keys, i, n);
            }

            for (TSize i = n; i > 1; i--) {
                Swap(ref keys[0], ref keys[i - 1]);
                DownHeap(keys, 1, i - 1);
            }
        }

        private static void DownHeap(ExSpan<T> keys, TSize i, TSize n) {
            T d = keys[i - 1];
            while (i <= n >> 1) {
                TSize child = 2 * i;
                if (child < n && (keys[child - 1] == null || LessThan(ref keys[child - 1], ref keys[child]))) {
                    child++;
                }

                if (keys[child - 1] == null || !LessThan(ref d, ref keys[child - 1]))
                    break;

                keys[i - 1] = keys[child - 1];
                i = child;
            }

            keys[i - 1] = d;
        }

        private static void InsertionSort(ExSpan<T> keys) {
            for (TSize i = 0; i < keys.Length - 1; i++) {
                T t = Unsafe.Add(ref ExMemoryMarshal.GetReference(keys), i + 1);

                TSize j = i;
                while (j >= 0 && (t == null || LessThan(ref t, ref Unsafe.Add(ref ExMemoryMarshal.GetReference(keys), j)))) {
                    Unsafe.Add(ref ExMemoryMarshal.GetReference(keys), j + 1) = Unsafe.Add(ref ExMemoryMarshal.GetReference(keys), j);
                    j--;
                }

                Unsafe.Add(ref ExMemoryMarshal.GetReference(keys), j + 1) = t!;
            }
        }

        // - These methods exist for use in sorting, where the additional operations present in
        //   the CompareTo methods that would otherwise be used on these primitives add non-trivial overhead,
        //   in particular for floating point where the CompareTo methods need to factor in NaNs.
        // - The floating-point comparisons here assume no NaNs, which is valid only because the sorting routines
        //   themselves special-case NaN with a pre-pass that ensures none are present in the values being sorted
        //   by moving them all to the front first and then sorting the rest.
        // - These are duplicated here rather than being on a helper type due to current limitations around generic inlining.

        [MethodImpl(MethodImplOptions.AggressiveInlining)] // compiles to a single comparison or method call
        private static bool LessThan(ref T left, ref T right) {
            if (typeof(T) == typeof(byte)) return (byte)(object)left < (byte)(object)right;
            if (typeof(T) == typeof(sbyte)) return (sbyte)(object)left < (sbyte)(object)right;
            if (typeof(T) == typeof(ushort)) return (ushort)(object)left < (ushort)(object)right;
            if (typeof(T) == typeof(short)) return (short)(object)left < (short)(object)right;
            if (typeof(T) == typeof(uint)) return (uint)(object)left < (uint)(object)right;
            if (typeof(T) == typeof(int)) return (int)(object)left < (int)(object)right;
            if (typeof(T) == typeof(ulong)) return (ulong)(object)left < (ulong)(object)right;
            if (typeof(T) == typeof(long)) return (long)(object)left < (long)(object)right;
            if (typeof(T) == typeof(nuint)) return (nuint)(object)left < (nuint)(object)right;
            if (typeof(T) == typeof(nint)) return (nint)(object)left < (nint)(object)right;
            if (typeof(T) == typeof(float)) return (float)(object)left < (float)(object)right;
            if (typeof(T) == typeof(double)) return (double)(object)left < (double)(object)right;
            if (typeof(T) == typeof(Half)) return (Half)(object)left < (Half)(object)right;
            return left.CompareTo(right) < 0 ? true : false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] // compiles to a single comparison or method call
        private static bool GreaterThan(ref T left, ref T right) {
            if (typeof(T) == typeof(byte)) return (byte)(object)left > (byte)(object)right;
            if (typeof(T) == typeof(sbyte)) return (sbyte)(object)left > (sbyte)(object)right;
            if (typeof(T) == typeof(ushort)) return (ushort)(object)left > (ushort)(object)right;
            if (typeof(T) == typeof(short)) return (short)(object)left > (short)(object)right;
            if (typeof(T) == typeof(uint)) return (uint)(object)left > (uint)(object)right;
            if (typeof(T) == typeof(int)) return (int)(object)left > (int)(object)right;
            if (typeof(T) == typeof(ulong)) return (ulong)(object)left > (ulong)(object)right;
            if (typeof(T) == typeof(long)) return (long)(object)left > (long)(object)right;
            if (typeof(T) == typeof(nuint)) return (nuint)(object)left > (nuint)(object)right;
            if (typeof(T) == typeof(nint)) return (nint)(object)left > (nint)(object)right;
            if (typeof(T) == typeof(float)) return (float)(object)left > (float)(object)right;
            if (typeof(T) == typeof(double)) return (double)(object)left > (double)(object)right;
            if (typeof(T) == typeof(Half)) return (Half)(object)left > (Half)(object)right;
            return left.CompareTo(right) > 0 ? true : false;
        }
    }

    #endregion

    #region ExArraySortHelper for paired key and value arrays

    internal sealed partial class ExArraySortHelper<TKey, TValue> {
        public void Sort(ExSpan<TKey> keys, ExSpan<TValue> values, IComparer<TKey>? comparer) {
            // Add a try block here to detect IComparers (or their
            // underlying IComparables, etc) that are bogus.
            try {
                IntrospectiveSort(keys, values, comparer ?? Comparer<TKey>.Default);
            } catch (IndexOutOfRangeException) {
                ThrowHelper.ThrowArgumentException_BadComparer(comparer);
            } catch (Exception e) {
                ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_IComparerFailed, e);
            }
        }

        private static void SwapIfGreaterWithValues(ExSpan<TKey> keys, ExSpan<TValue> values, IComparer<TKey> comparer, TSize i, TSize j) {
            Debug.Assert(comparer != null);
            if (null == comparer) throw new ArgumentNullException(nameof(comparer));
            Debug.Assert(0 <= i && i < keys.Length && i < values.Length);
            Debug.Assert(0 <= j && j < keys.Length && j < values.Length);
            Debug.Assert(i != j);

            if (comparer.Compare(keys[i], keys[j]) > 0) {
                TKey key = keys[i];
                keys[i] = keys[j];
                keys[j] = key;

                TValue value = values[i];
                values[i] = values[j];
                values[j] = value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Swap(ExSpan<TKey> keys, ExSpan<TValue> values, TSize i, TSize j) {
            Debug.Assert(i != j);

            TKey k = keys[i];
            keys[i] = keys[j];
            keys[j] = k;

            TValue v = values[i];
            values[i] = values[j];
            values[j] = v;
        }

        internal static void IntrospectiveSort(ExSpan<TKey> keys, ExSpan<TValue> values, IComparer<TKey> comparer) {
            Debug.Assert(comparer != null);
            if (null == comparer) throw new ArgumentNullException(nameof(comparer));
            Debug.Assert(keys.Length == values.Length);

            if (keys.Length > 1) {
                IntroSort(keys, values, 2 * (MathBitOperations.Log2((nuint)keys.Length) + 1), comparer);
            }
        }

        private static void IntroSort(ExSpan<TKey> keys, ExSpan<TValue> values, TSize depthLimit, IComparer<TKey> comparer) {
            Debug.Assert(!keys.IsEmpty);
            Debug.Assert(values.Length == keys.Length);
            Debug.Assert(depthLimit >= 0);
            Debug.Assert(comparer != null);
            if (null == comparer) throw new ArgumentNullException(nameof(comparer));

            TSize partitionSize = keys.Length;
            while (partitionSize > 1) {
                if (partitionSize <= ArrayHelper.IntrosortSizeThreshold) {

                    if (partitionSize == 2) {
                        SwapIfGreaterWithValues(keys, values, comparer, 0, 1);
                        return;
                    }

                    if (partitionSize == 3) {
                        SwapIfGreaterWithValues(keys, values, comparer, 0, 1);
                        SwapIfGreaterWithValues(keys, values, comparer, 0, 2);
                        SwapIfGreaterWithValues(keys, values, comparer, 1, 2);
                        return;
                    }

                    InsertionSort(keys.Slice(0, partitionSize), values.Slice(0, partitionSize), comparer);
                    return;
                }

                if (depthLimit == 0) {
                    HeapSort(keys.Slice(0, partitionSize), values.Slice(0, partitionSize), comparer);
                    return;
                }
                depthLimit--;

                TSize p = PickPivotAndPartition(keys.Slice(0, partitionSize), values.Slice(0, partitionSize), comparer);

                // Note we've already partitioned around the pivot and do not have to move the pivot again.
                IntroSort(keys.Slice(p + 1, partitionSize), values.Slice(p + 1, partitionSize), depthLimit, comparer);
                partitionSize = p;
            }
        }

        private static TSize PickPivotAndPartition(ExSpan<TKey> keys, ExSpan<TValue> values, IComparer<TKey> comparer) {
            Debug.Assert(keys.Length >= ArrayHelper.IntrosortSizeThreshold);
            Debug.Assert(comparer != null);
            if (null == comparer) throw new ArgumentNullException(nameof(comparer));

            TSize hi = keys.Length - 1;

            // Compute median-of-three.  But also partition them, since we've done the comparison.
            TSize middle = hi >> 1;

            // Sort lo, mid and hi appropriately, then pick mid as the pivot.
            SwapIfGreaterWithValues(keys, values, comparer, 0, middle);  // swap the low with the mid point
            SwapIfGreaterWithValues(keys, values, comparer, 0, hi);   // swap the low with the high
            SwapIfGreaterWithValues(keys, values, comparer, middle, hi); // swap the middle with the high

            TKey pivot = keys[middle];
            Swap(keys, values, middle, hi - 1);
            TSize left = 0, right = hi - 1;  // We already partitioned lo and hi and put the pivot in hi - 1.  And we pre-increment & decrement below.

            while (left < right) {
                while (comparer.Compare(keys[++left], pivot) < 0) ;
                while (comparer.Compare(pivot, keys[--right]) < 0) ;

                if (left >= right)
                    break;

                Swap(keys, values, left, right);
            }

            // Put pivot in the right location.
            if (left != hi - 1) {
                Swap(keys, values, left, hi - 1);
            }
            return left;
        }

        private static void HeapSort(ExSpan<TKey> keys, ExSpan<TValue> values, IComparer<TKey> comparer) {
            Debug.Assert(comparer != null);
            if (null == comparer) throw new ArgumentNullException(nameof(comparer));
            Debug.Assert(!keys.IsEmpty);

            TSize n = keys.Length;
            for (TSize i = n >> 1; i >= 1; i--) {
                DownHeap(keys, values, i, n, comparer);
            }

            for (TSize i = n; i > 1; i--) {
                Swap(keys, values, 0, i - 1);
                DownHeap(keys, values, 1, i - 1, comparer);
            }
        }

        private static void DownHeap(ExSpan<TKey> keys, ExSpan<TValue> values, TSize i, TSize n, IComparer<TKey> comparer) {
            Debug.Assert(comparer != null);
            if (null == comparer) throw new ArgumentNullException(nameof(comparer));

            TKey d = keys[i - 1];
            TValue dValue = values[i - 1];

            while (i <= n >> 1) {
                TSize child = 2 * i;
                if (child < n && comparer.Compare(keys[child - 1], keys[child]) < 0) {
                    child++;
                }

                if (!(comparer.Compare(d, keys[child - 1]) < 0))
                    break;

                keys[i - 1] = keys[child - 1];
                values[i - 1] = values[child - 1];
                i = child;
            }

            keys[i - 1] = d;
            values[i - 1] = dValue;
        }

        private static void InsertionSort(ExSpan<TKey> keys, ExSpan<TValue> values, IComparer<TKey> comparer) {
            Debug.Assert(comparer != null);
            if (null == comparer) throw new ArgumentNullException(nameof(comparer));

            for (TSize i = 0; i < keys.Length - 1; i++) {
                TKey t = keys[i + 1];
                TValue tValue = values[i + 1];

                TSize j = i;
                while (j >= 0 && comparer.Compare(t, keys[j]) < 0) {
                    keys[j + 1] = keys[j];
                    values[j + 1] = values[j];
                    j--;
                }

                keys[j + 1] = t;
                values[j + 1] = tValue;
            }
        }
    }

    internal sealed partial class GenericExArraySortHelper<TKey, TValue>
        where TKey : IComparable<TKey> {
        public void Sort(ExSpan<TKey> keys, ExSpan<TValue> values, IComparer<TKey>? comparer) {
            // Add a try block here to detect IComparers (or their
            // underlying IComparables, etc) that are bogus.
            try {
                if (comparer == null || comparer == Comparer<TKey>.Default) {
                    if (keys.Length > 1) {
                        // For floating-point, do a pre-pass to move all NaNs to the beginning
                        // so that we can do an optimized comparison as part of the actual sort
                        // on the remainder of the values.
                        if (typeof(TKey) == typeof(double) ||
                            typeof(TKey) == typeof(float) ||
                            typeof(TKey) == typeof(Half)) {
                            TSize nanLeft = SortUtils.MoveNansToFront(keys, values);
                            if (nanLeft == keys.Length) {
                                return;
                            }
                            keys = keys.Slice(nanLeft);
                            values = values.Slice(nanLeft);
                        }

                        IntroSort(keys, values, 2 * (MathBitOperations.Log2((nuint)keys.Length) + 1));
                    }
                } else {
                    ExArraySortHelper<TKey, TValue>.IntrospectiveSort(keys, values, comparer);
                }
            } catch (IndexOutOfRangeException) {
                ThrowHelper.ThrowArgumentException_BadComparer(comparer);
            } catch (Exception e) {
                ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_IComparerFailed, e);
            }
        }

        private static void SwapIfGreaterWithValues(ExSpan<TKey> keys, ExSpan<TValue> values, TSize i, TSize j) {
            Debug.Assert(i != j);

            ref TKey keyRef = ref keys[i];
            if (keyRef != null && GreaterThan(ref keyRef, ref keys[j])) {
                TKey key = keyRef;
                keys[i] = keys[j];
                keys[j] = key;

                TValue value = values[i];
                values[i] = values[j];
                values[j] = value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Swap(ExSpan<TKey> keys, ExSpan<TValue> values, TSize i, TSize j) {
            Debug.Assert(i != j);

            TKey k = keys[i];
            keys[i] = keys[j];
            keys[j] = k;

            TValue v = values[i];
            values[i] = values[j];
            values[j] = v;
        }

        private static void IntroSort(ExSpan<TKey> keys, ExSpan<TValue> values, TSize depthLimit) {
            Debug.Assert(!keys.IsEmpty);
            Debug.Assert(values.Length == keys.Length);
            Debug.Assert(depthLimit >= 0);

            TSize partitionSize = keys.Length;
            while (partitionSize > 1) {
                if (partitionSize <= ArrayHelper.IntrosortSizeThreshold) {

                    if (partitionSize == 2) {
                        SwapIfGreaterWithValues(keys, values, 0, 1);
                        return;
                    }

                    if (partitionSize == 3) {
                        SwapIfGreaterWithValues(keys, values, 0, 1);
                        SwapIfGreaterWithValues(keys, values, 0, 2);
                        SwapIfGreaterWithValues(keys, values, 1, 2);
                        return;
                    }

                    InsertionSort(keys.Slice(0, partitionSize), values.Slice(0, partitionSize));
                    return;
                }

                if (depthLimit == 0) {
                    HeapSort(keys.Slice(0, partitionSize), values.Slice(0, partitionSize));
                    return;
                }
                depthLimit--;

                TSize p = PickPivotAndPartition(keys.Slice(0, partitionSize), values.Slice(0, partitionSize));

                // Note we've already partitioned around the pivot and do not have to move the pivot again.
                IntroSort(keys.Slice(p + 1, partitionSize), values.Slice(p + 1, partitionSize), depthLimit);
                partitionSize = p;
            }
        }

        private static TSize PickPivotAndPartition(ExSpan<TKey> keys, ExSpan<TValue> values) {
            Debug.Assert(keys.Length >= ArrayHelper.IntrosortSizeThreshold);

            TSize hi = keys.Length - 1;

            // Compute median-of-three.  But also partition them, since we've done the comparison.
            TSize middle = hi >> 1;

            // Sort lo, mid and hi appropriately, then pick mid as the pivot.
            SwapIfGreaterWithValues(keys, values, 0, middle);  // swap the low with the mid point
            SwapIfGreaterWithValues(keys, values, 0, hi);   // swap the low with the high
            SwapIfGreaterWithValues(keys, values, middle, hi); // swap the middle with the high

            TKey pivot = keys[middle];
            Swap(keys, values, middle, hi - 1);
            TSize left = 0, right = hi - 1;  // We already partitioned lo and hi and put the pivot in hi - 1.  And we pre-increment & decrement below.

            while (left < right) {
                if (pivot == null) {
                    while (left < (hi - 1) && keys[++left] == null) ;
                    while (right > 0 && keys[--right] != null) ;
                } else {
                    while (GreaterThan(ref pivot, ref keys[++left])) ;
                    while (LessThan(ref pivot, ref keys[--right])) ;
                }

                if (left >= right)
                    break;

                Swap(keys, values, left, right);
            }

            // Put pivot in the right location.
            if (left != hi - 1) {
                Swap(keys, values, left, hi - 1);
            }
            return left;
        }

        private static void HeapSort(ExSpan<TKey> keys, ExSpan<TValue> values) {
            Debug.Assert(!keys.IsEmpty);

            TSize n = keys.Length;
            for (TSize i = n >> 1; i >= 1; i--) {
                DownHeap(keys, values, i, n);
            }

            for (TSize i = n; i > 1; i--) {
                Swap(keys, values, 0, i - 1);
                DownHeap(keys, values, 1, i - 1);
            }
        }

        private static void DownHeap(ExSpan<TKey> keys, ExSpan<TValue> values, TSize i, TSize n) {
            TKey d = keys[i - 1];
            TValue dValue = values[i - 1];

            while (i <= n >> 1) {
                TSize child = 2 * i;
                if (child < n && (keys[child - 1] == null || LessThan(ref keys[child - 1], ref keys[child]))) {
                    child++;
                }

                if (keys[child - 1] == null || !LessThan(ref d, ref keys[child - 1]))
                    break;

                keys[i - 1] = keys[child - 1];
                values[i - 1] = values[child - 1];
                i = child;
            }

            keys[i - 1] = d;
            values[i - 1] = dValue;
        }

        private static void InsertionSort(ExSpan<TKey> keys, ExSpan<TValue> values) {
            for (TSize i = 0; i < keys.Length - 1; i++) {
                TKey t = keys[i + 1];
                TValue tValue = values[i + 1];

                TSize j = i;
                while (j >= 0 && (t == null || LessThan(ref t, ref keys[j]))) {
                    keys[j + 1] = keys[j];
                    values[j + 1] = values[j];
                    j--;
                }

                keys[j + 1] = t!;
                values[j + 1] = tValue;
            }
        }

        // - These methods exist for use in sorting, where the additional operations present in
        //   the CompareTo methods that would otherwise be used on these primitives add non-trivial overhead,
        //   in particular for floating point where the CompareTo methods need to factor in NaNs.
        // - The floating-point comparisons here assume no NaNs, which is valid only because the sorting routines
        //   themselves special-case NaN with a pre-pass that ensures none are present in the values being sorted
        //   by moving them all to the front first and then sorting the rest.
        // - These are duplicated here rather than being on a helper type due to current limitations around generic inlining.

        [MethodImpl(MethodImplOptions.AggressiveInlining)] // compiles to a single comparison or method call
        private static bool LessThan(ref TKey left, ref TKey right) {
            if (typeof(TKey) == typeof(byte)) return (byte)(object)left < (byte)(object)right;
            if (typeof(TKey) == typeof(sbyte)) return (sbyte)(object)left < (sbyte)(object)right;
            if (typeof(TKey) == typeof(ushort)) return (ushort)(object)left < (ushort)(object)right;
            if (typeof(TKey) == typeof(short)) return (short)(object)left < (short)(object)right;
            if (typeof(TKey) == typeof(uint)) return (uint)(object)left < (uint)(object)right;
            if (typeof(TKey) == typeof(int)) return (int)(object)left < (int)(object)right;
            if (typeof(TKey) == typeof(ulong)) return (ulong)(object)left < (ulong)(object)right;
            if (typeof(TKey) == typeof(long)) return (long)(object)left < (long)(object)right;
            if (typeof(TKey) == typeof(nuint)) return (nuint)(object)left < (nuint)(object)right;
            if (typeof(TKey) == typeof(nint)) return (nint)(object)left < (nint)(object)right;
            if (typeof(TKey) == typeof(float)) return (float)(object)left < (float)(object)right;
            if (typeof(TKey) == typeof(double)) return (double)(object)left < (double)(object)right;
            if (typeof(TKey) == typeof(Half)) return (Half)(object)left < (Half)(object)right;
            return left.CompareTo(right) < 0 ? true : false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] // compiles to a single comparison or method call
        private static bool GreaterThan(ref TKey left, ref TKey right) {
            if (typeof(TKey) == typeof(byte)) return (byte)(object)left > (byte)(object)right;
            if (typeof(TKey) == typeof(sbyte)) return (sbyte)(object)left > (sbyte)(object)right;
            if (typeof(TKey) == typeof(ushort)) return (ushort)(object)left > (ushort)(object)right;
            if (typeof(TKey) == typeof(short)) return (short)(object)left > (short)(object)right;
            if (typeof(TKey) == typeof(uint)) return (uint)(object)left > (uint)(object)right;
            if (typeof(TKey) == typeof(int)) return (int)(object)left > (int)(object)right;
            if (typeof(TKey) == typeof(ulong)) return (ulong)(object)left > (ulong)(object)right;
            if (typeof(TKey) == typeof(long)) return (long)(object)left > (long)(object)right;
            if (typeof(TKey) == typeof(nuint)) return (nuint)(object)left > (nuint)(object)right;
            if (typeof(TKey) == typeof(nint)) return (nint)(object)left > (nint)(object)right;
            if (typeof(TKey) == typeof(float)) return (float)(object)left > (float)(object)right;
            if (typeof(TKey) == typeof(double)) return (double)(object)left > (double)(object)right;
            if (typeof(TKey) == typeof(Half)) return (Half)(object)left > (Half)(object)right;
            return left.CompareTo(right) > 0 ? true : false;
        }
    }

    #endregion

    /// <summary>Helper methods for use in array/span sorting routines.</summary>
    internal static class SortUtils {
        public static TSize MoveNansToFront<TKey, TValue>(ExSpan<TKey> keys, ExSpan<TValue> values) where TKey : notnull {
            Debug.Assert(typeof(TKey) == typeof(double) || typeof(TKey) == typeof(float));

            TSize left = 0;

            for (TSize i = 0; i < keys.Length; i++) {
                if ((typeof(TKey) == typeof(double) && double.IsNaN((double)(object)keys[i])) ||
                    (typeof(TKey) == typeof(float) && float.IsNaN((float)(object)keys[i])) ||
                    (typeof(TKey) == typeof(Half) && Half.IsNaN((Half)(object)keys[i]))) {
                    TKey temp = keys[left];
                    keys[left] = keys[i];
                    keys[i] = temp;

                    if ((nuint)i < (nuint)values.Length) // check to see if we have values
                    {
                        TValue tempValue = values[left];
                        values[left] = values[i];
                        values[i] = tempValue;
                    }

                    left++;
                }
            }

            return left;
        }
    }

}
