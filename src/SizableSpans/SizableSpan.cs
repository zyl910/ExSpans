#if NET7_0_OR_GREATER
#define STRUCT_REF_FIELD // C# 11 - ref fields and ref scoped variables
#endif // NET7_0_OR_GREATER

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Zyl.SizableSpans.Impl;

using EditorBrowsableAttribute = System.ComponentModel.EditorBrowsableAttribute;
using EditorBrowsableState = System.ComponentModel.EditorBrowsableState;

#pragma warning disable 0809 // Obsolete member 'SizableSpan<T>.Equals(object)' overrides non-obsolete member 'object.Equals(object)'

namespace Zyl.SizableSpans {
    /// <summary>
    /// SizableSpan represents a contiguous region of arbitrary memory. Unlike arrays, it can point to either managed
    /// or native memory, or to memory allocated on the stack. It is type-safe and memory-safe.
    /// </summary>
    //[DebuggerTypeProxy(typeof(SizableSpanDebugView<>))]
    [DebuggerDisplay("{ToString(),raw}")]
    //[NativeMarshalling(typeof(SizableSpanMarshaller<,>))]
    public readonly ref struct SizableSpan<T>
#if NETSTANDARD2_0_OR_GREATER || NETCOREAPP3_0_OR_GREATER
#else
            where T : struct
#endif
            {
        /// <summary>The number of elements this SizableSpan contains.</summary>
        private readonly int _length;
#if STRUCT_REF_FIELD
        /// <summary>A byref or a native ptr.</summary>
        internal readonly ref T _reference;
#else
        /// <summary>A byte offse of referenceSpan or a native ptr.</summary>
        internal readonly TSize _byteOffse;
        /// <summary>A span of reference. It is Empty on native ptr.</summary>
        internal readonly Span<T> _referenceSpan;
#endif

        /// <summary>
        /// Creates a new SizableSpan over the entirety of the target array.
        /// </summary>
        /// <param name="array">The target array.</param>
        /// <remarks>Returns default when <paramref name="array"/> is null.</remarks>
        /// <exception cref="ArrayTypeMismatchException">Thrown when <paramref name="array"/> is covariant and array's type is not exactly T[].</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SizableSpan(T[]? array) {
            if (array == null || array.Length <= 0) {
                this = default;
                return; // returns default
            }
            //if (!typeof(T).IsValueType && array.GetType() != typeof(T[]))
            //    ThrowHelper.ThrowArrayTypeMismatchException();
            if (array.GetType() != typeof(T[])) ThrowHelper.ThrowArrayTypeMismatchException();
#if NETSTANDARD2_0_OR_GREATER || NETCOREAPP3_0_OR_GREATER
            if (!typeof(T).IsValueType) ThrowHelper.ThrowArrayTypeMismatchException();
#else
            // where T : struct
#endif

            _length = array.Length;
#if STRUCT_REF_FIELD
            _reference = ref SizableMemoryMarshal.GetArrayDataReference(array);
#else
            _byteOffse = TSize.Zero;
            _referenceSpan = new Span<T>(array);
#endif
        }

