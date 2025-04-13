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
        /// That type may not contain pointers or references. This is checked at runtime in order to preserve type safety
        /// (将一个基元类型<typeparamref name="T"/>的SizableSpan 转换为字节的SizableSpan. 该类型不能包含指针或引用. 它会在运行时检查这一点, 以保护类型安全).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="span">The source slice, of type <typeparamref name="T"/> (<typeparamref name="T"/> 类型的源切片).</param>
        /// <returns>A SizableSpan of type <see cref="Byte"/> (<see cref="Byte"/> 类型的 SizableSpan).</returns>
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
        /// (将一个基元类型<typeparamref name="T"/>的SizableSpan 转换为字节的ReadOnlySizableSpan. 该类型不能包含指针或引用. 它会在运行时检查这一点, 以保护类型安全).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="span">The source slice, of type <typeparamref name="T"/> (<typeparamref name="T"/> 类型的源切片).</param>
        /// <returns>A ReadOnlySizableSpan of type <see cref="Byte"/> (<see cref="Byte"/> 类型的 ReadOnlySizableSpan).</returns>
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
        /// Returns a reference to the 0th element of the SizableSpan. Such a reference may or may not be null. It can be used for pinning but must never be dereferenced
        /// (返回 SizableSpan 中索引为 0 处元素的引用. 这样的引用可能为空, 也可能不为空. 它可以用于固定, 但绝不能解引用).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="span">The source span (源跨度).</param>
        /// <returns>a reference to the 0th element of the SizableSpan (SizableSpan 中索引为 0 处元素的引用).</returns>
        public static ref T GetReference<T>(SizableSpan<T> span) => ref span.GetPinnableReference();

        /// <summary>
        /// Returns a reference to the 0th element of the ReadOnlySizableSpan. Such a reference may or may not be null. It can be used for pinning but must never be dereferenced
        /// (返回 ReadOnlySizableSpan 中索引为 0 处元素的引用. 这样的引用可能为空, 也可能不为空. 它可以用于固定, 但绝不能解引用).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="span">The source span (源跨度).</param>
        /// <returns>a reference to the 0th element of the ReadOnlySizableSpan (ReadOnlySizableSpan 中索引为 0 处元素的引用).</returns>
        public static ref T GetReference<T>(ReadOnlySizableSpan<T> span) => ref Unsafe.AsRef(in span.GetPinnableReference());

        /// <summary>
        /// Returns a reference to the 0th element of the SizableSpan. If the SizableSpan is empty, returns a reference to fake non-null pointer. Such a reference can be used
        /// for pinning but must never be dereferenced. This is useful for interop with methods that do not accept null pointers for zero-sized buffers.
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="span">The source span (源跨度).</param>
        /// <returns>a reference to the 0th element of the SizableSpan (SizableSpan 中索引为 0 处元素的引用).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe ref T GetNonNullPinnableReference<T>(SizableSpan<T> span) => ref (!span.IsEmpty) ? ref span.GetPinnableReference() : ref Unsafe.AsRef<T>((void*)1);

        /// <summary>
        /// Returns a reference to the 0th element of the ReadOnlySizableSpan. If the ReadOnlySizableSpan is empty, returns a reference to fake non-null pointer. Such a reference
        /// can be used for pinning but must never be dereferenced. This is useful for interop with methods that do not accept null pointers for zero-sized buffers.
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="span">The source span (源跨度).</param>
        /// <returns>a reference to the 0th element of the ReadOnlySizableSpan (ReadOnlySizableSpan 中索引为 0 处元素的引用).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe ref T GetNonNullPinnableReference<T>(ReadOnlySizableSpan<T> span) => ref (!span.IsEmpty) ? ref Unsafe.AsRef(in span.GetPinnableReference()) : ref Unsafe.AsRef<T>((void*)1);

        /// <summary>
        /// Casts a SizableSpan of one primitive type <typeparamref name="TFrom"/> to another primitive type <typeparamref name="TTo"/>.
        /// These types may not contain pointers or references. This is checked at runtime in order to preserve type safety.
        /// (将一种基元类型 <typeparamref name="TFrom"/>的SizableSpan 转换为另一种基元类型 <typeparamref name="TTo"/>. 该类型不能包含指针或引用。它会在运行时检查这一点，以保护类型安全).
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
        /// (将一种基元类型<typeparamref name="TFrom"/>的 ReadOnlySizableSpan 转换为另一种基元类型 <typeparamref name="TTo"/>. 该类型不能包含指针或引用。它会在运行时检查这一点，以保护类型安全).
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
        public static SizableSpan<T> CreateSizableSpan<T>(scoped ref T reference, TSize length) => new SizableSpan<T>(ref Unsafe.AsRef(in reference), length);
