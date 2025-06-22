#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
#define CREATE_SPAN_BY_REF // Allow MemoryMarshal.CreateReadOnlySpan
#endif // NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
#if NET7_0_OR_GREATER
#define STRUCT_REF_FIELD // C# 11 - ref fields and ref scoped variables. https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/ref-struct#ref-fields
#endif // NET7_0_OR_GREATER

using System;
using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Zyl.ExSpans.Extensions;
using Zyl.ExSpans.Impl;
using EditorBrowsableAttribute = System.ComponentModel.EditorBrowsableAttribute;
using EditorBrowsableState = System.ComponentModel.EditorBrowsableState;

namespace Zyl.ExSpans {
    /// <summary>
    /// Represents a contiguous region of read only memory, similar to <see cref="ReadOnlyExSpan{T}"/>.
    /// Unlike <see cref="ReadOnlyExSpan{T}"/>, it is not a byref-like type. It can be regarded as the <see cref="ReadOnlyMemory{T}"/> of <see cref="TSize"/> index range
    /// (代表只读内存的连续区域，类似于 ReadOnlyExSpan. 与 ReadOnlyExSpan 不同的是，它不是 类似byref 的类型. 它可以被视为 <see cref="TSize"/> 索引范围的 <see cref="ReadOnlyMemory{T}"/>).
    /// </summary>
    /// <typeparam name="T">The element type (元素的类型).</typeparam>
    //[DebuggerTypeProxy(typeof(ExMemoryDebugView<>))]
    [DebuggerDisplay("{ToString(),raw}")]
    public readonly partial struct ReadOnlyExMemory<T> : IEquatable<ReadOnlyExMemory<T>> {
        // The highest order bit of _index is used to discern whether _object is a pre-pinned array (_index 的最高阶位用于判别 _object 是否是一个预先钉住的数组).
        // (_index < 0) => _object is a pre-pinned array, so Pin() will not allocate a new GCHandle (如果 _object 是一个预先钉住的数组，那么 Pin() 将不会分配一个新的 GCHandle)
        //       (else) => Pin() needs to allocate a new GCHandle to pin the object (Pin() 需要分配一个新的 GCHandle 来固定对象).
        internal readonly object? _object;
        internal readonly TSize _index;
        internal readonly TSize _length;

        internal static readonly TSize RemoveFlagsBitMask = IntPtrExtensions.IntPtrMaxValue; // Int32: 0x7FFFFFFF, Int64: 0x7FFFFFFFFFFFFFFF.

        /// <summary>
        /// Creates a new memory over the entirety of the target array (在整个目标数组上创建新的内存区域).
        /// </summary>
        /// <param name="array">The target array (目标数组).</param>
        /// <remarks>Returns default when <paramref name="array"/> is null (当 <paramref name="array"/> 为 null时返回 default).</remarks>
        /// <exception cref="ArrayTypeMismatchException">Thrown when <paramref name="array"/> is covariant and array's type is not exactly T[].</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyExMemory(T[]? array) {
            if (array == null) {
                this = default;
                return; // returns default
            }

            _object = array;
            _index = 0;
            _length = array.Length;
        }

        /// <summary>
        /// Creates a new memory over the portion of the target array beginning
        /// at 'start' index and ending at 'end' index (exclusive)
        /// (在目标数组的一部分上创建新的内存区域，从指定位置开始并包含指定数量的元素).
        /// </summary>
        /// <param name="array">The target array (目标数组).</param>
        /// <param name="start">The index at which to begin the memory (开始内存区域的索引).</param>
        /// <param name="length">The number of items in the memory (内存区域中的项数).</param>
        /// <remarks>Returns default when <paramref name="array"/> is null (当 <paramref name="array"/> 为 null时返回 default).</remarks>
        /// <exception cref="ArrayTypeMismatchException">Thrown when <paramref name="array"/> is covariant and array's type is not exactly T[].</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the specified <paramref name="start"/> or end index is not in the range (&lt;0 or &gt;Length).
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyExMemory(T[]? array, TSize start, TSize length) {
            if (array == null) {
                if (start != 0 || length != 0)
                    ThrowHelper.ThrowArgumentOutOfRangeException();
                this = default;
                return; // returns default
            }
            TUSize srcLength = array.ExLength().ToUIntPtr();
            if (start.ToUIntPtr() > srcLength || length.ToUIntPtr() > (srcLength - start.ToUIntPtr())) {
                ThrowHelper.ThrowArgumentOutOfRangeException();
            }

            _object = array;
            _index = start;
            _length = length;
        }

