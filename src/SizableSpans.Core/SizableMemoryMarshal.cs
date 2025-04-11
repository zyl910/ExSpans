#if NET7_0_OR_GREATER
#define STRUCT_REF_FIELD // C# 11 - ref fields and ref scoped variables. https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/ref-struct#ref-fields
#endif // NET7_0_OR_GREATER

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Zyl.SizableSpans.Extensions;
using Zyl.SizableSpans.Reflection;

namespace Zyl.SizableSpans {
    /// <summary>
    /// Provides a collection of methods for interoperating with <see cref="SizableSpan{T}"/>, and <see cref="ReadOnlySizableSpan{T}"/>. It can be regarded as the <see cref="MemoryMarshal"/> of <see cref="TSize"/> index range (提供与 SizableSpan 和 SizableReadOnlySpan 互操作的方法. 它可以被视为 <see cref="TSize"/> 索引范围的 MemoryMarshal).
    /// </summary>
    public static partial class SizableMemoryMarshal {
        /// <summary>
        /// Casts a SizableSpan of one primitive type <typeparamref name="T"/> to SizableSpan of bytes.
        /// That type may not contain pointers or references. This is checked at runtime in order to preserve type safety.
        /// </summary>
        /// <param name="span">The source slice, of type <typeparamref name="T"/>.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when <typeparamref name="T"/> contains pointers.
        /// </exception>
        /// <exception cref="OverflowException">
        /// Thrown if the Length property of the new SizableSpan would exceed <see cref="TSize.MaxValue"/>.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SizableSpan<byte> AsBytes<T>(SizableSpan<T> span)
            where T : struct {
            if (TypeHelper.IsReferenceOrContainsReferences<T>())
                ThrowHelper.ThrowInvalidTypeWithPointersNotSupported(typeof(T));
            TSize len = span.Length.MultiplyChecked(Unsafe.SizeOf<T>());
#if STRUCT_REF_FIELD
            return new SizableSpan<byte>(ref Unsafe.As<T, byte>(ref GetReference(span)), len);
#else
            return new SizableSpan<byte>(MemoryMarshal.AsBytes(span._referenceSpan), span._byteOffse, len);
#endif // STRUCT_REF_FIELD
        }

        /// <summary>
        /// Casts a ReadOnlySizableSpan of one primitive type <typeparamref name="T"/> to ReadOnlySizableSpan of bytes.
        /// That type may not contain pointers or references. This is checked at runtime in order to preserve type safety.
        /// </summary>
        /// <param name="span">The source slice, of type <typeparamref name="T"/>.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when <typeparamref name="T"/> contains pointers.
        /// </exception>
        /// <exception cref="OverflowException">
        /// Thrown if the Length property of the new SizableSpan would exceed <see cref="TSize.MaxValue"/>.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySizableSpan<byte> AsBytes<T>(ReadOnlySizableSpan<T> span)
            where T : struct {
            if (TypeHelper.IsReferenceOrContainsReferences<T>())
                ThrowHelper.ThrowInvalidTypeWithPointersNotSupported(typeof(T));

            TSize len = span.Length.MultiplyChecked(Unsafe.SizeOf<T>());
#if STRUCT_REF_FIELD
            return new ReadOnlySizableSpan<byte>(ref Unsafe.As<T, byte>(ref GetReference(span)), len);
#else
            return new ReadOnlySizableSpan<byte>(MemoryMarshal.AsBytes(span._referenceSpan), span._byteOffse, len);
#endif // STRUCT_REF_FIELD
        }

        /// <summary>
        /// Returns a reference to the 0th element of the SizableSpan. If the SizableSpan is empty, returns a reference to the location where the 0th element
        /// would have been stored. Such a reference may or may not be null. It can be used for pinning but must never be dereferenced.
        /// </summary>
        public static ref T GetReference<T>(SizableSpan<T> span) => ref span.GetPinnableReference();

        /// <summary>
        /// Returns a reference to the 0th element of the ReadOnlySizableSpan. If the ReadOnlySizableSpan is empty, returns a reference to the location where the 0th element
        /// would have been stored. Such a reference may or may not be null. It can be used for pinning but must never be dereferenced.
        /// </summary>
        public static ref T GetReference<T>(ReadOnlySizableSpan<T> span) => ref Unsafe.AsRef(in span.GetPinnableReference());

