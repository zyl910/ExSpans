#if NET9_0_OR_GREATER
#define ALLOWS_REF_STRUCT // C# 13 - ref struct interface; allows ref struct. https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-13#ref-struct-interfaces
#endif // NET9_0_OR_GREATER

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Zyl.ExSpans.Extensions;

namespace Zyl.ExSpans.Impl {
    /// <summary>
    /// Ex <see cref="Unsafe"/> methods.
    /// </summary>
    public static class ExUnsafe {

        /// <summary>
        /// Adds an element offset to the given managed pointer.
        /// </summary>
        /// <typeparam name="T">The elemental type of the managed pointer.</typeparam>
        /// <param name="source">The managed pointer to add the offset to.</param>
        /// <param name="elementOffset">The offset to add.</param>
        /// <returns>A new managed pointer that reflects the addition of the specified offset to the source pointer.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T Add<T>(ref T source, nint elementOffset)
#if ALLOWS_REF_STRUCT
                where T : allows ref struct
#endif // ALLOWS_REF_STRUCT
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
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T Add<T>(ref T source, nuint elementOffset)
#if ALLOWS_REF_STRUCT
                where T : allows ref struct
#endif // ALLOWS_REF_STRUCT
                {
#if NET6_0_OR_GREATER
            return ref Unsafe.Add(ref source, elementOffset);
#else
            return ref Unsafe.Add(ref source, IntPtrExtensions.ToIntPtr(elementOffset));
#endif // NET6_0_OR_GREATER
        }