#else
        public static SizableSpan<T> CreateSizableSpan<T>(ref T reference, TSize length) => new SizableSpan<T>(ref reference, length);
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
        public static ReadOnlySizableSpan<T> CreateReadOnlySizableSpan<T>(scoped ref readonly T reference, TSize length) => new ReadOnlySizableSpan<T>(ref Unsafe.AsRef(in reference), length);
#else
        public static ReadOnlySizableSpan<T> CreateReadOnlySizableSpan<T>(ref readonly T reference, TSize length) => new ReadOnlySizableSpan<T>(ref Unsafe.AsRef(in reference), length);
#endif

#endif // NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER

#if NET6_0_OR_GREATER

        /// <summary>Creates a new read-only span for a null-terminated string (为 空终止字符串 创建新的只读跨度).</summary>
        /// <param name="value">The pointer to the null-terminated string of characters (指向空终止字符串的字符指针).</param>
        /// <returns>A read-only span representing the specified null-terminated string, or an empty span if the pointer is null (表示指定空终结字符串的只读跨度, 如果指针为null, 则为空跨度).</returns>
        /// <remarks>The returned span does not include the null terminator (返回的跨度不包括空终止符).</remarks>
        /// <exception cref="ArgumentException">The string is longer than <see cref="int.MaxValue"/>.</exception>
        [CLSCompliant(false)]
        public static unsafe ReadOnlySizableSpan<char> CreateReadOnlySizableSpanFromNullTerminated(char* value) {
            if (null == value) return default;
            //return new ReadOnlySizableSpan<char>(value, string.wcslen(value));
            return MemoryMarshal.CreateReadOnlySpanFromNullTerminated(value).AsReadOnlySizableSpan();
        }

        /// <summary>Creates a new read-only span for a null-terminated UTF-8 string (为 空终止UTF-8字符串 创建新的只读跨度).</summary>
        /// <param name="value">The pointer to the null-terminated string of bytes (指向空终止字符串的字节指针).</param>
        /// <returns>A read-only span representing the specified null-terminated string, or an empty span if the pointer is null (表示指定空终结字符串的只读跨度, 如果指针为null, 则为空跨度).</returns>
        /// <remarks>The returned span does not include the null terminator, nor does it validate the well-formedness of the UTF-8 data (返回的跨度不包括空终止符, 也不验证UTF-8数据的格式正确性).</remarks>
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
        /// Reads a structure of type <typeparamref name="T"/> out of a read-only span of bytes (从字节的只读跨度中读取的 <typeparamref name="T"/> 类型结构体).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="source">The source span (源跨度).</param>
        /// <returns>The structure retrieved from the read-only span (从只读跨度中读取的结构体).</returns>
        /// <exception cref="ArgumentException">T contains managed object references.</exception>
        /// <exception cref="ArgumentOutOfRangeException">source is smaller than T's length in bytes.</exception>
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
        /// Tries to read a structure of type <typeparamref name="T"/> from a read-only span of bytes (尝试从字节的只读跨度中读取 <typeparamref name="T"/> 类型结构体).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="source">The source span (源跨度).</param>
        /// <param name="value">When the method returns, an instance of <typeparamref name="T"/> (此方法返回时，为 <typeparamref name="T"/> 的实例).</param>
        /// <returns>true if the method succeeds in retrieving an instance of the structure; otherwise, false (如果此方法成功检索到结构体的实例, 则为 true; 否则为 false).</returns>
        /// <exception cref="ArgumentException">T contains managed object references.</exception>
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
        /// Writes a structure of type <typeparamref name="T"/> into a span of bytes (将 <typeparamref name="T"/> 类型的结构体写入字节跨度内).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="destination">The destination span (目标跨度).</param>
        /// <param name="value">The structure to be written to the span (要写入到范围的结构体).</param>
        /// <exception cref="ArgumentException">T contains managed object references.</exception>
        /// <exception cref="ArgumentOutOfRangeException">destination is too small to contain value.</exception>
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
        /// Tries to write a structure of type <typeparamref name="T"/> into a span of bytes (尝试将类型为 <typeparamref name="T"/> 的结构体写入到字节的跨度中).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="destination">The destination span (目标跨度).</param>
        /// <param name="value">The structure to be written to the span (要写入到范围的结构体).</param>
        /// <returns>true if the write operation succeeded; otherwise, false. The method returns false if the span is too small to contain <typeparamref name="T"/> (如果写入操作成功，则为 true；否则为 false。 如果跨度太小无法包含 <typeparamref name="T"/>，则此方法返回 false).</returns>
        /// <exception cref="ArgumentException">T contains managed object references.</exception>
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
