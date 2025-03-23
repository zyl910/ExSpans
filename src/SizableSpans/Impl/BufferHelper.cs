#define USE_LOOP_UNROLLING // Use loop unrolling (使用循环展开).

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Zyl.SizableSpans.Impl {
    /// <summary>
    /// <see cref="Buffer"/> Helper.
    /// </summary>
    public static class BufferHelper {

#if USE_LOOP_UNROLLING
        private const int UnrollingSize = 8;
#endif // USE_LOOP_UNROLLING

        /// <summary>
        /// Copies a number of bytes specified as a long integer value from one address in memory to another. .If some regions of the source area and the destination overlap, the function ensures that the original source bytes in the overlapping region are copied before being overwritten (将指定为长整型值的一些字节从内存中的一个地址复制到另一个地址. 如果源区域的某些区域与目标区域重叠, 函数可确保在覆盖之前复制重叠区域中的原始源字节).
        /// </summary>
        /// <param name="source">The address of the bytes to copy (要复制的字节的地址).</param>
        /// <param name="destination">The target address (目标地址).</param>
        /// <param name="destinationSizeInBytes">The number of bytes available in the destination memory block (目标内存块中可用的字节数).</param>
        /// <param name="sourceBytesToCopy">The number of bytes to copy (要复制的字节数).</param>
        /// <exception cref="ArgumentOutOfRangeException">sourceBytesToCopy is greater than destinationSizeInBytes.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void MemoryCopy(void* source, void* destination, long destinationSizeInBytes, long sourceBytesToCopy) {
            if (sourceBytesToCopy > destinationSizeInBytes) {
                throw new ArgumentOutOfRangeException(nameof(sourceBytesToCopy), SR.sourceBytesToCopy);
            }
#if NETSTANDARD1_3_OR_GREATER || NETCOREAPP1_0_OR_GREATER || NET46_OR_GREATER
            Buffer.MemoryCopy(destination, source, destinationSizeInBytes, sourceBytesToCopy);
#else
            Memmove(ref *(byte*)destination, ref *(byte*)source, checked((nuint)sourceBytesToCopy));
#endif
        }

        /// <summary>
        /// Copies a number of bytes specified as a ulong integer value from one address in memory to another. .If some regions of the source area and the destination overlap, the function ensures that the original source bytes in the overlapping region are copied before being overwritten (将指定为无符号长整型值的一些字节从内存中的一个地址复制到另一个地址. 如果源区域的某些区域与目标区域重叠, 函数可确保在覆盖之前复制重叠区域中的原始源字节).
        /// </summary>
        /// <param name="source">The address of the bytes to copy (要复制的字节的地址).</param>
        /// <param name="destination">The target address (目标地址).</param>
        /// <param name="destinationSizeInBytes">The number of bytes available in the destination memory block (目标内存块中可用的字节数).</param>
        /// <param name="sourceBytesToCopy">The number of bytes to copy (要复制的字节数).</param>
        /// <exception cref="ArgumentOutOfRangeException">sourceBytesToCopy is greater than destinationSizeInBytes.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void MemoryCopy(void* source, void* destination, ulong destinationSizeInBytes, ulong sourceBytesToCopy) {
            if (sourceBytesToCopy > destinationSizeInBytes) {
                throw new ArgumentOutOfRangeException(nameof(sourceBytesToCopy), SR.sourceBytesToCopy);
            }
#if NETSTANDARD1_3_OR_GREATER || NETCOREAPP1_0_OR_GREATER || NET46_OR_GREATER
            Buffer.MemoryCopy(destination, source, destinationSizeInBytes, sourceBytesToCopy);
#else
            Memmove(ref *(byte*)destination, ref *(byte*)source, checked((nuint)sourceBytesToCopy));
#endif
        }

        /// <summary>
        /// Memory move.If some regions of the source area and the destination overlap, the function ensures that the original source bytes in the overlapping region are copied before being overwritten (内存移动. 如果源区域的某些区域与目标区域重叠, 函数可确保在覆盖之前复制重叠区域中的原始源字节).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="destination">Destination address (目标地址).</param>
        /// <param name="source">Source address (源地址).</param>
        /// <param name="elementCount">Element count(元素数量).</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Memmove<T>(ref T destination, ref readonly T source, nuint elementCount) {
            bool isBlittable = SizableUnsafe.IsBlittable<T>();
            if (isBlittable) {
                // Blittable memmove
                MemmoveBlittableUnsafe(ref destination, in source, elementCount);
            } else {
                // Non-blittable memmove
                MemmoveNonBlittable(ref destination, in source, elementCount);
            }
        }

        /// <summary>
        /// Unsafe memory move only available for Blittable type. If some regions of the source area and the destination overlap, the function ensures that the original source bytes in the overlapping region are copied before being overwritten (仅Blittable类型能用的非安全内存复制. 如果源区域的某些区域与目标区域重叠，函数可确保在覆盖之前复制重叠区域中的原始源字节).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="destination">Destination address (目标地址).</param>
        /// <param name="source">Source address (源地址).</param>
        /// <param name="elementCount">Element count(元素数量).</param>
        /// <seealso cref="SizableUnsafe.IsBlittable"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MemmoveBlittableUnsafe<T>(ref T destination, ref readonly T source, nuint elementCount) {
            nuint sourceBytesToCopy = SizableUnsafe.GetByteSize<T>(elementCount);
            ref byte p0 = ref Unsafe.As<T, byte>(ref Unsafe.AsRef(in source));