        /// <summary>
        /// Creates a new SizableSpan over the portion of the target array beginning
        /// at 'start' index and ending at 'end' index (exclusive).
        /// </summary>
        /// <param name="array">The target array.</param>
        /// <param name="start">The zero-based index at which to begin the SizableSpan.</param>
        /// <param name="length">The number of items in the SizableSpan.</param>
        /// <remarks>Returns default when <paramref name="array"/> is null.</remarks>
        /// <exception cref="ArrayTypeMismatchException">Thrown when <paramref name="array"/> is covariant and array's type is not exactly T[].</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the specified <paramref name="start"/> or end index is not in the range (&lt;0 or &gt;Length).
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SizableSpan(T[]? array, int start, int length) {
            if (array == null || array.Length <= 0) {
                if (start != 0 || length != 0)
                    ThrowHelper.ThrowArgumentOutOfRangeException();
                this = default;
                return; // returns default
            }
            if (array.GetType() != typeof(T[])) ThrowHelper.ThrowArrayTypeMismatchException();
#if NETSTANDARD2_0_OR_GREATER || NETCOREAPP3_0_OR_GREATER
            if (!typeof(T).IsValueType) ThrowHelper.ThrowArrayTypeMismatchException();
#endif
            if (SizableSpanHelpers.Is64BitProcess) {
                // See comment in SizableSpan<T>.Slice for how this works.
                if ((ulong)(uint)start + (ulong)(uint)length > (ulong)(uint)array.Length)
                    ThrowHelper.ThrowArgumentOutOfRangeException();
            } else {
                if ((uint)start > (uint)array.Length || (uint)length > (uint)(array.Length - start))
                    ThrowHelper.ThrowArgumentOutOfRangeException();
            }

            _length = length;
#if STRUCT_REF_FIELD
            _reference = ref Unsafe.Add(ref SizableMemoryMarshal.GetArrayDataReference(array), (nint)(uint)start /* force zero-extension */);
#else
            _byteOffse = SizableUnsafe.GetByteSize<T>((TSize)start);
            _referenceSpan = new Span<T>(array);
#endif
        }

        /// <summary>
        /// Creates a new SizableSpan over the target unmanaged buffer.  Clearly this
        /// is quite dangerous, because we are creating arbitrarily typed T's
        /// out of a void*-typed block of memory.  And the length is not checked.
        /// But if this creation is correct, then all subsequent uses are correct.
        /// </summary>
        /// <param name="pointer">An unmanaged pointer to memory.</param>
        /// <param name="length">The number of <typeparamref name="T"/> elements the memory contains.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when <typeparamref name="T"/> is reference type or contains pointers and hence cannot be stored in unmanaged memory.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the specified <paramref name="length"/> is negative.
        /// </exception>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe SizableSpan(void* pointer, int length) {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_0_OR_GREATER
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
                ThrowHelper.ThrowInvalidTypeWithPointersNotSupported(typeof(T));
#endif
            if (length < 0)
                ThrowHelper.ThrowArgumentOutOfRangeException();

            _length = length;
#if STRUCT_REF_FIELD
            _reference = ref Unsafe.AsRef<T>(pointer); // *(T*)pointer;
#else
            _byteOffse = (TSize)pointer;
            _referenceSpan = Span<T>.Empty;
#endif
        }

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        /// <summary>Creates a new <see cref="SizableSpan{T}"/> of length 1 around the specified reference.</summary>
        /// <param name="reference">A reference to data.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SizableSpan(ref T reference) {
            _length = 1;
#if STRUCT_REF_FIELD
            _reference = ref reference;
#else
            _byteOffse = TSize.Zero;
            _referenceSpan = MemoryMarshal.CreateSpan(ref reference, 1); // NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
#endif
        }

        // Constructor for internal use only. It is not safe to expose publicly, and is instead exposed via the unsafe MemoryMarshal.CreateSizableSpan.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal SizableSpan(ref T reference, int length) {
            Debug.Assert(length >= 0);

            _length = length;
#if STRUCT_REF_FIELD
            _reference = ref reference;
#else
            _byteOffse = TSize.Zero;
            _referenceSpan = MemoryMarshal.CreateSpan(ref reference, length); // NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
#endif
        }
#endif

#if STRUCT_REF_FIELD
#else
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal SizableSpan(Span<T> referenceSpan, TSize byteOffse, int length) {
            _length = length;
            _byteOffse = byteOffse;
            _referenceSpan = referenceSpan;
        }
