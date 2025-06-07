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

        /// <summary>Allocates a block of memory of the specified size, in bytes.</summary>
        /// <param name="byteCount">The size, in bytes, of the block to allocate.</param>
        /// <returns>A pointer to the allocated block of memory.</returns>
        /// <exception cref="OutOfMemoryException">Allocating <paramref name="byteCount" /> of memory failed.</exception>
        /// <remarks>
        ///     <para>This method allows <paramref name="byteCount" /> to be <c>0</c> and will return a valid pointer that should not be dereferenced and that should be passed to free to avoid memory leaks.</para>
        ///     <para>This method is a thin wrapper over the C <c>malloc</c> API.</para>
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

        /// <summary>Allocates a block of memory of the specified size, in elements.</summary>
        /// <param name="elementCount">The count, in elements, of the block to allocate.</param>
        /// <param name="elementSize">The size, in bytes, of each element in the allocation.</param>
        /// <returns>A pointer to the allocated block of memory.</returns>
        /// <exception cref="OutOfMemoryException">Allocating <paramref name="elementCount" /> * <paramref name="elementSize" /> bytes of memory failed.</exception>
        /// <remarks>
        ///     <para>This method allows <paramref name="elementCount" /> and/or <paramref name="elementSize" /> to be <c>0</c> and will return a valid pointer that should not be dereferenced and that should be passed to free to avoid memory leaks.</para>
        ///     <para>This method is a thin wrapper over the C <c>malloc</c> API.</para>
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

        /// <summary>Allocates and zeroes a block of memory of the specified size, in bytes.</summary>
        /// <param name="byteCount">The size, in bytes, of the block to allocate.</param>
        /// <returns>A pointer to the allocated and zeroed block of memory.</returns>
        /// <exception cref="OutOfMemoryException">Allocating <paramref name="byteCount" /> of memory failed.</exception>
        /// <remarks>
        ///     <para>This method allows <paramref name="byteCount" /> to be <c>0</c> and will return a valid pointer that should not be dereferenced and that should be passed to free to avoid memory leaks.</para>
        ///     <para>This method is a thin wrapper over the C <c>calloc</c> API.</para>
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

        /// <summary>Allocates and zeroes a block of memory of the specified size, in elements.</summary>
        /// <param name="elementCount">The count, in elements, of the block to allocate.</param>
        /// <param name="elementSize">The size, in bytes, of each element in the allocation.</param>
        /// <returns>A pointer to the allocated and zeroed block of memory.</returns>
        /// <exception cref="OutOfMemoryException">Allocating <paramref name="elementCount" /> * <paramref name="elementSize" /> bytes of memory failed.</exception>
        /// <remarks>
        ///     <para>This method allows <paramref name="elementCount" /> and/or <paramref name="elementSize" /> to be <c>0</c> and will return a valid pointer that should not be dereferenced and that should be passed to free to avoid memory leaks.</para>
        ///     <para>This method is a thin wrapper over the C <c>calloc</c> API.</para>
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

        /// <summary>Clears a block of memory.</summary>
        /// <param name="ptr">A pointer to the block of memory that should be cleared.</param>
        /// <param name="byteCount">The size, in bytes, of the block to clear.</param>
        /// <remarks>
        ///     <para>If this method is called with <paramref name="ptr" /> being <see langword="null"/> and <paramref name="byteCount" /> being <c>0</c>, it will be equivalent to a no-op.</para>
        ///     <para>The behavior when <paramref name="ptr" /> is <see langword="null"/> and <paramref name="byteCount" /> is greater than <c>0</c> is undefined.</para>
        /// </remarks>
        [CLSCompliant(false)]
        public unsafe static void Clear(void* ptr, nuint byteCount) {
            ExMemoryMarshal.ClearWithoutReferences(ref *(byte*)ptr, byteCount);
        }

        /// <summary>
        /// Copies a block of memory from memory location <paramref name="source"/>
        /// to memory location <paramref name="destination"/>.
        /// </summary>
        /// <param name="source">A pointer to the source of data to be copied.</param>
        /// <param name="destination">A pointer to the destination memory block where the data is to be copied.</param>
        /// <param name="byteCount">The size, in bytes, to be copied from the source location to the destination.</param>
        [CLSCompliant(false)]
        public unsafe static void Copy(void* source, void* destination, nuint byteCount) {
            BufferHelper.Memmove(ref *(byte*)destination, ref *(byte*)source, byteCount);
        }

        /// <summary>
        /// Copies the byte <paramref name="value"/> to the first <paramref name="byteCount"/> bytes
        /// of the memory located at <paramref name="ptr"/>.
        /// </summary>
        /// <param name="ptr">A pointer to the block of memory to fill.</param>
        /// <param name="byteCount">The number of bytes to be set to <paramref name="value"/>.</param>
        /// <param name="value">The value to be set.</param>
        [CLSCompliant(false)]
        public unsafe static void Fill(void* ptr, nuint byteCount, byte value) {
            ExSpanHelpers.Fill(ref *(byte*)ptr, byteCount, value);
        }

        /// <summary>Frees a block of memory.</summary>
        /// <param name="ptr">A pointer to the block of memory that should be freed.</param>
        /// <remarks>
        ///    <para>This method does nothing if <paramref name="ptr" /> is <c>null</c>.</para>
        ///    <para>This method is a thin wrapper over the C <c>free</c> API.</para>
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

        /// <summary>Reallocates a block of memory to be the specified size, in bytes.</summary>
        /// <param name="ptr">The previously allocated block of memory.</param>
        /// <param name="byteCount">The size, in bytes, of the reallocated block.</param>
        /// <returns>A pointer to the reallocated block of memory.</returns>
        /// <exception cref="OutOfMemoryException">Reallocating <paramref name="byteCount" /> of memory failed.</exception>
        /// <remarks>
        ///     <para>This method acts as <see cref="Alloc(nuint)" /> if <paramref name="ptr" /> is <c>null</c>.</para>
        ///     <para>This method allows <paramref name="byteCount" /> to be <c>0</c> and will return a valid pointer that should not be dereferenced and that should be passed to free to avoid memory leaks.</para>
        ///     <para>This method is a thin wrapper over the C <c>realloc</c> API.</para>
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
