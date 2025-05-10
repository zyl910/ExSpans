#if NET7_0_OR_GREATER
#define STRUCT_REF_FIELD // C# 11 - ref fields and ref scoped variables. https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/ref-struct#ref-fields
#endif // NET7_0_OR_GREATER

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Zyl.ExSpans.Extensions;
using Zyl.ExSpans.Impl;
using Zyl.ExSpans.Reflection;
using EditorBrowsableAttribute = System.ComponentModel.EditorBrowsableAttribute;
using EditorBrowsableState = System.ComponentModel.EditorBrowsableState;

#pragma warning disable 0809 // Obsolete member 'ExSpan<T>.Equals(object)' overrides non-obsolete member 'object.Equals(object)'

namespace Zyl.ExSpans {
    /// <summary>
    /// Provides a type-safe and memory-safe read-only representation of a contiguous region of arbitrary memory. It can be regarded as the <see cref="ReadOnlySpan{T}"/> of <see cref="TSize"/> index range (提供任意内存连续区域的类型安全且内存安全的只读表示形式. 它可以被视为 <see cref="TSize"/> 索引范围的 <see cref="ReadOnlySpan{T}"/>).
    /// </summary>
    /// <typeparam name="T">The element type (元素的类型).</typeparam>
    [DebuggerTypeProxy(typeof(ExSpanDebugView<>))]
    [DebuggerDisplay("{ToString(),raw}")]
    //[NativeMarshalling(typeof(ReadOnlyExSpanMarshaller<,>))]
    public readonly ref partial struct ReadOnlyExSpan<T> : IExLength, IReadOnlyExSpanBase<T> {
        /// <summary>The number of elements this span contains (跨度中的项数).</summary>
        private readonly TSize _length;
#if STRUCT_REF_FIELD
        /// <summary>A byref or a native ptr (引用或原生指针).</summary>
        internal readonly ref T _reference;
#else
        /// <summary>A byte offset of _referenceSpan or a native ptr (_referenceSpan 的偏移, 或是原生指针).</summary>
        internal readonly TSize _byteOffset;
        /// <summary>A span of reference. It is Empty on native ptr (引用的跨度. 原生指针时它为空).</summary>
        internal readonly ReadOnlySpan<T> _referenceSpan;
#endif

        /// <summary>
        /// Creates a new <see cref="ReadOnlyExSpan{T}"/> over the entirety of the target array (在整个指定数组中创建新的 <see cref="ReadOnlyExSpan{T}"/>).
        /// </summary>
        /// <param name="array">The target array (指定数组).</param>
        /// <remarks>Returns default when <paramref name="array"/> is null (当 array 为 null 时返回默认值).</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyExSpan(T[]? array) {
            if (array == null) {
                this = default;
                return; // returns default
            }

            _length = array.ExLength();
#if STRUCT_REF_FIELD
            _reference = ref ExMemoryMarshal.GetArrayDataReference(array);
#else
            _byteOffset = (TSize)0;
            _referenceSpan = new ReadOnlySpan<T>(array);
#endif
        }