        /// <summary>
        /// Returns a reference to the 0th element of the SizableSpan. If the SizableSpan is empty, returns a reference to fake non-null pointer. Such a reference can be used
        /// for pinning but must never be dereferenced. This is useful for interop with methods that do not accept null pointers for zero-sized buffers.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe ref T GetNonNullPinnableReference<T>(SizableSpan<T> span) => ref (!span.IsEmpty) ? ref span.GetPinnableReference() : ref Unsafe.AsRef<T>((void*)1);

        /// <summary>
        /// Returns a reference to the 0th element of the ReadOnlySizableSpan. If the ReadOnlySizableSpan is empty, returns a reference to fake non-null pointer. Such a reference
        /// can be used for pinning but must never be dereferenced. This is useful for interop with methods that do not accept null pointers for zero-sized buffers.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe ref T GetNonNullPinnableReference<T>(ReadOnlySizableSpan<T> span) => ref (!span.IsEmpty) ? ref Unsafe.AsRef(in span.GetPinnableReference()) : ref Unsafe.AsRef<T>((void*)1);

        /// <summary>
        /// Casts a SizableSpan of one primitive type <typeparamref name="TFrom"/> to another primitive type <typeparamref name="TTo"/>.
        /// These types may not contain pointers or references. This is checked at runtime in order to preserve type safety.
        /// </summary>
        /// <remarks>
        /// Supported only for platforms that support misaligned memory access or when the memory block is aligned by other means.
        /// </remarks>
        /// <param name="span">The source slice, of type <typeparamref name="TFrom"/>.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when <typeparamref name="TFrom"/> or <typeparamref name="TTo"/> contains pointers.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SizableSpan<TTo> Cast<TFrom, TTo>(SizableSpan<TFrom> span)
            where TFrom : struct
            where TTo : struct {
            if (TypeHelper.IsReferenceOrContainsReferences<TFrom>())
                ThrowHelper.ThrowInvalidTypeWithPointersNotSupported(typeof(TFrom));
            if (TypeHelper.IsReferenceOrContainsReferences<TTo>())
                ThrowHelper.ThrowInvalidTypeWithPointersNotSupported(typeof(TTo));

            // Use unsigned integers - unsigned division by constant (especially by power of 2)
            // and checked casts are faster and smaller.
            uint fromSize = (uint)Unsafe.SizeOf<TFrom>();
            uint toSize = (uint)Unsafe.SizeOf<TTo>();
            ulong fromLength = (ulong)span.Length;
            TSize toLength;
            if (fromSize == toSize) {
                // Special case for same size types - `(ulong)fromLength * (ulong)fromSize / (ulong)toSize`
                // should be optimized to just `length` but the JIT doesn't do that today.
                toLength = (TSize)fromLength;
            } else if (fromSize == 1) {
                // Special case for byte sized TFrom - `(ulong)fromLength * (ulong)fromSize / (ulong)toSize`
                // becomes `(ulong)fromLength / (ulong)toSize` but the JIT can't narrow it down to TSize
                // and can't eliminate the checked cast. This also avoids a 32 bit specific issue,
                // the JIT can't eliminate long multiply by 1.
                toLength = (TSize)(fromLength / toSize);
            } else {
                // Ensure that casts are done in such a way that the JIT is able to "see"
                // the uint->ulong casts and the multiply together so that on 32 bit targets
                // 32x32to64 multiplication is used.
                ulong toLengthUInt64 = (ulong)fromLength * (ulong)fromSize / (ulong)toSize;
                toLength = checked((TSize)toLengthUInt64);
            }

#if STRUCT_REF_FIELD
            return new SizableSpan<TTo>(ref Unsafe.As<TFrom, TTo>(ref GetReference(span)), toLength);
#else
            return new SizableSpan<TTo>(MemoryMarshal.Cast<TFrom, TTo>(span._referenceSpan), span._byteOffse, toLength);
#endif // STRUCT_REF_FIELD
        }