#endif

        /// <summary>
        /// Returns a reference to specified element of the SizableSpan.
        /// </summary>
        /// <param name="index">The zero-based index.</param>
        /// <returns></returns>
        /// <exception cref="IndexOutOfRangeException">
        /// Thrown when index less than 0 or index greater than or equal to Length
        /// </exception>
        public ref T this[int index] {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get {
                if (index >= _length)
                    ThrowHelper.ThrowIndexOutOfRangeException();
#if STRUCT_REF_FIELD
                return ref Unsafe.Add(ref _reference, (nint)(uint)index /* force zero-extension */);
#else
                unsafe {
                    if (_referenceSpan.IsEmpty) {
                        return ref Unsafe.Add(ref Unsafe.AsRef<T>((void*)_byteOffse), index);
                    } else {
                        return ref Unsafe.Add(ref Unsafe.AddByteOffset(ref _referenceSpan.GetPinnableReference(), SizableUnsafe.ToIntPtr(_byteOffse)), (nint)(uint)index);
                    }
                }
#endif
            }
        }

        /// <summary>
        /// The number of items in the SizableSpan.
        /// </summary>
        public int Length {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _length;
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="SizableSpan{T}"/> is empty.
        /// </summary>
        /// <value><see langword="true"/> if this SizableSpan is empty; otherwise, <see langword="false"/>.</value>
        public bool IsEmpty {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _length == 0;
        }

        /// <summary>
        /// Returns false if left and right point at the same memory and have the same length.  Note that
        /// this does *not* check to see if the *contents* are equal.
        /// </summary>
        public static bool operator !=(SizableSpan<T> left, SizableSpan<T> right) => !(left == right);

        /// <summary>
        /// This method is not supported as SizableSpans cannot be boxed. To compare two SizableSpans, use operator==.
        /// </summary>
        /// <exception cref="NotSupportedException">
        /// Always thrown by this method.
        /// </exception>
        [Obsolete("Equals() on SizableSpan will always throw an exception. Use the equality operator instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object? obj) =>
            throw new NotSupportedException(SR.NotSupported_CannotCallEqualsOnSizableSpan);

        /// <summary>
        /// This method is not supported as SizableSpans cannot be boxed.
        /// </summary>
        /// <exception cref="NotSupportedException">
        /// Always thrown by this method.
        /// </exception>
        [Obsolete("GetHashCode() on SizableSpan will always throw an exception.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode() =>
            throw new NotSupportedException(SR.NotSupported_CannotCallGetHashCodeOnSizableSpan);

        /// <summary>
        /// Defines an implicit conversion of an array to a <see cref="SizableSpan{T}"/>
        /// </summary>
        public static implicit operator SizableSpan<T>(T[]? array) => new SizableSpan<T>(array);

        /// <summary>
        /// Defines an implicit conversion of a <see cref="ArraySegment{T}"/> to a <see cref="SizableSpan{T}"/>
        /// </summary>
        public static implicit operator SizableSpan<T>(ArraySegment<T> segment) =>
            new SizableSpan<T>(segment.Array, segment.Offset, segment.Count);

        /// <summary>
        /// Returns an empty <see cref="SizableSpan{T}"/>
        /// </summary>
        public static SizableSpan<T> Empty => default;

        /// <summary>Gets an enumerator for this SizableSpan.</summary>
        public Enumerator GetEnumerator() => new Enumerator(this);

        /// <summary>Enumerates the elements of a <see cref="SizableSpan{T}"/>.</summary>
        public ref struct Enumerator {
            /// <summary>The SizableSpan being enumerated.</summary>
            private readonly SizableSpan<T> _SizableSpan;
            /// <summary>The next index to yield.</summary>
            private int _index;

            /// <summary>Initialize the enumerator.</summary>
            /// <param name="SizableSpan">The SizableSpan to enumerate.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(SizableSpan<T> SizableSpan) {
                _SizableSpan = SizableSpan;
                _index = -1;
            }

            /// <summary>Advances the enumerator to the next element of the SizableSpan.</summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext() {
                int index = _index + 1;
                if (index < _SizableSpan.Length) {
                    _index = index;
                    return true;
                }

                return false;
            }

            /// <summary>Gets the element at the current position of the enumerator.</summary>
            public ref T Current {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => ref _SizableSpan[_index];
            }
        }

        /// <summary>
        /// Returns a reference to the 0th element of the SizableSpan. If the SizableSpan is empty, returns null reference.
        /// It can be used for pinning and is required to support the use of SizableSpan within a fixed statement.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetPinnableReference() {
            // Ensure that the native code has just one forward branch that is predicted-not-taken.
            ref T ret = ref Unsafe.NullRef<T>();
            if (_length != 0) {
#if STRUCT_REF_FIELD
                ret = ref _reference;
#else
                unsafe {
                    if (_referenceSpan.IsEmpty) {
                        return ref Unsafe.AsRef<T>((void*)_byteOffse);
                    } else {
                        return ref Unsafe.AddByteOffset(ref _referenceSpan.GetPinnableReference(), SizableUnsafe.ToIntPtr(_byteOffse));
                    }
                }
#endif
            }
            return ref ret;
        }
        /*
        /// <summary>
        /// Clears the contents of this SizableSpan.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Clear() {
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>()) {
                SizableSpanHelpers.ClearWithReferences(ref Unsafe.As<T, IntPtr>(ref _reference), (uint)_length * (nuint)(sizeof(T) / sizeof(nuint)));
            } else {
                SizableSpanHelpers.ClearWithoutReferences(ref Unsafe.As<T, byte>(ref _reference), (uint)_length * (nuint)sizeof(T));
            }
        }

        /// <summary>
        /// Fills the contents of this SizableSpan with the given value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Fill(T value) {
            SizableSpanHelpers.Fill(ref _reference, (uint)_length, value);
        }

        /// <summary>
        /// Copies the contents of this SizableSpan into destination SizableSpan. If the source
        /// and destinations overlap, this method behaves as if the original values in
        /// a temporary location before the destination is overwritten.
        /// </summary>
        /// <param name="destination">The SizableSpan to copy items into.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when the destination SizableSpan is shorter than the source SizableSpan.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(SizableSpan<T> destination) {
            // Using "if (!TryCopyTo(...))" results in two branches: one for the length
            // check, and one for the result of TryCopyTo. Since these checks are equivalent,
            // we can optimize by performing the check once ourselves then calling Memmove directly.

            if ((uint)_length <= (uint)destination.Length) {
                Buffer.Memmove(ref destination._reference, ref _reference, (uint)_length);
            } else {
                ThrowHelper.ThrowArgumentException_DestinationTooShort();
            }
        }
        
        /// <summary>
        /// Copies the contents of this SizableSpan into destination SizableSpan. If the source
        /// and destinations overlap, this method behaves as if the original values in
        /// a temporary location before the destination is overwritten.
        /// </summary>
        /// <param name="destination">The SizableSpan to copy items into.</param>
        /// <returns>If the destination SizableSpan is shorter than the source SizableSpan, this method
        /// return false and no data is written to the destination.</returns>
        public bool TryCopyTo(SizableSpan<T> destination) {
            bool retVal = false;
            if ((uint)_length <= (uint)destination.Length) {
                Buffer.Memmove(ref destination._reference, ref _reference, (uint)_length); // Buffer.Memmove need .NET Standard 1.3
                retVal = true;
            }
            return retVal;
        }
        */
        /// <summary>
        /// Returns true if left and right point at the same memory and have the same length.  Note that
        /// this does *not* check to see if the *contents* are equal.
        /// </summary>
        public static bool operator ==(SizableSpan<T> left, SizableSpan<T> right) =>
            left._length == right._length &&
#if STRUCT_REF_FIELD
            Unsafe.AreSame(ref left._reference, ref right._reference)
#else
            left._byteOffse == right._byteOffse &&
            left._referenceSpan == right._referenceSpan
#endif
            ;
        /*
        /// <summary>
        /// Defines an implicit conversion of a <see cref="SizableSpan{T}"/> to a <see cref="ReadOnlySizableSpan{T}"/>
        /// </summary>
        public static implicit operator ReadOnlySizableSpan<T>(SizableSpan<T> SizableSpan) =>
            new ReadOnlySizableSpan<T>(ref SizableSpan._reference, SizableSpan._length);
        */
        /// <summary>
        /// For <see cref="SizableSpan{Char}"/>, returns a new instance of string that represents the characters pointed to by the SizableSpan.
        /// Otherwise, returns a <see cref="string"/> with the name of the type and the number of elements.
        /// </summary>
        public override unsafe string ToString() {
            //if (typeof(T) == typeof(char)) {
            //    return new string(new ReadOnlySpan<char>(ref Unsafe.As<T, char>(ref _reference), _length));
            //}
            nint ptr = (nint)Unsafe.AsPointer(ref GetPinnableReference());
            return $"System.SizableSpan<{typeof(T).Name}>[{_length}, ptr=0x{ptr:X}]";
        }

        /// <summary>
        /// Forms a slice out of the given SizableSpan, beginning at 'start'.
        /// </summary>
        /// <param name="start">The zero-based index at which to begin this slice.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the specified <paramref name="start"/> index is not in range (&lt;0 or &gt;Length).
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SizableSpan<T> Slice(int start) {
            if ((uint)start > (uint)_length)
                ThrowHelper.ThrowArgumentOutOfRangeException();

#if STRUCT_REF_FIELD
            return new SizableSpan<T>(ref Unsafe.Add(ref _reference, (nint)(uint)start /* force zero-extension */), _length - start);
#else
            unsafe {
                if (_referenceSpan.IsEmpty) {
                    return new SizableSpan<T>((void*)SizableUnsafe.AddPointer<T>(_byteOffse, (TSize)(uint)start /* force zero-extension */), _length - start);
                } else {
                    return new SizableSpan<T>(_referenceSpan, SizableUnsafe.AddPointer<T>(_byteOffse, (TSize)(uint)start /* force zero-extension */), _length - start);
                }
            }
#endif
        }

        /// <summary>
        /// Forms a slice out of the given SizableSpan, beginning at 'start', of given length
        /// </summary>
        /// <param name="start">The zero-based index at which to begin this slice.</param>
        /// <param name="length">The desired length for the slice (exclusive).</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the specified <paramref name="start"/> or end index is not in range (&lt;0 or &gt;Length).
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SizableSpan<T> Slice(int start, int length) {
            if (SizableSpanHelpers.Is64BitProcess) {
                // Since start and length are both 32-bit, their sum can be computed across a 64-bit domain
                // without loss of fidelity. The cast to uint before the cast to ulong ensures that the
                // extension from 32- to 64-bit is zero-extending rather than sign-extending. The end result
                // of this is that if either input is negative or if the input sum overflows past Int32.MaxValue,
                // that information is captured correctly in the comparison against the backing _length field.
                // We don't use this same mechanism in a 32-bit process due to the overhead of 64-bit arithmetic.
                if ((ulong)(uint)start + (ulong)(uint)length > (ulong)(uint)_length)
                    ThrowHelper.ThrowArgumentOutOfRangeException();
            } else {
                if ((uint)start > (uint)_length || (uint)length > (uint)(_length - start))
                    ThrowHelper.ThrowArgumentOutOfRangeException();
            }

#if STRUCT_REF_FIELD
            return new SizableSpan<T>(ref Unsafe.Add(ref _reference, (nint)(uint)start /* force zero-extension */), length);
#else
            unsafe {
                if (_referenceSpan.IsEmpty) {
                    return new SizableSpan<T>((void*)SizableUnsafe.AddPointer<T>(_byteOffse, (TSize)(uint)start /* force zero-extension */), length);
                } else {
                    return new SizableSpan<T>(_referenceSpan, SizableUnsafe.AddPointer<T>(_byteOffse, (TSize)(uint)start /* force zero-extension */), length);
                }
            }
#endif
        }
        /*
        /// <summary>
        /// Copies the contents of this SizableSpan into a new array.  This heap
        /// allocates, so should generally be avoided, however it is sometimes
        /// necessary to bridge the gap with APIs written in terms of arrays.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] ToArray() {
            if (_length == 0)
                return Array.Empty<T>();

            var destination = new T[_length];
            Buffer.Memmove(ref MemoryMarshal.GetArrayDataReference(destination), ref _reference, (uint)_length);
            return destination;
        }*/
    }
}
