using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Zyl.SizableSpans.Impl {
    /// <summary>
    /// Sizable <see cref="Unsafe"/> methods.
    /// </summary>
    public static class SizableUnsafe {

        /// <summary>
        /// Adds an element offset to the given managed pointer.
        /// </summary>
        /// <typeparam name="T">The elemental type of the managed pointer.</typeparam>
        /// <param name="source">The managed pointer to add the offset to.</param>
        /// <param name="elementOffset">The offset to add.</param>
        /// <returns>A new managed pointer that reflects the addition of the specified offset to the source pointer.</returns>
        //[CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T Add<T>(ref T source, nint elementOffset) {
            return ref Unsafe.Add(ref source, elementOffset);
        }

        /// <summary>
        /// Adds an element offset to the given managed pointer.
        /// </summary>
        /// <typeparam name="T">The elemental type of the managed pointer.</typeparam>
        /// <param name="source">The managed pointer to add the offset to.</param>
        /// <param name="elementOffset">The offset to add.</param>
        /// <returns>A new managed pointer that reflects the addition of the specified offset to the source pointer.</returns>
        //[CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T Add<T>(ref T source, nuint elementOffset) {
#if NET6_0_OR_GREATER
            return ref Unsafe.Add(ref source, elementOffset);
#else
            return ref Unsafe.Add(ref source, ToIntPtr(elementOffset));
#endif // NET6_0_OR_GREATER
        }

        /// <summary>
        /// Adds an element offset to the given managed pointer.
        /// </summary>
        /// <typeparam name="T">The elemental type of the managed pointer.</typeparam>
        /// <param name="source">The managed pointer to add the offset to.</param>
        /// <param name="elementOffset">The offset to add.</param>
        /// <returns>A new managed pointer that reflects the addition of the specified offset to the source pointer.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static ref T AddAsRef<T>(TSize source, TSize elementOffset) {
            //ulong num2 = (ulong)index * (ulong)Unsafe.SizeOf<T>();
            //return (TSize)(IntPtr)((byte*)(void*)start + num2);
            return ref Add(ref Unsafe.AsRef<T>((void*)source), elementOffset);
        }

        /// <summary>
        /// Native pointer add (原生指针加法).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="source">The managed pointer to add the offset to.</param>
        /// <param name="elementOffset">The offset to add.</param>
        /// <returns>Returns added pointer (返回相加后的指针).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static TSize AddPointer<T>(TSize source, TSize elementOffset) {
            ref T p = ref AddAsRef<T>(source, elementOffset);
            return (TSize)Unsafe.AsPointer(ref p);
        }

        /// <summary>
        /// The <see cref="UIntPtr"/> to <see cref="IntPtr"/>.
        /// </summary>
        /// <param name="source">The source (源).</param>
        /// <returns><see cref="IntPtr"/> value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nint ToIntPtr(nuint source) {
#if NET7_0_OR_GREATER
            return (nint)source;
#else
            //return (nint)(void*)source;
            return Unsafe.As<nuint, nint>(ref source);
#endif // NET7_0_OR_GREATER
        }

        /// <summary>
        /// Get byte size (取得字节长度).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="length">The length (长度).</param>
        /// <returns>The byte length (字节长度).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSize GetByteSize<T>(TSize length) {
            return AddPointer<T>(UIntPtr.Zero, length);
        }

        /// <summary>
        /// The <see cref="IntPtr"/> to <see cref="UIntPtr"/>.
        /// </summary>
        /// <param name="source">The source (源).</param>
        /// <returns><see cref="UIntPtr"/> value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint ToUIntPtr(nint source) {
#if NET7_0_OR_GREATER
            return (nuint)source;
#else
            //return (nuint)(void*)source;
            return Unsafe.As<nint, nuint>(ref source);
#endif // NET7_0_OR_GREATER
        }

    }
}