        /// <summary>
        /// Creates a new <see cref="ReadOnlyExSpan{T}"/> over the portion of the target array beginning at 'start' index and ending at 'end' index (exclusive) (创建一个新的 <see cref="ReadOnlyExSpan{T}"/>，其中包含从指定索引开始的数组的指定数量的元素).
        /// </summary>
        /// <param name="array">The target array (指定数组).</param>
        /// <param name="start">The zero-based index at which to begin the span (从零开始的跨度索引).</param>
        /// <param name="length">The number of items in the span (跨度的项数).</param>
        /// <remarks>Returns default when <paramref name="array"/> is null (当 array 为 null 时返回默认值).</remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the specified <paramref name="start"/> or end index is not in the range (&lt;0 or &gt;Length).
        /// </exception>
        [MyCLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyExSpan(T[]? array, TSize start, TSize length) {
            if (array == null) {
                if (start != (TSize)0 || length != (TSize)0)
                    ThrowHelper.ThrowArgumentOutOfRangeException();
                this = default;
                return; // returns default
            }
            TUSize srcLength = array.ExLength().ToUIntPtr();
            if (start.ToUIntPtr()> srcLength || length.ToUIntPtr() > (srcLength - start.ToUIntPtr())) {
                ThrowHelper.ThrowArgumentOutOfRangeException();
            }

            _length = length;
#if STRUCT_REF_FIELD
            _reference = ref Unsafe.Add(ref ExMemoryMarshal.GetArrayDataReference(array), start);
#else
            _byteOffset = ExUnsafe.GetByteSize<T>(start);
            _referenceSpan = new ReadOnlySpan<T>(array);
#endif
        }

        /// <summary>
        /// Creates a new <see cref="ReadOnlyExSpan{T}"/> over the target unmanaged buffer (在目标非托管缓冲区上创建新 <see cref="ReadOnlyExSpan{T}"/>).
        /// </summary>
        /// <param name="pointer">An unmanaged pointer to memory (指向内存的非托管指针).</param>
        /// <param name="length">The number of <typeparamref name="T"/> elements the memory contains (内存中包含的 <typeparamref name="T"/> 元素数量).</param>
        /// <exception cref="ArgumentException">
        /// Thrown when <typeparamref name="T"/> is reference type or contains pointers and hence cannot be stored in unmanaged memory.
        /// </exception>
        // /// <exception cref="ArgumentOutOfRangeException">
        // /// Thrown when the specified <paramref name="length"/> is negative.
        // /// </exception>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe ReadOnlyExSpan(void* pointer, TSize length) {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_0_OR_GREATER
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
                ThrowHelper.ThrowInvalidTypeWithPointersNotSupported(typeof(T));
#endif
            if (length < (TSize)0)
                ThrowHelper.ThrowArgumentOutOfRangeException();
            if (length == (TSize)0) {
                this = default;
                return; // returns default
            }

            _length = length;
#if STRUCT_REF_FIELD
            _reference = ref Unsafe.AsRef<T>(pointer); // *(T*)pointer;
#else
            _byteOffset = (TSize)pointer;
            _referenceSpan = ReadOnlySpan<T>.Empty;
#endif
        }

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        /// <summary>Creates a new <see cref="ReadOnlyExSpan{T}"/> of length 1 around the specified reference (在指定的引用周围创建长度为 1 的新 <see cref="ReadOnlyExSpan{T}"/>).</summary>
        /// <param name="reference">A reference to data (数据的引用).</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyExSpan(ref readonly T reference) {
            _length = (TSize)1;
#if STRUCT_REF_FIELD
            _reference = ref Unsafe.AsRef(in reference);
#else
            _byteOffset = (TSize)0;
            _referenceSpan = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef(in reference), 1); // Need NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
#endif
        }

        // Constructor for internal use only. It is not safe to expose publicly, and is instead exposed via the unsafe MemoryMarshal.CreateReadOnlyExSpan.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ReadOnlyExSpan(ref T reference, TSize length) {
            Debug.Assert(length >= (TSize)0);

            _length = length;
#if STRUCT_REF_FIELD
            _reference = ref Unsafe.AsRef(in reference);
#else
            _byteOffset = (TSize)0;
            _referenceSpan = MemoryMarshalHelper.CreateReadOnlySpanSaturating(ref Unsafe.AsRef(in reference), length); // Need NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
#endif
        }
#endif // NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER

#if STRUCT_REF_FIELD
#else
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ReadOnlyExSpan(ReadOnlySpan<T> referenceSpan, TSize byteOffse, TSize length) {
            _length = length;
            _byteOffset = byteOffse;
            _referenceSpan = referenceSpan;
        }