        /// <summary>Creates a new memory over the existing object, start, and length. No validation is performed (在现有对象、开始和长度上创建新内存区域. 不执行验证).</summary>
        /// <param name="obj">The target object (目标对象).</param>
        /// <param name="start">The index at which to begin the memory (开始内存区域的索引).</param>
        /// <param name="length">The number of items in the memory (内存区域中的项数).</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ReadOnlyExMemory(object? obj, TSize start, TSize length) {
            // No validation performed in release builds; caller must provide any necessary validation.

            // 'obj is T[]' below also handles things like int[] <-> uint[] being convertible
            Debug.Assert((obj == null)
                || (typeof(T) == typeof(char) && obj is string)
                || (obj is T[])
                || (obj is MemoryManager<T>)
                //|| (obj is ExMemoryManager<T>)
                );

            _object = obj;
            _index = start;
            _length = length;
        }

        /// <summary>
        /// Defines an implicit conversion of an array to a <see cref="ReadOnlyExMemory{T}"/> (定义数组到 <see cref="ReadOnlyExMemory{T}"/> 的隐式转换)
        /// </summary>
        public static implicit operator ReadOnlyExMemory<T>(T[]? array) => new ReadOnlyExMemory<T>(array);

        /// <summary>
        /// Defines an implicit conversion of a <see cref="ArraySegment{T}"/> to a <see cref="ReadOnlyExMemory{T}"/> (定义 <see cref="ArraySegment{T}"/> 到 <see cref="ReadOnlyExMemory{T}"/> 的隐式转换)
        /// </summary>
        public static implicit operator ReadOnlyExMemory<T>(ArraySegment<T> segment) => new ReadOnlyExMemory<T>(segment.Array, segment.Offset, segment.Count);

        /// <summary>
        /// Returns an empty <see cref="ReadOnlyExMemory{T}"/> (返回空的 <see cref="ReadOnlyExMemory{T}"/>).
        /// </summary>
        public static ReadOnlyExMemory<T> Empty => default;

        /// <summary>
        /// The number of items in the memory (内存中的项数).
        /// </summary>
        public TSize Length => _length;

        /// <summary>
        /// Gets a value indicating whether this <see cref="ReadOnlyExMemory{T}"/> is empty (返回一个值，该值指示当前只读内存为空).
        /// </summary>
        /// <value>Returns true if Length is 0.</value>
        public bool IsEmpty => _length == 0;

        /// <summary>
        /// Returns the string representation of this <see cref="ReadOnlyExMemory{Char}"/> (返回此 <see cref="ReadOnlyExMemory{Char}"/> 的字符串表示形式).
        /// </summary>
        /// <returns>
        /// For <see cref="ReadOnlyExMemory{Char}"/>, returns a new instance of string that represents the characters pointed to by the memory.
        /// Otherwise, returns a <see cref="string"/> with the name of the type and the number of elements.
        /// </returns>
        public override string ToString() {
            if (typeof(T) == typeof(char) && _length.IsLengthInInt32()) {
                return (_object is string str) ? str.Substring((int)_index, (int)_length) : Span.ToString();
            }
            return $"Zyl.ExSpans.ReadOnlyExMemory<{typeof(T).Name}>[{_length}]";
        }

        /// <summary>
        /// Forms a slice out of the given memory region, beginning at a specified position and continuing to its end (从给定的内存区域形成切片，从指定位置开始，然后继续到其末尾).
        /// </summary>
        /// <param name="start">The index at which to begin this slice (开始切片处的索引).</param>
        /// <returns>A read-only memory region representing the desired slice (表示切片后的只读内存区域).</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the specified <paramref name="start"/> index is not in range (&lt;0 or &gt;Length).
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyExMemory<T> Slice(TSize start) {
            if ((TUSize)start > (TUSize)_length) {
                ThrowHelper.ThrowArgumentOutOfRangeException();
            }

            // It is expected for _index + start to be negative if the memory is already pre-pinned.
            return new ReadOnlyExMemory<T>(_object, _index + start, _length - start);
        }

        /// <summary>
        /// Forms a slice out of the given memory, beginning at 'start', of given length (从给定的内存区域形成切片，从指定位置开始，根据所给长度).
        /// </summary>
        /// <param name="start">The index at which to begin this slice (开始切片处的索引).</param>
        /// <param name="length">The desired length for the slice (exclusive) (切片所需的长度).</param>
        /// <returns>A read-only memory region representing the desired slice (表示切片后的只读内存区域).</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the specified <paramref name="start"/> or end index is not in range (&lt;0 or &gt;Length).
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyExMemory<T> Slice(TSize start, TSize length) {
            TUSize srcLength = _length.ToUIntPtr();
            if (start.ToUIntPtr() > srcLength || length.ToUIntPtr() > (srcLength - start.ToUIntPtr())) {
                ThrowHelper.ThrowArgumentOutOfRangeException();
            }

            // It is expected for _index + start to be negative if the memory is already pre-pinned.
            return new ReadOnlyExMemory<T>(_object, _index + start, length);
        }

        /// <summary>
        /// Returns a span from the memory (从内存获取跨度).
        /// </summary>
        public ReadOnlySpan<T> Span {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get {
#if CREATE_SPAN_BY_REF
                ref T refToReturn = ref Unsafe.NullRef<T>();
                int lengthOfUnderlyingSpan = 0;
#endif // CREATE_SPAN_BY_REF

                // Copy this field into a local so that it can't change out from under us mid-operation.

                object? tmpObject = _object;
                if (tmpObject != null) {
                    nint indexFix = _index & RemoveFlagsBitMask;
                    if (typeof(T) == typeof(char) && tmpObject is string str) {
                        // Special-case string since it's the most common for ROM<char>.

                        //refToReturn = ref Unsafe.As<char, T>(ref Unsafe.As<string>(tmpObject).GetRawStringData());
                        //lengthOfUnderlyingSpan = Unsafe.As<string>(tmpObject).Length;
#if CREATE_SPAN_BY_REF
                        ReadOnlySpan<char> spanChar = str.AsSpan();
                        refToReturn = ref Unsafe.As<char, T>(ref Unsafe.AsRef(in spanChar[0]));
                        lengthOfUnderlyingSpan = str.Length;
#else
                        //spanChar = spanChar.Slice((int)indexFix, (int)_length);
                        //return MemoryMarshal.Cast<char, T>(spanChar); // CS0453 The type 'T' must be a non-nullable value type in order to use it as parameter 'TTo' in the generic type or method 'MemoryMarshal.Cast<TFrom, TTo>(ReadOnlySpan<TFrom>)'.
                        //return Unsafe.As<ReadOnlySpan<char>, ReadOnlySpan<T>>(ref spanChar); // CS9244 The type 'ReadOnlySpan<char>' may not be a ref struct or a type parameter allowing ref structs in order to use it as parameter 'TFrom' in the generic type or method 'Unsafe.As<TFrom, TTo>(ref TFrom)'.
                        //throw new NotSupportedException("Before .NET Standard 2.1 (Or .NET Core 3.0), ReadOnlyExMemory did not support string type!");
                        ReadOnlyMemory<char> memChar = str.AsMemory().Slice((int)indexFix, (int)_length);
                        return Unsafe.As<ReadOnlyMemory<char>, ReadOnlyMemory<T>>(ref memChar).Span;
#endif // CREATE_SPAN_BY_REF
                    } else if (tmpObject is T[] arr) { // RuntimeHelpers.ObjectHasComponentSize(tmpObject)
                        // We know the object is not null, it's not a string, and it is variable-length. The only
                        // remaining option is for it to be a T[] (or a U[] which is blittable to T[], like int[]
                        // and uint[]). As a special case of this, ROM<T> allows some amount of array variance
                        // that ExMemory<T> disallows. For example, an array of actual type string[] cannot be turned
                        // into a ExMemory<object> or a Span<object>, but it can be turned into a ROM/ROS<object>.
                        // We'll assume these checks succeeded because they're performed during ExMemory<T> construction.
                        // It's always possible for somebody to use private reflection to bypass these checks, but
                        // preventing type safety violations due to misuse of reflection is out of scope of this logic.

                        // 'tmpObject is T[]' below also handles things like int[] <-> uint[] being convertible
                        //Debug.Assert(tmpObject is T[]);

                        //refToReturn = ref ExMemoryMarshal.GetArrayDataReference(Unsafe.As<T[]>(tmpObject));
                        //lengthOfUnderlyingSpan = Unsafe.As<T[]>(tmpObject).Length;
#if CREATE_SPAN_BY_REF
                        refToReturn = ref arr[0];
                        lengthOfUnderlyingSpan = arr.Length;
#else
                        return arr.AsMemory().Slice((int)indexFix, (int)_length).Span;
#endif // CREATE_SPAN_BY_REF
                    } else if (tmpObject is MemoryManager<T> mgr) {
                        // We know the object is not null, and it's not variable-length, so it must be a ExMemoryManager<T>.
                        // Otherwise somebody used private reflection to set this field, and we're not too worried about
                        // type safety violations at that point. Note that it can't be a ExMemoryManager<U>, even if U and
                        // T are blittable (e.g., ExMemoryManager<int> to ExMemoryManager<uint>), since there exists no
                        // constructor or other public API which would allow such a conversion.

                        //Debug.Assert(tmpObject is ExMemoryManager<T>);
#if CREATE_SPAN_BY_REF
                        Span<T> memoryManagerSpan = mgr.GetSpan(); // Unsafe.As<MemoryManager<T>>(tmpObject).GetSpan();
                        refToReturn = ref MemoryMarshal.GetReference(memoryManagerSpan);
                        lengthOfUnderlyingSpan = memoryManagerSpan.Length;
#else
                        return mgr.Memory.Slice((int)indexFix, (int)_length).Span;
#endif // CREATE_SPAN_BY_REF
                    } else {
                        throw new NotSupportedException("ReadOnlyExMemory not support `" + tmpObject.GetType().Name + "` type!");
                    }

#if CREATE_SPAN_BY_REF
                    // If the ExMemory<T> or ReadOnlyExMemory<T> instance is torn, this property getter has undefined behavior.
                    // We try to detect this condition and throw an exception, but it's possible that a torn struct might
                    // appear to us to be valid, and we'll return an undesired span. Such a span is always guaranteed at
                    // least to be in-bounds when compared with the original ExMemory<T> instance, so using the span won't
                    // AV the process.

                    // We use 'nuint' because it gives us a free early zero-extension to 64 bits when running on a 64-bit platform.
                    nuint desiredStartIndex = (nuint)indexFix;

                    int desiredLength = (int)_length;

                    if (desiredStartIndex > (nuint)lengthOfUnderlyingSpan || (nuint)desiredLength > (nuint)lengthOfUnderlyingSpan - desiredStartIndex) {
                        ThrowHelper.ThrowArgumentOutOfRangeException();
                    }

                    refToReturn = ref ExUnsafe.Add(ref refToReturn, desiredStartIndex);
                    lengthOfUnderlyingSpan = desiredLength;
                    //return new ReadOnlySpan<T>(ref refToReturn, lengthOfUnderlyingSpan);
                    return MemoryMarshal.CreateReadOnlySpan<T>(ref refToReturn, lengthOfUnderlyingSpan);
#endif // CREATE_SPAN_BY_REF
                }
                return ReadOnlySpan<T>.Empty;
            }
        }

