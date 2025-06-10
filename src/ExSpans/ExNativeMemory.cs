using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Zyl.ExSpans.Extensions;
using Zyl.ExSpans.Impl;

namespace Zyl.ExSpans {
    /// <summary>
    /// This class contains methods that are mainly used to manage native memory. It can enable early versions NET can also use the method of <see cref="NativeMemory"/>, which will revert back to the implementation of <see cref="Marshal.AllocHGlobal(IntPtr)"/> (此类包含了主要用于管理本机内存的方法. 它能使早期版本的 .NET 也能使用 <see cref="NativeMemory"/> 的方法, 此时会回退为 <see cref="Marshal.AllocHGlobal(IntPtr)"/> 的实现).
    /// </summary>
    public static class ExNativeMemory {

        /// <summary>Allocates a block of memory of the specified size, in bytes (分配指定大小的内存块（以字节为单位）).</summary>
        /// <param name="byteCount">The size, in bytes, of the block to allocate (要分配的块的大小（以字节为单位）).</param>
        /// <returns>A pointer to the allocated block of memory (指向分配的内存块的指针).</returns>
        /// <exception cref="OutOfMemoryException">Allocating <paramref name="byteCount" /> of memory failed.</exception>
        /// <remarks>
        ///     <para>This method allows <paramref name="byteCount" /> to be <c>0</c> and will return a valid pointer that should not be dereferenced and that should be passed to free to avoid memory leaks (此方法允许 byteCount 为 0 ，将返回一个有效的指针, 该指针不应被取消引用, 应传递给 free 以避免内存泄漏).</para>
        ///     <para>This method is a thin wrapper over the C <c>malloc</c> API (此方法是 C malloc API 上的精简包装器).</para>
        /// </remarks>
        [CLSCompliant(false)]
        public unsafe static void* Alloc(nuint byteCount) {
#if NET6_0_OR_GREATER
            return NativeMemory.Alloc(byteCount);
#else
            nint byteCountUsed = (nint)byteCount;
            if (byteCountUsed < 0) {
                throw new OutOfMemoryException(string.Format("There is insufficient memory to satisfy the request. The byteCount is {0}.", byteCount));
            }
            nint result = Marshal.AllocHGlobal(byteCountUsed);
            return (void*)result;
#endif // NET6_0_OR_GREATER
        }

        /// <summary>Allocates a block of memory of the specified size, in elements (按元素分配指定大小的内存块).</summary>
        /// <param name="elementCount">The count, in elements, of the block to allocate (要分配的块的数量（以元素为单位）).</param>
        /// <param name="elementSize">The size, in bytes, of each element in the allocation (分配中每个元素的大小（以字节为单位）).</param>
        /// <returns>A pointer to the allocated block of memory (指向分配的内存块的指针).</returns>
        /// <exception cref="OutOfMemoryException">Allocating <paramref name="elementCount" /> * <paramref name="elementSize" /> bytes of memory failed.</exception>
        /// <remarks>
        ///     <para>This method allows <paramref name="elementCount" /> and/or <paramref name="elementSize" /> to be <c>0</c> and will return a valid pointer that should not be dereferenced and that should be passed to free to avoid memory leaks (此方法允许 elementCount/ elementSize 为 0 ，将返回一个有效的指针, 该指针不应被取消引用, 应传递给 free 以避免内存泄漏).</para>
        ///     <para>This method is a thin wrapper over the C <c>malloc</c> API (此方法是 C malloc API 上的精简包装器).</para>
        /// </remarks>
        [CLSCompliant(false)]
        public unsafe static void* Alloc(nuint elementCount, nuint elementSize) {
#if NET6_0_OR_GREATER
            return NativeMemory.Alloc(elementCount, elementSize);
#else
            nuint byteCount = GetByteCount(elementCount, elementSize);
            return Alloc(byteCount);
#endif // NET6_0_OR_GREATER
        }

