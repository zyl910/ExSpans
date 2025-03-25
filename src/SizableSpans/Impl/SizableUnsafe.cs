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
        public static ref T Add<T>(ref T source, nint elementOffset)
#if NET9_0_OR_GREATER
                where T : allows ref struct
#endif // NET9_0_OR_GREATER
                {
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
        public static ref T Add<T>(ref T source, nuint elementOffset)
#if NET9_0_OR_GREATER
                where T : allows ref struct
#endif // NET9_0_OR_GREATER
                {
#if NET6_0_OR_GREATER
            return ref Unsafe.Add(ref source, elementOffset);
#else
            return ref Unsafe.Add(ref source, IntPtrs.ToIntPtr(elementOffset));
#endif // NET6_0_OR_GREATER
        }

        /// <summary>
        /// Adds a byte offset to the given managed pointer.
        /// </summary>
        /// <typeparam name="T">The elemental type of the managed pointer.</typeparam>
        /// <param name="source">The managed pointer to add the offset to.</param>
        /// <param name="byteOffset">The offset to add.</param>
        /// <returns>A new managed pointer that reflects the addition of the specified byte offset to the source pointer.</returns>
        public static ref T AddByteOffset<T>(ref T source, nint byteOffset)
#if NET9_0_OR_GREATER
                where T : allows ref struct
#endif // NET9_0_OR_GREATER
                {
#if NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NET461_OR_GREATER
            return ref Unsafe.AddByteOffset(ref source, byteOffset);
#else
            return ref Unsafe.As<byte, T>(ref Unsafe.Add(ref Unsafe.As<T, byte>(ref source), byteOffset));
#endif
        }

        /// <summary>
        /// Adds a byte offset to the given managed pointer.
        /// </summary>
        /// <typeparam name="T">The elemental type of the managed pointer.</typeparam>
        /// <param name="source">The managed pointer to add the offset to.</param>
        /// <param name="byteOffset">The offset to add.</param>
        /// <returns>A new managed pointer that reflects the addition of the specified byte offset to the source pointer.</returns>
        //[CLSCompliant(false)]
        public static ref T AddByteOffset<T>(ref T source, nuint byteOffset)
#if NET9_0_OR_GREATER
                where T : allows ref struct
#endif // NET9_0_OR_GREATER
                {
#if NET6_0_OR_GREATER
            return ref Unsafe.AddByteOffset(ref source, byteOffset);
#else
            return ref AddByteOffset(ref source, IntPtrs.ToIntPtr(byteOffset));
#endif
        }

        /// <summary>
        /// Adds an element offset to the given managed pointer.
        /// </summary>
        /// <typeparam name="T">The elemental type of the managed pointer.</typeparam>
        /// <param name="source">The managed pointer to add the offset to.</param>
        /// <param name="elementOffset">The offset to add.</param>
        /// <returns>A new managed pointer that reflects the addition of the specified offset to the source pointer.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static ref T AddAsRef<T>(TSize source, TSize elementOffset)
#if NET9_0_OR_GREATER
                where T : allows ref struct
#endif // NET9_0_OR_GREATER
                {
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
        public unsafe static TSize AddPointer<T>(TSize source, TSize elementOffset)
#if NET9_0_OR_GREATER
                where T : allows ref struct
#endif // NET9_0_OR_GREATER
                {
            ref T p = ref AddAsRef<T>(source, elementOffset);
            return (TSize)Unsafe.AsPointer(ref p);
        }

        /// <summary>
        /// Get byte size (取得字节长度).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="length">The length (长度).</param>
        /// <returns>The byte length (字节长度).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSize GetByteSize<T>(TSize length)
#if NET9_0_OR_GREATER
                where T : allows ref struct
#endif // NET9_0_OR_GREATER
                {
            return AddPointer<T>(UIntPtr.Zero, length);
        }

        /// <summary>
        /// Returns a pointer integer to the given by-ref parameter (返回所给引用的指针整数值).
        /// </summary>
        /// <typeparam name="T">The type of object (对象类型).</typeparam>
        /// <param name="value">The object whose pointer is obtained (指针被获取的对象).</param>
        /// <returns>A pointer integer to the given value (所给引用的指针整数值).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static IntPtr AsPointerInt<T>(ref readonly T value)
#if NET9_0_OR_GREATER
                where T : allows ref struct
#endif // NET9_0_OR_GREATER
                {
            //return (IntPtr)Unsafe.AsPointer(ref value);
            return (IntPtr)Unsafe.AsPointer(ref Unsafe.AsRef(in value));
        }

        /// <summary>
        /// Subtracts an element offset to the given managed pointer.
        /// </summary>
        /// <typeparam name="T">The elemental type of the managed pointer.</typeparam>
        /// <param name="source">The managed pointer to subtract the offset to.</param>
        /// <param name="elementOffset">The offset to subtract.</param>
        /// <returns>A new managed pointer that reflects the subtractition of the specified offset to the source pointer.</returns>
        //[CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T Subtract<T>(ref T source, nint elementOffset)
#if NET9_0_OR_GREATER
                where T : allows ref struct
#endif // NET9_0_OR_GREATER
                {
            return ref Unsafe.Subtract(ref source, elementOffset);
        }

        /// <summary>
        /// Subtracts an element offset to the given managed pointer.
        /// </summary>
        /// <typeparam name="T">The elemental type of the managed pointer.</typeparam>
        /// <param name="source">The managed pointer to subtract the offset to.</param>
        /// <param name="elementOffset">The offset to subtract.</param>
        /// <returns>A new managed pointer that reflects the subtractition of the specified offset to the source pointer.</returns>
        //[CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T Subtract<T>(ref T source, nuint elementOffset)
#if NET9_0_OR_GREATER
                where T : allows ref struct
#endif // NET9_0_OR_GREATER
                {
#if NET6_0_OR_GREATER
            return ref Unsafe.Subtract(ref source, elementOffset);
#else
            return ref Unsafe.Subtract(ref source, IntPtrs.ToIntPtr(elementOffset));
#endif // NET6_0_OR_GREATER
        }

    }
}