#if NETSTANDARD1_3_OR_GREATER || NETCOREAPP1_0_OR_GREATER || NET46_OR_GREATER
            unsafe {
                Buffer.MemoryCopy(Unsafe.AsPointer(ref destination), Unsafe.AsPointer(ref p0), (ulong)sourceBytesToCopy, (ulong)sourceBytesToCopy);
            }
#else
            const int MaxBlockSize = 1024 * 1024 * 1024; // 1G
            ref byte q0 = ref Unsafe.As<T, byte>(ref destination);
            ref byte pEnd = ref SizableUnsafe.Add(ref p0, sourceBytesToCopy);
            ref byte qEnd = ref SizableUnsafe.Add(ref q0, sourceBytesToCopy);
            uint blockSize;
            bool needReverse = false;
            if (Unsafe.IsAddressLessThan(ref p0, ref q0)) {
                if (Unsafe.IsAddressLessThan(ref q0, ref pEnd)) { // overwritten next.
                    needReverse = true;
                    nint offset = Unsafe.ByteOffset(ref q0, ref p0);
                    blockSize = ((long)offset < (long)MaxBlockSize) ? (uint)offset : MaxBlockSize;
                } else {
                    blockSize = MaxBlockSize;
                }
            } else { // p0 > q0
                if (Unsafe.IsAddressLessThan(ref p0, ref qEnd)) { // overwritten previous.
                    nint offset = Unsafe.ByteOffset(ref p0, ref q0);
                    blockSize = ((long)offset < (long)MaxBlockSize) ? (uint)offset : MaxBlockSize;
                } else {
                    blockSize = MaxBlockSize;
                }
            }
            // Copy.
            if (blockSize <= Unsafe.SizeOf<T>()) {
                MemmoveNonBlittable(ref destination, in source, elementCount);
            }
            ulong count = (ulong)sourceBytesToCopy;
            if (needReverse) {
                ref byte p = ref pEnd;
                ref byte q = ref qEnd;
                while (count >= blockSize) {
                    p = ref Unsafe.SubtractByteOffset(ref p, (nint)blockSize);
                    q = ref Unsafe.SubtractByteOffset(ref q, (nint)blockSize);
                    Unsafe.CopyBlockUnaligned(ref p, ref q, blockSize);
                    // Next.
                    count -= blockSize;
                }
                if (count > 0) {
                    Unsafe.CopyBlockUnaligned(ref p0, ref q0, (uint)count);
                }
            } else {
                ref byte p = ref p0;
                ref byte q = ref q0;
                while (count >= blockSize) {
                    Unsafe.CopyBlockUnaligned(ref p, ref q, blockSize);
                    // Next.
                    count -= blockSize;
                    p = ref Unsafe.AddByteOffset(ref p, (nint)blockSize);
                    q = ref Unsafe.AddByteOffset(ref q, (nint)blockSize);
                }
                if (count > 0) {
                    Unsafe.CopyBlockUnaligned(ref p, ref q, (uint)count);
                }
            }
#endif
        }

        /// <inheritdoc cref="Memmove"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void MemmoveNonBlittable<T>(ref T destination, ref readonly T source, nuint elementCount) {
            ref T p0 = ref Unsafe.AsRef(in source);
            ref T q0 = ref destination;
            if (UIntPtr.Zero == elementCount) return;
            if (Unsafe.AreSame(ref p0, ref q0)) return;
            ref T pEnd = ref SizableUnsafe.Add(ref p0, elementCount);
            ref T qEnd = ref SizableUnsafe.Add(ref q0, elementCount);
            bool needReverse = Unsafe.IsAddressLessThan(ref p0, ref q0) && Unsafe.IsAddressLessThan(ref q0, ref pEnd); // overwritten next.