#endif

        /// <summary>
        /// Returns the specified element of the read-only Ex span (从只读扩展跨度中返回指定项).
        /// </summary>
        /// <param name="index">The zero-based index (从零开始的索引).</param>
        /// <returns>Returns the specified element (返回指定项).</returns>
        /// <exception cref="IndexOutOfRangeException">
        /// Thrown when index less than 0 or index greater than or equal to Length
        /// </exception>
        [MyCLSCompliant(false)]
        public ref readonly T this[TSize index] {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get {
                if (index.ToUIntPtr() >= _length.ToUIntPtr())
                    ThrowHelper.ThrowIndexOutOfRangeException();
#if STRUCT_REF_FIELD
                return ref Unsafe.Add(ref _reference, index);
#else
                unsafe {
                    if (_referenceSpan.IsEmpty) {
                        return ref ExUnsafe.Add(ref Unsafe.AsRef<T>((void*)_byteOffset), index);
                    } else {
                        return ref ExUnsafe.Add(ref Unsafe.AddByteOffset(ref Unsafe.AsRef(_referenceSpan.GetPinnableReference()), IntPtrExtensions.ToIntPtr(_byteOffset)), index);
                    }
                }
#endif
            }
        }

        /// <summary>
        /// The number of items in the read-only Ex span (只读扩展跨度中的项数).
        /// </summary>
        [MyCLSCompliant(false)]
        public TSize Length {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _length;
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="ReadOnlyExSpan{T}"/> is empty (返回一个值，该值指示当前只读扩展跨度为空).
        /// </summary>
        /// <value><see langword="true"/> if this Ex span is empty; otherwise, <see langword="false"/> (当前跨度为空时为 <see langword="true"/>; 否则为 <see langword="false"/>).</value>
        public bool IsEmpty {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _length == (TSize)0;
        }

        /// <summary>
        /// Returns a value that indicates whether two <see cref="ReadOnlyExSpan{T}"/> instances are not equal (返回一个值，该值指示两个 <see cref="ReadOnlyExSpan{T}"/> 实例是否不相等).
        /// </summary>
        public static bool operator !=(ReadOnlyExSpan<T> left, ReadOnlyExSpan<T> right) => !(left == right);

        /// <summary>
        /// This method is not supported as ReadOnlyExSpan cannot be boxed. To compare two ReadOnlyExSpan, use operator==.
        /// </summary>
        /// <exception cref="NotSupportedException">
        /// Always thrown by this method.
        /// </exception>
        [Obsolete("Equals() on ReadOnlyExSpan will always throw an exception. Use the equality operator instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object? obj) =>
            throw new NotSupportedException(SR.NotSupported_CannotCallEqualsOnExSpan);

        /// <summary>
        /// This method is not supported as ReadOnlyExSpan cannot be boxed.
        /// </summary>
        /// <exception cref="NotSupportedException">
        /// Always thrown by this method.
        /// </exception>
        [Obsolete("GetHashCode() on ReadOnlyExSpan will always throw an exception.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode() =>
            throw new NotSupportedException(SR.NotSupported_CannotCallGetHashCodeOnExSpan);

        /// <summary>
        /// Defines an implicit conversion of an array to a <see cref="ReadOnlyExSpan{T}"/> (定义数组到 <see cref="ReadOnlyExSpan{T}"/> 的隐式转换)
        /// </summary>
        public static implicit operator ReadOnlyExSpan<T>(T[]? array) => new ReadOnlyExSpan<T>(array);

        /// <summary>
        /// Defines an implicit conversion of a <see cref="ArraySegment{T}"/> to a <see cref="ReadOnlyExSpan{T}"/> (定义 <see cref="ArraySegment{T}"/> 到 <see cref="ReadOnlyExSpan{T}"/> 的隐式转换)
        /// </summary>
        public static implicit operator ReadOnlyExSpan<T>(ArraySegment<T> segment)
            => new ReadOnlyExSpan<T>(segment.Array, (TSize)segment.Offset, (TSize)segment.Count);

        /// <summary>
        /// Returns a 0-length read-only Ex span whose base is the null pointer (返回一个值，该值指示当前只读扩展跨度为空).
        /// </summary>
        public static ReadOnlyExSpan<T> Empty => default;

#if STRUCT_REF_FIELD
        /// <summary>
        /// Casts a read-only ExSpan of <typeparamref name="TDerived"/> to a read-only ExSpan of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="TDerived">The element type of the source read-only ExSpan, which must be derived from <typeparamref name="T"/>.</typeparam>
        /// <param name="items">The source read-only ExSpan. No copy is made.</param>
        /// <returns>A read-only ExSpan with elements cast to the new type.</returns>
        /// <remarks>This method uses a covariant cast, producing a read-only ExSpan that shares the same memory as the source. The relationships expressed in the type constraints ensure that the cast is a safe operation.</remarks>
        public static ReadOnlyExSpan<T> CastUp<TDerived>(ReadOnlyExSpan<TDerived> items) where TDerived : class?, T {
            return new ReadOnlyExSpan<T>(ref Unsafe.As<TDerived, T>(ref items._reference), items.Length);
        }
#endif

        /// <summary>Gets an enumerator for this span (返回此跨度的枚举器).</summary>
        public Enumerator GetEnumerator() => new Enumerator(this);

        /// <summary>Enumerates the elements of a <see cref="ReadOnlyExSpan{T}"/> (为 <see cref="ReadOnlyExSpan{T}"/> 的元素提供枚举器).</summary>
        public ref struct Enumerator {
            /// <summary>The span being enumerated.</summary>
            private readonly ReadOnlyExSpan<T> _span;
            /// <summary>The next index to yield.</summary>
            private TSize _index;

            /// <summary>Initialize the enumerator.</summary>
            /// <param name="span">The span to enumerate.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(ReadOnlyExSpan<T> span) {
                _span = span;
                _index = (TSize)0 - 1;
            }

            /// <summary>Advances the enumerator to the next element of the span (将枚举器推进到跨度的下一元素).</summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext() {
                TSize index = _index + 1;
                if (index < _span.Length) {
                    _index = index;
                    return true;
                }

                return false;
            }

            /// <summary>Gets the element at the current position of the enumerator (获取对枚举器当前位置的元素的引用).</summary>
            public readonly ref readonly T Current {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => ref _span[_index];
            }
        }

        /// <summary>
        /// Returns a read only reference to the 0th element of the span. If the span is empty, returns null reference.
        /// It can be used for pinning and is required to support the use of span within a fixed statement (返回对只读跨度的第0个元素的引用。如果跨度为空，则返回null引用. 它可用于固定，并且需要支持在 fixed 语句中使用跨度).
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref readonly T GetPinnableReference() {
#if STRUCT_REF_FIELD
            return ref _reference;
#else
            // Ensure that the native code has just one forward branch that is predicted-not-taken.
            ref T ret = ref Unsafe.NullRef<T>();
            if (_length != (TSize)0) {
                unsafe {
                    if (_referenceSpan.IsEmpty) {
                        return ref Unsafe.AsRef<T>((void*)_byteOffset);
                    } else {
                        return ref Unsafe.AddByteOffset(ref Unsafe.AsRef(_referenceSpan.GetPinnableReference()), IntPtrExtensions.ToIntPtr(_byteOffset));
                    }
                }
            }
            return ref ret;
#endif
        }

        /// <inheritdoc/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref readonly T GetPinnableReadOnlyReference() {
            return ref GetPinnableReference();
        }
        // ref readonly T IReadOnlyExSpanBase<T>.GetPinnableReadOnlyReference() => ref GetPinnableReference(); // CS0540 containing type does not implement interface

        /// <summary>
        /// Copies the contents of this read-only span into destination span. If the source
        /// and destinations overlap, this method behaves as if the original values in
        /// a temporary location before the destination is overwritten
        /// (将此只读跨度的内容复制到目标跨度. 如果源和目标重叠, 则此方法的行为就像覆盖目标之前临时位置中的原始值一样).
        /// </summary>
        /// <param name="destination">The destination span (目标跨度).</param>
        /// <exception cref="ArgumentException">
        /// Thrown when the destination span is shorter than the source span.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(ExSpan<T> destination) {
            // Using "if (!TryCopyTo(...))" results in two branches: one for the length
            // check, and one for the result of TryCopyTo. Since these checks are equivalent,
            // we can optimize by performing the check once ourselves then calling Memmove directly.

            if (_length <= destination.Length) {
                BufferHelper.Memmove(ref destination.GetPinnableReference(), in GetPinnableReference(), _length.ToUIntPtr());
            } else {
                ThrowHelper.ThrowArgumentException_DestinationTooShort();
            }
        }

        /// <summary>
        /// Try copies the contents of this read-only span into destination span. If the source
        /// and destinations overlap, this method behaves as if the original values in
        /// a temporary location before the destination is overwritten
        /// (尝试将此只读跨度的内容复制到目标跨度. 如果源和目标重叠, 则此方法的行为就像覆盖目标之前临时位置中的原始值一样).
        /// </summary>
        /// <param name="destination">The destination span (目标跨度).</param>
        /// <returns>true if the copy operation succeeded; otherwise, false (如果复制操作已成功，则为 true；否则，为 false).</returns>
        public bool TryCopyTo(ExSpan<T> destination) {
            bool retVal = false;
            if (_length <= destination.Length) {
                BufferHelper.Memmove(ref destination.GetPinnableReference(), in GetPinnableReference(), _length.ToUIntPtr());
                retVal = true;
            }
            return retVal;
        }
        
        /// <summary>
        /// Returns a value that indicates whether two <see cref="ReadOnlySpan{T}"/> instances are equal (返回一个值，该值指示两个 <see cref="ReadOnlySpan{T}"/> 实例是否相等).
        /// </summary>
        public static bool operator ==(ReadOnlyExSpan<T> left, ReadOnlyExSpan<T> right) =>
            left._length == right._length &&
#if STRUCT_REF_FIELD
                Unsafe.AreSame(ref left._reference, ref right._reference)
#else
                left._byteOffset == right._byteOffset &&
                left._referenceSpan == right._referenceSpan
#endif
            ;

        /// <summary>
        /// Returns the string representation of this <see cref="ReadOnlyExSpan{Char}"/> (返回此 <see cref="ReadOnlyExSpan{Char}"/> 的字符串表示形式).
        /// </summary>
        /// <seealso cref="ExSpanExtensions.ItemsToString{T}(ReadOnlyExSpan{T}, Func{TSize, T, string}?, ItemsToStringFlags, TypeNameFlags)"/>
        public override string ToString() {
            //if (typeof(T) == typeof(char)) {
            //    return new string(new ReadOnlySpan<char>(ref Unsafe.As<T, char>(ref _reference), _length));
            //}
            return $"Zyl.ExSpans.ReadOnlyExSpan<{typeof(T).Name}>[{_length}]";
        }

        /// <summary>
        /// Forms a slice out of the given read-only Ex span, beginning at 'start' (从指定索引处开始的只读扩展跨度形成切片).
        /// </summary>
        /// <param name="start">The zero-based index at which to begin this slice (从零开始切片的索引).</param>
        /// <returns>Returns the new read-only Ex span (返回新的只读扩展跨度).</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the specified <paramref name="start"/> index is not in range (&lt;0 or &gt;Length).
        /// </exception>
        [MyCLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyExSpan<T> Slice(TSize start) {
            if (start > _length)
                ThrowHelper.ThrowArgumentOutOfRangeException();

            TSize len = _length - start;
#if STRUCT_REF_FIELD
            return new ReadOnlyExSpan<T>(ref Unsafe.Add(ref _reference, start), len);
#else
            unsafe {
                if (_referenceSpan.IsEmpty) {
                    return new ReadOnlyExSpan<T>((void*)ExUnsafe.AddPointer<T>(_byteOffset, start), len);
                } else {
                    return new ReadOnlyExSpan<T>(_referenceSpan, ExUnsafe.AddPointer<T>(_byteOffset, start), len);
                }
            }
#endif
        }

        /// <summary>
        /// Forms a slice out of the given read-only Ex span, beginning at 'start', of given length (从指定长度的指定索引处开始的当前只读扩展跨度形成切片)
        /// </summary>
        /// <param name="start">The zero-based index at which to begin this slice (从零开始切片的索引).</param>
        /// <param name="length">The desired length for the slice (exclusive) (切片所需的长度).</param>
        /// <returns>Returns the new read-only Ex span (返回新的只读扩展跨度).</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the specified <paramref name="start"/> or end index is not in range (&lt;0 or &gt;Length).
        /// </exception>
        [MyCLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyExSpan<T> Slice(TSize start, TSize length) {
            if (start.ToUIntPtr() > _length.ToUIntPtr())
                ThrowHelper.ThrowArgumentOutOfRangeException();
            if (length.ToUIntPtr() > (_length - start).ToUIntPtr())
                ThrowHelper.ThrowArgumentOutOfRangeException();

#if STRUCT_REF_FIELD
            return new ReadOnlyExSpan<T>(ref Unsafe.Add(ref _reference, start), length);
#else
            unsafe {
                if (_referenceSpan.IsEmpty) {
                    return new ReadOnlyExSpan<T>((void*)ExUnsafe.AddPointer<T>(_byteOffset, start), length);
                } else {
                    return new ReadOnlyExSpan<T>(_referenceSpan, ExUnsafe.AddPointer<T>(_byteOffset, start), length);
                }
            }
#endif
        }

        /// <summary>
        /// Copies the contents of this span into a new array. The maxLength parameter uses the value of <see cref="ExMemoryMarshal.ArrayMaxLengthSafe"/> (将此范围的内容复制到新建数组中. maxLength 参数使用 <see cref="ExMemoryMarshal.ArrayMaxLengthSafe"/> 的值).
        /// </summary>
        /// <returns>An array containing the data in the current span (包含当前跨度中数据的数组).</returns>
        public T[] ToArray() {
            return ToArray(ExMemoryMarshal.ArrayMaxLengthSafe);
        }

        /// <summary>
        /// Copies the contents of this span into a new array. It has a maxLength parameters (将此跨度的内容复制到新建数组中. 它具有 maxLength 参数).
        /// </summary>
        /// <param name="maxLength">The max length of array (数组的最大长度).</param>
        /// <returns>An array containing the data in the current span (包含当前跨度中数据的数组).</returns>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="maxLength"/> parameter must be greater than 0</exception>
        public T[] ToArray(int maxLength) {
            if (maxLength <= 0) throw new ArgumentOutOfRangeException(nameof(maxLength), "The maxLength parameter must be greater than 0!");
            if (_length == (TSize)0)
                return ArrayHelper.Empty<T>();

            int len = (_length < (TSize)maxLength) ? (int)_length : maxLength;
            var destination = new T[len];
            BufferHelper.Memmove(ref ExMemoryMarshal.GetArrayDataReference(destination), in GetPinnableReference(), (uint)len);
            return destination;
        }

    }
}