#if TODO
        /// <summary>
        /// Copies the contents of the read-only memory into the destination. If the source
        /// and destination overlap, this method behaves as if the original values are in
        /// a temporary location before the destination is overwritten.
        /// </summary>
        /// <param name="destination">The ExMemory to copy items into.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when the destination is shorter than the source.
        /// </exception>
        public void CopyTo(ExMemory<T> destination) => Span.CopyTo(destination.Span);

        /// <summary>
        /// Copies the contents of the readonly-only memory into the destination. If the source
        /// and destination overlap, this method behaves as if the original values are in
        /// a temporary location before the destination is overwritten.
        /// </summary>
        /// <returns>If the destination is shorter than the source, this method
        /// return false and no data is written to the destination.</returns>
        /// <param name="destination">The span to copy items into.</param>
        public bool TryCopyTo(ExMemory<T> destination) => Span.TryCopyTo(destination.Span);
#endif // (TODO)

        /// <summary>
        /// Creates a handle for the memory.
        /// The GC will not move the memory until the returned <see cref="MemoryHandle"/>
        /// is disposed, enabling taking and using the memory's address.
        /// (为内存创建一个句柄. 在处置(disposed)返回的 MemoryHandle 之前，GC 不会移动内存，从而可以获取并使用内存地址)
        /// </summary>
        /// <returns>A handle for the memory (内存的句柄).</returns>
        /// <exception cref="ArgumentException">
        /// An instance with nonprimitive (non-blittable) members cannot be pinned.
        /// </exception>
        public unsafe MemoryHandle Pin() {
            // It's possible that the below logic could result in an AV if the struct
            // is torn. This is ok since the caller is expecting to use raw pointers,
            // and we're not required to keep this as safe as the other Span-based APIs.

            object? tmpObject = _object;
            if (tmpObject != null) {
                nint indexFix = _index & RemoveFlagsBitMask;
                if (typeof(T) == typeof(char) && tmpObject is string s) {
                    // Unsafe.AsPointer is safe since the handle pins it
                    GCHandle handle = GCHandle.Alloc(tmpObject, GCHandleType.Pinned);
                    ref char stringData = ref Unsafe.Add(ref Unsafe.AsRef(in s.AsSpan()[0]), indexFix);
                    return new MemoryHandle(Unsafe.AsPointer(ref stringData), handle);
                } else if (tmpObject is T[] arr) { // RuntimeHelpers.ObjectHasComponentSize(tmpObject)
                    // 'tmpObject is T[]' below also handles things like int[] <-> uint[] being convertible
                    //Debug.Assert(tmpObject is T[]);

                    // Array is already pre-pinned
                    if (_index < 0) {
                        // Unsafe.AsPointer is safe since it's pinned
                        void* pointer = Unsafe.AsPointer(ref arr[(int)indexFix]);
                        return new MemoryHandle(pointer);
                    } else {
                        // Unsafe.AsPointer is safe since the handle pins it
                        GCHandle handle = GCHandle.Alloc(tmpObject, GCHandleType.Pinned);
#if NETSTANDARD2_0_OR_GREATER || NETCOREAPP1_0_OR_GREATER || NET40_OR_GREATER
                        System.Threading.Thread.MemoryBarrier();
#endif // NETSTANDARD2_0_OR_GREATER || NETCOREAPP1_0_OR_GREATER || NET40_OR_GREATER
                        void* pointer = Unsafe.AsPointer(ref arr[(int)indexFix]);
                        return new MemoryHandle(pointer, handle);
                    }
                } else if (tmpObject is MemoryManager<T> mgr) {
                    //Debug.Assert(tmpObject is MemoryManager<T>);
                    return mgr.Pin((int)indexFix);
                } else {
                    throw new NotSupportedException("ReadOnlyExMemory not support `" + tmpObject.GetType().Name + "` type!");
                }
            }

            return default;
        }

        /// <summary>
        /// Copies the contents from the memory into a new array.  This heap
        /// allocates, so should generally be avoided, however it is sometimes
        /// necessary to bridge the gap with APIs written in terms of arrays
        /// (将内存中的内容复制到一个新数组中. 该操作会进行堆分配，因此一般应避免使用，但有时需要与以数组编写的应用程序接口弥合差距).
        /// </summary>
        public T[] ToArray() {
            return Span.ToArray();
        }

        /// <summary>Determines whether the specified object is equal to the current object (确定指定的对象是否等于当前对象).</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals([NotNullWhen(true)] object? obj) {
            if (obj is ReadOnlyExMemory<T> readOnlyExMemory) {
                return Equals(readOnlyExMemory);
#if TODO
            } else if (obj is ExMemory<T> memory) {
                return Equals(memory);
#endif // (TODO)
            } else {
                return false;
            }
        }

        /// <summary>
        /// Determines whether the current instance and a specified <see cref="ReadOnlyExMemory{T}"/> objects are equal (确定当前实例与指定的 <see cref="ReadOnlyExMemory{T}"/> 对象是否相等).
        /// </summary>
        /// <returns>
        /// Returns true if the memory points to the same array and has the same length.  Note that
        /// this does *not* check to see if the *contents* are equal
        /// </returns>
        public bool Equals(ReadOnlyExMemory<T> other) {
            return
                _object == other._object &&
                _index == other._index &&
                _length == other._length;
        }

        /// <summary>Returns the hash code for this <see cref="ReadOnlyExMemory{T}"/></summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode() {
            // We use RuntimeHelpers.GetHashCode instead of Object.GetHashCode because the hash
            // code is based on object identity and referential equality, not deep equality (as common with string).
            return (_object != null) ? HashCodeHelper.Combine(RuntimeHelpers.GetHashCode(_object), _index, _length) : 0;
        }

        /// <summary>Gets the state of the memory as individual fields.</summary>
        /// <param name="start">The offset.</param>
        /// <param name="length">The count.</param>
        /// <returns>The object.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal object? GetObjectStartLength(out TSize start, out TSize length) {
            start = _index;
            length = _length;
            return _object;
        }
    }
}