        /// <summary>
        /// Adds a byte offset to the given managed pointer.
        /// </summary>
        /// <typeparam name="T">The elemental type of the managed pointer.</typeparam>
        /// <param name="source">The managed pointer to add the offset to.</param>
        /// <param name="byteOffset">The offset to add.</param>
        /// <returns>A new managed pointer that reflects the addition of the specified byte offset to the source pointer.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T AddByteOffset<T>(ref T source, nint byteOffset)
#if ALLOWS_REF_STRUCT
                where T : allows ref struct
#endif // ALLOWS_REF_STRUCT
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
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T AddByteOffset<T>(ref T source, nuint byteOffset)
#if ALLOWS_REF_STRUCT
                where T : allows ref struct
#endif // ALLOWS_REF_STRUCT
                {
#if NET6_0_OR_GREATER
            return ref Unsafe.AddByteOffset(ref source, byteOffset);
#else
            return ref AddByteOffset(ref source, IntPtrExtensions.ToIntPtr(byteOffset));
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
        public unsafe static ref T AddAsRef<T>(nint source, nint elementOffset)
#if ALLOWS_REF_STRUCT
                where T : allows ref struct
#endif // ALLOWS_REF_STRUCT
                {
            return ref Add(ref Unsafe.AsRef<T>((void*)source), elementOffset);
        }

        /// <summary>
        /// Adds an element offset to the given managed pointer.
        /// </summary>
        /// <typeparam name="T">The elemental type of the managed pointer.</typeparam>
        /// <param name="source">The managed pointer to add the offset to.</param>
        /// <param name="elementOffset">The offset to add.</param>
        /// <returns>A new managed pointer that reflects the addition of the specified offset to the source pointer.</returns>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static ref T AddAsRef<T>(nuint source, nuint elementOffset)
#if ALLOWS_REF_STRUCT
                where T : allows ref struct
#endif // ALLOWS_REF_STRUCT
                {
            //ulong num2 = (ulong)index * (ulong)Unsafe.SizeOf<T>();
            //return (nuint)(IntPtr)((byte*)(void*)start + num2);
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
        public unsafe static nint AddPointer<T>(nint source, nint elementOffset)
#if ALLOWS_REF_STRUCT
                where T : allows ref struct
#endif // ALLOWS_REF_STRUCT
                {
            ref T p = ref AddAsRef<T>(source, elementOffset);
            return (nint)Unsafe.AsPointer(ref p);
        }

        /// <summary>
        /// Native pointer add (原生指针加法).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="source">The managed pointer to add the offset to.</param>
        /// <param name="elementOffset">The offset to add.</param>
        /// <returns>Returns added pointer (返回相加后的指针).</returns>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        //[MethodImpl(MethodImplOptions.NoInlining)]
        public unsafe static nuint AddPointer<T>(nuint source, nuint elementOffset)
#if ALLOWS_REF_STRUCT
                where T : allows ref struct
#endif // ALLOWS_REF_STRUCT
                {
            ref T p = ref AddAsRef<T>(source, elementOffset);
            nuint rt = (nuint)Unsafe.AsPointer(ref p);
#if NETSTANDARD1_3_OR_GREATER || NETCOREAPP1_0_OR_GREATER || NET46_OR_GREATER
            //Console.WriteLine("AddPointer: {0}, {1}, {2}", source, elementOffset, (nuint)rt);
#endif
            return rt;
        }

        /// <summary>
        /// Reinterprets the given value of type <typeparamref name="TFrom" /> as a value of type <typeparamref name="TTo" />.
        /// </summary>
        /// <exception cref="NotSupportedException">The sizes of <typeparamref name="TFrom" /> and <typeparamref name="TTo" /> are not the same
        /// or the type parameters are not <see langword="struct"/>s.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TTo BitCast<TFrom, TTo>(TFrom source)
#if ALLOWS_REF_STRUCT
            where TFrom : allows ref struct
            where TTo : allows ref struct
#endif // ALLOWS_REF_STRUCT
            {
#if NET8_0_OR_GALLOWS_REF_STRUCTREATER
            return Unsafe.BitCast<TFrom, TTo>(source); // NET8_0_OR_GREATER
#else
            if (Unsafe.SizeOf<TFrom>() != Unsafe.SizeOf<TTo>() || default(TFrom) is null || default(TTo) is null) {
                throw new NotSupportedException();
            }
            return Unsafe.ReadUnaligned<TTo>(ref Unsafe.As<TFrom, byte>(ref source));
#endif // ALLOWS_REF_STRUCT
        }

        /// <summary>
        /// Get byte size (取得字节长度).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="length">The length (长度).</param>
        /// <returns>The byte length (字节长度).</returns>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nint GetByteSize<T>(nint length)
#if ALLOWS_REF_STRUCT
                where T : allows ref struct
#endif // ALLOWS_REF_STRUCT
                {
            return GetByteSize<T>(length.ToUIntPtr()).ToIntPtr();
        }

        /// <summary>
        /// Get byte size (取得字节长度).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="length">The length (长度).</param>
        /// <returns>The byte length (字节长度).</returns>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        //[MethodImpl(MethodImplOptions.NoInlining)]
        public static nuint GetByteSize<T>(nuint length)
#if ALLOWS_REF_STRUCT
                where T : allows ref struct
#endif // ALLOWS_REF_STRUCT
                {
#if NETCOREAPP3_0_OR_GREATER
            //return AddPointer<T>(0, length);
#else
#endif // NETCOREAPP3_0_OR_GREATER
            return length * (nuint)Unsafe.SizeOf<T>();
        }

        /// <summary>
        /// Returns a pointer integer to the given by-ref parameter (返回所给引用的指针整数值).
        /// </summary>
        /// <typeparam name="T">The type of object (对象类型).</typeparam>
        /// <param name="value">The object whose pointer is obtained (指针被获取的对象).</param>
        /// <returns>A pointer integer to the given value (所给引用的指针整数值).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static IntPtr AsPointerInt<T>(ref readonly T value)
#if ALLOWS_REF_STRUCT
                where T : allows ref struct
#endif // ALLOWS_REF_STRUCT
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T Subtract<T>(ref T source, nint elementOffset)
#if ALLOWS_REF_STRUCT
                where T : allows ref struct
#endif // ALLOWS_REF_STRUCT
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
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T Subtract<T>(ref T source, nuint elementOffset)
#if ALLOWS_REF_STRUCT
                where T : allows ref struct
#endif // ALLOWS_REF_STRUCT
                {
#if NET6_0_OR_GREATER
            return ref Unsafe.Subtract(ref source, elementOffset);
#else
            return ref Unsafe.Subtract(ref source, IntPtrExtensions.ToIntPtr(elementOffset));
#endif // NET6_0_OR_GREATER
        }

    }
}