        /// <summary>
        /// Casts a ReadOnlySizableSpan of one primitive type <typeparamref name="TFrom"/> to another primitive type <typeparamref name="TTo"/>.
        /// These types may not contain pointers or references. This is checked at runtime in order to preserve type safety.
        /// </summary>
        /// <remarks>
        /// Supported only for platforms that support misaligned memory access or when the memory block is aligned by other means.
        /// </remarks>
        /// <param name="span">The source slice, of type <typeparamref name="TFrom"/>.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when <typeparamref name="TFrom"/> or <typeparamref name="TTo"/> contains pointers.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySizableSpan<TTo> Cast<TFrom, TTo>(ReadOnlySizableSpan<TFrom> span)
            where TFrom : struct
            where TTo : struct {
            if (TypeHelper.IsReferenceOrContainsReferences<TFrom>())
                ThrowHelper.ThrowInvalidTypeWithPointersNotSupported(typeof(TFrom));
            if (TypeHelper.IsReferenceOrContainsReferences<TTo>())
                ThrowHelper.ThrowInvalidTypeWithPointersNotSupported(typeof(TTo));

            // Use unsigned integers - unsigned division by constant (especially by power of 2)
            // and checked casts are faster and smaller.
            uint fromSize = (uint)Unsafe.SizeOf<TFrom>();
            uint toSize = (uint)Unsafe.SizeOf<TTo>();
            ulong fromLength = (ulong)span.Length;
            TSize toLength;
            if (fromSize == toSize) {
                // Special case for same size types - `(ulong)fromLength * (ulong)fromSize / (ulong)toSize`
                // should be optimized to just `length` but the JIT doesn't do that today.
                toLength = (TSize)fromLength;
            } else if (fromSize == 1) {
                // Special case for byte sized TFrom - `(ulong)fromLength * (ulong)fromSize / (ulong)toSize`
                // becomes `(ulong)fromLength / (ulong)toSize` but the JIT can't narrow it down to TSize
                // and can't eliminate the checked cast. This also avoids a 32 bit specific issue,
                // the JIT can't eliminate long multiply by 1.
                toLength = (TSize)(fromLength / toSize);
            } else {
                // Ensure that casts are done in such a way that the JIT is able to "see"
                // the uint->ulong casts and the multiply together so that on 32 bit targets
                // 32x32to64 multiplication is used.
                ulong toLengthUInt64 = (ulong)fromLength * (ulong)fromSize / (ulong)toSize;
                toLength = checked((TSize)toLengthUInt64);
            }

#if STRUCT_REF_FIELD
            return new ReadOnlySizableSpan<TTo>(ref Unsafe.As<TFrom, TTo>(ref GetReference(span)), toLength);
#else
            return new ReadOnlySizableSpan<TTo>(MemoryMarshal.Cast<TFrom, TTo>(span._referenceSpan), span._byteOffse, toLength);
#endif // STRUCT_REF_FIELD
        }

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER

        /// <summary>
        /// Creates a new span over a portion of a regular managed object. This can be useful
        /// if part of a managed object represents a "fixed array." This is dangerous because the
        /// <paramref name="length"/> is not checked.
        /// </summary>
        /// <param name="reference">A reference to data.</param>
        /// <param name="length">The number of <typeparamref name="T"/> elements the memory contains.</param>
        /// <returns>A span representing the specified reference and length.</returns>
        /// <remarks>
        /// This method should be used with caution. It is dangerous because the length argument is not checked.
        /// Even though the ref is annotated as scoped, it will be stored into the returned span, and the lifetime
        /// of the returned span will not be validated for safety, even by span-aware languages.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#if STRUCT_REF_FIELD
        public static SizableSpan<T> CreateSizableSpan<T>(scoped ref T reference, TSize length) => new SizableSpan<T>(ref Unsafe.AsRef(in reference), length);
#else
        public static SizableSpan<T> CreateSizableSpan<T>(ref T reference, TSize length) => new SizableSpan<T>(ref reference, length);
#endif