#if USE_LOOP_UNROLLING
            bool allowLoopUnrolling;
            if (Unsafe.IsAddressLessThan(ref p0, ref q0)) {
                if (needReverse) {
                    allowLoopUnrolling = !Unsafe.IsAddressGreaterThan(ref Unsafe.Add(ref p0, UnrollingSize), ref q0); // p0+UnrollingSize <= q0
                } else {
                    allowLoopUnrolling = true;
                }
            } else { // p0 > q0
                if (Unsafe.IsAddressLessThan(ref p0, ref qEnd)) { // overwritten previous.
                    allowLoopUnrolling = !Unsafe.IsAddressGreaterThan(ref Unsafe.Add(ref q0, UnrollingSize), ref p0); // q0+UnrollingSize <= p0
                } else {
                    allowLoopUnrolling = true;
                }
            }
            if (allowLoopUnrolling) {
                nuint rem = elementCount & (UnrollingSize - 1);
                nuint alignCount = elementCount - rem;
                if (needReverse) {
                    ref T pStartAlign = ref SizableUnsafe.Subtract(ref pEnd, alignCount);
                    ref T p = ref pEnd;
                    ref T q = ref qEnd;
                    do {
                        p = ref Unsafe.Subtract(ref p, UnrollingSize);
                        q = ref Unsafe.Subtract(ref q, UnrollingSize);
                        Unsafe.Add(ref q, 7) = Unsafe.Add(ref p, 7);
                        Unsafe.Add(ref q, 6) = Unsafe.Add(ref p, 6);
                        Unsafe.Add(ref q, 5) = Unsafe.Add(ref p, 5);
                        Unsafe.Add(ref q, 4) = Unsafe.Add(ref p, 4);
                        Unsafe.Add(ref q, 3) = Unsafe.Add(ref p, 3);
                        Unsafe.Add(ref q, 2) = Unsafe.Add(ref p, 2);
                        Unsafe.Add(ref q, 1) = Unsafe.Add(ref p, 1);
                        q = p;
                    } while (!Unsafe.AreSame(ref p, ref pStartAlign));
                    p = ref p0;
                    q = ref q0;
                    switch (rem) {
                        case 7: Unsafe.Add(ref q, 6) = Unsafe.Add(ref p, 6); goto case 6;
                        case 6: Unsafe.Add(ref q, 5) = Unsafe.Add(ref p, 5); goto case 5;
                        case 5: Unsafe.Add(ref q, 4) = Unsafe.Add(ref p, 4); goto case 4;
                        case 4: Unsafe.Add(ref q, 3) = Unsafe.Add(ref p, 3); goto case 3;
                        case 3: Unsafe.Add(ref q, 2) = Unsafe.Add(ref p, 2); goto case 2;
                        case 2: Unsafe.Add(ref q, 1) = Unsafe.Add(ref p, 1); goto case 1;
                        case 1: q = p; break;
                    }
                } else {
                    ref T pEndAlign = ref SizableUnsafe.Add(ref p0, alignCount);
                    ref T p = ref p0;
                    ref T q = ref q0;
                    do {
                        q = p;
                        Unsafe.Add(ref q, 1) = Unsafe.Add(ref p, 1);
                        Unsafe.Add(ref q, 2) = Unsafe.Add(ref p, 2);
                        Unsafe.Add(ref q, 3) = Unsafe.Add(ref p, 3);
                        Unsafe.Add(ref q, 4) = Unsafe.Add(ref p, 4);
                        Unsafe.Add(ref q, 5) = Unsafe.Add(ref p, 5);
                        Unsafe.Add(ref q, 6) = Unsafe.Add(ref p, 6);
                        Unsafe.Add(ref q, 7) = Unsafe.Add(ref p, 7);
                        p = ref Unsafe.Add(ref p, UnrollingSize);
                        q = ref Unsafe.Add(ref q, UnrollingSize);
                    } while (!Unsafe.AreSame(ref p, ref pEndAlign));
                    int remInt = (int)rem;
                    for (int i = 0; i < remInt; ++i) {
                        Unsafe.Add(ref q, i) = Unsafe.Add(ref p, i);
                    }
                }
                return;
            }
#else
            if (needReverse) {
                ref T p = ref pEnd;
                ref T q = ref qEnd;
                do {
                    p = ref Unsafe.Subtract(ref p, 1);
                    q = ref Unsafe.Subtract(ref q, 1);
                    q = p;
                } while (!Unsafe.AreSame(ref p, ref p0));
            } else {
                ref T p = ref p0;
                ref T q = ref q0;
                do {
                    q = p;
                    p = ref Unsafe.Add(ref p, 1);
                    q = ref Unsafe.Add(ref q, 1);
                } while (!Unsafe.AreSame(ref p, ref pEnd));
            }
#endif // USE_LOOP_UNROLLING
        }

    }
}
