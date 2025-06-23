#if NET7_0_OR_GREATER
#define STRUCT_REF_FIELD // C# 11 - ref fields and ref scoped variables. https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/ref-struct#ref-fields
#endif // NET7_0_OR_GREATER

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Zyl.ExSpans.Extensions;
using Zyl.ExSpans.Reflection;

namespace Zyl.ExSpans {
    /// <summary>
    /// Provides a collection of methods for interoperating with <see cref="ExSpan{T}"/>, and <see cref="ReadOnlyExSpan{T}"/>. It can be regarded as the <see cref="MemoryMarshal"/> of <see cref="TSize"/> index range (提供与 ExSpan 和 ExReadOnlySpan 互操作的方法. 它可以被视为 <see cref="TSize"/> 索引范围的 MemoryMarshal).
    /// </summary>
    public static partial class ExMemoryMarshal {
        /// <summary>
        /// Casts a ExSpan of one primitive type <typeparamref name="T"/> to ExSpan of bytes.
        /// That type may not contain pointers or references. This is checked at runtime in order to preserve type safety
        /// (将一个基元类型<typeparamref name="T"/>的ExSpan 转换为字节的ExSpan. 该类型不能包含指针或引用. 它会在运行时检查这一点, 以保护类型安全).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="span">The source slice, of type <typeparamref name="T"/> (<typeparamref name="T"/> 类型的源切片).</param>
        /// <returns>A ExSpan of type <see cref="Byte"/> (<see cref="Byte"/> 类型的 ExSpan).</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <typeparamref name="T"/> contains pointers.
        /// </exception>
        /// <exception cref="OverflowException">
        /// Thrown if the Length property of the new ExSpan would exceed <see cref="TSize.MaxValue"/>.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ExSpan<byte> AsBytes<T>(ExSpan<T> span)
            where T : struct {
            if (TypeHelper.IsReferenceOrContainsReferences<T>())
                ThrowHelper.ThrowInvalidTypeWithPointersNotSupported(typeof(T));
            TSize len = checked(span.Length * Unsafe.SizeOf<T>());
#if STRUCT_REF_FIELD
            return new ExSpan<byte>(ref Unsafe.As<T, byte>(ref GetReference(span)), len);
#else
            return new ExSpan<byte>(MemoryMarshal.AsBytes(span._referenceSpan), span._byteOffset, len);
#endif // STRUCT_REF_FIELD
        }

        /// <summary>
        /// Casts a ReadOnlyExSpan of one primitive type <typeparamref name="T"/> to ReadOnlyExSpan of bytes.
        /// That type may not contain pointers or references. This is checked at runtime in order to preserve type safety.
        /// (将一个基元类型<typeparamref name="T"/>的ExSpan 转换为字节的ReadOnlyExSpan. 该类型不能包含指针或引用. 它会在运行时检查这一点, 以保护类型安全).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="span">The source slice, of type <typeparamref name="T"/> (<typeparamref name="T"/> 类型的源切片).</param>
        /// <returns>A ReadOnlyExSpan of type <see cref="Byte"/> (<see cref="Byte"/> 类型的 ReadOnlyExSpan).</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <typeparamref name="T"/> contains pointers.
        /// </exception>
        /// <exception cref="OverflowException">
        /// Thrown if the Length property of the new ExSpan would exceed <see cref="TSize.MaxValue"/>.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyExSpan<byte> AsBytes<T>(ReadOnlyExSpan<T> span)
            where T : struct {
            if (TypeHelper.IsReferenceOrContainsReferences<T>())
                ThrowHelper.ThrowInvalidTypeWithPointersNotSupported(typeof(T));

            TSize len = checked(span.Length * Unsafe.SizeOf<T>());
#if STRUCT_REF_FIELD
            return new ReadOnlyExSpan<byte>(ref Unsafe.As<T, byte>(ref GetReference(span)), len);
#else
            return new ReadOnlyExSpan<byte>(MemoryMarshal.AsBytes(span._referenceSpan), span._byteOffset, len);
#endif // STRUCT_REF_FIELD
        }

        /// <summary>
        /// Returns a reference to the 0th element of the ExSpan. Such a reference may or may not be null. It can be used for pinning but must never be dereferenced
        /// (返回 ExSpan 中索引为 0 处元素的引用. 这样的引用可能为空, 也可能不为空. 它可以用于固定, 但绝不能解引用).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="span">The source span (源跨度).</param>
        /// <returns>a reference to the 0th element of the ExSpan (ExSpan 中索引为 0 处元素的引用).</returns>
        public static ref T GetReference<T>(ExSpan<T> span) => ref span.GetPinnableReference();