        /// <summary>
        /// Creates a new read-only span over a portion of a regular managed object. This can be useful
        /// if part of a managed object represents a "fixed array." This is dangerous because the
        /// <paramref name="length"/> is not checked.
        /// </summary>
        /// <param name="reference">A reference to data.</param>
        /// <param name="length">The number of <typeparamref name="T"/> elements the memory contains.</param>
        /// <returns>A read-only span representing the specified reference and length.</returns>
        /// <remarks>
        /// This method should be used with caution. It is dangerous because the length argument is not checked.
        /// Even though the ref is annotated as scoped, it will be stored into the returned span, and the lifetime
        /// of the returned span will not be validated for safety, even by span-aware languages.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#if STRUCT_REF_FIELD
        public static ReadOnlySizableSpan<T> CreateReadOnlySizableSpan<T>(scoped ref readonly T reference, TSize length) => new ReadOnlySizableSpan<T>(ref Unsafe.AsRef(in reference), length);
#else
        public static ReadOnlySizableSpan<T> CreateReadOnlySizableSpan<T>(ref readonly T reference, TSize length) => new ReadOnlySizableSpan<T>(ref Unsafe.AsRef(in reference), length);
#endif

#endif // NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER

#if NET6_0_OR_GREATER

        /// <summary>Creates a new read-only span for a null-terminated string.</summary>
        /// <param name="value">The pointer to the null-terminated string of characters.</param>
        /// <returns>A read-only span representing the specified null-terminated string, or an empty span if the pointer is null.</returns>
        /// <remarks>The returned span does not include the null terminator.</remarks>
        /// <exception cref="ArgumentException">The string is longer than <see cref="int.MaxValue"/>.</exception>
        [CLSCompliant(false)]
        public static unsafe ReadOnlySizableSpan<char> CreateReadOnlySizableSpanFromNullTerminated(char* value) {
            if (null == value) return default;
            //return new ReadOnlySizableSpan<char>(value, string.wcslen(value));
            return MemoryMarshal.CreateReadOnlySpanFromNullTerminated(value).AsReadOnlySizableSpan();
        }

        /// <summary>Creates a new read-only span for a null-terminated UTF-8 string.</summary>
        /// <param name="value">The pointer to the null-terminated string of bytes.</param>
        /// <returns>A read-only span representing the specified null-terminated string, or an empty span if the pointer is null.</returns>
        /// <remarks>The returned span does not include the null terminator, nor does it validate the well-formedness of the UTF-8 data.</remarks>
        /// <exception cref="ArgumentException">The string is longer than <see cref="int.MaxValue"/>.</exception>
        [CLSCompliant(false)]
        public static unsafe ReadOnlySizableSpan<byte> CreateReadOnlySizableSpanFromNullTerminated(byte* value) {
            if (null == value) return default;
            //return new ReadOnlySizableSpan<byte>(value, string.strlen(value));
            return MemoryMarshal.CreateReadOnlySpanFromNullTerminated(value).AsReadOnlySizableSpan();
        }

#endif // NET6_0_OR_GREATER

        // -- No need to support
        // public static bool TryGetArray<T>(ReadOnlyMemory<T> memory, out ArraySegment<T> segment);
        // public static bool TryGetMemoryManager<T, TManager>(ReadOnlyMemory<T> memory, [NotNullWhen(true)] out TManager? manager) where TManager : MemoryManager<T>
        // public static bool TryGetMemoryManager<T, TManager>(ReadOnlyMemory<T> memory, [NotNullWhen(true)] out TManager? manager, out int start, out int length) where TManager : MemoryManager<T>;
        // public static IEnumerable<T> ToEnumerable<T>(ReadOnlyMemory<T> memory);
        // public static bool TryGetString(ReadOnlyMemory<char> memory, [NotNullWhen(true)] out string? text, out int start, out int length);

        /// <summary>
        /// Reads a structure of type T out of a read-only span of bytes.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Read<T>(ReadOnlySizableSpan<byte> source)
            where T : struct {
            if (TypeHelper.IsReferenceOrContainsReferences<T>()) {
                ThrowHelper.ThrowInvalidTypeWithPointersNotSupported(typeof(T));
            }
            TSize size = (TSize)Unsafe.SizeOf<T>();
            if (size.GreaterThan(source.Length)) {
                throw new ArgumentOutOfRangeException(nameof(source), string.Format("The type size({0}) is greater then source length({1})!", size, source.Length));
            }
            return Unsafe.ReadUnaligned<T>(ref GetReference(source));
        }