        /// <summary>Allocates and zeroes a block of memory of the specified size, in bytes (分配并清零指定大小的内存块（以字节为单位）).</summary>
        /// <param name="byteCount">The size, in bytes, of the block to allocate (要分配的块的大小（以字节为单位）).</param>
        /// <returns>A pointer to the allocated and zeroed block of memory (指向已分配且清零的内存块的指针).</returns>
        /// <exception cref="OutOfMemoryException">Allocating <paramref name="byteCount" /> of memory failed.</exception>
        /// <remarks>
        ///     <para>This method allows <paramref name="byteCount" /> to be <c>0</c> and will return a valid pointer that should not be dereferenced and that should be passed to free to avoid memory leaks (此方法允许 byteCount 为 0 ，将返回一个有效的指针, 该指针不应被取消引用, 应传递给 free 以避免内存泄漏).</para>
        ///     <para>This method is a thin wrapper over the C <c>calloc</c> API (此方法是 C calloc API 上的精简包装器).</para>
        /// </remarks>
        [CLSCompliant(false)]
        public unsafe static void* AllocZeroed(nuint byteCount) {
#if NET6_0_OR_GREATER
            return NativeMemory.AllocZeroed(byteCount);
#else
            void* result = Alloc(byteCount);
            Clear(result, byteCount);
            return result;
#endif // NET6_0_OR_GREATER
        }

        /// <summary>Allocates and zeroes a block of memory of the specified size, in elements (按元素分配并清零指定大小的内存块).</summary>
        /// <param name="elementCount">The count, in elements, of the block to allocate (要分配的块的数量（以元素为单位）).</param>
        /// <param name="elementSize">The size, in bytes, of each element in the allocation (分配中每个元素的大小（以字节为单位）).</param>
        /// <returns>A pointer to the allocated and zeroed block of memory (指向已分配且清零的内存块的指针).</returns>
        /// <exception cref="OutOfMemoryException">Allocating <paramref name="elementCount" /> * <paramref name="elementSize" /> bytes of memory failed.</exception>
        /// <remarks>
        ///     <para>This method allows <paramref name="elementCount" /> and/or <paramref name="elementSize" /> to be <c>0</c> and will return a valid pointer that should not be dereferenced and that should be passed to free to avoid memory leaks (此方法允许 elementCount/ elementSize 为 0 ，将返回一个有效的指针, 该指针不应被取消引用, 应传递给 free 以避免内存泄漏).</para>
        ///     <para>This method is a thin wrapper over the C <c>calloc</c> API (此方法是 C calloc API 上的精简包装器).</para>
        /// </remarks>
        [CLSCompliant(false)]
        public unsafe static void* AllocZeroed(nuint elementCount, nuint elementSize) {
#if NET6_0_OR_GREATER
            return NativeMemory.AllocZeroed(elementCount, elementSize);
#else
            nuint byteCount = GetByteCount(elementCount, elementSize);
            return AllocZeroed(byteCount);
#endif // NET6_0_OR_GREATER
        }

        /// <summary>Clears a block of memory (清零内存块).</summary>
        /// <param name="ptr">A pointer to the block of memory that should be cleared (指向应清零的内存块的指针).</param>
        /// <param name="byteCount">The size, in bytes, of the block to clear (要清零的块的大小（以字节为单位）).</param>
        /// <remarks>
        ///     <para>If this method is called with <paramref name="ptr" /> being <see langword="null"/> and <paramref name="byteCount" /> being <c>0</c>, it will be equivalent to a no-op (如果以 ptr 为 null 且 byteCount 为 0调用此方法，则该方法等效于 no-op).</para>
        ///     <para>The behavior when <paramref name="ptr" /> is <see langword="null"/> and <paramref name="byteCount" /> is greater than <c>0</c> is undefined (当 ptr 为 null 且 byteCount 大于 0 时的行为未定义).</para>
        /// </remarks>
        [CLSCompliant(false)]
        public unsafe static void Clear(void* ptr, nuint byteCount) {
            ExMemoryMarshal.ClearWithoutReferences(ref *(byte*)ptr, byteCount);
        }

        /// <summary>
        /// Copies a block of memory from memory location <paramref name="source"/>
        /// to memory location <paramref name="destination"/> (将内存块从内存位置 source 复制到内存位置 destination).
        /// </summary>
        /// <param name="source">A pointer to the source of data to be copied (指向要复制的数据源的指针).</param>
        /// <param name="destination">A pointer to the destination memory block where the data is to be copied (指向要在其中复制数据的目标内存块的指针).</param>
        /// <param name="byteCount">The size, in bytes, to be copied from the source location to the destination (要从源位置复制到目标的大小（以字节为单位）).</param>
        [CLSCompliant(false)]
        public unsafe static void Copy(void* source, void* destination, nuint byteCount) {
            BufferHelper.Memmove(ref *(byte*)destination, ref *(byte*)source, byteCount);
        }