        /// <summary>
        /// Returns a reference to the 0th element of the ReadOnlyExSpan. Such a reference may or may not be null. It can be used for pinning but must never be dereferenced
        /// (返回 ReadOnlyExSpan 中索引为 0 处元素的引用. 这样的引用可能为空, 也可能不为空. 它可以用于固定, 但绝不能解引用).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="span">The source span (源跨度).</param>
        /// <returns>a reference to the 0th element of the ReadOnlyExSpan (ReadOnlyExSpan 中索引为 0 处元素的引用).</returns>
        public static ref T GetReference<T>(ReadOnlyExSpan<T> span) => ref Unsafe.AsRef(in span.GetPinnableReference());

        /// <summary>
        /// Returns a reference to the 0th element of the ExSpan. If the ExSpan is empty, returns a reference to fake non-null pointer. Such a reference can be used
        /// for pinning but must never be dereferenced. This is useful for interop with methods that do not accept null pointers for zero-sized buffers.
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="span">The source span (源跨度).</param>
        /// <returns>a reference to the 0th element of the ExSpan (ExSpan 中索引为 0 处元素的引用).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe ref T GetNonNullPinnableReference<T>(ExSpan<T> span) => ref (!span.IsEmpty) ? ref span.GetPinnableReference() : ref Unsafe.AsRef<T>((void*)1);

        /// <summary>
        /// Returns a reference to the 0th element of the ReadOnlyExSpan. If the ReadOnlyExSpan is empty, returns a reference to fake non-null pointer. Such a reference
        /// can be used for pinning but must never be dereferenced. This is useful for interop with methods that do not accept null pointers for zero-sized buffers.
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="span">The source span (源跨度).</param>
        /// <returns>a reference to the 0th element of the ReadOnlyExSpan (ReadOnlyExSpan 中索引为 0 处元素的引用).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe ref T GetNonNullPinnableReference<T>(ReadOnlyExSpan<T> span) => ref (!span.IsEmpty) ? ref Unsafe.AsRef(in span.GetPinnableReference()) : ref Unsafe.AsRef<T>((void*)1);