        /// <summary>
        /// Reads a structure of type T out of a span of bytes.
        /// </summary>
        /// <returns>If the span is too small to contain the type T, return false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryRead<T>(ReadOnlySizableSpan<byte> source, out T value)
            where T : struct {
            if (TypeHelper.IsReferenceOrContainsReferences<T>()) {
                ThrowHelper.ThrowInvalidTypeWithPointersNotSupported(typeof(T));
            }
            TSize size = (TSize)Unsafe.SizeOf<T>();
            if (size.GreaterThan(source.Length)) {
                value = default;
                return false;
            }
            value = Unsafe.ReadUnaligned<T>(ref GetReference(source));
            return true;
        }

        /// <summary>
        /// Writes a structure of type T into a span of bytes.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write<T>(SizableSpan<byte> destination, in T value)
            where T : struct {
            if (TypeHelper.IsReferenceOrContainsReferences<T>()) {
                ThrowHelper.ThrowInvalidTypeWithPointersNotSupported(typeof(T));
            }
            TSize size = (TSize)Unsafe.SizeOf<T>();
            if (size.GreaterThan(destination.Length)) {
                throw new ArgumentOutOfRangeException(nameof(destination), string.Format("The type size({0}) is greater then destination length({1})!", size, destination.Length));
            }
            Unsafe.WriteUnaligned(ref GetReference(destination), value);
        }

        /// <summary>
        /// Writes a structure of type T into a span of bytes.
        /// </summary>
        /// <returns>If the span is too small to contain the type T, return false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryWrite<T>(SizableSpan<byte> destination, in T value)
            where T : struct {
            if (TypeHelper.IsReferenceOrContainsReferences<T>()) {
                ThrowHelper.ThrowInvalidTypeWithPointersNotSupported(typeof(T));
            }
            TSize size = (TSize)Unsafe.SizeOf<T>();
            if (size.GreaterThan(destination.Length)) {
                return false;
            }
            Unsafe.WriteUnaligned(ref GetReference(destination), value);
            return true;
        }

        /// <summary>
        /// Re-interprets a span of bytes as a reference to structure of type T.
        /// The type may not contain pointers or references. This is checked at runtime in order to preserve type safety.
        /// </summary>
        /// <remarks>
        /// Supported only for platforms that support misaligned memory access or when the memory block is aligned by other means.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T AsRef<T>(SizableSpan<byte> span)
            where T : struct {
            if (TypeHelper.IsReferenceOrContainsReferences<T>()) {
                ThrowHelper.ThrowInvalidTypeWithPointersNotSupported(typeof(T));
            }
            TSize size = (TSize)Unsafe.SizeOf<T>();
            if (size.GreaterThan(span.Length)) {
                throw new ArgumentOutOfRangeException(nameof(span), string.Format("The type size({0}) is greater then span length({1})!", size, span.Length));
            }
            return ref Unsafe.As<byte, T>(ref GetReference(span));
        }

        /// <summary>
        /// Re-interprets a span of bytes as a reference to structure of type T.
        /// The type may not contain pointers or references. This is checked at runtime in order to preserve type safety.
        /// </summary>
        /// <remarks>
        /// Supported only for platforms that support misaligned memory access or when the memory block is aligned by other means.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref readonly T AsRef<T>(ReadOnlySizableSpan<byte> span)
            where T : struct {
            if (TypeHelper.IsReferenceOrContainsReferences<T>()) {
                ThrowHelper.ThrowInvalidTypeWithPointersNotSupported(typeof(T));
            }
            TSize size = (TSize)Unsafe.SizeOf<T>();
            if (size.GreaterThan(span.Length)) {
                throw new ArgumentOutOfRangeException(nameof(span), string.Format("The type size({0}) is greater then span length({1})!", size, span.Length));
            }
            return ref Unsafe.As<byte, T>(ref GetReference(span));
        }

        // -- No need to support
        //public static Memory<T> CreateFromPinnedArray<T>(T[]? array, int start, int length);

    }
}