        /// <summary>
        /// Copies the byte <paramref name="value"/> to the first <paramref name="byteCount"/> bytes
        /// of the memory located at <paramref name="ptr"/> (将字节value复制到 ptr处起始、byteCount字节的内存块).
        /// </summary>
        /// <param name="ptr">A pointer to the block of memory to fill (指向要填充的内存块的指针).</param>
        /// <param name="byteCount">The number of bytes to be set to <paramref name="value"/> (要设置为 value的字节数).</param>
        /// <param name="value">The value to be set (要设置的值).</param>
        [CLSCompliant(false)]
        public unsafe static void Fill(void* ptr, nuint byteCount, byte value) {
            ExSpanHelpers.Fill(ref *(byte*)ptr, byteCount, value);
        }

        /// <summary>Frees a block of memory (释放内存块).</summary>
        /// <param name="ptr">A pointer to the block of memory that should be freed (指向应释放的内存块的指针).</param>
        /// <remarks>
        ///    <para>This method does nothing if <paramref name="ptr" /> is <c>null</c> (如果 ptr 为 null ，则此方法不执行任何作用).</para>
        ///    <para>This method is a thin wrapper over the C <c>free</c> API (此方法是 C free API 上的精简包装器).</para>
        /// </remarks>
        [CLSCompliant(false)]
        public unsafe static void Free(void* ptr) {
#if NET6_0_OR_GREATER
            NativeMemory.Free(ptr);
#else
            if (null == ptr) return;
            Marshal.FreeHGlobal((nint)ptr);
#endif // NET6_0_OR_GREATER
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static nuint GetByteCount(nuint elementCount, nuint elementSize) {
            // This is based on the `mi_count_size_overflow` and `mi_mul_overflow` methods from microsoft/mimalloc.
            // Original source is Copyright (c) 2019 Microsoft Corporation, Daan Leijen. Licensed under the MIT license

            // sqrt(nuint.MaxValue)
            nuint multiplyNoOverflow = (nuint)1 << (4 * Unsafe.SizeOf<nuint>());

            return ((elementSize >= multiplyNoOverflow) || (elementCount >= multiplyNoOverflow)) && (elementSize > 0) && ((IntPtrExtensions.UIntPtrMaxValue / elementSize) < elementCount) ? IntPtrExtensions.UIntPtrMaxValue : (elementCount * elementSize);
        }

        /// <summary>Reallocates a block of memory to be the specified size, in bytes (将内存块重新分配为指定大小（以字节为单位).</summary>
        /// <param name="ptr">The previously allocated block of memory (以前分配的内存块).</param>
        /// <param name="byteCount">The size, in bytes, of the reallocated block (重新分配的块的大小（以字节为单位）).</param>
        /// <returns>A pointer to the reallocated block of memory (指向重新分配的内存块的指针).</returns>
        /// <exception cref="OutOfMemoryException">Reallocating <paramref name="byteCount" /> of memory failed.</exception>
        /// <remarks>
        ///     <para>This method acts as <see cref="Alloc(nuint)" /> if <paramref name="ptr" /> is <c>null</c> (如果ptr 是 null, 此方法的作用就像 <see cref="Alloc(nuint)"/>).</para>
        ///     <para>This method allows <paramref name="byteCount" /> to be <c>0</c> and will return a valid pointer that should not be dereferenced and that should be passed to free to avoid memory leaks (此方法允许 byteCount 和 0 将返回不应取消引用的有效指针，并且应将其传递给 free 以避免内存泄漏).</para>
        ///     <para>This method is a thin wrapper over the C <c>realloc</c> API (此方法是 C realloc API 上的精简包装器).</para>
        /// </remarks>
        [CLSCompliant(false)]
        public unsafe static void* Realloc(void* ptr, nuint byteCount) {
#if NET6_0_OR_GREATER
            return NativeMemory.Realloc(ptr, byteCount);
#else
            nint byteCountUsed = (nint)byteCount;
            if (byteCountUsed < 0) {
                throw new OutOfMemoryException(string.Format("There is insufficient memory to satisfy the request. The byteCount is {0}.", byteCount));
            }
            nint result;
            if (null != ptr) {
                result = Marshal.ReAllocHGlobal((nint)ptr, byteCountUsed);
            } else {
                // It is used for unit test ReallocNullPtrTest compatible with NativeMemory.
                result = Marshal.AllocHGlobal(byteCountUsed);
            }
            return (void*)result;
#endif // NET6_0_OR_GREATER
        }

    }
}