        /// <summary>
        /// Casts a ExSpan of one primitive type <typeparamref name="TFrom"/> to another primitive type <typeparamref name="TTo"/>.
        /// These types may not contain pointers or references. This is checked at runtime in order to preserve type safety.
        /// (将一种基元类型 <typeparamref name="TFrom"/>的ExSpan 转换为另一种基元类型 <typeparamref name="TTo"/>. 该类型不能包含指针或引用。它会在运行时检查这一点，以保护类型安全).
        /// </summary>
        /// <typeparam name="TFrom">The element type of the source span (源跨度的元素类型).</typeparam>
        /// <typeparam name="TTo">The element type of the target span (目标跨度的元素类型).</typeparam>
        /// <param name="span">The source slice, of type <typeparamref name="TFrom"/> (<typeparamref name="TFrom"/> 类型的源切片).</param>
        /// <returns>The converted span (转换后的跨度).</returns>
        /// <remarks>
        /// Supported only for platforms that support misaligned memory access or when the memory block is aligned by other means (仅当支持未对齐内存访问的平台或内存块通过其他方式对齐时, 才支持此方法).
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// Thrown when <typeparamref name="TFrom"/> or <typeparamref name="TTo"/> contains pointers.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ExSpan<TTo> Cast<TFrom, TTo>(ExSpan<TFrom> span)
            where TFrom : struct
            where TTo : struct {
            if (TypeHelper.IsReferenceOrContainsReferences<TFrom>())
                ThrowHelper.ThrowInvalidTypeWithPointersNotSupported(typeof(TFrom));
            if (TypeHelper.IsReferenceOrContainsReferences<TTo>())
                ThrowHelper.ThrowInvalidTypeWithPointersNotSupported(typeof(TTo));
            return CastUnsafe<TFrom, TTo>(span);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ExSpan<TTo> CastUnsafe<TFrom, TTo>(ExSpan<TFrom> span)
#if STRUCT_REF_FIELD
#else
            where TFrom : struct
            where TTo : struct
#endif // STRUCT_REF_FIELD
            {
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
            return new ExSpan<TTo>(ref Unsafe.As<TFrom, TTo>(ref GetReference(span)), toLength);
#else
            return new ExSpan<TTo>(MemoryMarshal.Cast<TFrom, TTo>(span._referenceSpan), span._byteOffset, toLength);
#endif // STRUCT_REF_FIELD
        }

        /// <summary>
        /// Casts a ReadOnlyExSpan of one primitive type <typeparamref name="TFrom"/> to another primitive type <typeparamref name="TTo"/>.
        /// These types may not contain pointers or references. This is checked at runtime in order to preserve type safety.
        /// (将一种基元类型<typeparamref name="TFrom"/>的 ReadOnlyExSpan 转换为另一种基元类型 <typeparamref name="TTo"/>. 该类型不能包含指针或引用。它会在运行时检查这一点，以保护类型安全).
        /// </summary>
        /// <typeparam name="TFrom">The element type of the source span (源跨度的元素类型).</typeparam>
        /// <typeparam name="TTo">The element type of the target span (目标跨度的元素类型).</typeparam>
        /// <param name="span">The source slice, of type <typeparamref name="TFrom"/> (<typeparamref name="TFrom"/> 类型的源切片).</param>
        /// <returns>The converted read-only span (转换后的只读跨度).</returns>
        /// <remarks>
        /// Supported only for platforms that support misaligned memory access or when the memory block is aligned by other means (仅当支持未对齐内存访问的平台或内存块通过其他方式对齐时, 才支持此方法).
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// Thrown when <typeparamref name="TFrom"/> or <typeparamref name="TTo"/> contains pointers.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyExSpan<TTo> Cast<TFrom, TTo>(ReadOnlyExSpan<TFrom> span)
            where TFrom : struct
            where TTo : struct {
            if (TypeHelper.IsReferenceOrContainsReferences<TFrom>())
                ThrowHelper.ThrowInvalidTypeWithPointersNotSupported(typeof(TFrom));
            if (TypeHelper.IsReferenceOrContainsReferences<TTo>())
                ThrowHelper.ThrowInvalidTypeWithPointersNotSupported(typeof(TTo));
            return CastUnsafe<TFrom, TTo>(span);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ReadOnlyExSpan<TTo> CastUnsafe<TFrom, TTo>(ReadOnlyExSpan<TFrom> span)
#if STRUCT_REF_FIELD
#else
            where TFrom : struct
            where TTo : struct
#endif // STRUCT_REF_FIELD
            {
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
            return new ReadOnlyExSpan<TTo>(ref Unsafe.As<TFrom, TTo>(ref GetReference(span)), toLength);
#else
            return new ReadOnlyExSpan<TTo>(MemoryMarshal.Cast<TFrom, TTo>(span._referenceSpan), span._byteOffset, toLength);
#endif // STRUCT_REF_FIELD
        }

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER

        /// <summary>
        /// Creates a new span over a portion of a regular managed object. This can be useful
        /// if part of a managed object represents a "fixed array." This is dangerous because the
        /// <paramref name="length"/> is not checked
        /// (根据常规托管对象的一部分来创建新的跨度. 如果托管对象的一部分表示了 “固定数组”, 这可能会很有用. 这很危险, 因为不会检查 <paramref name="length"/>).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="reference">A reference to data (数据的引用).</param>
        /// <param name="length">The number of <typeparamref name="T"/> elements the memory contains (存储里包含的 <typeparamref name="T"/> 元素的数量).</param>
        /// <returns>A span representing the specified reference and length (表示了指定引用和长度的跨度).</returns>
        /// <remarks>
        /// This method should be used with caution. It is dangerous because the length argument is not checked.
        /// Even though the ref is annotated as scoped, it will be stored into the returned span, and the lifetime
        /// of the returned span will not be validated for safety, even by span-aware languages
        /// (该方法应谨慎使用. 这种方法很危险, 因为它不会检查长度参数. 即使 ref 被申明为 scoped, 它也会存储在返回的跨度中, 而且返回的跨度的生命周期将不会进行安全验证, 即使是具有跨度意识的语言也是如此).
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#if STRUCT_REF_FIELD
        public static ExSpan<T> CreateExSpan<T>(scoped ref T reference, TSize length) => new ExSpan<T>(ref Unsafe.AsRef(in reference), length);
#else
        public static ExSpan<T> CreateExSpan<T>(ref T reference, TSize length) => new ExSpan<T>(ref reference, length);
#endif

        /// <summary>
        /// Creates a new read-only span over a portion of a regular managed object. This can be useful
        /// if part of a managed object represents a "fixed array." This is dangerous because the
        /// <paramref name="length"/> is not checked
        /// (根据常规托管对象的一部分来创建新的只读跨度. 如果托管对象的一部分表示了 “固定数组”, 这可能会很有用. 这很危险, 因为不会检查 <paramref name="length"/>).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="reference">A reference to data (数据的引用).</param>
        /// <param name="length">The number of <typeparamref name="T"/> elements the memory contains (存储里包含的 <typeparamref name="T"/> 元素的数量).</param>
        /// <returns>A read-only span representing the specified reference and length (表示了指定引用和长度的只读跨度).</returns>
        /// <remarks>
        /// This method should be used with caution. It is dangerous because the length argument is not checked.
        /// Even though the ref is annotated as scoped, it will be stored into the returned span, and the lifetime
        /// of the returned span will not be validated for safety, even by span-aware languages
        /// (该方法应谨慎使用. 这种方法很危险, 因为它不会检查长度参数. 即使 ref 被申明为 scoped, 它也会存储在返回的跨度中, 而且返回的跨度的生命周期将不会进行安全验证, 即使是具有跨度意识的语言也是如此).
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#if STRUCT_REF_FIELD
        public static ReadOnlyExSpan<T> CreateReadOnlyExSpan<T>(scoped ref readonly T reference, TSize length) => new ReadOnlyExSpan<T>(ref Unsafe.AsRef(in reference), length);
#else
        public static ReadOnlyExSpan<T> CreateReadOnlyExSpan<T>(ref readonly T reference, TSize length) => new ReadOnlyExSpan<T>(ref Unsafe.AsRef(in reference), length);
#endif

#endif // NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER

#if NET6_0_OR_GREATER

        /// <summary>Creates a new read-only span for a null-terminated string (为 空终止字符串 创建新的只读跨度).</summary>
        /// <param name="value">The pointer to the null-terminated string of characters (指向空终止字符串的字符指针).</param>
        /// <returns>A read-only span representing the specified null-terminated string, or an empty span if the pointer is null (表示指定空终结字符串的只读跨度, 如果指针为null, 则为空跨度).</returns>
        /// <remarks>The returned span does not include the null terminator (返回的跨度不包括空终止符).</remarks>
        /// <exception cref="ArgumentException">The string is longer than <see cref="int.MaxValue"/>.</exception>
        [CLSCompliant(false)]
        public static unsafe ReadOnlyExSpan<char> CreateReadOnlyExSpanFromNullTerminated(char* value) {
            if (null == value) return default;
            //return new ReadOnlyExSpan<char>(value, string.wcslen(value));
            return MemoryMarshal.CreateReadOnlySpanFromNullTerminated(value).AsReadOnlyExSpan();
        }

        /// <summary>Creates a new read-only span for a null-terminated UTF-8 string (为 空终止UTF-8字符串 创建新的只读跨度).</summary>
        /// <param name="value">The pointer to the null-terminated string of bytes (指向空终止字符串的字节指针).</param>
        /// <returns>A read-only span representing the specified null-terminated string, or an empty span if the pointer is null (表示指定空终结字符串的只读跨度, 如果指针为null, 则为空跨度).</returns>
        /// <remarks>The returned span does not include the null terminator, nor does it validate the well-formedness of the UTF-8 data (返回的跨度不包括空终止符, 也不验证UTF-8数据的格式正确性).</remarks>
        /// <exception cref="ArgumentException">The string is longer than <see cref="int.MaxValue"/>.</exception>
        [CLSCompliant(false)]
        public static unsafe ReadOnlyExSpan<byte> CreateReadOnlyExSpanFromNullTerminated(byte* value) {
            if (null == value) return default;
            //return new ReadOnlyExSpan<byte>(value, string.strlen(value));
            return MemoryMarshal.CreateReadOnlySpanFromNullTerminated(value).AsReadOnlyExSpan();
        }

#endif // NET6_0_OR_GREATER

        /// <summary>
        /// Get an array segment from the underlying memory.
        /// If unable to get the array segment, return false with a default array segment.
        /// </summary>
        public static bool TryGetArray<T>(ReadOnlyExMemory<T> memory, out ArraySegment<T> segment) {
            object? obj = memory.GetObjectStartLength(out int index, out int length);

            // As an optimization, we skip the "is string?" check below if typeof(T) is not char,
            // as ExMemory<T> / ROM<T> can't possibly contain a string instance in this case.

            if (obj != null && !(
                (typeof(T) == typeof(char) && obj.GetType() == typeof(string))
                )) {
                if (RuntimeHelpers.ObjectHasComponentSize(obj)) {
                    // The object has a component size, which means it's variable-length, but we already
                    // checked above that it's not a string. The only remaining option is that it's a T[]
                    // or a U[] which is blittable to a T[] (e.g., int[] and uint[]).

                    // The array may be prepinned, so remove the high bit from the start index in the line below.
                    // The ArraySegment<T> ctor will perform bounds checking on index & length.

                    segment = new ArraySegment<T>(Unsafe.As<T[]>(obj), index & ReadOnlyExMemory<T>.RemoveFlagsBitMask, length);
                    return true;
                } else {
                    // The object isn't null, and it's not variable-length, so the only remaining option
                    // is MemoryManager<T>. The ArraySegment<T> ctor will perform bounds checking on index & length.

                    Debug.Assert(obj is MemoryManager<T>);
                    if (Unsafe.As<MemoryManager<T>>(obj).TryGetArray(out ArraySegment<T> tempArraySegment)) {
                        segment = new ArraySegment<T>(tempArraySegment.Array!, tempArraySegment.Offset + index, length);
                        return true;
                    }
                }
            }

            // If we got to this point, the object is null, or it's a string, or it's a MemoryManager<T>
            // which isn't backed by an array. We'll quickly homogenize all zero-length ExMemory<T> instances
            // to an empty array for the purposes of reporting back to our caller.

            if (length == 0) {
                segment = ArraySegment<T>.Empty;
                return true;
            }

            // Otherwise, there's *some* data, but it's not convertible to T[].

            segment = default;
            return false;
        }

        /// <summary>
        /// Gets an <see cref="MemoryManager{T}"/> from the underlying read-only memory.
        /// If unable to get the <typeparamref name="TManager"/> type, returns false.
        /// </summary>
        /// <typeparam name="T">The element type of the <paramref name="memory" />.</typeparam>
        /// <typeparam name="TManager">The type of <see cref="MemoryManager{T}"/> to try and retrieve.</typeparam>
        /// <param name="memory">The memory to get the manager for.</param>
        /// <param name="manager">The returned manager of the <see cref="ReadOnlyExMemory{T}"/>.</param>
        /// <returns>A <see cref="bool"/> indicating if it was successful.</returns>
        public static bool TryGetMemoryManager<T, TManager>(ReadOnlyExMemory<T> memory, [NotNullWhen(true)] out TManager? manager)
            where TManager : MemoryManager<T> {
            TManager? localManager; // Use register for null comparison rather than byref
            manager = localManager = memory.GetObjectStartLength(out _, out _) as TManager;
#pragma warning disable 8762 // "Parameter 'manager' may not have a null value when exiting with 'true'."
            return localManager != null;
#pragma warning restore 8762
        }

        /// <summary>
        /// Gets an <see cref="MemoryManager{T}"/> and <paramref name="start" />, <paramref name="length" /> from the underlying read-only memory.
        /// If unable to get the <typeparamref name="TManager"/> type, returns false.
        /// </summary>
        /// <typeparam name="T">The element type of the <paramref name="memory" />.</typeparam>
        /// <typeparam name="TManager">The type of <see cref="MemoryManager{T}"/> to try and retrieve.</typeparam>
        /// <param name="memory">The memory to get the manager for.</param>
        /// <param name="manager">The returned manager of the <see cref="ReadOnlyExMemory{T}"/>.</param>
        /// <param name="start">The offset from the start of the <paramref name="manager" /> that the <paramref name="memory" /> represents.</param>
        /// <param name="length">The length of the <paramref name="manager" /> that the <paramref name="memory" /> represents.</param>
        /// <returns>A <see cref="bool"/> indicating if it was successful.</returns>
        public static bool TryGetMemoryManager<T, TManager>(ReadOnlyExMemory<T> memory, [NotNullWhen(true)] out TManager? manager, out int start, out int length)
           where TManager : MemoryManager<T> {
            TManager? localManager; // Use register for null comparison rather than byref
            manager = localManager = memory.GetObjectStartLength(out start, out length) as TManager;

            Debug.Assert(length >= 0);

            if (localManager == null) {
                start = default;
                length = default;
                return false;
            }
#pragma warning disable 8762 // "Parameter 'manager' may not have a null value when exiting with 'true'."
            return true;
#pragma warning restore 8762
        }

        /// <summary>
        /// Creates an <see cref="IEnumerable{T}"/> view of the given <paramref name="memory" /> to allow
        /// the <paramref name="memory" /> to be used in existing APIs that take an <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <typeparam name="T">The element type of the <paramref name="memory" />.</typeparam>
        /// <param name="memory">The ReadOnlyExMemory to view as an <see cref="IEnumerable{T}"/></param>
        /// <returns>An <see cref="IEnumerable{T}"/> view of the given <paramref name="memory" /></returns>
        public static IEnumerable<T> ToEnumerable<T>(ReadOnlyExMemory<T> memory) {
            object? obj = memory.GetObjectStartLength(out int index, out int length);

            // If the memory is empty, just return an empty array as the enumerable.
            if (length is 0 || obj is null) {
                return Array.Empty<T>();
            }

            // If the object is a string, we can optimize. If it isn't a slice, just return the string as the
            // enumerable. Otherwise, return an iterator dedicated to enumerating the object; while we could
            // use the general one for any ReadOnlyExMemory, that will incur a .Span access for every element.
            if (typeof(T) == typeof(char) && obj is string str) {
                return (IEnumerable<T>)(object)(index == 0 && length == str.Length ?
                    str :
                    FromString(str, index, length));

                static IEnumerable<char> FromString(string s, int offset, int count) {
                    for (int i = 0; i < count; i++) {
                        yield return s[offset + i];
                    }
                }
            }

            // If the object is an array, we can optimize. If it isn't a slice, just return the array as the
            // enumerable. Otherwise, return an iterator dedicated to enumerating the object.
            if (RuntimeHelpers.ObjectHasComponentSize(obj)) // Same check as in TryGetArray to confirm that obj is a T[] or a U[] which is blittable to a T[].
            {
                T[] array = Unsafe.As<T[]>(obj);
                index &= ReadOnlyExMemory<T>.RemoveFlagsBitMask; // the array may be prepinned, so remove the high bit from the start index in the line below.
                return index == 0 && length == array.Length ?
                    array :
                    FromArray(array, index, length);

                static IEnumerable<T> FromArray(T[] array, int offset, int count) {
                    for (int i = 0; i < count; i++) {
                        yield return array[offset + i];
                    }
                }
            }

            // The ROM<T> wraps a MemoryManager<T>. The best we can do is iterate, accessing .Span on each MoveNext.
            return FromMemoryManager(memory);

            static IEnumerable<T> FromMemoryManager(ReadOnlyExMemory<T> memory) {
                for (int i = 0; i < memory.Length; i++) {
                    yield return memory.Span[i];
                }
            }
        }

        /// <summary>Attempts to get the underlying <see cref="string"/> from a <see cref="ReadOnlyExMemory{T}"/>.</summary>
        /// <param name="memory">The memory that may be wrapping a <see cref="string"/> object.</param>
        /// <param name="text">The string.</param>
        /// <param name="start">The starting location in <paramref name="text"/>.</param>
        /// <param name="length">The number of items in <paramref name="text"/>.</param>
        /// <returns></returns>
        public static bool TryGetString(ReadOnlyExMemory<char> memory, [NotNullWhen(true)] out string? text, out int start, out int length) {
            if (memory.GetObjectStartLength(out int offset, out int count) is string s) {
                Debug.Assert(offset >= 0);
                Debug.Assert(count >= 0);
                text = s;
                start = offset;
                length = count;
                return true;
            } else {
                text = null;
                start = 0;
                length = 0;
                return false;
            }
        }

        /// <summary>
        /// Reads a structure of type <typeparamref name="T"/> out of a read-only span of bytes (从字节的只读跨度中读取的 <typeparamref name="T"/> 类型结构体).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="source">The source span (源跨度).</param>
        /// <returns>The structure retrieved from the read-only span (从只读跨度中读取的结构体).</returns>
        /// <exception cref="ArgumentException">T contains managed object references.</exception>
        /// <exception cref="ArgumentOutOfRangeException">source is smaller than T's length in bytes.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Read<T>(ReadOnlyExSpan<byte> source)
            where T : struct {
            if (TypeHelper.IsReferenceOrContainsReferences<T>()) {
                ThrowHelper.ThrowInvalidTypeWithPointersNotSupported(typeof(T));
            }
            TSize size = (TSize)Unsafe.SizeOf<T>();
            if (size > source.Length) {
                throw new ArgumentOutOfRangeException(nameof(source), string.Format("The type size({0}) is greater then source length({1})!", size, source.Length));
            }
            return Unsafe.ReadUnaligned<T>(ref GetReference(source));
        }

        /// <summary>
        /// Tries to read a structure of type <typeparamref name="T"/> from a read-only span of bytes (尝试从字节的只读跨度中读取 <typeparamref name="T"/> 类型结构体).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="source">The source span (源跨度).</param>
        /// <param name="value">When the method returns, an instance of <typeparamref name="T"/> (此方法返回时，为 <typeparamref name="T"/> 的实例).</param>
        /// <returns>true if the method succeeds in retrieving an instance of the structure; otherwise, false (如果此方法成功检索到结构体的实例, 则为 true; 否则为 false).</returns>
        /// <exception cref="ArgumentException">T contains managed object references.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryRead<T>(ReadOnlyExSpan<byte> source, out T value)
            where T : struct {
            if (TypeHelper.IsReferenceOrContainsReferences<T>()) {
                ThrowHelper.ThrowInvalidTypeWithPointersNotSupported(typeof(T));
            }
            TSize size = (TSize)Unsafe.SizeOf<T>();
            if (size > source.Length) {
                value = default;
                return false;
            }
            value = Unsafe.ReadUnaligned<T>(ref GetReference(source));
            return true;
        }

        /// <summary>
        /// Writes a structure of type <typeparamref name="T"/> into a span of bytes (将 <typeparamref name="T"/> 类型的结构体写入字节跨度内).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="destination">The destination span (目标跨度).</param>
        /// <param name="value">The structure to be written to the span (要写入到范围的结构体).</param>
        /// <exception cref="ArgumentException">T contains managed object references.</exception>
        /// <exception cref="ArgumentOutOfRangeException">destination is too small to contain value.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write<T>(ExSpan<byte> destination, in T value)
            where T : struct {
            if (TypeHelper.IsReferenceOrContainsReferences<T>()) {
                ThrowHelper.ThrowInvalidTypeWithPointersNotSupported(typeof(T));
            }
            TSize size = (TSize)Unsafe.SizeOf<T>();
            if (size > destination.Length) {
                throw new ArgumentOutOfRangeException(nameof(destination), string.Format("The type size({0}) is greater then destination length({1})!", size, destination.Length));
            }
            Unsafe.WriteUnaligned(ref GetReference(destination), value);
        }

        /// <summary>
        /// Tries to write a structure of type <typeparamref name="T"/> into a span of bytes (尝试将类型为 <typeparamref name="T"/> 的结构体写入到字节的跨度中).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="destination">The destination span (目标跨度).</param>
        /// <param name="value">The structure to be written to the span (要写入到范围的结构体).</param>
        /// <returns>true if the write operation succeeded; otherwise, false. The method returns false if the span is too small to contain <typeparamref name="T"/> (如果写入操作成功，则为 true；否则为 false。 如果跨度太小无法包含 <typeparamref name="T"/>，则此方法返回 false).</returns>
        /// <exception cref="ArgumentException">T contains managed object references.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryWrite<T>(ExSpan<byte> destination, in T value)
            where T : struct {
            if (TypeHelper.IsReferenceOrContainsReferences<T>()) {
                ThrowHelper.ThrowInvalidTypeWithPointersNotSupported(typeof(T));
            }
            TSize size = (TSize)Unsafe.SizeOf<T>();
            if (size > destination.Length) {
                return false;
            }
            Unsafe.WriteUnaligned(ref GetReference(destination), value);
            return true;
        }

        /// <summary>
        /// Re-interprets a span of bytes as a reference to structure of type <typeparamref name="T"/>.
        /// The type may not contain pointers or references. This is checked at runtime in order to preserve type safety.
        /// (将字节跨度重新解释为对 <typeparamref name="T"/> 类型结构体的引用. 该类型不能包含指针或引用. 它会在运行时检查这一点, 以保护类型安全).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="span">The source span (源跨度).</param>
        /// <returns>The reference to the structure of type <typeparamref name="T"/> (对 <typeparamref name="T"/> 类型结构的引用).</returns>
        /// <remarks>
        /// Supported only for platforms that support misaligned memory access or when the memory block is aligned by other means (仅当支持未对齐内存访问的平台或内存块通过其他方式对齐时, 才支持此方法).
        /// </remarks>
        /// <exception cref="ArgumentException">T contains managed object references.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T AsRef<T>(ExSpan<byte> span)
            where T : struct {
            if (TypeHelper.IsReferenceOrContainsReferences<T>()) {
                ThrowHelper.ThrowInvalidTypeWithPointersNotSupported(typeof(T));
            }
            TSize size = (TSize)Unsafe.SizeOf<T>();
            if (size > span.Length) {
                throw new ArgumentOutOfRangeException(nameof(span), string.Format("The type size({0}) is greater then span length({1})!", size, span.Length));
            }
            return ref Unsafe.As<byte, T>(ref GetReference(span));
        }

        /// <summary>
        /// Re-interprets a read-only span of bytes as a reference to structure of type <typeparamref name="T"/>.
        /// The type may not contain pointers or references. This is checked at runtime in order to preserve type safety.
        /// (将字节只读跨度重新解释为对 <typeparamref name="T"/> 类型结构体的引用. 该类型不能包含指针或引用. 它会在运行时检查这一点, 以保护类型安全).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="span">The source span (源跨度).</param>
        /// <returns>The reference to the structure of type <typeparamref name="T"/> (对 <typeparamref name="T"/> 类型结构的引用).</returns>
        /// <remarks>
        /// Supported only for platforms that support misaligned memory access or when the memory block is aligned by other means (仅当支持未对齐内存访问的平台或内存块通过其他方式对齐时, 才支持此方法).
        /// </remarks>
        /// <exception cref="ArgumentException">T contains managed object references.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref readonly T AsRef<T>(ReadOnlyExSpan<byte> span)
            where T : struct {
            if (TypeHelper.IsReferenceOrContainsReferences<T>()) {
                ThrowHelper.ThrowInvalidTypeWithPointersNotSupported(typeof(T));
            }
            TSize size = (TSize)Unsafe.SizeOf<T>();
            if (size > span.Length) {
                throw new ArgumentOutOfRangeException(nameof(span), string.Format("The type size({0}) is greater then span length({1})!", size, span.Length));
            }
            return ref Unsafe.As<byte, T>(ref GetReference(span));
        }

        /// <summary>
        /// Creates a new memory over the portion of the pre-pinned target array beginning
        /// at 'start' index and ending at 'end' index (exclusive).
        /// </summary>
        /// <param name="array">The pre-pinned target array.</param>
        /// <param name="start">The index at which to begin the memory.</param>
        /// <param name="length">The number of items in the memory.</param>
        /// <remarks>This method should only be called on an array that is already pinned and
        /// that array should not be unpinned while the returned ExMemory<typeparamref name="T"/> is still in use.
        /// Calling this method on an unpinned array could result in memory corruption.</remarks>
        /// <remarks>Returns default when <paramref name="array"/> is null.</remarks>
        /// <exception cref="ArrayTypeMismatchException">Thrown when <paramref name="array"/> is covariant and array's type is not exactly T[].</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the specified <paramref name="start"/> or end index is not in the range (&lt;0 or &gt;=Length).
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ExMemory<T> CreateFromPinnedArray<T>(T[]? array, int start, int length) {
            if (array == null) {
                if (start != 0 || length != 0)
                    ThrowHelper.ThrowArgumentOutOfRangeException();
                return default;
            }
            if (!typeof(T).IsValueType && array.GetType() != typeof(T[]))
                ThrowHelper.ThrowArrayTypeMismatchException();
            if ((uint)start > (uint)array.Length || (uint)length > (uint)(array.Length - start))
                ThrowHelper.ThrowArgumentOutOfRangeException();

            // Before using _index, check if _index < 0, then 'and' it with RemoveFlagsBitMask
            return new ExMemory<T>((object)array, start | (1 << 31), length);
        }

    }
}
