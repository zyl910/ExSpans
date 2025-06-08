//#undef INVOKE_SPAN_METHOD

#if NET7_0_OR_GREATER
#define GENERIC_MATH // C# 11 - Generic math support. https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-11#generic-math-support
#endif // NET7_0_OR_GREATER
#if NET9_0_OR_GREATER
#define ALLOWS_REF_STRUCT // C# 13 - ref struct interface; allows ref struct. https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-13#ref-struct-interfaces
#define PARAMS_COLLECTIONS // C# 13 - params collections. https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-13#params-collections
#endif // NET9_0_OR_GREATER

using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
#if NETCOREAPP3_0_OR_GREATER
using System.Runtime.Intrinsics;
#endif // NETCOREAPP3_0_OR_GREATER
using System.Text;
using Zyl.ExSpans.Extensions;
using Zyl.ExSpans.Impl;
using Zyl.ExSpans.Reflection;
using Zyl.VectorTraits;

namespace Zyl.ExSpans {
    /// <summary>
    /// Provides extension methods for the span-related types, such as <see cref="ExSpan{T}"/> and <see cref="ReadOnlyExSpan{T}"/>. It can be regarded as the <see cref="MemoryExtensions"/> of <see cref="TSize"/> index range (提供跨度相关类型的扩展方法，例如 <see cref="ExSpan{T}"/> 和 <see cref="ReadOnlyExSpan{T}"/>. 它可以被视为 <see cref="TSize"/> 索引范围的 MemoryExtensions).
    /// </summary>
    /// <remarks>
    /// <para>Commonly extension methods such as <see cref="ExSpanExtensions.AsExSpan{T}(T[])">AsExSpan</see> are located in <see cref="ExSpanExtensions"/> (AsExSpan 等常用扩展方法位于 ExSpanExtensions).</para>
    /// </remarks>
    public static partial class ExMemoryExtensions {

#if NOT_RELATED
        /// <summary>Creates a new <see cref="ReadOnlyMemory{T}"/> over the portion of the target string.</summary>
        /// <param name="text">The target string.</param>
        /// <remarks>Returns default when <paramref name="text"/> is null.</remarks>
        public static ReadOnlyMemory<char> AsMemory(this string? text) {
            if (text == null)
                return default;

            return new ReadOnlyMemory<char>(text, 0, text.Length);
        }

        /// <summary>Creates a new <see cref="ReadOnlyMemory{T}"/> over the portion of the target string.</summary>
        /// <param name="text">The target string.</param>
        /// <param name="start">The index at which to begin this slice.</param>
        /// <remarks>Returns default when <paramref name="text"/> is null.</remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the specified <paramref name="start"/> index is not in range (&lt;0 or &gt;text.Length).
        /// </exception>
        public static ReadOnlyMemory<char> AsMemory(this string? text, int start) {
            if (text == null) {
                if (start != 0)
                    ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.start);
                return default;
            }

            if ((uint)start > (uint)text.Length)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.start);

            return new ReadOnlyMemory<char>(text, start, text.Length - start);
        }

        /// <summary>Creates a new <see cref="ReadOnlyMemory{T}"/> over the portion of the target string.</summary>
        /// <param name="text">The target string.</param>
        /// <param name="startIndex">The index at which to begin this slice.</param>
        public static ReadOnlyMemory<char> AsMemory(this string? text, Index startIndex) {
            if (text == null) {
                if (!startIndex.Equals(Index.Start))
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.text);

                return default;
            }

            int actualIndex = startIndex.GetOffset(text.Length);
            if ((uint)actualIndex > (uint)text.Length)
                ThrowHelper.ThrowArgumentOutOfRangeException();

            return new ReadOnlyMemory<char>(text, actualIndex, text.Length - actualIndex);
        }

        /// <summary>Creates a new <see cref="ReadOnlyMemory{T}"/> over the portion of the target string.</summary>
        /// <param name="text">The target string.</param>
        /// <param name="start">The index at which to begin this slice.</param>
        /// <param name="length">The desired length for the slice (exclusive).</param>
        /// <remarks>Returns default when <paramref name="text"/> is null.</remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the specified <paramref name="start"/> index or <paramref name="length"/> is not in range.
        /// </exception>
        public static ReadOnlyMemory<char> AsMemory(this string? text, int start, int length) {
            if (text == null) {
                if (start != 0 || length != 0)
                    ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.start);
                return default;
            }

#if TARGET_64BIT
            // See comment in ExSpan<T>.Slice for how this works.
            if ((ulong)(uint)start + (ulong)(uint)length > (ulong)(uint)text.Length)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.start);
#else
            if ((uint)start > (uint)text.Length || (uint)length > (uint)(text.Length - start))
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.start);
#endif

            return new ReadOnlyMemory<char>(text, start, length);
        }

        /// <summary>Creates a new <see cref="ReadOnlyMemory{T}"/> over the portion of the target string.</summary>
        /// <param name="text">The target string.</param>
        /// <param name="range">The range used to indicate the start and length of the sliced string.</param>
        public static ReadOnlyMemory<char> AsMemory(this string? text, Range range) {
            if (text == null) {
                Index startIndex = range.Start;
                Index endIndex = range.End;

                if (!startIndex.Equals(Index.Start) || !endIndex.Equals(Index.Start))
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.text);

                return default;
            }

            (int start, int length) = range.GetOffsetAndLength(text.Length);
            return new ReadOnlyMemory<char>(text, start, length);
        }
#endif // NOT_RELATED

        /// <inheritdoc cref="Contains{T}(ReadOnlyExSpan{T}, T)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [OverloadResolutionPriority(-1)]
        public static bool Contains<T>(this ExSpan<T> span, T value) where T : IEquatable<T>? =>
            Contains((ReadOnlyExSpan<T>)span, value);

        /// <summary>
        /// Searches for the specified value and returns true if found. If not found, returns false. Values are compared using IEquatable{T}.Equals(T).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="span">The span to search.</param>
        /// <param name="value">The value to search for.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contains<T>(this ReadOnlyExSpan<T> span, T value) where T : IEquatable<T>? {
            if (TypeHelper.IsBitwiseEquatable<T>()) {
                if (Unsafe.SizeOf<T>() == sizeof(byte)) {
                    return ExSpanHelpers.ContainsValueType(
                        ref Unsafe.As<T, byte>(ref ExMemoryMarshal.GetReference(span)),
                        ExUnsafe.BitCast<T, byte>(value),
                        span.Length);
                } else if (Unsafe.SizeOf<T>() == sizeof(short)) {
                    return ExSpanHelpers.ContainsValueType(
                        ref Unsafe.As<T, short>(ref ExMemoryMarshal.GetReference(span)),
                        ExUnsafe.BitCast<T, short>(value),
                        span.Length);
                } else if (Unsafe.SizeOf<T>() == sizeof(int)) {
                    return ExSpanHelpers.ContainsValueType(
                        ref Unsafe.As<T, int>(ref ExMemoryMarshal.GetReference(span)),
                        ExUnsafe.BitCast<T, int>(value),
                        span.Length);
                } else if (Unsafe.SizeOf<T>() == sizeof(long)) {
                    return ExSpanHelpers.ContainsValueType(
                        ref Unsafe.As<T, long>(ref ExMemoryMarshal.GetReference(span)),
                        ExUnsafe.BitCast<T, long>(value),
                        span.Length);
                }
            }

            return ExSpanHelpers.Contains(ref ExMemoryMarshal.GetReference(span), value, span.Length);
        }

        /// <summary>
        /// Searches for the specified value and returns true if found. If not found, returns false.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="span">The span to search.</param>
        /// <param name="value">The value to search for.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default <see cref="IEqualityComparer{T}"/> for the type of an element.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contains<T>(this ReadOnlyExSpan<T> span, T value, IEqualityComparer<T>? comparer = null) =>
            IndexOf(span, value, comparer) >= 0;

        /// <inheritdoc cref="ContainsAny{T}(ReadOnlyExSpan{T}, T, T)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [OverloadResolutionPriority(-1)]
        public static bool ContainsAny<T>(this ExSpan<T> span, T value0, T value1) where T : IEquatable<T>? =>
            ContainsAny((ReadOnlyExSpan<T>)span, value0, value1);

        /// <inheritdoc cref="ContainsAny{T}(ReadOnlyExSpan{T}, T, T, T)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [OverloadResolutionPriority(-1)]
        public static bool ContainsAny<T>(this ExSpan<T> span, T value0, T value1, T value2) where T : IEquatable<T>? =>
            ContainsAny((ReadOnlyExSpan<T>)span, value0, value1, value2);

        /// <inheritdoc cref="ContainsAny{T}(ReadOnlyExSpan{T}, ReadOnlyExSpan{T})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [OverloadResolutionPriority(-1)]
        public static bool ContainsAny<T>(this ExSpan<T> span, ReadOnlyExSpan<T> values) where T : IEquatable<T>? =>
            ContainsAny((ReadOnlyExSpan<T>)span, values);

#if NET8_0_OR_GREATER && TODO // [TODO why] SearchValues.IndexOfAny is internal
        /// <inheritdoc cref="ContainsAny{T}(ReadOnlyExSpan{T}, SearchValues{T})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [OverloadResolutionPriority(-1)]
        public static bool ContainsAny<T>(this ExSpan<T> span, SearchValues<T> values) where T : IEquatable<T>? =>
            ContainsAny((ReadOnlyExSpan<T>)span, values);

        /// <inheritdoc cref="ContainsAny(ReadOnlyExSpan{char}, SearchValues{string})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [OverloadResolutionPriority(-1)]
        public static bool ContainsAny(this ExSpan<char> span, SearchValues<string> values) =>
            ContainsAny((ReadOnlyExSpan<char>)span, values);
#endif // NET8_0_OR_GREATER

        /// <inheritdoc cref="ContainsAnyExcept{T}(ReadOnlyExSpan{T}, T)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [OverloadResolutionPriority(-1)]
        public static bool ContainsAnyExcept<T>(this ExSpan<T> span, T value) where T : IEquatable<T>? =>
            ContainsAnyExcept((ReadOnlyExSpan<T>)span, value);

        /// <inheritdoc cref="ContainsAnyExcept{T}(ReadOnlyExSpan{T}, T, T)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [OverloadResolutionPriority(-1)]
        public static bool ContainsAnyExcept<T>(this ExSpan<T> span, T value0, T value1) where T : IEquatable<T>? =>
            ContainsAnyExcept((ReadOnlyExSpan<T>)span, value0, value1);

        /// <inheritdoc cref="ContainsAnyExcept{T}(ReadOnlyExSpan{T}, T, T, T)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [OverloadResolutionPriority(-1)]
        public static bool ContainsAnyExcept<T>(this ExSpan<T> span, T value0, T value1, T value2) where T : IEquatable<T>? =>
            ContainsAnyExcept((ReadOnlyExSpan<T>)span, value0, value1, value2);

        /// <inheritdoc cref="ContainsAnyExcept{T}(ReadOnlyExSpan{T}, ReadOnlyExSpan{T})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [OverloadResolutionPriority(-1)]
        public static bool ContainsAnyExcept<T>(this ExSpan<T> span, ReadOnlyExSpan<T> values) where T : IEquatable<T>? =>
            ContainsAnyExcept((ReadOnlyExSpan<T>)span, values);

#if NET8_0_OR_GREATER && TODO // [TODO why] SearchValues.IndexOfAny is internal
        /// <inheritdoc cref="ContainsAnyExcept{T}(ReadOnlyExSpan{T}, SearchValues{T})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [OverloadResolutionPriority(-1)]
        public static bool ContainsAnyExcept<T>(this ExSpan<T> span, SearchValues<T> values) where T : IEquatable<T>? =>
            ContainsAnyExcept((ReadOnlyExSpan<T>)span, values);
#endif // NET8_0_OR_GREATER

        /// <inheritdoc cref="ContainsAnyInRange{T}(ReadOnlyExSpan{T}, T, T)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [OverloadResolutionPriority(-1)]
        public static bool ContainsAnyInRange<T>(this ExSpan<T> span, T lowInclusive, T highInclusive) where T : IComparable<T> =>
            ContainsAnyInRange((ReadOnlyExSpan<T>)span, lowInclusive, highInclusive);

        /// <inheritdoc cref="ContainsAnyExceptInRange{T}(ReadOnlyExSpan{T}, T, T)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [OverloadResolutionPriority(-1)]
        public static bool ContainsAnyExceptInRange<T>(this ExSpan<T> span, T lowInclusive, T highInclusive) where T : IComparable<T> =>
            ContainsAnyExceptInRange((ReadOnlyExSpan<T>)span, lowInclusive, highInclusive);

        /// <summary>
        /// Searches for any occurrence of the specified <paramref name="value0"/> or <paramref name="value1"/>, and returns true if found. If not found, returns false.
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="value0">One of the values to search for.</param>
        /// <param name="value1">One of the values to search for.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ContainsAny<T>(this ReadOnlyExSpan<T> span, T value0, T value1) where T : IEquatable<T>? =>
            IndexOfAny(span, value0, value1) >= 0;

        /// <summary>
        /// Searches for any occurrence of the specified <paramref name="value0"/> or <paramref name="value1"/>, and returns true if found. If not found, returns false.
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="value0">One of the values to search for.</param>
        /// <param name="value1">One of the values to search for.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default <see cref="IEqualityComparer{T}"/> for the type of an element.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ContainsAny<T>(this ReadOnlyExSpan<T> span, T value0, T value1, IEqualityComparer<T>? comparer = null) =>
            IndexOfAny(span, value0, value1, comparer) >= 0;

        /// <summary>
        /// Searches for any occurrence of the specified <paramref name="value0"/>, <paramref name="value1"/>, or <paramref name="value2"/>, and returns true if found. If not found, returns false.
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="value0">One of the values to search for.</param>
        /// <param name="value1">One of the values to search for.</param>
        /// <param name="value2">One of the values to search for.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ContainsAny<T>(this ReadOnlyExSpan<T> span, T value0, T value1, T value2) where T : IEquatable<T>? =>
            IndexOfAny(span, value0, value1, value2) >= 0;

        /// <summary>
        /// Searches for any occurrence of the specified <paramref name="value0"/>, <paramref name="value1"/>, or <paramref name="value2"/>, and returns true if found. If not found, returns false.
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="value0">One of the values to search for.</param>
        /// <param name="value1">One of the values to search for.</param>
        /// <param name="value2">One of the values to search for.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default <see cref="IEqualityComparer{T}"/> for the type of an element.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ContainsAny<T>(this ReadOnlyExSpan<T> span, T value0, T value1, T value2, IEqualityComparer<T>? comparer = null) =>
            IndexOfAny(span, value0, value1, value2, comparer) >= 0;

        /// <summary>
        /// Searches for any occurrence of any of the specified <paramref name="values"/> and returns true if found. If not found, returns false.
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="values">The set of values to search for.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ContainsAny<T>(this ReadOnlyExSpan<T> span, ReadOnlyExSpan<T> values) where T : IEquatable<T>? =>
            IndexOfAny(span, values) >= 0;

        /// <summary>
        /// Searches for any occurrence of any of the specified <paramref name="values"/> and returns true if found. If not found, returns false.
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="values">The set of values to search for.</param>
        /// <param name="comparer">The comparer to use. If <see langword="null"/>, <see cref="EqualityComparer{T}.Default"/> is used.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ContainsAny<T>(this ReadOnlyExSpan<T> span, ReadOnlyExSpan<T> values, IEqualityComparer<T>? comparer = null) =>
            IndexOfAny(span, values, comparer) >= 0;

#if NET8_0_OR_GREATER && TODO // [TODO why] SearchValues.IndexOfAny is internal
        /// <summary>
        /// Searches for any occurrence of any of the specified <paramref name="values"/> and returns true if found. If not found, returns false.
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="values">The set of values to search for.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ContainsAny<T>(this ReadOnlyExSpan<T> span, SearchValues<T> values) where T : IEquatable<T>? {
            if (values is null) {
                throw new ArgumentNullException(nameof(values));
            }

            return values.ContainsAny(span);
        }

        /// <summary>
        /// Searches for any occurrence of any of the specified substring <paramref name="values"/> and returns true if found. If not found, returns false.
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="values">The set of values to search for.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ContainsAny(this ReadOnlyExSpan<char> span, SearchValues<string> values) =>
            IndexOfAny(span, values) >= 0;
#endif // NET8_0_OR_GREATER

        /// <summary>
        /// Searches for any value other than the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="value">A value to avoid.</param>
        /// <returns>
        /// True if any value other than <paramref name="value"/> is present in the span.
        /// If all of the values are <paramref name="value"/>, returns false.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ContainsAnyExcept<T>(this ReadOnlyExSpan<T> span, T value) where T : IEquatable<T>? =>
            IndexOfAnyExcept(span, value) >= 0;

        /// <summary>
        /// Searches for any value other than the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="value">A value to avoid.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default <see cref="IEqualityComparer{T}"/> for the type of an element.</param>
        /// <returns>
        /// True if any value other than <paramref name="value"/> is present in the span.
        /// If all of the values are <paramref name="value"/>, returns false.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ContainsAnyExcept<T>(this ReadOnlyExSpan<T> span, T value, IEqualityComparer<T>? comparer = null) =>
            IndexOfAnyExcept(span, value, comparer) >= 0;

        /// <summary>
        /// Searches for any value other than the specified <paramref name="value0"/> or <paramref name="value1"/>.
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="value0">A value to avoid.</param>
        /// <param name="value1">A value to avoid.</param>
        /// <returns>
        /// True if any value other than <paramref name="value0"/> and <paramref name="value1"/> is present in the span.
        /// If all of the values are <paramref name="value0"/> or <paramref name="value1"/>, returns false.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ContainsAnyExcept<T>(this ReadOnlyExSpan<T> span, T value0, T value1) where T : IEquatable<T>? =>
            IndexOfAnyExcept(span, value0, value1) >= 0;

        /// <summary>
        /// Searches for any value other than the specified <paramref name="value0"/> or <paramref name="value1"/>.
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="value0">A value to avoid.</param>
        /// <param name="value1">A value to avoid.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default <see cref="IEqualityComparer{T}"/> for the type of an element.</param>
        /// <returns>
        /// True if any value other than <paramref name="value0"/> and <paramref name="value1"/> is present in the span.
        /// If all of the values are <paramref name="value0"/> or <paramref name="value1"/>, returns false.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ContainsAnyExcept<T>(this ReadOnlyExSpan<T> span, T value0, T value1, IEqualityComparer<T>? comparer = null) =>
            IndexOfAnyExcept(span, value0, value1, comparer) >= 0;

        /// <summary>
        /// Searches for any value other than the specified <paramref name="value0"/>, <paramref name="value1"/>, or <paramref name="value2"/>.
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="value0">A value to avoid.</param>
        /// <param name="value1">A value to avoid.</param>
        /// <param name="value2">A value to avoid.</param>
        /// <returns>
        /// True if any value other than <paramref name="value0"/>, <paramref name="value1"/>, and <paramref name="value2"/> is present in the span.
        /// If all of the values are <paramref name="value0"/>, <paramref name="value1"/>, or <paramref name="value2"/>, returns false.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ContainsAnyExcept<T>(this ReadOnlyExSpan<T> span, T value0, T value1, T value2) where T : IEquatable<T>? =>
            IndexOfAnyExcept(span, value0, value1, value2) >= 0;

        /// <summary>
        /// Searches for any value other than the specified <paramref name="value0"/>, <paramref name="value1"/>, or <paramref name="value2"/>.
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="value0">A value to avoid.</param>
        /// <param name="value1">A value to avoid.</param>
        /// <param name="value2">A value to avoid.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default <see cref="IEqualityComparer{T}"/> for the type of an element.</param>
        /// <returns>
        /// True if any value other than <paramref name="value0"/>, <paramref name="value1"/>, and <paramref name="value2"/> is present in the span.
        /// If all of the values are <paramref name="value0"/>, <paramref name="value1"/>, or <paramref name="value2"/>, returns false.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ContainsAnyExcept<T>(this ReadOnlyExSpan<T> span, T value0, T value1, T value2, IEqualityComparer<T>? comparer = null) =>
            IndexOfAnyExcept(span, value0, value1, value2, comparer) >= 0;

        /// <summary>
        /// Searches for any value other than the specified <paramref name="values"/>.
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="values">The values to avoid.</param>
        /// <returns>
        /// True if any value other than those in <paramref name="values"/> is present in the span.
        /// If all of the values are in <paramref name="values"/>, returns false.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ContainsAnyExcept<T>(this ReadOnlyExSpan<T> span, ReadOnlyExSpan<T> values) where T : IEquatable<T>? =>
            IndexOfAnyExcept(span, values) >= 0;

        /// <summary>
        /// Searches for any value other than the specified <paramref name="values"/>.
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="values">The values to avoid.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default <see cref="IEqualityComparer{T}"/> for the type of an element.</param>
        /// <returns>
        /// True if any value other than those in <paramref name="values"/> is present in the span.
        /// If all of the values are in <paramref name="values"/>, returns false.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ContainsAnyExcept<T>(this ReadOnlyExSpan<T> span, ReadOnlyExSpan<T> values, IEqualityComparer<T>? comparer = null) =>
            IndexOfAnyExcept(span, values, comparer) >= 0;

#if NET8_0_OR_GREATER && TODO // [TODO why] SearchValues.IndexOfAny is internal
        /// <summary>
        /// Searches for any value other than the specified <paramref name="values"/>.
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="values">The values to avoid.</param>
        /// <returns>
        /// True if any value other than those in <paramref name="values"/> is present in the span.
        /// If all of the values are in <paramref name="values"/>, returns false.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ContainsAnyExcept<T>(this ReadOnlyExSpan<T> span, SearchValues<T> values) where T : IEquatable<T>? {
            if (values is null) {
                throw new ArgumentNullException(nameof(values));
            }

            return values.ContainsAnyExcept(span);
        }
#endif // NET8_0_OR_GREATER

        /// <summary>
        /// Searches for any value in the range between <paramref name="lowInclusive"/> and <paramref name="highInclusive"/>, inclusive, and returns true if found. If not found, returns false.
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="lowInclusive">A lower bound, inclusive, of the range for which to search.</param>
        /// <param name="highInclusive">A upper bound, inclusive, of the range for which to search.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ContainsAnyInRange<T>(this ReadOnlyExSpan<T> span, T lowInclusive, T highInclusive) where T : IComparable<T> =>
            IndexOfAnyInRange(span, lowInclusive, highInclusive) >= 0;

        /// <summary>
        /// Searches for any value outside of the range between <paramref name="lowInclusive"/> and <paramref name="highInclusive"/>, inclusive.
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="lowInclusive">A lower bound, inclusive, of the excluded range.</param>
        /// <param name="highInclusive">A upper bound, inclusive, of the excluded range.</param>
        /// <returns>
        /// True if any value other than those in the specified range is present in the span.
        /// If all of the values are inside of the specified range, returns false.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ContainsAnyExceptInRange<T>(this ReadOnlyExSpan<T> span, T lowInclusive, T highInclusive) where T : IComparable<T> =>
            IndexOfAnyExceptInRange(span, lowInclusive, highInclusive) >= 0;

        /// <summary>
        /// Searches for the specified value and returns the index of its first occurrence. If not found, returns -1. Values are compared using IEquatable{T}.Equals(T).
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="value">The value to search for.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [OverloadResolutionPriority(-1)]
        public static nint IndexOf<T>(this ExSpan<T> span, T value) where T : IEquatable<T>? =>
            IndexOf((ReadOnlyExSpan<T>)span, value);

        /// <summary>
        /// Searches for the specified sequence and returns the index of its first occurrence. If not found, returns -1. Values are compared using IEquatable{T}.Equals(T).
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="value">The sequence to search for.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [OverloadResolutionPriority(-1)]
        public static nint IndexOf<T>(this ExSpan<T> span, ReadOnlyExSpan<T> value) where T : IEquatable<T>? =>
            IndexOf((ReadOnlyExSpan<T>)span, value);

        /// <summary>
        /// Searches for the specified value and returns the index of its last occurrence. If not found, returns -1. Values are compared using IEquatable{T}.Equals(T).
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="value">The value to search for.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [OverloadResolutionPriority(-1)]
        public static TSize LastIndexOf<T>(this ExSpan<T> span, T value) where T : IEquatable<T>? =>
            LastIndexOf((ReadOnlyExSpan<T>)span, value);

        /// <summary>
        /// Searches for the specified sequence and returns the index of its last occurrence. If not found, returns -1. Values are compared using IEquatable{T}.Equals(T).
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="value">The sequence to search for.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [OverloadResolutionPriority(-1)]
        public static TSize LastIndexOf<T>(this ExSpan<T> span, ReadOnlyExSpan<T> value) where T : IEquatable<T>? =>
            LastIndexOf((ReadOnlyExSpan<T>)span, value);

        /// <summary>Searches for the first index of any value other than the specified <paramref name="value"/>.</summary>
        /// <typeparam name="T">The type of the span and values.</typeparam>
        /// <param name="span">The span to search.</param>
        /// <param name="value">A value to avoid.</param>
        /// <returns>
        /// The index in the span of the first occurrence of any value other than <paramref name="value"/>.
        /// If all of the values are <paramref name="value"/>, returns -1.
        /// </returns>
        [OverloadResolutionPriority(-1)]
        public static TSize IndexOfAnyExcept<T>(this ExSpan<T> span, T value) where T : IEquatable<T>? =>
            IndexOfAnyExcept((ReadOnlyExSpan<T>)span, value);

        /// <summary>Searches for the first index of any value other than the specified <paramref name="value0"/> or <paramref name="value1"/>.</summary>
        /// <typeparam name="T">The type of the span and values.</typeparam>
        /// <param name="span">The span to search.</param>
        /// <param name="value0">A value to avoid.</param>
        /// <param name="value1">A value to avoid</param>
        /// <returns>
        /// The index in the span of the first occurrence of any value other than <paramref name="value0"/> and <paramref name="value1"/>.
        /// If all of the values are <paramref name="value0"/> or <paramref name="value1"/>, returns -1.
        /// </returns>
        [OverloadResolutionPriority(-1)]
        public static TSize IndexOfAnyExcept<T>(this ExSpan<T> span, T value0, T value1) where T : IEquatable<T>? =>
            IndexOfAnyExcept((ReadOnlyExSpan<T>)span, value0, value1);

        /// <summary>Searches for the first index of any value other than the specified <paramref name="value0"/>, <paramref name="value1"/>, or <paramref name="value2"/>.</summary>
        /// <typeparam name="T">The type of the span and values.</typeparam>
        /// <param name="span">The span to search.</param>
        /// <param name="value0">A value to avoid.</param>
        /// <param name="value1">A value to avoid</param>
        /// <param name="value2">A value to avoid</param>
        /// <returns>
        /// The index in the span of the first occurrence of any value other than <paramref name="value0"/>, <paramref name="value1"/>, and <paramref name="value2"/>.
        /// If all of the values are <paramref name="value0"/>, <paramref name="value1"/>, or <paramref name="value2"/>, returns -1.
        /// </returns>
        [OverloadResolutionPriority(-1)]
        public static TSize IndexOfAnyExcept<T>(this ExSpan<T> span, T value0, T value1, T value2) where T : IEquatable<T>? =>
            IndexOfAnyExcept((ReadOnlyExSpan<T>)span, value0, value1, value2);

        /// <summary>Searches for the first index of any value other than the specified <paramref name="values"/>.</summary>
        /// <typeparam name="T">The type of the span and values.</typeparam>
        /// <param name="span">The span to search.</param>
        /// <param name="values">The values to avoid.</param>
        /// <returns>
        /// The index in the span of the first occurrence of any value other than those in <paramref name="values"/>.
        /// If all of the values are in <paramref name="values"/>, returns -1.
        /// </returns>
        [OverloadResolutionPriority(-1)]
        public static TSize IndexOfAnyExcept<T>(this ExSpan<T> span, ReadOnlyExSpan<T> values) where T : IEquatable<T>? =>
            IndexOfAnyExcept((ReadOnlyExSpan<T>)span, values);

#if NET8_0_OR_GREATER && TODO // [TODO why] SearchValues.IndexOfAny is internal
        /// <summary>Searches for the first index of any value other than the specified <paramref name="values"/>.</summary>
        /// <typeparam name="T">The type of the span and values.</typeparam>
        /// <param name="span">The span to search.</param>
        /// <param name="values">The values to avoid.</param>
        /// <returns>
        /// The index in the span of the first occurrence of any value other than those in <paramref name="values"/>.
        /// If all of the values are in <paramref name="values"/>, returns -1.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [OverloadResolutionPriority(-1)]
        public static TSize IndexOfAnyExcept<T>(this ExSpan<T> span, SearchValues<T> values) where T : IEquatable<T>? =>
            IndexOfAnyExcept((ReadOnlyExSpan<T>)span, values);
#endif // NET8_0_OR_GREATER

        /// <summary>Searches for the first index of any value other than the specified <paramref name="value"/>.</summary>
        /// <typeparam name="T">The type of the span and values.</typeparam>
        /// <param name="span">The span to search.</param>
        /// <param name="value">A value to avoid.</param>
        /// <returns>
        /// The index in the span of the first occurrence of any value other than <paramref name="value"/>.
        /// If all of the values are <paramref name="value"/>, returns -1.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSize IndexOfAnyExcept<T>(this ReadOnlyExSpan<T> span, T value) where T : IEquatable<T>? {
            if (TypeHelper.IsBitwiseEquatable<T>()) {
                if (Unsafe.SizeOf<T>() == sizeof(byte)) {
                    return ExSpanHelpers.IndexOfAnyExceptValueType(
                        ref Unsafe.As<T, byte>(ref ExMemoryMarshal.GetReference(span)),
                        ExUnsafe.BitCast<T, byte>(value),
                        span.Length);
                } else if (Unsafe.SizeOf<T>() == sizeof(short)) {
                    return ExSpanHelpers.IndexOfAnyExceptValueType(
                        ref Unsafe.As<T, short>(ref ExMemoryMarshal.GetReference(span)),
                        ExUnsafe.BitCast<T, short>(value),
                        span.Length);
                } else if (Unsafe.SizeOf<T>() == sizeof(int)) {
                    return ExSpanHelpers.IndexOfAnyExceptValueType(
                        ref Unsafe.As<T, int>(ref ExMemoryMarshal.GetReference(span)),
                        ExUnsafe.BitCast<T, int>(value),
                        span.Length);
                } else if (Unsafe.SizeOf<T>() == sizeof(long)) {
                    return ExSpanHelpers.IndexOfAnyExceptValueType(
                        ref Unsafe.As<T, long>(ref ExMemoryMarshal.GetReference(span)),
                        ExUnsafe.BitCast<T, long>(value),
                        span.Length);
                }
            }

            return ExSpanHelpers.IndexOfAnyExcept(ref ExMemoryMarshal.GetReference(span), value, span.Length);
        }

        /// <summary>Searches for the first index of any value other than the specified <paramref name="value"/>.</summary>
        /// <typeparam name="T">The type of the span and values.</typeparam>
        /// <param name="span">The span to search.</param>
        /// <param name="value">A value to avoid.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default <see cref="IEqualityComparer{T}"/> for the type of an element.</param>
        /// <returns>
        /// The index in the span of the first occurrence of any value other than <paramref name="value"/>.
        /// If all of the values are <paramref name="value"/>, returns -1.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSize IndexOfAnyExcept<T>(this ReadOnlyExSpan<T> span, T value, IEqualityComparer<T>? comparer = null) {
            if (TypeHelper.IsValueType<T>() && (comparer is null || comparer == EqualityComparer<T>.Default)) {
                if (TypeHelper.IsBitwiseEquatable<T>()) {
                    if (Unsafe.SizeOf<T>() == sizeof(byte)) {
                        return ExSpanHelpers.IndexOfAnyExceptValueType(
                            ref Unsafe.As<T, byte>(ref ExMemoryMarshal.GetReference(span)),
                            ExUnsafe.BitCast<T, byte>(value),
                            span.Length);
                    } else if (Unsafe.SizeOf<T>() == sizeof(short)) {
                        return ExSpanHelpers.IndexOfAnyExceptValueType(
                            ref Unsafe.As<T, short>(ref ExMemoryMarshal.GetReference(span)),
                            ExUnsafe.BitCast<T, short>(value),
                            span.Length);
                    } else if (Unsafe.SizeOf<T>() == sizeof(int)) {
                        return ExSpanHelpers.IndexOfAnyExceptValueType(
                            ref Unsafe.As<T, int>(ref ExMemoryMarshal.GetReference(span)),
                            ExUnsafe.BitCast<T, int>(value),
                            span.Length);
                    } else if (Unsafe.SizeOf<T>() == sizeof(long)) {
                        return ExSpanHelpers.IndexOfAnyExceptValueType(
                            ref Unsafe.As<T, long>(ref ExMemoryMarshal.GetReference(span)),
                            ExUnsafe.BitCast<T, long>(value),
                            span.Length);
                    }
                }

                return IndexOfAnyExceptDefaultComparer(span, value);
                static TSize IndexOfAnyExceptDefaultComparer(ReadOnlyExSpan<T> span, T value) {
                    for (TSize i = 0; i < span.Length; i++) {
                        if (!EqualityComparer<T>.Default.Equals(span[i], value)) {
                            return i;
                        }
                    }

                    return -1;
                }
            } else {
                return IndexOfAnyExceptComparer(span, value, comparer);
                static TSize IndexOfAnyExceptComparer(ReadOnlyExSpan<T> span, T value, IEqualityComparer<T>? comparer) {
                    comparer ??= EqualityComparer<T>.Default;

                    for (TSize i = 0; i < span.Length; i++) {
                        if (!comparer.Equals(span[i], value)) {
                            return i;
                        }
                    }

                    return -1;
                }
            }
        }

        /// <summary>Searches for the first index of any value other than the specified <paramref name="value0"/> or <paramref name="value1"/>.</summary>
        /// <typeparam name="T">The type of the span and values.</typeparam>
        /// <param name="span">The span to search.</param>
        /// <param name="value0">A value to avoid.</param>
        /// <param name="value1">A value to avoid</param>
        /// <returns>
        /// The index in the span of the first occurrence of any value other than <paramref name="value0"/> and <paramref name="value1"/>.
        /// If all of the values are <paramref name="value0"/> or <paramref name="value1"/>, returns -1.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSize IndexOfAnyExcept<T>(this ReadOnlyExSpan<T> span, T value0, T value1) where T : IEquatable<T>? {
            if (TypeHelper.IsBitwiseEquatable<T>()) {
                if (Unsafe.SizeOf<T>() == sizeof(byte)) {
                    return ExSpanHelpers.IndexOfAnyExceptValueType(
                        ref Unsafe.As<T, byte>(ref ExMemoryMarshal.GetReference(span)),
                        ExUnsafe.BitCast<T, byte>(value0),
                        ExUnsafe.BitCast<T, byte>(value1),
                        span.Length);
                } else if (Unsafe.SizeOf<T>() == sizeof(short)) {
                    return ExSpanHelpers.IndexOfAnyExceptValueType(
                        ref Unsafe.As<T, short>(ref ExMemoryMarshal.GetReference(span)),
                        ExUnsafe.BitCast<T, short>(value0),
                        ExUnsafe.BitCast<T, short>(value1),
                        span.Length);
                }
            }

            return ExSpanHelpers.IndexOfAnyExcept(ref ExMemoryMarshal.GetReference(span), value0, value1, span.Length);
        }

        /// <summary>Searches for the first index of any value other than the specified <paramref name="value0"/> or <paramref name="value1"/>.</summary>
        /// <typeparam name="T">The type of the span and values.</typeparam>
        /// <param name="span">The span to search.</param>
        /// <param name="value0">A value to avoid.</param>
        /// <param name="value1">A value to avoid</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default <see cref="IEqualityComparer{T}"/> for the type of an element.</param>
        /// <returns>
        /// The index in the span of the first occurrence of any value other than <paramref name="value0"/> and <paramref name="value1"/>.
        /// If all of the values are <paramref name="value0"/> or <paramref name="value1"/>, returns -1.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSize IndexOfAnyExcept<T>(this ReadOnlyExSpan<T> span, T value0, T value1, IEqualityComparer<T>? comparer = null) {
            if (TypeHelper.IsValueType<T>() && (comparer is null || comparer == EqualityComparer<T>.Default)) {
                if (TypeHelper.IsBitwiseEquatable<T>()) {
                    if (Unsafe.SizeOf<T>() == sizeof(byte)) {
                        return ExSpanHelpers.IndexOfAnyExceptValueType(
                            ref Unsafe.As<T, byte>(ref ExMemoryMarshal.GetReference(span)),
                            ExUnsafe.BitCast<T, byte>(value0),
                            ExUnsafe.BitCast<T, byte>(value1),
                            span.Length);
                    } else if (Unsafe.SizeOf<T>() == sizeof(short)) {
                        return ExSpanHelpers.IndexOfAnyExceptValueType(
                            ref Unsafe.As<T, short>(ref ExMemoryMarshal.GetReference(span)),
                            ExUnsafe.BitCast<T, short>(value0),
                            ExUnsafe.BitCast<T, short>(value1),
                            span.Length);
                    }
                }

                return IndexOfAnyExceptDefaultComparer(span, value0, value1);
                static TSize IndexOfAnyExceptDefaultComparer(ReadOnlyExSpan<T> span, T value0, T value1) {
                    for (TSize i = 0; i < span.Length; i++) {
                        if (!EqualityComparer<T>.Default.Equals(span[i], value0) &&
                            !EqualityComparer<T>.Default.Equals(span[i], value1)) {
                            return i;
                        }
                    }

                    return -1;
                }
            } else {
                return IndexOfAnyExceptComparer(span, value0, value1, comparer);
                static TSize IndexOfAnyExceptComparer(ReadOnlyExSpan<T> span, T value0, T value1, IEqualityComparer<T>? comparer) {
                    comparer ??= EqualityComparer<T>.Default;

                    for (TSize i = 0; i < span.Length; i++) {
                        if (!comparer.Equals(span[i], value0) &&
                            !comparer.Equals(span[i], value1)) {
                            return i;
                        }
                    }

                    return -1;
                }
            }
        }

        /// <summary>Searches for the first index of any value other than the specified <paramref name="value0"/>, <paramref name="value1"/>, or <paramref name="value2"/>.</summary>
        /// <typeparam name="T">The type of the span and values.</typeparam>
        /// <param name="span">The span to search.</param>
        /// <param name="value0">A value to avoid.</param>
        /// <param name="value1">A value to avoid</param>
        /// <param name="value2">A value to avoid</param>
        /// <returns>
        /// The index in the span of the first occurrence of any value other than <paramref name="value0"/>, <paramref name="value1"/>, and <paramref name="value2"/>.
        /// If all of the values are <paramref name="value0"/>, <paramref name="value1"/>, and <paramref name="value2"/>, returns -1.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSize IndexOfAnyExcept<T>(this ReadOnlyExSpan<T> span, T value0, T value1, T value2) where T : IEquatable<T>? {
            if (TypeHelper.IsBitwiseEquatable<T>()) {
                if (Unsafe.SizeOf<T>() == sizeof(byte)) {
                    return ExSpanHelpers.IndexOfAnyExceptValueType(
                        ref Unsafe.As<T, byte>(ref ExMemoryMarshal.GetReference(span)),
                        ExUnsafe.BitCast<T, byte>(value0),
                        ExUnsafe.BitCast<T, byte>(value1),
                        ExUnsafe.BitCast<T, byte>(value2),
                        span.Length);
                } else if (Unsafe.SizeOf<T>() == sizeof(short)) {
                    return ExSpanHelpers.IndexOfAnyExceptValueType(
                        ref Unsafe.As<T, short>(ref ExMemoryMarshal.GetReference(span)),
                        ExUnsafe.BitCast<T, short>(value0),
                        ExUnsafe.BitCast<T, short>(value1),
                        ExUnsafe.BitCast<T, short>(value2),
                        span.Length);
                }
            }

            return ExSpanHelpers.IndexOfAnyExcept(ref ExMemoryMarshal.GetReference(span), value0, value1, value2, span.Length);
        }

        /// <summary>Searches for the first index of any value other than the specified <paramref name="value0"/>, <paramref name="value1"/>, or <paramref name="value2"/>.</summary>
        /// <typeparam name="T">The type of the span and values.</typeparam>
        /// <param name="span">The span to search.</param>
        /// <param name="value0">A value to avoid.</param>
        /// <param name="value1">A value to avoid</param>
        /// <param name="value2">A value to avoid</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default <see cref="IEqualityComparer{T}"/> for the type of an element.</param>
        /// <returns>
        /// The index in the span of the first occurrence of any value other than <paramref name="value0"/>, <paramref name="value1"/>, and <paramref name="value2"/>.
        /// If all of the values are <paramref name="value0"/>, <paramref name="value1"/>, and <paramref name="value2"/>, returns -1.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSize IndexOfAnyExcept<T>(this ReadOnlyExSpan<T> span, T value0, T value1, T value2, IEqualityComparer<T>? comparer = null) {
            if (TypeHelper.IsValueType<T>() && (comparer is null || comparer == EqualityComparer<T>.Default)) {
                if (TypeHelper.IsBitwiseEquatable<T>()) {
                    if (Unsafe.SizeOf<T>() == sizeof(byte)) {
                        return ExSpanHelpers.IndexOfAnyExceptValueType(
                            ref Unsafe.As<T, byte>(ref ExMemoryMarshal.GetReference(span)),
                            ExUnsafe.BitCast<T, byte>(value0),
                            ExUnsafe.BitCast<T, byte>(value1),
                            ExUnsafe.BitCast<T, byte>(value2),
                            span.Length);
                    } else if (Unsafe.SizeOf<T>() == sizeof(short)) {
                        return ExSpanHelpers.IndexOfAnyExceptValueType(
                            ref Unsafe.As<T, short>(ref ExMemoryMarshal.GetReference(span)),
                            ExUnsafe.BitCast<T, short>(value0),
                            ExUnsafe.BitCast<T, short>(value1),
                            ExUnsafe.BitCast<T, short>(value2),
                            span.Length);
                    }
                }

                return IndexOfAnyExceptDefaultComparer(span, value0, value1, value2);
                static TSize IndexOfAnyExceptDefaultComparer(ReadOnlyExSpan<T> span, T value0, T value1, T value2) {
                    for (TSize i = 0; i < span.Length; i++) {
                        if (!EqualityComparer<T>.Default.Equals(span[i], value0) &&
                            !EqualityComparer<T>.Default.Equals(span[i], value1) &&
                            !EqualityComparer<T>.Default.Equals(span[i], value2)) {
                            return i;
                        }
                    }

                    return -1;
                }
            } else {
                return IndexOfAnyExceptComparer(span, value0, value1, value2, comparer);
                static int IndexOfAnyExceptComparer(ReadOnlyExSpan<T> span, T value0, T value1, T value2, IEqualityComparer<T>? comparer) {
                    comparer ??= EqualityComparer<T>.Default;
                    for (int i = 0; i < span.Length; i++) {
                        if (!comparer.Equals(span[i], value0) &&
                            !comparer.Equals(span[i], value1) &&
                            !comparer.Equals(span[i], value2)) {
                            return i;
                        }
                    }

                    return -1;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static TSize IndexOfAnyExcept<T>(this ReadOnlyExSpan<T> span, T value0, T value1, T value2, T value3) where T : IEquatable<T>? {
            if (TypeHelper.IsBitwiseEquatable<T>()) {
                if (Unsafe.SizeOf<T>() == sizeof(byte)) {
                    return ExSpanHelpers.IndexOfAnyExceptValueType(
                        ref Unsafe.As<T, byte>(ref ExMemoryMarshal.GetReference(span)),
                        ExUnsafe.BitCast<T, byte>(value0),
                        ExUnsafe.BitCast<T, byte>(value1),
                        ExUnsafe.BitCast<T, byte>(value2),
                        ExUnsafe.BitCast<T, byte>(value3),
                        span.Length);
                } else if (Unsafe.SizeOf<T>() == sizeof(short)) {
                    return ExSpanHelpers.IndexOfAnyExceptValueType(
                        ref Unsafe.As<T, short>(ref ExMemoryMarshal.GetReference(span)),
                        ExUnsafe.BitCast<T, short>(value0),
                        ExUnsafe.BitCast<T, short>(value1),
                        ExUnsafe.BitCast<T, short>(value2),
                        ExUnsafe.BitCast<T, short>(value3),
                        span.Length);
                }
            }

            return ExSpanHelpers.IndexOfAnyExcept(ref ExMemoryMarshal.GetReference(span), value0, value1, value2, value3, span.Length);
        }

        /// <summary>Searches for the first index of any value other than the specified <paramref name="values"/>.</summary>
        /// <typeparam name="T">The type of the span and values.</typeparam>
        /// <param name="span">The span to search.</param>
        /// <param name="values">The values to avoid.</param>
        /// <returns>
        /// The index in the span of the first occurrence of any value other than those in <paramref name="values"/>.
        /// If all of the values are in <paramref name="values"/>, returns -1.
        /// </returns>
        public static TSize IndexOfAnyExcept<T>(this ReadOnlyExSpan<T> span, ReadOnlyExSpan<T> values) where T : IEquatable<T>? {
            switch (values.Length) {
                case 0:
                    // If the span is empty, we want to return -1.
                    // If the span is non-empty, we want to return the index of the first char that's not in the empty set,
                    // which is every character, and so the first char in the span.
                    return span.IsEmpty ? -1 : 0;

                case 1:
                    return IndexOfAnyExcept(span, values[0]);

                case 2:
                    return IndexOfAnyExcept(span, values[0], values[1]);

                case 3:
                    return IndexOfAnyExcept(span, values[0], values[1], values[2]);

                case 4:
                    return IndexOfAnyExcept(span, values[0], values[1], values[2], values[3]);

                default:
                    if (TypeHelper.IsBitwiseEquatable<T>()) {
                        if (Unsafe.SizeOf<T>() == sizeof(byte) && values.Length == 5) {
                            ref byte valuesRef = ref Unsafe.As<T, byte>(ref ExMemoryMarshal.GetReference(values));

                            return ExSpanHelpers.IndexOfAnyExceptValueType(
                                ref Unsafe.As<T, byte>(ref ExMemoryMarshal.GetReference(span)),
                                valuesRef,
                                Unsafe.Add(ref valuesRef, 1),
                                Unsafe.Add(ref valuesRef, 2),
                                Unsafe.Add(ref valuesRef, 3),
                                Unsafe.Add(ref valuesRef, 4),
                                span.Length);
                        } else if (Unsafe.SizeOf<T>() == sizeof(short) && values.Length == 5) {
                            ref short valuesRef = ref Unsafe.As<T, short>(ref ExMemoryMarshal.GetReference(values));

                            return ExSpanHelpers.IndexOfAnyExceptValueType(
                                ref Unsafe.As<T, short>(ref ExMemoryMarshal.GetReference(span)),
                                valuesRef,
                                Unsafe.Add(ref valuesRef, 1),
                                Unsafe.Add(ref valuesRef, 2),
                                Unsafe.Add(ref valuesRef, 3),
                                Unsafe.Add(ref valuesRef, 4),
                                span.Length);
                        }
                    }

#if NET8_0_OR_GREATER && TODO // [TODO why] ProbabilisticMap is internal
                    if (TypeHelper.IsBitwiseEquatable<T>() && Unsafe.SizeOf<T>() == sizeof(char)) {
                        return ProbabilisticMap.IndexOfAnyExcept(
                            ref Unsafe.As<T, char>(ref ExMemoryMarshal.GetReference(span)),
                            span.Length,
                            ref Unsafe.As<T, char>(ref ExMemoryMarshal.GetReference(values)),
                            values.Length);
                    }
#endif // NET8_0_OR_GREATER

                    for (TSize i = 0; i < span.Length; i++) {
                        if (!values.Contains(span[i])) {
                            return i;
                        }
                    }

                    return -1;
            }
        }

        /// <summary>Searches for the first index of any value other than the specified <paramref name="values"/>.</summary>
        /// <typeparam name="T">The type of the span and values.</typeparam>
        /// <param name="span">The span to search.</param>
        /// <param name="values">The values to avoid.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default <see cref="IEqualityComparer{T}"/> for the type of an element.</param>
        /// <returns>
        /// The index in the span of the first occurrence of any value other than those in <paramref name="values"/>.
        /// If all of the values are in <paramref name="values"/>, returns -1.
        /// </returns>
        public static TSize IndexOfAnyExcept<T>(this ReadOnlyExSpan<T> span, ReadOnlyExSpan<T> values, IEqualityComparer<T>? comparer = null) {
            switch (values.Length) {
                case 0:
                    return span.IsEmpty ? -1 : 0;

                case 1:
                    return IndexOfAnyExcept(span, values[0], comparer);

                case 2:
                    return IndexOfAnyExcept(span, values[0], values[1], comparer);

                case 3:
                    return IndexOfAnyExcept(span, values[0], values[1], values[2], comparer);

                default:
                    for (TSize i = 0; i < span.Length; i++) {
                        if (!values.Contains(span[i], comparer)) {
                            return i;
                        }
                    }

                    return -1;
            }
        }

#if NET8_0_OR_GREATER && TODO // [TODO why] SearchValues.IndexOfAny is internal
        /// <summary>Searches for the first index of any value other than the specified <paramref name="values"/>.</summary>
        /// <typeparam name="T">The type of the span and values.</typeparam>
        /// <param name="span">The span to search.</param>
        /// <param name="values">The values to avoid.</param>
        /// <returns>
        /// The index in the span of the first occurrence of any value other than those in <paramref name="values"/>.
        /// If all of the values are in <paramref name="values"/>, returns -1.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSize IndexOfAnyExcept<T>(this ReadOnlyExSpan<T> span, SearchValues<T> values) where T : IEquatable<T>? {
            if (values is null) {
                throw new ArgumentNullException(nameof(values));
            }

            return values.IndexOfAnyExcept(span);
        }
#endif // NET8_0_OR_GREATER

        /// <summary>Searches for the last index of any value other than the specified <paramref name="value"/>.</summary>
        /// <typeparam name="T">The type of the span and values.</typeparam>
        /// <param name="span">The span to search.</param>
        /// <param name="value">A value to avoid.</param>
        /// <returns>
        /// The index in the span of the last occurrence of any value other than <paramref name="value"/>.
        /// If all of the values are <paramref name="value"/>, returns -1.
        /// </returns>
        [OverloadResolutionPriority(-1)]
        public static TSize LastIndexOfAnyExcept<T>(this ExSpan<T> span, T value) where T : IEquatable<T>? =>
            LastIndexOfAnyExcept((ReadOnlyExSpan<T>)span, value);

        /// <summary>Searches for the last index of any value other than the specified <paramref name="value0"/> or <paramref name="value1"/>.</summary>
        /// <typeparam name="T">The type of the span and values.</typeparam>
        /// <param name="span">The span to search.</param>
        /// <param name="value0">A value to avoid.</param>
        /// <param name="value1">A value to avoid</param>
        /// <returns>
        /// The index in the span of the last occurrence of any value other than <paramref name="value0"/> and <paramref name="value1"/>.
        /// If all of the values are <paramref name="value0"/> or <paramref name="value1"/>, returns -1.
        /// </returns>
        [OverloadResolutionPriority(-1)]
        public static TSize LastIndexOfAnyExcept<T>(this ExSpan<T> span, T value0, T value1) where T : IEquatable<T>? =>
            LastIndexOfAnyExcept((ReadOnlyExSpan<T>)span, value0, value1);

        /// <summary>Searches for the last index of any value other than the specified <paramref name="value0"/>, <paramref name="value1"/>, or <paramref name="value2"/>.</summary>
        /// <typeparam name="T">The type of the span and values.</typeparam>
        /// <param name="span">The span to search.</param>
        /// <param name="value0">A value to avoid.</param>
        /// <param name="value1">A value to avoid</param>
        /// <param name="value2">A value to avoid</param>
        /// <returns>
        /// The index in the span of the last occurrence of any value other than <paramref name="value0"/>, <paramref name="value1"/>, and <paramref name="value2"/>.
        /// If all of the values are <paramref name="value0"/>, <paramref name="value1"/>, and <paramref name="value2"/>, returns -1.
        /// </returns>
        [OverloadResolutionPriority(-1)]
        public static TSize LastIndexOfAnyExcept<T>(this ExSpan<T> span, T value0, T value1, T value2) where T : IEquatable<T>? =>
            LastIndexOfAnyExcept((ReadOnlyExSpan<T>)span, value0, value1, value2);

        /// <summary>Searches for the last index of any value other than the specified <paramref name="values"/>.</summary>
        /// <typeparam name="T">The type of the span and values.</typeparam>
        /// <param name="span">The span to search.</param>
        /// <param name="values">The values to avoid.</param>
        /// <returns>
        /// The index in the span of the last occurrence of any value other than those in <paramref name="values"/>.
        /// If all of the values are in <paramref name="values"/>, returns -1.
        /// </returns>
        [OverloadResolutionPriority(-1)]
        public static TSize LastIndexOfAnyExcept<T>(this ExSpan<T> span, ReadOnlyExSpan<T> values) where T : IEquatable<T>? =>
            LastIndexOfAnyExcept((ReadOnlyExSpan<T>)span, values);

#if NET8_0_OR_GREATER && TODO // [TODO why] SearchValues.IndexOfAny is internal
        /// <summary>Searches for the last index of any value other than the specified <paramref name="values"/>.</summary>
        /// <typeparam name="T">The type of the span and values.</typeparam>
        /// <param name="span">The span to search.</param>
        /// <param name="values">The values to avoid.</param>
        /// <returns>
        /// The index in the span of the first occurrence of any value other than those in <paramref name="values"/>.
        /// If all of the values are in <paramref name="values"/>, returns -1.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [OverloadResolutionPriority(-1)]
        public static TSize LastIndexOfAnyExcept<T>(this ExSpan<T> span, SearchValues<T> values) where T : IEquatable<T>? =>
            LastIndexOfAnyExcept((ReadOnlyExSpan<T>)span, values);
#endif // NET8_0_OR_GREATER

        /// <summary>Searches for the last index of any value other than the specified <paramref name="value"/>.</summary>
        /// <typeparam name="T">The type of the span and values.</typeparam>
        /// <param name="span">The span to search.</param>
        /// <param name="value">A value to avoid.</param>
        /// <returns>
        /// The index in the span of the last occurrence of any value other than <paramref name="value"/>.
        /// If all of the values are <paramref name="value"/>, returns -1.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSize LastIndexOfAnyExcept<T>(this ReadOnlyExSpan<T> span, T value) where T : IEquatable<T>? {
            if (TypeHelper.IsBitwiseEquatable<T>()) {
                if (Unsafe.SizeOf<T>() == sizeof(byte)) {
                    return ExSpanHelpers.LastIndexOfAnyExceptValueType(
                        ref Unsafe.As<T, byte>(ref ExMemoryMarshal.GetReference(span)),
                        ExUnsafe.BitCast<T, byte>(value),
                        span.Length);
                } else if (Unsafe.SizeOf<T>() == sizeof(short)) {
                    return ExSpanHelpers.LastIndexOfAnyExceptValueType(
                        ref Unsafe.As<T, short>(ref ExMemoryMarshal.GetReference(span)),
                        ExUnsafe.BitCast<T, short>(value),
                        span.Length);
                } else if (Unsafe.SizeOf<T>() == sizeof(int)) {
                    return ExSpanHelpers.LastIndexOfAnyExceptValueType(
                        ref Unsafe.As<T, int>(ref ExMemoryMarshal.GetReference(span)),
                        ExUnsafe.BitCast<T, int>(value),
                        span.Length);
                } else if (Unsafe.SizeOf<T>() == sizeof(long)) {
                    return ExSpanHelpers.LastIndexOfAnyExceptValueType(
                        ref Unsafe.As<T, long>(ref ExMemoryMarshal.GetReference(span)),
                        ExUnsafe.BitCast<T, long>(value),
                        span.Length);
                }
            }

            return ExSpanHelpers.LastIndexOfAnyExcept(ref ExMemoryMarshal.GetReference(span), value, span.Length);
        }

        /// <summary>Searches for the last index of any value other than the specified <paramref name="value"/>.</summary>
        /// <typeparam name="T">The type of the span and values.</typeparam>
        /// <param name="span">The span to search.</param>
        /// <param name="value">A value to avoid.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default <see cref="IEqualityComparer{T}"/> for the type of an element.</param>
        /// <returns>
        /// The index in the span of the last occurrence of any value other than <paramref name="value"/>.
        /// If all of the values are <paramref name="value"/>, returns -1.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSize LastIndexOfAnyExcept<T>(this ReadOnlyExSpan<T> span, T value, IEqualityComparer<T>? comparer = null) {
            if (TypeHelper.IsValueType<T>() && (comparer is null || comparer == EqualityComparer<T>.Default)) {
                if (TypeHelper.IsBitwiseEquatable<T>()) {
                    if (Unsafe.SizeOf<T>() == sizeof(byte)) {
                        return ExSpanHelpers.LastIndexOfAnyExceptValueType(
                            ref Unsafe.As<T, byte>(ref ExMemoryMarshal.GetReference(span)),
                            ExUnsafe.BitCast<T, byte>(value),
                            span.Length);
                    } else if (Unsafe.SizeOf<T>() == sizeof(short)) {
                        return ExSpanHelpers.LastIndexOfAnyExceptValueType(
                            ref Unsafe.As<T, short>(ref ExMemoryMarshal.GetReference(span)),
                            ExUnsafe.BitCast<T, short>(value),
                            span.Length);
                    } else if (Unsafe.SizeOf<T>() == sizeof(int)) {
                        return ExSpanHelpers.LastIndexOfAnyExceptValueType(
                            ref Unsafe.As<T, int>(ref ExMemoryMarshal.GetReference(span)),
                            ExUnsafe.BitCast<T, int>(value),
                            span.Length);
                    } else if (Unsafe.SizeOf<T>() == sizeof(long)) {
                        return ExSpanHelpers.LastIndexOfAnyExceptValueType(
                            ref Unsafe.As<T, long>(ref ExMemoryMarshal.GetReference(span)),
                            ExUnsafe.BitCast<T, long>(value),
                            span.Length);
                    }
                }

                return LastIndexOfAnyExceptDefaultComparer(span, value);
                static TSize LastIndexOfAnyExceptDefaultComparer(ReadOnlyExSpan<T> span, T value) {
                    for (TSize i = span.Length - 1; i >= 0; i--) {
                        if (!EqualityComparer<T>.Default.Equals(span[i], value)) {
                            return i;
                        }
                    }

                    return -1;
                }
            } else {
                return LastIndexOfAnyExceptComparer(span, value, comparer);
                static TSize LastIndexOfAnyExceptComparer(ReadOnlyExSpan<T> span, T value, IEqualityComparer<T>? comparer) {
                    comparer ??= EqualityComparer<T>.Default;

                    for (TSize i = span.Length - 1; i >= 0; i--) {
                        if (!comparer.Equals(span[i], value)) {
                            return i;
                        }
                    }

                    return -1;
                }
            }
        }

        /// <summary>Searches for the last index of any value other than the specified <paramref name="value0"/> or <paramref name="value1"/>.</summary>
        /// <typeparam name="T">The type of the span and values.</typeparam>
        /// <param name="span">The span to search.</param>
        /// <param name="value0">A value to avoid.</param>
        /// <param name="value1">A value to avoid</param>
        /// <returns>
        /// The index in the span of the last occurrence of any value other than <paramref name="value0"/> and <paramref name="value1"/>.
        /// If all of the values are <paramref name="value0"/> or <paramref name="value1"/>, returns -1.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSize LastIndexOfAnyExcept<T>(this ReadOnlyExSpan<T> span, T value0, T value1) where T : IEquatable<T>? {
            if (TypeHelper.IsBitwiseEquatable<T>()) {
                if (Unsafe.SizeOf<T>() == sizeof(byte)) {
                    return ExSpanHelpers.LastIndexOfAnyExceptValueType(
                        ref Unsafe.As<T, byte>(ref ExMemoryMarshal.GetReference(span)),
                        ExUnsafe.BitCast<T, byte>(value0),
                        ExUnsafe.BitCast<T, byte>(value1),
                        span.Length);
                } else if (Unsafe.SizeOf<T>() == sizeof(short)) {
                    return ExSpanHelpers.LastIndexOfAnyExceptValueType(
                        ref Unsafe.As<T, short>(ref ExMemoryMarshal.GetReference(span)),
                        ExUnsafe.BitCast<T, short>(value0),
                        ExUnsafe.BitCast<T, short>(value1),
                        span.Length);
                }
            }

            return ExSpanHelpers.LastIndexOfAnyExcept(ref ExMemoryMarshal.GetReference(span), value0, value1, span.Length);
        }

        /// <summary>Searches for the last index of any value other than the specified <paramref name="value0"/> or <paramref name="value1"/>.</summary>
        /// <typeparam name="T">The type of the span and values.</typeparam>
        /// <param name="span">The span to search.</param>
        /// <param name="value0">A value to avoid.</param>
        /// <param name="value1">A value to avoid</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default <see cref="IEqualityComparer{T}"/> for the type of an element.</param>
        /// <returns>
        /// The index in the span of the last occurrence of any value other than <paramref name="value0"/> and <paramref name="value1"/>.
        /// If all of the values are <paramref name="value0"/> or <paramref name="value1"/>, returns -1.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSize LastIndexOfAnyExcept<T>(this ReadOnlyExSpan<T> span, T value0, T value1, IEqualityComparer<T>? comparer = null) {
            if (TypeHelper.IsValueType<T>() && (comparer is null || comparer == EqualityComparer<T>.Default)) {
                if (TypeHelper.IsBitwiseEquatable<T>()) {
                    if (Unsafe.SizeOf<T>() == sizeof(byte)) {
                        return ExSpanHelpers.LastIndexOfAnyExceptValueType(
                            ref Unsafe.As<T, byte>(ref ExMemoryMarshal.GetReference(span)),
                            ExUnsafe.BitCast<T, byte>(value0),
                            ExUnsafe.BitCast<T, byte>(value1),
                            span.Length);
                    } else if (Unsafe.SizeOf<T>() == sizeof(short)) {
                        return ExSpanHelpers.LastIndexOfAnyExceptValueType(
                            ref Unsafe.As<T, short>(ref ExMemoryMarshal.GetReference(span)),
                            ExUnsafe.BitCast<T, short>(value0),
                            ExUnsafe.BitCast<T, short>(value1),
                            span.Length);
                    }
                }

                return LastIndexOfAnyExceptDefaultComparer(span, value0, value1);
                static TSize LastIndexOfAnyExceptDefaultComparer(ReadOnlyExSpan<T> span, T value0, T value1) {
                    for (TSize i = span.Length - 1; i >= 0; i--) {
                        if (!EqualityComparer<T>.Default.Equals(span[i], value0) &&
                            !EqualityComparer<T>.Default.Equals(span[i], value1)) {
                            return i;
                        }
                    }

                    return -1;
                }
            } else {
                return LastIndexOfAnyExceptComparer(span, value0, value1, comparer);
                static TSize LastIndexOfAnyExceptComparer(ReadOnlyExSpan<T> span, T value0, T value1, IEqualityComparer<T>? comparer) {
                    comparer ??= EqualityComparer<T>.Default;

                    for (TSize i = span.Length - 1; i >= 0; i--) {
                        if (!comparer.Equals(span[i], value0) &&
                            !comparer.Equals(span[i], value1)) {
                            return i;
                        }
                    }

                    return -1;
                }
            }
        }

        /// <summary>Searches for the last index of any value other than the specified <paramref name="value0"/>, <paramref name="value1"/>, or <paramref name="value2"/>.</summary>
        /// <typeparam name="T">The type of the span and values.</typeparam>
        /// <param name="span">The span to search.</param>
        /// <param name="value0">A value to avoid.</param>
        /// <param name="value1">A value to avoid</param>
        /// <param name="value2">A value to avoid</param>
        /// <returns>
        /// The index in the span of the last occurrence of any value other than <paramref name="value0"/>, <paramref name="value1"/>, and <paramref name="value2"/>.
        /// If all of the values are <paramref name="value0"/>, <paramref name="value1"/>, and <paramref name="value2"/>, returns -1.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSize LastIndexOfAnyExcept<T>(this ReadOnlyExSpan<T> span, T value0, T value1, T value2) where T : IEquatable<T>? {
            if (TypeHelper.IsBitwiseEquatable<T>()) {
                if (Unsafe.SizeOf<T>() == sizeof(byte)) {
                    return ExSpanHelpers.LastIndexOfAnyExceptValueType(
                        ref Unsafe.As<T, byte>(ref ExMemoryMarshal.GetReference(span)),
                        ExUnsafe.BitCast<T, byte>(value0),
                        ExUnsafe.BitCast<T, byte>(value1),
                        ExUnsafe.BitCast<T, byte>(value2),
                        span.Length);
                } else if (Unsafe.SizeOf<T>() == sizeof(short)) {
                    return ExSpanHelpers.LastIndexOfAnyExceptValueType(
                        ref Unsafe.As<T, short>(ref ExMemoryMarshal.GetReference(span)),
                        ExUnsafe.BitCast<T, short>(value0),
                        ExUnsafe.BitCast<T, short>(value1),
                        ExUnsafe.BitCast<T, short>(value2),
                        span.Length);
                }
            }

            return ExSpanHelpers.LastIndexOfAnyExcept(ref ExMemoryMarshal.GetReference(span), value0, value1, value2, span.Length);
        }

        /// <summary>Searches for the last index of any value other than the specified <paramref name="value0"/>, <paramref name="value1"/>, or <paramref name="value2"/>.</summary>
        /// <typeparam name="T">The type of the span and values.</typeparam>
        /// <param name="span">The span to search.</param>
        /// <param name="value0">A value to avoid.</param>
        /// <param name="value1">A value to avoid</param>
        /// <param name="value2">A value to avoid</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default <see cref="IEqualityComparer{T}"/> for the type of an element.</param>
        /// <returns>
        /// The index in the span of the last occurrence of any value other than <paramref name="value0"/>, <paramref name="value1"/>, and <paramref name="value2"/>.
        /// If all of the values are <paramref name="value0"/>, <paramref name="value1"/>, and <paramref name="value2"/>, returns -1.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSize LastIndexOfAnyExcept<T>(this ReadOnlyExSpan<T> span, T value0, T value1, T value2, IEqualityComparer<T>? comparer = null) {
            if (TypeHelper.IsValueType<T>() && (comparer is null || comparer == EqualityComparer<T>.Default)) {
                if (TypeHelper.IsBitwiseEquatable<T>()) {
                    if (Unsafe.SizeOf<T>() == sizeof(byte)) {
                        return ExSpanHelpers.LastIndexOfAnyExceptValueType(
                            ref Unsafe.As<T, byte>(ref ExMemoryMarshal.GetReference(span)),
                            ExUnsafe.BitCast<T, byte>(value0),
                            ExUnsafe.BitCast<T, byte>(value1),
                            ExUnsafe.BitCast<T, byte>(value2),
                            span.Length);
                    } else if (Unsafe.SizeOf<T>() == sizeof(short)) {
                        return ExSpanHelpers.LastIndexOfAnyExceptValueType(
                            ref Unsafe.As<T, short>(ref ExMemoryMarshal.GetReference(span)),
                            ExUnsafe.BitCast<T, short>(value0),
                            ExUnsafe.BitCast<T, short>(value1),
                            ExUnsafe.BitCast<T, short>(value2),
                            span.Length);
                    }
                }

                return LastIndexOfAnyExceptDefaultComparer(span, value0, value1, value2);
                static TSize LastIndexOfAnyExceptDefaultComparer(ReadOnlyExSpan<T> span, T value0, T value1, T value2) {
                    for (TSize i = span.Length - 1; i >= 0; i--) {
                        if (!EqualityComparer<T>.Default.Equals(span[i], value0) &&
                            !EqualityComparer<T>.Default.Equals(span[i], value1) &&
                            !EqualityComparer<T>.Default.Equals(span[i], value2)) {
                            return i;
                        }
                    }

                    return -1;
                }
            } else {
                return LastIndexOfAnyExceptComparer(span, value0, value1, value2, comparer);
                static TSize LastIndexOfAnyExceptComparer(ReadOnlyExSpan<T> span, T value0, T value1, T value2, IEqualityComparer<T>? comparer) {
                    comparer ??= EqualityComparer<T>.Default;

                    for (TSize i = span.Length - 1; i >= 0; i--) {
                        if (!comparer.Equals(span[i], value0) &&
                            !comparer.Equals(span[i], value1) &&
                            !comparer.Equals(span[i], value2)) {
                            return i;
                        }
                    }

                    return -1;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static TSize LastIndexOfAnyExcept<T>(this ReadOnlyExSpan<T> span, T value0, T value1, T value2, T value3) where T : IEquatable<T>? {
            if (TypeHelper.IsBitwiseEquatable<T>()) {
                if (Unsafe.SizeOf<T>() == sizeof(byte)) {
                    return ExSpanHelpers.LastIndexOfAnyExceptValueType(
                        ref Unsafe.As<T, byte>(ref ExMemoryMarshal.GetReference(span)),
                        ExUnsafe.BitCast<T, byte>(value0),
                        ExUnsafe.BitCast<T, byte>(value1),
                        ExUnsafe.BitCast<T, byte>(value2),
                        ExUnsafe.BitCast<T, byte>(value3),
                        span.Length);
                } else if (Unsafe.SizeOf<T>() == sizeof(short)) {
                    return ExSpanHelpers.LastIndexOfAnyExceptValueType(
                        ref Unsafe.As<T, short>(ref ExMemoryMarshal.GetReference(span)),
                        ExUnsafe.BitCast<T, short>(value0),
                        ExUnsafe.BitCast<T, short>(value1),
                        ExUnsafe.BitCast<T, short>(value2),
                        ExUnsafe.BitCast<T, short>(value3),
                        span.Length);
                }
            }

            return ExSpanHelpers.LastIndexOfAnyExcept(ref ExMemoryMarshal.GetReference(span), value0, value1, value2, value3, span.Length);
        }

        /// <summary>Searches for the last index of any value other than the specified <paramref name="values"/>.</summary>
        /// <typeparam name="T">The type of the span and values.</typeparam>
        /// <param name="span">The span to search.</param>
        /// <param name="values">The values to avoid.</param>
        /// <returns>
        /// The index in the span of the first occurrence of any value other than those in <paramref name="values"/>.
        /// If all of the values are in <paramref name="values"/>, returns -1.
        /// </returns>
        public static TSize LastIndexOfAnyExcept<T>(this ReadOnlyExSpan<T> span, ReadOnlyExSpan<T> values) where T : IEquatable<T>? {
            switch (values.Length) {
                case 0:
                    // If the span is empty, we want to return -1.
                    // If the span is non-empty, we want to return the index of the last char that's not in the empty set,
                    // which is every character, and so the last char in the span.
                    // Either way, we want to return span.Length - 1.
                    return span.Length - 1;

                case 1:
                    return LastIndexOfAnyExcept(span, values[0]);

                case 2:
                    return LastIndexOfAnyExcept(span, values[0], values[1]);

                case 3:
                    return LastIndexOfAnyExcept(span, values[0], values[1], values[2]);

                case 4:
                    return LastIndexOfAnyExcept(span, values[0], values[1], values[2], values[3]);

                default:
                    if (TypeHelper.IsBitwiseEquatable<T>()) {
                        if (Unsafe.SizeOf<T>() == sizeof(byte) && values.Length == 5) {
                            ref byte valuesRef = ref Unsafe.As<T, byte>(ref ExMemoryMarshal.GetReference(values));

                            return ExSpanHelpers.LastIndexOfAnyExceptValueType(
                                ref Unsafe.As<T, byte>(ref ExMemoryMarshal.GetReference(span)),
                                valuesRef,
                                Unsafe.Add(ref valuesRef, 1),
                                Unsafe.Add(ref valuesRef, 2),
                                Unsafe.Add(ref valuesRef, 3),
                                Unsafe.Add(ref valuesRef, 4),
                                span.Length);
                        } else if (Unsafe.SizeOf<T>() == sizeof(short) && values.Length == 5) {
                            ref short valuesRef = ref Unsafe.As<T, short>(ref ExMemoryMarshal.GetReference(values));

                            return ExSpanHelpers.LastIndexOfAnyExceptValueType(
                                ref Unsafe.As<T, short>(ref ExMemoryMarshal.GetReference(span)),
                                valuesRef,
                                Unsafe.Add(ref valuesRef, 1),
                                Unsafe.Add(ref valuesRef, 2),
                                Unsafe.Add(ref valuesRef, 3),
                                Unsafe.Add(ref valuesRef, 4),
                                span.Length);
                        }
                    }

                    //if (TypeHelper.IsBitwiseEquatable<T>() && Unsafe.SizeOf<T>() == sizeof(char)) {
                    //    return ProbabilisticMap.LastIndexOfAnyExcept(
                    //        ref Unsafe.As<T, char>(ref ExMemoryMarshal.GetReference(span)),
                    //        span.Length,
                    //        ref Unsafe.As<T, char>(ref ExMemoryMarshal.GetReference(values)),
                    //        values.Length);
                    //}

                    for (TSize i = span.Length - 1; i >= 0; i--) {
                        if (!values.Contains(span[i])) {
                            return i;
                        }
                    }

                    return -1;
            }
        }

        /// <summary>Searches for the last index of any value other than the specified <paramref name="values"/>.</summary>
        /// <typeparam name="T">The type of the span and values.</typeparam>
        /// <param name="span">The span to search.</param>
        /// <param name="values">The values to avoid.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default <see cref="IEqualityComparer{T}"/> for the type of an element.</param>
        /// <returns>
        /// The index in the span of the first occurrence of any value other than those in <paramref name="values"/>.
        /// If all of the values are in <paramref name="values"/>, returns -1.
        /// </returns>
        public static TSize LastIndexOfAnyExcept<T>(this ReadOnlyExSpan<T> span, ReadOnlyExSpan<T> values, IEqualityComparer<T>? comparer = null) {
            switch (values.Length) {
                case 0:
                    return span.Length - 1;

                case 1:
                    return LastIndexOfAnyExcept(span, values[0], comparer);

                case 2:
                    return LastIndexOfAnyExcept(span, values[0], values[1], comparer);

                case 3:
                    return LastIndexOfAnyExcept(span, values[0], values[1], values[2], comparer);

                default:
                    for (TSize i = span.Length - 1; i >= 0; i--) {
                        if (!values.Contains(span[i], comparer)) {
                            return i;
                        }
                    }

                    return -1;
            }
        }

#if NET8_0_OR_GREATER && TODO // [TODO why] SearchValues.IndexOfAny is internal
        /// <summary>Searches for the last index of any value other than the specified <paramref name="values"/>.</summary>
        /// <typeparam name="T">The type of the span and values.</typeparam>
        /// <param name="span">The span to search.</param>
        /// <param name="values">The values to avoid.</param>
        /// <returns>
        /// The index in the span of the first occurrence of any value other than those in <paramref name="values"/>.
        /// If all of the values are in <paramref name="values"/>, returns -1.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSize LastIndexOfAnyExcept<T>(this ReadOnlyExSpan<T> span, SearchValues<T> values) where T : IEquatable<T>? {
            if (values is null) {
                throw new ArgumentNullException(nameof(values));
            }

            return values.LastIndexOfAnyExcept(span);
        }
#endif // NET8_0_OR_GREATER

        /// <inheritdoc cref="IndexOfAnyInRange{T}(ReadOnlyExSpan{T}, T, T)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [OverloadResolutionPriority(-1)]
        public static TSize IndexOfAnyInRange<T>(this ExSpan<T> span, T lowInclusive, T highInclusive) where T : IComparable<T> =>
            IndexOfAnyInRange((ReadOnlyExSpan<T>)span, lowInclusive, highInclusive);

        /// <summary>Searches for the first index of any value in the range between <paramref name="lowInclusive"/> and <paramref name="highInclusive"/>, inclusive.</summary>
        /// <typeparam name="T">The type of the span and values.</typeparam>
        /// <param name="span">The span to search.</param>
        /// <param name="lowInclusive">A lower bound, inclusive, of the range for which to search.</param>
        /// <param name="highInclusive">A upper bound, inclusive, of the range for which to search.</param>
        /// <returns>
        /// The index in the span of the first occurrence of any value in the specified range.
        /// If all of the values are outside of the specified range, returns -1.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSize IndexOfAnyInRange<T>(this ReadOnlyExSpan<T> span, T lowInclusive, T highInclusive) where T : IComparable<T> {
            if (lowInclusive is null || highInclusive is null) {
                ThrowNullLowHighInclusive(lowInclusive, highInclusive);
            }

#if GENERIC_MATH
            if (Vector128.IsHardwareAccelerated) {
                if (lowInclusive is byte or sbyte) {
                    return ExSpanHelpers.IndexOfAnyInRangeUnsignedNumber(
                        ref Unsafe.As<T, byte>(ref ExMemoryMarshal.GetReference(span)),
                        ExUnsafe.BitCast<T, byte>(lowInclusive),
                        ExUnsafe.BitCast<T, byte>(highInclusive),
                        span.Length);
                }

                if (lowInclusive is short or ushort or char) {
                    return ExSpanHelpers.IndexOfAnyInRangeUnsignedNumber(
                        ref Unsafe.As<T, ushort>(ref ExMemoryMarshal.GetReference(span)),
                        ExUnsafe.BitCast<T, ushort>(lowInclusive),
                        ExUnsafe.BitCast<T, ushort>(highInclusive),
                        span.Length);
                }

                if (lowInclusive is int or uint || (IntPtr.Size == 4 && (lowInclusive is nint or nuint))) {
                    return ExSpanHelpers.IndexOfAnyInRangeUnsignedNumber(
                        ref Unsafe.As<T, uint>(ref ExMemoryMarshal.GetReference(span)),
                        ExUnsafe.BitCast<T, uint>(lowInclusive),
                        ExUnsafe.BitCast<T, uint>(highInclusive),
                        span.Length);
                }

                if (lowInclusive is long or ulong || (IntPtr.Size == 8 && (lowInclusive is nint or nuint))) {
                    return ExSpanHelpers.IndexOfAnyInRangeUnsignedNumber(
                        ref Unsafe.As<T, ulong>(ref ExMemoryMarshal.GetReference(span)),
                        ExUnsafe.BitCast<T, ulong>(lowInclusive),
                        ExUnsafe.BitCast<T, ulong>(highInclusive),
                        span.Length);
                }
            }
#endif // GENERIC_MATH

            return ExSpanHelpers.IndexOfAnyInRange(ref ExMemoryMarshal.GetReference(span), lowInclusive, highInclusive, span.Length);
        }

        /// <inheritdoc cref="IndexOfAnyExceptInRange{T}(ReadOnlyExSpan{T}, T, T)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [OverloadResolutionPriority(-1)]
        public static TSize IndexOfAnyExceptInRange<T>(this ExSpan<T> span, T lowInclusive, T highInclusive) where T : IComparable<T> =>
            IndexOfAnyExceptInRange((ReadOnlyExSpan<T>)span, lowInclusive, highInclusive);

        /// <summary>Searches for the first index of any value outside of the range between <paramref name="lowInclusive"/> and <paramref name="highInclusive"/>, inclusive.</summary>
        /// <typeparam name="T">The type of the span and values.</typeparam>
        /// <param name="span">The span to search.</param>
        /// <param name="lowInclusive">A lower bound, inclusive, of the excluded range.</param>
        /// <param name="highInclusive">A upper bound, inclusive, of the excluded range.</param>
        /// <returns>
        /// The index in the span of the first occurrence of any value outside of the specified range.
        /// If all of the values are inside of the specified range, returns -1.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSize IndexOfAnyExceptInRange<T>(this ReadOnlyExSpan<T> span, T lowInclusive, T highInclusive) where T : IComparable<T> {
            if (lowInclusive is null || highInclusive is null) {
                ThrowNullLowHighInclusive(lowInclusive, highInclusive);
            }

#if GENERIC_MATH
            if (Vector128.IsHardwareAccelerated) {
                if (lowInclusive is byte or sbyte) {
                    return ExSpanHelpers.IndexOfAnyExceptInRangeUnsignedNumber(
                        ref Unsafe.As<T, byte>(ref ExMemoryMarshal.GetReference(span)),
                        ExUnsafe.BitCast<T, byte>(lowInclusive),
                        ExUnsafe.BitCast<T, byte>(highInclusive),
                        span.Length);
                }

                if (lowInclusive is short or ushort or char) {
                    return ExSpanHelpers.IndexOfAnyExceptInRangeUnsignedNumber(
                        ref Unsafe.As<T, ushort>(ref ExMemoryMarshal.GetReference(span)),
                        ExUnsafe.BitCast<T, ushort>(lowInclusive),
                        ExUnsafe.BitCast<T, ushort>(highInclusive),
                        span.Length);
                }

                if (lowInclusive is int or uint || (IntPtr.Size == 4 && (lowInclusive is nint or nuint))) {
                    return ExSpanHelpers.IndexOfAnyExceptInRangeUnsignedNumber(
                        ref Unsafe.As<T, uint>(ref ExMemoryMarshal.GetReference(span)),
                        ExUnsafe.BitCast<T, uint>(lowInclusive),
                        ExUnsafe.BitCast<T, uint>(highInclusive),
                        span.Length);
                }

                if (lowInclusive is long or ulong || (IntPtr.Size == 8 && (lowInclusive is nint or nuint))) {
                    return ExSpanHelpers.IndexOfAnyExceptInRangeUnsignedNumber(
                        ref Unsafe.As<T, ulong>(ref ExMemoryMarshal.GetReference(span)),
                        ExUnsafe.BitCast<T, ulong>(lowInclusive),
                        ExUnsafe.BitCast<T, ulong>(highInclusive),
                        span.Length);
                }
            }
#endif // GENERIC_MATH

            return ExSpanHelpers.IndexOfAnyExceptInRange(ref ExMemoryMarshal.GetReference(span), lowInclusive, highInclusive, span.Length);
        }

        /// <inheritdoc cref="LastIndexOfAnyInRange{T}(ReadOnlyExSpan{T}, T, T)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [OverloadResolutionPriority(-1)]
        public static TSize LastIndexOfAnyInRange<T>(this ExSpan<T> span, T lowInclusive, T highInclusive) where T : IComparable<T> =>
            LastIndexOfAnyInRange((ReadOnlyExSpan<T>)span, lowInclusive, highInclusive);

        /// <summary>Searches for the last index of any value in the range between <paramref name="lowInclusive"/> and <paramref name="highInclusive"/>, inclusive.</summary>
        /// <typeparam name="T">The type of the span and values.</typeparam>
        /// <param name="span">The span to search.</param>
        /// <param name="lowInclusive">A lower bound, inclusive, of the range for which to search.</param>
        /// <param name="highInclusive">A upper bound, inclusive, of the range for which to search.</param>
        /// <returns>
        /// The index in the span of the last occurrence of any value in the specified range.
        /// If all of the values are outside of the specified range, returns -1.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSize LastIndexOfAnyInRange<T>(this ReadOnlyExSpan<T> span, T lowInclusive, T highInclusive) where T : IComparable<T> {
            if (lowInclusive is null || highInclusive is null) {
                ThrowNullLowHighInclusive(lowInclusive, highInclusive);
            }

#if GENERIC_MATH
            if (Vector.IsHardwareAccelerated) {
                if (lowInclusive is byte or sbyte) {
                    return ExSpanHelpers.LastIndexOfAnyInRangeUnsignedNumber(
                        ref Unsafe.As<T, byte>(ref ExMemoryMarshal.GetReference(span)),
                        ExUnsafe.BitCast<T, byte>(lowInclusive),
                        ExUnsafe.BitCast<T, byte>(highInclusive),
                        span.Length);
                }

                if (lowInclusive is short or ushort or char) {
                    return ExSpanHelpers.LastIndexOfAnyInRangeUnsignedNumber(
                        ref Unsafe.As<T, ushort>(ref ExMemoryMarshal.GetReference(span)),
                        ExUnsafe.BitCast<T, ushort>(lowInclusive),
                        ExUnsafe.BitCast<T, ushort>(highInclusive),
                        span.Length);
                }

                if (lowInclusive is int or uint || (IntPtr.Size == 4 && (lowInclusive is nint or nuint))) {
                    return ExSpanHelpers.LastIndexOfAnyInRangeUnsignedNumber(
                        ref Unsafe.As<T, uint>(ref ExMemoryMarshal.GetReference(span)),
                        ExUnsafe.BitCast<T, uint>(lowInclusive),
                        ExUnsafe.BitCast<T, uint>(highInclusive),
                        span.Length);
                }

                if (lowInclusive is long or ulong || (IntPtr.Size == 8 && (lowInclusive is nint or nuint))) {
                    return ExSpanHelpers.LastIndexOfAnyInRangeUnsignedNumber(
                        ref Unsafe.As<T, ulong>(ref ExMemoryMarshal.GetReference(span)),
                        ExUnsafe.BitCast<T, ulong>(lowInclusive),
                        ExUnsafe.BitCast<T, ulong>(highInclusive),
                        span.Length);
                }
            }
#endif // GENERIC_MATH

            return ExSpanHelpers.LastIndexOfAnyInRange(ref ExMemoryMarshal.GetReference(span), lowInclusive, highInclusive, span.Length);
        }

        /// <inheritdoc cref="LastIndexOfAnyExceptInRange{T}(ReadOnlyExSpan{T}, T, T)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [OverloadResolutionPriority(-1)]
        public static TSize LastIndexOfAnyExceptInRange<T>(this ExSpan<T> span, T lowInclusive, T highInclusive) where T : IComparable<T> =>
            LastIndexOfAnyExceptInRange((ReadOnlyExSpan<T>)span, lowInclusive, highInclusive);

        /// <summary>Searches for the last index of any value outside of the range between <paramref name="lowInclusive"/> and <paramref name="highInclusive"/>, inclusive.</summary>
        /// <typeparam name="T">The type of the span and values.</typeparam>
        /// <param name="span">The span to search.</param>
        /// <param name="lowInclusive">A lower bound, inclusive, of the excluded range.</param>
        /// <param name="highInclusive">A upper bound, inclusive, of the excluded range.</param>
        /// <returns>
        /// The index in the span of the last occurrence of any value outside of the specified range.
        /// If all of the values are inside of the specified range, returns -1.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSize LastIndexOfAnyExceptInRange<T>(this ReadOnlyExSpan<T> span, T lowInclusive, T highInclusive) where T : IComparable<T> {
            if (lowInclusive is null || highInclusive is null) {
                ThrowNullLowHighInclusive(lowInclusive, highInclusive);
            }

#if GENERIC_MATH
            if (Vector.IsHardwareAccelerated) {
                if (lowInclusive is byte or sbyte) {
                    return ExSpanHelpers.LastIndexOfAnyExceptInRangeUnsignedNumber(
                        ref Unsafe.As<T, byte>(ref ExMemoryMarshal.GetReference(span)),
                        ExUnsafe.BitCast<T, byte>(lowInclusive),
                        ExUnsafe.BitCast<T, byte>(highInclusive),
                        span.Length);
                }

                if (lowInclusive is short or ushort or char) {
                    return ExSpanHelpers.LastIndexOfAnyExceptInRangeUnsignedNumber(
                        ref Unsafe.As<T, ushort>(ref ExMemoryMarshal.GetReference(span)),
                        ExUnsafe.BitCast<T, ushort>(lowInclusive),
                        ExUnsafe.BitCast<T, ushort>(highInclusive),
                        span.Length);
                }

                if (lowInclusive is int or uint || (IntPtr.Size == 4 && (lowInclusive is nint or nuint))) {
                    return ExSpanHelpers.LastIndexOfAnyExceptInRangeUnsignedNumber(
                        ref Unsafe.As<T, uint>(ref ExMemoryMarshal.GetReference(span)),
                        ExUnsafe.BitCast<T, uint>(lowInclusive),
                        ExUnsafe.BitCast<T, uint>(highInclusive),
                        span.Length);
                }

                if (lowInclusive is long or ulong || (IntPtr.Size == 8 && (lowInclusive is nint or nuint))) {
                    return ExSpanHelpers.LastIndexOfAnyExceptInRangeUnsignedNumber(
                        ref Unsafe.As<T, ulong>(ref ExMemoryMarshal.GetReference(span)),
                        ExUnsafe.BitCast<T, ulong>(lowInclusive),
                        ExUnsafe.BitCast<T, ulong>(highInclusive),
                        span.Length);
                }
            }
#endif // GENERIC_MATH

            return ExSpanHelpers.LastIndexOfAnyExceptInRange(ref ExMemoryMarshal.GetReference(span), lowInclusive, highInclusive, span.Length);
        }

        /// <summary>Throws an <see cref="ArgumentNullException"/> for <paramref name="lowInclusive"/> or <paramref name="highInclusive"/> being null.</summary>
        [DoesNotReturn]
        private static void ThrowNullLowHighInclusive<T>(T? lowInclusive, T? highInclusive) {
            Debug.Assert(lowInclusive is null || highInclusive is null);
            throw new ArgumentNullException(lowInclusive is null ? nameof(lowInclusive) : nameof(highInclusive));
        }

        /// <summary>
        /// Determines whether a span and a read-only span are equal by comparing the elements using <see cref="IEquatable{T}.Equals(T)"/> (通过使用 <see cref="IEquatable{T}.Equals(T)"/> 比较元素, 确定跨度和只读跨度是否相等).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="span">The first sequence to compare (要比较的第一个序列).</param>
        /// <param name="other">The second sequence to compare (要比较的第二个序列).</param>
        /// <returns>true if the two sequences are equal; otherwise, false (如果两个序列相等, 则 true; 否则 false).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [OverloadResolutionPriority(-1)]
        public static bool SequenceEqual<T>(this ExSpan<T> span, ReadOnlyExSpan<T> other) where T : IEquatable<T>? =>
            SequenceEqual((ReadOnlyExSpan<T>)span, other);

        /// <summary>
        /// Determines the relative order of the sequences being compared by comparing the elements using IComparable{T}.CompareTo(T).
        /// </summary>
        [OverloadResolutionPriority(-1)]
        public static int SequenceCompareTo<T>(this ExSpan<T> span, ReadOnlyExSpan<T> other) where T : IComparable<T>? =>
            SequenceCompareTo((ReadOnlyExSpan<T>)span, other);

        /// <summary>
        /// Searches for the specified value and returns the index of its first occurrence. If not found, returns -1. Values are compared using IEquatable{T}.Equals(T).
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="value">The value to search for.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSize IndexOf<T>(this ReadOnlyExSpan<T> span, T value) where T : IEquatable<T>? {
            if (TypeHelper.IsBitwiseEquatable<T>()) {
                if (Unsafe.SizeOf<T>() == sizeof(byte))
                    return ExSpanHelpers.IndexOfValueType(
                        ref Unsafe.As<T, byte>(ref ExMemoryMarshal.GetReference(span)),
                        ExUnsafe.BitCast<T, byte>(value),
                        span.Length);

                if (Unsafe.SizeOf<T>() == sizeof(short))
                    return ExSpanHelpers.IndexOfValueType(
                        ref Unsafe.As<T, short>(ref ExMemoryMarshal.GetReference(span)),
                        ExUnsafe.BitCast<T, short>(value),
                        span.Length);

                if (Unsafe.SizeOf<T>() == sizeof(int))
                    return ExSpanHelpers.IndexOfValueType(
                        ref Unsafe.As<T, int>(ref ExMemoryMarshal.GetReference(span)),
                        ExUnsafe.BitCast<T, int>(value),
                        span.Length);

                if (Unsafe.SizeOf<T>() == sizeof(long))
                    return ExSpanHelpers.IndexOfValueType(
                        ref Unsafe.As<T, long>(ref ExMemoryMarshal.GetReference(span)),
                        ExUnsafe.BitCast<T, long>(value),
                        span.Length);
            }

            return ExSpanHelpers.IndexOf(ref ExMemoryMarshal.GetReference(span), value, span.Length);
        }

        /// <summary>
        /// Searches for the specified value and returns the index of its first occurrence. If not found, returns -1. Values are compared using IEquatable{T}.Equals(T).
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="value">The value to search for.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default <see cref="IEqualityComparer{T}"/> for the type of an element.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSize IndexOf<T>(this ReadOnlyExSpan<T> span, T value, IEqualityComparer<T>? comparer = null) {
            if (TypeHelper.IsValueType<T>() && (comparer is null || comparer == EqualityComparer<T>.Default)) {
                if (TypeHelper.IsBitwiseEquatable<T>()) {
                    if (Unsafe.SizeOf<T>() == sizeof(byte))
                        return ExSpanHelpers.IndexOfValueType(
                            ref Unsafe.As<T, byte>(ref ExMemoryMarshal.GetReference(span)),
                            ExUnsafe.BitCast<T, byte>(value),
                            span.Length);

                    if (Unsafe.SizeOf<T>() == sizeof(short))
                        return ExSpanHelpers.IndexOfValueType(
                            ref Unsafe.As<T, short>(ref ExMemoryMarshal.GetReference(span)),
                            ExUnsafe.BitCast<T, short>(value),
                            span.Length);

                    if (Unsafe.SizeOf<T>() == sizeof(int))
                        return ExSpanHelpers.IndexOfValueType(
                            ref Unsafe.As<T, int>(ref ExMemoryMarshal.GetReference(span)),
                            ExUnsafe.BitCast<T, int>(value),
                            span.Length);

                    if (Unsafe.SizeOf<T>() == sizeof(long))
                        return ExSpanHelpers.IndexOfValueType(
                            ref Unsafe.As<T, long>(ref ExMemoryMarshal.GetReference(span)),
                            ExUnsafe.BitCast<T, long>(value),
                            span.Length);
                }

                return IndexOfDefaultComparer(span, value);
                static TSize IndexOfDefaultComparer(ReadOnlyExSpan<T> span, T value) {
                    for (TSize i = (TSize)0; i < span.Length; i += 1) {
                        if (EqualityComparer<T>.Default.Equals(span[i], value)) {
                            return i;
                        }
                    }

                    return (TSize)(int)-1;
                }
            } else {
                return IndexOfComparer(span, value, comparer);
                static TSize IndexOfComparer(ReadOnlyExSpan<T> span, T value, IEqualityComparer<T>? comparer) {
                    comparer ??= EqualityComparer<T>.Default;
                    for (TSize i = (TSize)0; i < span.Length; i += 1) {
                        if (comparer.Equals(span[i], value)) {
                            return i;
                        }
                    }

                    return (TSize)(int)-1;
                }
            }
        }

        /// <summary>
        /// Searches for the specified sequence and returns the index of its first occurrence. If not found, returns -1. Values are compared using IEquatable{T}.Equals(T).
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="value">The sequence to search for.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSize IndexOf<T>(this ReadOnlyExSpan<T> span, ReadOnlyExSpan<T> value) where T : IEquatable<T>? {
            if (TypeHelper.IsBitwiseEquatable<T>()) {
                if (Unsafe.SizeOf<T>() == sizeof(byte))
                    return ExSpanHelpers.IndexOf(
                        ref Unsafe.As<T, byte>(ref ExMemoryMarshal.GetReference(span)),
                        span.Length,
                        ref Unsafe.As<T, byte>(ref ExMemoryMarshal.GetReference(value)),
                        value.Length);

                if (Unsafe.SizeOf<T>() == sizeof(char))
                    return ExSpanHelpers.IndexOf(
                        ref Unsafe.As<T, char>(ref ExMemoryMarshal.GetReference(span)),
                        span.Length,
                        ref Unsafe.As<T, char>(ref ExMemoryMarshal.GetReference(value)),
                        value.Length);
            }

            return ExSpanHelpers.IndexOf(ref ExMemoryMarshal.GetReference(span), span.Length, ref ExMemoryMarshal.GetReference(value), value.Length);
        }

        /// <summary>
        /// Searches for the specified sequence and returns the index of its first occurrence. If not found, returns -1. Values are compared using IEquatable{T}.Equals(T).
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="value">The sequence to search for.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default <see cref="IEqualityComparer{T}"/> for the type of an element.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSize IndexOf<T>(this ReadOnlyExSpan<T> span, ReadOnlyExSpan<T> value, IEqualityComparer<T>? comparer = null) {
            if (TypeHelper.IsBitwiseEquatable<T>() && (comparer is null || comparer == EqualityComparer<T>.Default)) {
                if (Unsafe.SizeOf<T>() == sizeof(byte))
                    return ExSpanHelpers.IndexOf(
                        ref Unsafe.As<T, byte>(ref ExMemoryMarshal.GetReference(span)),
                        span.Length,
                        ref Unsafe.As<T, byte>(ref ExMemoryMarshal.GetReference(value)),
                        value.Length);

                if (Unsafe.SizeOf<T>() == sizeof(char))
                    return ExSpanHelpers.IndexOf(
                        ref Unsafe.As<T, char>(ref ExMemoryMarshal.GetReference(span)),
                        span.Length,
                        ref Unsafe.As<T, char>(ref ExMemoryMarshal.GetReference(value)),
                        value.Length);
            }

            return IndexOfComparer(span, value, comparer);
            static TSize IndexOfComparer(ReadOnlyExSpan<T> span, ReadOnlyExSpan<T> value, IEqualityComparer<T>? comparer) {
                if (value.Length == (TSize)0) {
                    return (TSize)0;
                }

                comparer ??= EqualityComparer<T>.Default;

                TSize total = (TSize)0;
                while (!span.IsEmpty) {
                    TSize pos = span.IndexOf(value[(TSize)0], comparer);
                    if (pos < 0) {
                        break;
                    }

                    if (span.Slice(pos + 1).StartsWith(value.Slice((TSize)1), comparer)) {
                        return total + pos;
                    }

                    total = total + pos + 1;
                    span = span.Slice(pos + 1);
                }

                return (TSize)(int)-1;
            }
        }

        /// <summary>
        /// Searches for the specified value and returns the index of its last occurrence. If not found, returns -1. Values are compared using IEquatable{T}.Equals(T).
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="value">The value to search for.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSize LastIndexOf<T>(this ReadOnlyExSpan<T> span, T value) where T : IEquatable<T>? {
            if (TypeHelper.IsBitwiseEquatable<T>()) {
                if (Unsafe.SizeOf<T>() == sizeof(byte)) {
                    return ExSpanHelpers.LastIndexOfValueType(
                        ref Unsafe.As<T, byte>(ref ExMemoryMarshal.GetReference(span)),
                        ExUnsafe.BitCast<T, byte>(value),
                        span.Length);
                } else if (Unsafe.SizeOf<T>() == sizeof(short)) {
                    return ExSpanHelpers.LastIndexOfValueType(
                        ref Unsafe.As<T, short>(ref ExMemoryMarshal.GetReference(span)),
                        ExUnsafe.BitCast<T, short>(value),
                        span.Length);
                } else if (Unsafe.SizeOf<T>() == sizeof(int)) {
                    return ExSpanHelpers.LastIndexOfValueType(
                        ref Unsafe.As<T, int>(ref ExMemoryMarshal.GetReference(span)),
                        ExUnsafe.BitCast<T, int>(value),
                        span.Length);
                } else if (Unsafe.SizeOf<T>() == sizeof(long)) {
                    return ExSpanHelpers.LastIndexOfValueType(
                        ref Unsafe.As<T, long>(ref ExMemoryMarshal.GetReference(span)),
                        ExUnsafe.BitCast<T, long>(value),
                        span.Length);
                }
            }

            return ExSpanHelpers.LastIndexOf(ref ExMemoryMarshal.GetReference(span), value, span.Length);
        }

        /// <summary>
        /// Searches for the specified value and returns the index of its last occurrence. If not found, returns -1. Values are compared using IEquatable{T}.Equals(T).
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="value">The value to search for.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default <see cref="IEqualityComparer{T}"/> for the type of an element.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSize LastIndexOf<T>(this ReadOnlyExSpan<T> span, T value, IEqualityComparer<T>? comparer = null) {
            if (TypeHelper.IsValueType<T>() && (comparer is null || comparer == EqualityComparer<T>.Default)) {
                if (TypeHelper.IsBitwiseEquatable<T>()) {
                    if (Unsafe.SizeOf<T>() == sizeof(byte)) {
                        return ExSpanHelpers.LastIndexOfValueType(
                            ref Unsafe.As<T, byte>(ref ExMemoryMarshal.GetReference(span)),
                            ExUnsafe.BitCast<T, byte>(value),
                            span.Length);
                    } else if (Unsafe.SizeOf<T>() == sizeof(short)) {
                        return ExSpanHelpers.LastIndexOfValueType(
                            ref Unsafe.As<T, short>(ref ExMemoryMarshal.GetReference(span)),
                            ExUnsafe.BitCast<T, short>(value),
                            span.Length);
                    } else if (Unsafe.SizeOf<T>() == sizeof(int)) {
                        return ExSpanHelpers.LastIndexOfValueType(
                            ref Unsafe.As<T, int>(ref ExMemoryMarshal.GetReference(span)),
                            ExUnsafe.BitCast<T, int>(value),
                            span.Length);
                    } else if (Unsafe.SizeOf<T>() == sizeof(long)) {
                        return ExSpanHelpers.LastIndexOfValueType(
                            ref Unsafe.As<T, long>(ref ExMemoryMarshal.GetReference(span)),
                            ExUnsafe.BitCast<T, long>(value),
                            span.Length);
                    }
                }

                return LastIndexOfDefaultComparer(span, value);
                static TSize LastIndexOfDefaultComparer(ReadOnlyExSpan<T> span, T value) {
                    for (TSize i = span.Length - 1; i >= 0; i--) {
                        if (EqualityComparer<T>.Default.Equals(span[i], value)) {
                            return i;
                        }
                    }

                    return -1;
                }
            } else {
                return LastIndexOfComparer(span, value, comparer);
                static TSize LastIndexOfComparer(ReadOnlyExSpan<T> span, T value, IEqualityComparer<T>? comparer) {
                    comparer ??= EqualityComparer<T>.Default;

                    for (TSize i = span.Length - 1; i >= 0; i--) {
                        if (comparer.Equals(span[i], value)) {
                            return i;
                        }
                    }

                    return -1;
                }
            }
        }

        /// <summary>
        /// Searches for the specified sequence and returns the index of its last occurrence. If not found, returns -1. Values are compared using IEquatable{T}.Equals(T).
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="value">The sequence to search for.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSize LastIndexOf<T>(this ReadOnlyExSpan<T> span, ReadOnlyExSpan<T> value) where T : IEquatable<T>? {
            if (TypeHelper.IsBitwiseEquatable<T>()) {
                if (Unsafe.SizeOf<T>() == sizeof(byte)) {
                    return ExSpanHelpers.LastIndexOf(
                        ref Unsafe.As<T, byte>(ref ExMemoryMarshal.GetReference(span)),
                        span.Length,
                        ref Unsafe.As<T, byte>(ref ExMemoryMarshal.GetReference(value)),
                        value.Length);
                }
                if (Unsafe.SizeOf<T>() == sizeof(char)) {
                    return ExSpanHelpers.LastIndexOf(
                        ref Unsafe.As<T, char>(ref ExMemoryMarshal.GetReference(span)),
                        span.Length,
                        ref Unsafe.As<T, char>(ref ExMemoryMarshal.GetReference(value)),
                        value.Length);
                }
            }

            return ExSpanHelpers.LastIndexOf(ref ExMemoryMarshal.GetReference(span), span.Length, ref ExMemoryMarshal.GetReference(value), value.Length);
        }

        /// <summary>
        /// Searches for the specified sequence and returns the index of its last occurrence. If not found, returns -1. Values are compared using IEquatable{T}.Equals(T).
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="value">The sequence to search for.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default <see cref="IEqualityComparer{T}"/> for the type of an element.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSize LastIndexOf<T>(this ReadOnlyExSpan<T> span, ReadOnlyExSpan<T> value, IEqualityComparer<T>? comparer = null) {
            if (TypeHelper.IsBitwiseEquatable<T>()) {
                if (Unsafe.SizeOf<T>() == sizeof(byte)) {
                    return ExSpanHelpers.LastIndexOf(
                        ref Unsafe.As<T, byte>(ref ExMemoryMarshal.GetReference(span)),
                        span.Length,
                        ref Unsafe.As<T, byte>(ref ExMemoryMarshal.GetReference(value)),
                        value.Length);
                }
                if (Unsafe.SizeOf<T>() == sizeof(char)) {
                    return ExSpanHelpers.LastIndexOf(
                        ref Unsafe.As<T, char>(ref ExMemoryMarshal.GetReference(span)),
                        span.Length,
                        ref Unsafe.As<T, char>(ref ExMemoryMarshal.GetReference(value)),
                        value.Length);
                }
            }

            return LastIndexOfComparer(span, value, comparer);
            static TSize LastIndexOfComparer(ReadOnlyExSpan<T> span, ReadOnlyExSpan<T> value, IEqualityComparer<T>? comparer) {
                if (value.Length == 0) {
                    return span.Length;
                }

                comparer ??= EqualityComparer<T>.Default;

                TSize pos = span.Length;
                while (true) {
                    pos = span.Slice(0, pos).LastIndexOf(value[0], comparer);
                    if (pos < 0) {
                        break;
                    }

                    if (span.Slice(pos + 1).StartsWith(value.Slice(1), comparer)) {
                        return pos;
                    }
                }

                return -1;
            }
        }

        /// <summary>
        /// Searches for the first index of any of the specified values similar to calling IndexOf several times with the logical OR operator. If not found, returns -1.
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="value0">One of the values to search for.</param>
        /// <param name="value1">One of the values to search for.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [OverloadResolutionPriority(-1)]
        public static TSize IndexOfAny<T>(this ExSpan<T> span, T value0, T value1) where T : IEquatable<T>? =>
            IndexOfAny((ReadOnlyExSpan<T>)span, value0, value1);

        /// <summary>
        /// Searches for the first index of any of the specified values similar to calling IndexOf several times with the logical OR operator. If not found, returns -1.
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="value0">One of the values to search for.</param>
        /// <param name="value1">One of the values to search for.</param>
        /// <param name="value2">One of the values to search for.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [OverloadResolutionPriority(-1)]
        public static TSize IndexOfAny<T>(this ExSpan<T> span, T value0, T value1, T value2) where T : IEquatable<T>? =>
            IndexOfAny((ReadOnlyExSpan<T>)span, value0, value1, value2);

        /// <summary>
        /// Searches for the first index of any of the specified values similar to calling IndexOf several times with the logical OR operator. If not found, returns -1.
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="values">The set of values to search for.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [OverloadResolutionPriority(-1)]
        public static TSize IndexOfAny<T>(this ExSpan<T> span, ReadOnlyExSpan<T> values) where T : IEquatable<T>? =>
            IndexOfAny((ReadOnlyExSpan<T>)span, values);

#if NET8_0_OR_GREATER && TODO // [TODO why] SearchValues.IndexOfAny is internal
        /// <summary>
        /// Searches for the first index of any of the specified values similar to calling IndexOf several times with the logical OR operator. If not found, returns -1.
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="values">The set of values to search for.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [OverloadResolutionPriority(-1)]
        public static TSize IndexOfAny<T>(this ExSpan<T> span, SearchValues<T> values) where T : IEquatable<T>? =>
            IndexOfAny((ReadOnlyExSpan<T>)span, values);

        /// <summary>
        /// Searches for the first index of any of the specified substring values.
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="values">The set of values to search for.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [OverloadResolutionPriority(-1)]
        public static TSize IndexOfAny(this ExSpan<char> span, SearchValues<string> values) =>
            IndexOfAny((ReadOnlyExSpan<char>)span, values);
#endif // NET8_0_OR_GREATER

        /// <summary>
        /// Searches for the first index of any of the specified values similar to calling IndexOf several times with the logical OR operator. If not found, returns -1.
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="value0">One of the values to search for.</param>
        /// <param name="value1">One of the values to search for.</param>
#if NET7_0_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)] // Bug on before .net 7.0: Unit test AIndexOfAny.MakeSureNoChecksGoOutOfRangeTwo_Char and more run fail on Release. The _byteOffset field value is wrong.
#else
        [MethodImpl(MethodImplOptions.NoInlining)]
#endif // NET7_0_OR_GREATER
        public static TSize IndexOfAny<T>(this ReadOnlyExSpan<T> span, T value0, T value1) where T : IEquatable<T>? {
            if (TypeHelper.IsBitwiseEquatable<T>()) {
                if (Unsafe.SizeOf<T>() == sizeof(byte)) {
                    return ExSpanHelpers.IndexOfAnyValueType(
                        ref Unsafe.As<T, byte>(ref ExMemoryMarshal.GetReference(span)),
                        ExUnsafe.BitCast<T, byte>(value0),
                        ExUnsafe.BitCast<T, byte>(value1),
                        span.Length);
                } else if (Unsafe.SizeOf<T>() == sizeof(short)) {
                    return ExSpanHelpers.IndexOfAnyValueType(
                        ref Unsafe.As<T, short>(ref ExMemoryMarshal.GetReference(span)),
                        ExUnsafe.BitCast<T, short>(value0),
                        ExUnsafe.BitCast<T, short>(value1),
                        span.Length);
                }
            }

            return ExSpanHelpers.IndexOfAny(ref ExMemoryMarshal.GetReference(span), value0, value1, span.Length);
        }

        /// <summary>
        /// Searches for the first index of any of the specified values similar to calling IndexOf several times with the logical OR operator. If not found, returns -1.
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="value0">One of the values to search for.</param>
        /// <param name="value1">One of the values to search for.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default <see cref="IEqualityComparer{T}"/> for the type of an element.</param>
#if NET7_0_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)] // Bug on before .net 7.0: Unit test AIndexOfAny.MakeSureNoChecksGoOutOfRangeTwo_Char and more run fail on Release. The _byteOffset field value is wrong.
#else
        [MethodImpl(MethodImplOptions.NoInlining)]
#endif // NET7_0_OR_GREATER
        public static TSize IndexOfAny<T>(this ReadOnlyExSpan<T> span, T value0, T value1, IEqualityComparer<T>? comparer = null) {
            if (TypeHelper.IsValueType<T>() && (comparer is null || comparer == EqualityComparer<T>.Default)) {
                if (TypeHelper.IsBitwiseEquatable<T>()) {
                    if (Unsafe.SizeOf<T>() == sizeof(byte)) {
                        return ExSpanHelpers.IndexOfAnyValueType(
                            ref Unsafe.As<T, byte>(ref ExMemoryMarshal.GetReference(span)),
                            ExUnsafe.BitCast<T, byte>(value0),
                            ExUnsafe.BitCast<T, byte>(value1),
                            span.Length);
                    } else if (Unsafe.SizeOf<T>() == sizeof(short)) {
                        return ExSpanHelpers.IndexOfAnyValueType(
                            ref Unsafe.As<T, short>(ref ExMemoryMarshal.GetReference(span)),
                            ExUnsafe.BitCast<T, short>(value0),
                            ExUnsafe.BitCast<T, short>(value1),
                            span.Length);
                    }
                }

                return IndexOfAnyDefaultComparer(span, value0, value1);
                static TSize IndexOfAnyDefaultComparer(ReadOnlyExSpan<T> span, T value0, T value1) {
                    for (TSize i = 0; i < span.Length; i++) {
                        if (EqualityComparer<T>.Default.Equals(span[i], value0) ||
                            EqualityComparer<T>.Default.Equals(span[i], value1)) {
                            return i;
                        }
                    }

                    return -1;
                }
            } else {
                return IndexOfAnyComparer(span, value0, value1, comparer);
                static TSize IndexOfAnyComparer(ReadOnlyExSpan<T> span, T value0, T value1, IEqualityComparer<T>? comparer) {
                    comparer ??= EqualityComparer<T>.Default;
                    for (TSize i = 0; i < span.Length; i++) {
                        if (comparer.Equals(span[i], value0) ||
                            comparer.Equals(span[i], value1)) {
                            return i;
                        }
                    }

                    return -1;
                }
            }
        }

        /// <summary>
        /// Searches for the first index of any of the specified values similar to calling IndexOf several times with the logical OR operator. If not found, returns -1.
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="value0">One of the values to search for.</param>
        /// <param name="value1">One of the values to search for.</param>
        /// <param name="value2">One of the values to search for.</param>
#if NET7_0_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)] // Bug on before .net 7.0: Unit test AIndexOfAny.MakeSureNoChecksGoOutOfRangeTwo_Char and more run fail on Release. The _byteOffset field value is wrong.
#else
        [MethodImpl(MethodImplOptions.NoInlining)]
#endif // NET7_0_OR_GREATER
        public static TSize IndexOfAny<T>(this ReadOnlyExSpan<T> span, T value0, T value1, T value2) where T : IEquatable<T>? {
            if (TypeHelper.IsBitwiseEquatable<T>()) {
                if (Unsafe.SizeOf<T>() == sizeof(byte)) {
                    return ExSpanHelpers.IndexOfAnyValueType(
                        ref Unsafe.As<T, byte>(ref ExMemoryMarshal.GetReference(span)),
                        ExUnsafe.BitCast<T, byte>(value0),
                        ExUnsafe.BitCast<T, byte>(value1),
                        ExUnsafe.BitCast<T, byte>(value2),
                        span.Length);
                } else if (Unsafe.SizeOf<T>() == sizeof(short)) {
                    return ExSpanHelpers.IndexOfAnyValueType(
                        ref Unsafe.As<T, short>(ref ExMemoryMarshal.GetReference(span)),
                        ExUnsafe.BitCast<T, short>(value0),
                        ExUnsafe.BitCast<T, short>(value1),
                        ExUnsafe.BitCast<T, short>(value2),
                        span.Length);
                }
            }

            return ExSpanHelpers.IndexOfAny(ref ExMemoryMarshal.GetReference(span), value0, value1, value2, span.Length);
        }

        /// <summary>
        /// Searches for the first index of any of the specified values similar to calling IndexOf several times with the logical OR operator. If not found, returns -1.
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="value0">One of the values to search for.</param>
        /// <param name="value1">One of the values to search for.</param>
        /// <param name="value2">One of the values to search for.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default <see cref="IEqualityComparer{T}"/> for the type of an element.</param>
#if NET7_0_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)] // Bug on before .net 7.0: Unit test AIndexOfAny.MakeSureNoChecksGoOutOfRangeTwo_Char and more run fail on Release. The _byteOffset field value is wrong.
#else
        [MethodImpl(MethodImplOptions.NoInlining)]
#endif // NET7_0_OR_GREATER
        public static TSize IndexOfAny<T>(this ReadOnlyExSpan<T> span, T value0, T value1, T value2, IEqualityComparer<T>? comparer = null) {
            if (TypeHelper.IsValueType<T>() && (comparer is null || comparer == EqualityComparer<T>.Default)) {
                if (TypeHelper.IsBitwiseEquatable<T>()) {
                    if (Unsafe.SizeOf<T>() == sizeof(byte)) {
                        return ExSpanHelpers.IndexOfAnyValueType(
                            ref Unsafe.As<T, byte>(ref ExMemoryMarshal.GetReference(span)),
                            ExUnsafe.BitCast<T, byte>(value0),
                            ExUnsafe.BitCast<T, byte>(value1),
                            ExUnsafe.BitCast<T, byte>(value2),
                            span.Length);
                    } else if (Unsafe.SizeOf<T>() == sizeof(short)) {
                        return ExSpanHelpers.IndexOfAnyValueType(
                            ref Unsafe.As<T, short>(ref ExMemoryMarshal.GetReference(span)),
                            ExUnsafe.BitCast<T, short>(value0),
                            ExUnsafe.BitCast<T, short>(value1),
                            ExUnsafe.BitCast<T, short>(value2),
                            span.Length);
                    }
                }

                return IndexOfAnyDefaultComparer(span, value0, value1, value2);
                static TSize IndexOfAnyDefaultComparer(ReadOnlyExSpan<T> span, T value0, T value1, T value2) {
                    for (TSize i = 0; i < span.Length; i++) {
                        if (EqualityComparer<T>.Default.Equals(span[i], value0) ||
                            EqualityComparer<T>.Default.Equals(span[i], value1) ||
                            EqualityComparer<T>.Default.Equals(span[i], value2)) {
                            return i;
                        }
                    }

                    return -1;
                }
            } else {
                return IndexOfAnyComparer(span, value0, value1, value2, comparer);
                static TSize IndexOfAnyComparer(ReadOnlyExSpan<T> span, T value0, T value1, T value2, IEqualityComparer<T>? comparer) {
                    comparer ??= EqualityComparer<T>.Default;
                    for (TSize i = 0; i < span.Length; i++) {
                        if (comparer.Equals(span[i], value0) ||
                            comparer.Equals(span[i], value1) ||
                            comparer.Equals(span[i], value2)) {
                            return i;
                        }
                    }

                    return -1;
                }
            }
        }

        /// <summary>
        /// Searches for the first index of any of the specified values similar to calling IndexOf several times with the logical OR operator. If not found, returns -1.
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="values">The set of values to search for.</param>
#if NET7_0_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)] // Bug on before .net 7.0: Unit test AIndexOfAny.MakeSureNoChecksGoOutOfRangeTwo_Char and more run fail on Release. The _byteOffset field value is wrong.
#else
        [MethodImpl(MethodImplOptions.NoInlining)]
#endif // NET7_0_OR_GREATER
        public static TSize IndexOfAny<T>(this ReadOnlyExSpan<T> span, ReadOnlyExSpan<T> values) where T : IEquatable<T>? {
            if (TypeHelper.IsBitwiseEquatable<T>()) {
                if (Unsafe.SizeOf<T>() == sizeof(byte)) {
                    ref byte spanRef = ref Unsafe.As<T, byte>(ref ExMemoryMarshal.GetReference(span));
                    ref byte valueRef = ref Unsafe.As<T, byte>(ref ExMemoryMarshal.GetReference(values));
                    switch (values.Length) {
                        case 0:
                            return -1;

                        case 1:
                            return ExSpanHelpers.IndexOfValueType(ref spanRef, valueRef, span.Length);

                        case 2:
                            return ExSpanHelpers.IndexOfAnyValueType(
                                ref spanRef,
                                valueRef,
                                Unsafe.Add(ref valueRef, 1),
                                span.Length);

                        case 3:
                            return ExSpanHelpers.IndexOfAnyValueType(
                                ref spanRef,
                                valueRef,
                                Unsafe.Add(ref valueRef, 1),
                                Unsafe.Add(ref valueRef, 2),
                                span.Length);

                        case 4:
                            return ExSpanHelpers.IndexOfAnyValueType(
                                ref spanRef,
                                valueRef,
                                Unsafe.Add(ref valueRef, 1),
                                Unsafe.Add(ref valueRef, 2),
                                Unsafe.Add(ref valueRef, 3),
                                span.Length);

                        case 5:
                            return ExSpanHelpers.IndexOfAnyValueType(
                                ref spanRef,
                                valueRef,
                                Unsafe.Add(ref valueRef, 1),
                                Unsafe.Add(ref valueRef, 2),
                                Unsafe.Add(ref valueRef, 3),
                                Unsafe.Add(ref valueRef, 4),
                                span.Length);

                        case 6:
                            return ExSpanHelpers.IndexOfAnyValueType(
                                ref spanRef,
                                valueRef,
                                Unsafe.Add(ref valueRef, 1),
                                Unsafe.Add(ref valueRef, 2),
                                Unsafe.Add(ref valueRef, 3),
                                Unsafe.Add(ref valueRef, 4),
                                Unsafe.Add(ref valueRef, 5),
                                span.Length);
                    }
                }

                if (Unsafe.SizeOf<T>() == sizeof(short) && values.Length<=6) {
                    ref short spanRef = ref Unsafe.As<T, short>(ref ExMemoryMarshal.GetReference(span));
                    ref short valueRef = ref Unsafe.As<T, short>(ref ExMemoryMarshal.GetReference(values));
                    return values.Length switch {
                        0 => -1,
                        1 => ExSpanHelpers.IndexOfValueType(ref spanRef, valueRef, span.Length),
                        2 => ExSpanHelpers.IndexOfAnyValueType(
                                ref spanRef,
                                valueRef,
                                Unsafe.Add(ref valueRef, 1),
                                span.Length),
                        3 => ExSpanHelpers.IndexOfAnyValueType(
                                 ref spanRef,
                                 valueRef,
                                 Unsafe.Add(ref valueRef, 1),
                                 Unsafe.Add(ref valueRef, 2),
                                 span.Length),
                        4 => ExSpanHelpers.IndexOfAnyValueType(
                                 ref spanRef,
                                 valueRef,
                                 Unsafe.Add(ref valueRef, 1),
                                 Unsafe.Add(ref valueRef, 2),
                                 Unsafe.Add(ref valueRef, 3),
                                 span.Length),
                        5 => ExSpanHelpers.IndexOfAnyValueType(
                                 ref spanRef,
                                 valueRef,
                                 Unsafe.Add(ref valueRef, 1),
                                 Unsafe.Add(ref valueRef, 2),
                                 Unsafe.Add(ref valueRef, 3),
                                 Unsafe.Add(ref valueRef, 4),
                                 span.Length),
                        6 => ExSpanHelpers.IndexOfAnyValueType(
                                 ref spanRef,
                                 valueRef,
                                 Unsafe.Add(ref valueRef, 1),
                                 Unsafe.Add(ref valueRef, 2),
                                 Unsafe.Add(ref valueRef, 3),
                                 Unsafe.Add(ref valueRef, 4),
                                 Unsafe.Add(ref valueRef, 5),
                                 span.Length),
                        //_ => ProbabilisticMap.IndexOfAny(ref Unsafe.As<short, char>(ref spanRef), span.Length, ref Unsafe.As<short, char>(ref valueRef), values.Length),
                        _ => ExSpanHelpers.IndexOfAny(ref ExMemoryMarshal.GetReference(span), span.Length, ref ExMemoryMarshal.GetReference(values), values.Length),
                    };
                }
            }

            return ExSpanHelpers.IndexOfAny(ref ExMemoryMarshal.GetReference(span), span.Length, ref ExMemoryMarshal.GetReference(values), values.Length);
        }

        /// <summary>
        /// Searches for the first index of any of the specified values similar to calling IndexOf several times with the logical OR operator. If not found, returns -1.
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="values">The set of values to search for.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default <see cref="IEqualityComparer{T}"/> for the type of an element.</param>
#if NET7_0_OR_GREATER
        //[MethodImpl(MethodImplOptions.AggressiveInlining)] // Bug on before .net 7.0: Unit test AIndexOfAny.MakeSureNoChecksGoOutOfRangeTwo_Char and more run fail on Release. The _byteOffset field value is wrong.
#else
        [MethodImpl(MethodImplOptions.NoInlining)]
#endif // NET7_0_OR_GREATER
        public static TSize IndexOfAny<T>(this ReadOnlyExSpan<T> span, ReadOnlyExSpan<T> values, IEqualityComparer<T>? comparer = null) {
            switch (values.Length) {
                case 0:
                    return -1;

                case 1:
                    return IndexOf(span, values[0], comparer);

                case 2:
                    return IndexOfAny(span, values[0], values[1], comparer);

                case 3:
                    return IndexOfAny(span, values[0], values[1], values[2], comparer);

                default:
                    comparer ??= EqualityComparer<T>.Default;

                    for (int i = 0; i < span.Length; i++) {
                        if (values.Contains(span[i], comparer)) {
                            return i;
                        }
                    }

                    return -1;
            }
        }

#if NET8_0_OR_GREATER && TODO // [TODO why] SearchValues.IndexOfAny is internal
        /// <summary>
        /// Searches for the first index of any of the specified values similar to calling IndexOf several times with the logical OR operator. If not found, returns -1.
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="values">The set of values to search for.</param>
        /// <returns>The first index of any of the specified values, or -1 if none are found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSize IndexOfAny<T>(this ReadOnlyExSpan<T> span, SearchValues<T> values) where T : IEquatable<T>? {
            if (values is null) {
                throw new ArgumentNullException(nameof(values));
            }

            return values.IndexOfAny(span);
        }

        /// <summary>
        /// Searches for the first index of any of the specified substring values.
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="values">The set of values to search for.</param>
        /// <returns>The first index of any of the specified values, or -1 if none are found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSize IndexOfAny(this ReadOnlyExSpan<char> span, SearchValues<string> values) {
            if (values is null) {
                throw new ArgumentNullException(nameof(values));
            }

            return values.IndexOfAnyMultiString(span);
        }
#endif // NET8_0_OR_GREATER

        /// <summary>
        /// Searches for the last index of any of the specified values similar to calling LastIndexOf several times with the logical OR operator. If not found, returns -1.
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="value0">One of the values to search for.</param>
        /// <param name="value1">One of the values to search for.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [OverloadResolutionPriority(-1)]
        public static TSize LastIndexOfAny<T>(this ExSpan<T> span, T value0, T value1) where T : IEquatable<T>? =>
            LastIndexOfAny((ReadOnlyExSpan<T>)span, value0, value1);

        /// <summary>
        /// Searches for the last index of any of the specified values similar to calling LastIndexOf several times with the logical OR operator. If not found, returns -1.
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="value0">One of the values to search for.</param>
        /// <param name="value1">One of the values to search for.</param>
        /// <param name="value2">One of the values to search for.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [OverloadResolutionPriority(-1)]
        public static TSize LastIndexOfAny<T>(this ExSpan<T> span, T value0, T value1, T value2) where T : IEquatable<T>? =>
            LastIndexOfAny((ReadOnlyExSpan<T>)span, value0, value1, value2);

        /// <summary>
        /// Searches for the last index of any of the specified values similar to calling LastIndexOf several times with the logical OR operator. If not found, returns -1.
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="values">The set of values to search for.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [OverloadResolutionPriority(-1)]
        public static TSize LastIndexOfAny<T>(this ExSpan<T> span, ReadOnlyExSpan<T> values) where T : IEquatable<T>? =>
            LastIndexOfAny((ReadOnlyExSpan<T>)span, values);

#if NET8_0_OR_GREATER && TODO // [TODO why] SearchValues.IndexOfAny is internal
        /// <summary>
        /// Searches for the last index of any of the specified values similar to calling LastIndexOf several times with the logical OR operator. If not found, returns -1.
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="values">The set of values to search for.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [OverloadResolutionPriority(-1)]
        public static TSize LastIndexOfAny<T>(this ExSpan<T> span, SearchValues<T> values) where T : IEquatable<T>? =>
            LastIndexOfAny((ReadOnlyExSpan<T>)span, values);
#endif // NET8_0_OR_GREATER

        /// <summary>
        /// Searches for the last index of any of the specified values similar to calling LastIndexOf several times with the logical OR operator. If not found, returns -1.
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="value0">One of the values to search for.</param>
        /// <param name="value1">One of the values to search for.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSize LastIndexOfAny<T>(this ReadOnlyExSpan<T> span, T value0, T value1) where T : IEquatable<T>? {
            if (TypeHelper.IsBitwiseEquatable<T>()) {
                if (Unsafe.SizeOf<T>() == sizeof(byte)) {
                    return ExSpanHelpers.LastIndexOfAnyValueType(
                        ref Unsafe.As<T, byte>(ref ExMemoryMarshal.GetReference(span)),
                        ExUnsafe.BitCast<T, byte>(value0),
                        ExUnsafe.BitCast<T, byte>(value1),
                        span.Length);
                } else if (Unsafe.SizeOf<T>() == sizeof(short)) {
                    return ExSpanHelpers.LastIndexOfAnyValueType(
                        ref Unsafe.As<T, short>(ref ExMemoryMarshal.GetReference(span)),
                        ExUnsafe.BitCast<T, short>(value0),
                        ExUnsafe.BitCast<T, short>(value1),
                        span.Length);
                }
            }

            return ExSpanHelpers.LastIndexOfAny(ref ExMemoryMarshal.GetReference(span), value0, value1, span.Length);
        }

        /// <summary>
        /// Searches for the last index of any of the specified values similar to calling LastIndexOf several times with the logical OR operator. If not found, returns -1.
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="value0">One of the values to search for.</param>
        /// <param name="value1">One of the values to search for.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default <see cref="IEqualityComparer{T}"/> for the type of an element.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSize LastIndexOfAny<T>(this ReadOnlyExSpan<T> span, T value0, T value1, IEqualityComparer<T>? comparer = null) {
            if (TypeHelper.IsValueType<T>() && (comparer is null || comparer == EqualityComparer<T>.Default)) {
                if (TypeHelper.IsBitwiseEquatable<T>()) {
                    if (Unsafe.SizeOf<T>() == sizeof(byte)) {
                        return ExSpanHelpers.LastIndexOfAnyValueType(
                            ref Unsafe.As<T, byte>(ref ExMemoryMarshal.GetReference(span)),
                            ExUnsafe.BitCast<T, byte>(value0),
                            ExUnsafe.BitCast<T, byte>(value1),
                            span.Length);
                    } else if (Unsafe.SizeOf<T>() == sizeof(short)) {
                        return ExSpanHelpers.LastIndexOfAnyValueType(
                            ref Unsafe.As<T, short>(ref ExMemoryMarshal.GetReference(span)),
                            ExUnsafe.BitCast<T, short>(value0),
                            ExUnsafe.BitCast<T, short>(value1),
                            span.Length);
                    }
                }

                return LastIndexOfAnyDefaultComparer(span, value0, value1);
                static TSize LastIndexOfAnyDefaultComparer(ReadOnlyExSpan<T> span, T value0, T value1) {
                    for (TSize i = span.Length - 1; i >= 0; i--) {
                        if (EqualityComparer<T>.Default.Equals(span[i], value0) ||
                            EqualityComparer<T>.Default.Equals(span[i], value1)) {
                            return i;
                        }
                    }

                    return -1;
                }
            } else {
                return LastIndexOfAnyComparer(span, value0, value1, comparer);
                static TSize LastIndexOfAnyComparer(ReadOnlyExSpan<T> span, T value0, T value1, IEqualityComparer<T>? comparer) {
                    comparer ??= EqualityComparer<T>.Default;

                    for (TSize i = span.Length - 1; i >= 0; i--) {
                        if (comparer.Equals(span[i], value0) ||
                            comparer.Equals(span[i], value1)) {
                            return i;
                        }
                    }

                    return -1;
                }
            }
        }

        /// <summary>
        /// Searches for the last index of any of the specified values similar to calling LastIndexOf several times with the logical OR operator. If not found, returns -1.
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="value0">One of the values to search for.</param>
        /// <param name="value1">One of the values to search for.</param>
        /// <param name="value2">One of the values to search for.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSize LastIndexOfAny<T>(this ReadOnlyExSpan<T> span, T value0, T value1, T value2) where T : IEquatable<T>? {
            if (TypeHelper.IsBitwiseEquatable<T>()) {
                if (Unsafe.SizeOf<T>() == sizeof(byte)) {
                    return ExSpanHelpers.LastIndexOfAnyValueType(
                        ref Unsafe.As<T, byte>(ref ExMemoryMarshal.GetReference(span)),
                        ExUnsafe.BitCast<T, byte>(value0),
                        ExUnsafe.BitCast<T, byte>(value1),
                        ExUnsafe.BitCast<T, byte>(value2),
                        span.Length);
                } else if (Unsafe.SizeOf<T>() == sizeof(short)) {
                    return ExSpanHelpers.LastIndexOfAnyValueType(
                        ref Unsafe.As<T, short>(ref ExMemoryMarshal.GetReference(span)),
                        ExUnsafe.BitCast<T, short>(value0),
                        ExUnsafe.BitCast<T, short>(value1),
                        ExUnsafe.BitCast<T, short>(value2),
                        span.Length);
                }
            }

            return ExSpanHelpers.LastIndexOfAny(ref ExMemoryMarshal.GetReference(span), value0, value1, value2, span.Length);
        }

        /// <summary>
        /// Searches for the last index of any of the specified values similar to calling LastIndexOf several times with the logical OR operator. If not found, returns -1.
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="value0">One of the values to search for.</param>
        /// <param name="value1">One of the values to search for.</param>
        /// <param name="value2">One of the values to search for.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default <see cref="IEqualityComparer{T}"/> for the type of an element.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSize LastIndexOfAny<T>(this ReadOnlyExSpan<T> span, T value0, T value1, T value2, IEqualityComparer<T>? comparer = null) {
            if (TypeHelper.IsValueType<T>() && (comparer is null || comparer == EqualityComparer<T>.Default)) {
                if (TypeHelper.IsBitwiseEquatable<T>()) {
                    if (Unsafe.SizeOf<T>() == sizeof(byte)) {
                        return ExSpanHelpers.LastIndexOfAnyValueType(
                            ref Unsafe.As<T, byte>(ref ExMemoryMarshal.GetReference(span)),
                            ExUnsafe.BitCast<T, byte>(value0),
                            ExUnsafe.BitCast<T, byte>(value1),
                            ExUnsafe.BitCast<T, byte>(value2),
                            span.Length);
                    } else if (Unsafe.SizeOf<T>() == sizeof(short)) {
                        return ExSpanHelpers.LastIndexOfAnyValueType(
                            ref Unsafe.As<T, short>(ref ExMemoryMarshal.GetReference(span)),
                            ExUnsafe.BitCast<T, short>(value0),
                            ExUnsafe.BitCast<T, short>(value1),
                            ExUnsafe.BitCast<T, short>(value2),
                            span.Length);
                    }
                }

                return LastIndexOfAnyDefaultComparer(span, value0, value1, value2);
                static TSize LastIndexOfAnyDefaultComparer(ReadOnlyExSpan<T> span, T value0, T value1, T value2) {
                    for (TSize i = span.Length - 1; i >= 0; i--) {
                        if (EqualityComparer<T>.Default.Equals(span[i], value0) ||
                            EqualityComparer<T>.Default.Equals(span[i], value1) ||
                            EqualityComparer<T>.Default.Equals(span[i], value2)) {
                            return i;
                        }
                    }

                    return -1;
                }
            } else {
                return LastIndexOfAnyComparer(span, value0, value1, value2, comparer);
                static TSize LastIndexOfAnyComparer(ReadOnlyExSpan<T> span, T value0, T value1, T value2, IEqualityComparer<T>? comparer) {
                    comparer ??= EqualityComparer<T>.Default;

                    for (TSize i = span.Length - 1; i >= 0; i--) {
                        if (comparer.Equals(span[i], value0) ||
                            comparer.Equals(span[i], value1) ||
                            comparer.Equals(span[i], value2)) {
                            return i;
                        }
                    }

                    return -1;
                }
            }
        }

        /// <summary>
        /// Searches for the last index of any of the specified values similar to calling LastIndexOf several times with the logical OR operator. If not found, returns -1.
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="values">The set of values to search for.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSize LastIndexOfAny<T>(this ReadOnlyExSpan<T> span, ReadOnlyExSpan<T> values) where T : IEquatable<T>? {
            if (TypeHelper.IsBitwiseEquatable<T>()) {
                if (Unsafe.SizeOf<T>() == sizeof(byte)) {
                    ref byte spanRef = ref Unsafe.As<T, byte>(ref ExMemoryMarshal.GetReference(span));
                    ref byte valueRef = ref Unsafe.As<T, byte>(ref ExMemoryMarshal.GetReference(values));
                    switch (values.Length) {
                        case 0:
                            return -1;

                        case 1:
                            return ExSpanHelpers.LastIndexOfValueType(ref spanRef, valueRef, span.Length);

                        case 2:
                            return ExSpanHelpers.LastIndexOfAnyValueType(
                                ref spanRef,
                                valueRef,
                                Unsafe.Add(ref valueRef, 1),
                                span.Length);

                        case 3:
                            return ExSpanHelpers.LastIndexOfAnyValueType(
                                ref spanRef,
                                valueRef,
                                Unsafe.Add(ref valueRef, 1),
                                Unsafe.Add(ref valueRef, 2),
                                span.Length);

                        case 4:
                            return ExSpanHelpers.LastIndexOfAnyValueType(
                                ref spanRef,
                                valueRef,
                                Unsafe.Add(ref valueRef, 1),
                                Unsafe.Add(ref valueRef, 2),
                                Unsafe.Add(ref valueRef, 3),
                                span.Length);

                        case 5:
                            return ExSpanHelpers.LastIndexOfAnyValueType(
                                ref spanRef,
                                valueRef,
                                Unsafe.Add(ref valueRef, 1),
                                Unsafe.Add(ref valueRef, 2),
                                Unsafe.Add(ref valueRef, 3),
                                Unsafe.Add(ref valueRef, 4),
                                span.Length);
                    }
                }

                if (Unsafe.SizeOf<T>() == sizeof(short)) {
                    ref short spanRef = ref Unsafe.As<T, short>(ref ExMemoryMarshal.GetReference(span));
                    ref short valueRef = ref Unsafe.As<T, short>(ref ExMemoryMarshal.GetReference(values));
                    return values.Length switch {
                        0 => -1,
                        1 => ExSpanHelpers.LastIndexOfValueType(ref spanRef, valueRef, span.Length),
                        2 => ExSpanHelpers.LastIndexOfAnyValueType(
                                 ref spanRef,
                                 valueRef,
                                 Unsafe.Add(ref valueRef, 1),
                                 span.Length),
                        3 => ExSpanHelpers.LastIndexOfAnyValueType(
                                 ref spanRef,
                                 valueRef,
                                 Unsafe.Add(ref valueRef, 1),
                                 Unsafe.Add(ref valueRef, 2),
                                 span.Length),
                        4 => ExSpanHelpers.LastIndexOfAnyValueType(
                                 ref spanRef,
                                 valueRef,
                                 Unsafe.Add(ref valueRef, 1),
                                 Unsafe.Add(ref valueRef, 2),
                                 Unsafe.Add(ref valueRef, 3),
                                 span.Length),
                        5 => ExSpanHelpers.LastIndexOfAnyValueType(
                                 ref spanRef,
                                 valueRef,
                                 Unsafe.Add(ref valueRef, 1),
                                 Unsafe.Add(ref valueRef, 2),
                                 Unsafe.Add(ref valueRef, 3),
                                 Unsafe.Add(ref valueRef, 4),
                                 span.Length),
                        //_ => ProbabilisticMap.LastIndexOfAny(ref Unsafe.As<short, char>(ref spanRef), span.Length, ref Unsafe.As<short, char>(ref valueRef), values.Length),
                        _ => ExSpanHelpers.LastIndexOfAny(ref ExMemoryMarshal.GetReference(span), span.Length, ref ExMemoryMarshal.GetReference(values), values.Length),
                    };
                }
            }

            return ExSpanHelpers.LastIndexOfAny(ref ExMemoryMarshal.GetReference(span), span.Length, ref ExMemoryMarshal.GetReference(values), values.Length);
        }

        /// <summary>
        /// Searches for the last index of any of the specified values similar to calling LastIndexOf several times with the logical OR operator. If not found, returns -1.
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="values">The set of values to search for.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default <see cref="IEqualityComparer{T}"/> for the type of an element.</param>
        public static TSize LastIndexOfAny<T>(this ReadOnlyExSpan<T> span, ReadOnlyExSpan<T> values, IEqualityComparer<T>? comparer = null) {
            switch (values.Length) {
                case 0:
                    return -1;

                case 1:
                    return LastIndexOf(span, values[0], comparer);

                case 2:
                    return LastIndexOfAny(span, values[0], values[1], comparer);

                case 3:
                    return LastIndexOfAny(span, values[0], values[1], values[2], comparer);

                default:
                    comparer ??= EqualityComparer<T>.Default;

                    for (TSize i = span.Length - 1; i >= 0; i--) {
                        if (values.Contains(span[i], comparer)) {
                            return i;
                        }
                    }

                    return -1;
            }
        }

#if NET8_0_OR_GREATER && TODO // [TODO why] SearchValues.IndexOfAny is internal
        /// <summary>
        /// Searches for the last index of any of the specified values similar to calling LastIndexOf several times with the logical OR operator. If not found, returns -1.
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="values">The set of values to search for.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSize LastIndexOfAny<T>(this ReadOnlyExSpan<T> span, SearchValues<T> values) where T : IEquatable<T>? {
            if (values is null) {
                throw new ArgumentNullException(nameof(values));
            }

            return values.LastIndexOfAny(span);
        }
#endif // NET8_0_OR_GREATER

        /// <summary>
        /// Determines whether two read-only sequences are equal by comparing the elements using <see cref="IEquatable{T}.Equals(T)"/> (通过使用 <see cref="IEquatable{T}.Equals(T)"/> 比较元素, 确定两个只读序列是否相等).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="span">The first sequence to compare (要比较的第一个序列).</param>
        /// <param name="other">The second sequence to compare (要比较的第二个序列).</param>
        /// <returns>true if the two sequences are equal; otherwise, false (如果两个序列相等, 则 true; 否则 false).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        //[MethodImpl(MethodImplOptions.NoInlining)]
        public static bool SequenceEqual<T>(this ReadOnlyExSpan<T> span, ReadOnlyExSpan<T> other) where T : IEquatable<T>? {
            if (span.Length != other.Length) {
                return false;
            }
            TSize length = span.Length;
#if INVOKE_SPAN_METHOD && NET6_0_OR_GREATER
            if (length.IsByteLengthInInt32<T>()) {
#pragma warning disable CS8631 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match constraint type.
                return span.AsReadOnlySpan().SequenceEqual(other.AsReadOnlySpan());
#pragma warning restore CS8631 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match constraint type.
            } else {
                int blockSize = ExMemoryMarshal.ArrayMaxLengthSafe / Unsafe.SizeOf<T>();
                if (blockSize < 1) blockSize = 1;
                TSize blockSizeN = (TSize)blockSize;
                TSize index = (TSize)0;
                while (index < length) {
                    TSize count = length - index;
                    TSize indexNext;
                    bool rt;
                    if (count < blockSizeN) {
#pragma warning disable CS8631 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match constraint type.
                        rt = span.Slice(index, count).AsReadOnlySpan().SequenceEqual(other.Slice(index, count).AsReadOnlySpan());
#pragma warning restore CS8631 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match constraint type.
                        indexNext = length;
                    } else {
#pragma warning disable CS8631 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match constraint type.
                        rt = span.Slice(index, blockSizeN).AsReadOnlySpan().SequenceEqual(other.Slice(index, blockSizeN).AsReadOnlySpan());
#pragma warning restore CS8631 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match constraint type.
                        indexNext = index + blockSize;
                    }
                    if (!rt) {
                        return false;
                    }
                    // Next.
                    index = indexNext;
                }
                return true;
            }
#else
            if (TypeHelper.IsBitwiseEquatable<T>()) {
                return ExSpanHelpers.SequenceEqual(
                        ref Unsafe.As<T, byte>(ref ExMemoryMarshal.GetReference(span)),
                        ref Unsafe.As<T, byte>(ref ExMemoryMarshal.GetReference(other)),
                        (nuint)((ulong)length * (ulong)Unsafe.SizeOf<T>()));  // // If this multiplication overflows, the Span we got overflows the entire address range. There's no happy outcome for this API in such a case so we choose not to take the overhead of checking.
            }
            return ExSpanHelpers.SequenceEqual<T>(ref ExMemoryMarshal.GetReference(span), ref ExMemoryMarshal.GetReference(other), length.ToUIntPtr());
#endif // INVOKE_SPAN_METHOD && NET6_0_OR_GREATER
        }

        /// <summary>
        /// Determines whether a span and a read-only span are equal by comparing the elements using <see cref="IEquatable{T}.Equals(T)"/> (通过使用 <see cref="IEquatable{T}.Equals(T)"/> 比较元素, 确定跨度和只读跨度是否相等).
        /// </summary>
        /// <param name="span">The first sequence to compare (要比较的第一个序列).</param>
        /// <param name="other">The second sequence to compare (要比较的第二个序列).</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default <see cref="IEqualityComparer{T}"/> for the type of an element (比较元素时要使用的 <see cref="IEqualityComparer{T}"/> 实现. 或 null, 将使用元素的类型的默认 <see cref="IEqualityComparer{T}"/>).</param>
        /// <returns>true if the two sequences are equal; otherwise, false (如果两个序列相等, 则 true; 否则 false).</returns>
        [OverloadResolutionPriority(-1)]
        public static bool SequenceEqual<T>(this ExSpan<T> span, ReadOnlyExSpan<T> other, IEqualityComparer<T>? comparer = null) =>
            SequenceEqual((ReadOnlyExSpan<T>)span, other, comparer);

        /// <summary>
        /// Determines whether two read-only sequences are equal by comparing the elements using <see cref="IEquatable{T}.Equals(T)"/> (通过使用 <see cref="IEquatable{T}.Equals(T)"/> 比较元素, 确定两个只读序列是否相等).
        /// </summary>
        /// <param name="span">The first sequence to compare (要比较的第一个序列).</param>
        /// <param name="other">The second sequence to compare (要比较的第二个序列).</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default <see cref="IEqualityComparer{T}"/> for the type of an element.</param>
        /// <returns>true if the two sequences are equal; otherwise, false (如果两个序列相等, 则 true; 否则 false).</returns>
        public static bool SequenceEqual<T>(this ReadOnlyExSpan<T> span, ReadOnlyExSpan<T> other, IEqualityComparer<T>? comparer = null) {
            // If the spans differ in length, they're not equal.
            if (span.Length != other.Length) {
                return false;
            }
            TSize length = span.Length;
#if INVOKE_SPAN_METHOD && NET6_0_OR_GREATER
            if (length.IsByteLengthInInt32<T>()) {
                return span.AsReadOnlySpan().SequenceEqual(other.AsReadOnlySpan(), comparer);
            } else {
                int blockSize = ExMemoryMarshal.ArrayMaxLengthSafe / Unsafe.SizeOf<T>();
                if (blockSize < 1) blockSize = 1;
                TSize blockSizeN = (TSize)blockSize;
                TSize index = (TSize)0;
                while (index < length) {
                    TSize count = length - index;
                    TSize indexNext;
                    bool rt;
                    if (count < blockSizeN) {
                        rt = span.Slice(index, count).AsReadOnlySpan().SequenceEqual(other.Slice(index, count).AsReadOnlySpan(), comparer);
                        indexNext = length;
                    } else {
                        rt = span.Slice(index, blockSizeN).AsReadOnlySpan().SequenceEqual(other.Slice(index, blockSizeN).AsReadOnlySpan(), comparer);
                        indexNext = index + blockSize;
                    }
                    if (!rt) {
                        return false;
                    }
                    // Next.
                    index = indexNext;
                }
                return true;
            }
#else
            if (TypeHelper.IsValueType(typeof(T))) {
                if (comparer is null || comparer == EqualityComparer<T>.Default) {
                    // If no comparer was supplied and the type is bitwise equatable, take the fast path doing a bitwise comparison.
                    if (TypeHelper.IsBitwiseEquatable<T>()) {
                        return ExSpanHelpers.SequenceEqual(
                            ref Unsafe.As<T, byte>(ref ExMemoryMarshal.GetReference(span)),
                            ref Unsafe.As<T, byte>(ref ExMemoryMarshal.GetReference(other)),
                            ((uint)span.Length) * (nuint)Unsafe.SizeOf<T>());  // If this multiplication overflows, the Span we got overflows the entire address range. There's no happy outcome for this API in such a case so we choose not to take the overhead of checking.
                    }

                    // Otherwise, compare each element using EqualityComparer<T>.Default.Equals in a way that will enable it to devirtualize.
                    for (TSize i = (TSize)0; i < length; i += 1) {
                        if (!EqualityComparer<T>.Default.Equals(span[i], other[i])) {
                            return false;
                        }
                    }

                    return true;
                }
            }

            // Use the comparer to compare each element.
            comparer ??= EqualityComparer<T>.Default;
            for (TSize i = (TSize)0; i < length; i += 1) {
                if (!comparer.Equals(span[i], other[i])) {
                    return false;
                }
            }

            return true;
#endif // INVOKE_SPAN_METHOD && NET6_0_OR_GREATER
        }

        /// <summary>
        /// Determines the relative order of the sequences being compared by comparing the elements using IComparable{T}.CompareTo(T).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SequenceCompareTo<T>(this ReadOnlyExSpan<T> span, ReadOnlyExSpan<T> other) where T : IComparable<T>? {
            // Can't use IsBitwiseEquatable<T>() below because that only tells us about
            // equality checks, not about CompareTo checks.

            if (typeof(T) == typeof(byte))
                return ExSpanHelpers.SequenceCompareTo(
                    ref Unsafe.As<T, byte>(ref ExMemoryMarshal.GetReference(span)),
                    span.Length,
                    ref Unsafe.As<T, byte>(ref ExMemoryMarshal.GetReference(other)),
                    other.Length);

            if (typeof(T) == typeof(char))
                return ExSpanHelpers.SequenceCompareTo(
                    ref Unsafe.As<T, char>(ref ExMemoryMarshal.GetReference(span)),
                    span.Length,
                    ref Unsafe.As<T, char>(ref ExMemoryMarshal.GetReference(other)),
                    other.Length);

            return ExSpanHelpers.SequenceCompareTo(ref ExMemoryMarshal.GetReference(span), span.Length, ref ExMemoryMarshal.GetReference(other), other.Length);
        }

        /// <summary>
        /// Determines the relative order of the sequences being compared by comparing the elements using IComparable{T}.CompareTo(T).
        /// </summary>
        public static int SequenceCompareTo<T>(this ReadOnlyExSpan<T> span, ReadOnlyExSpan<T> other, IComparer<T>? comparer = null) {
            TSize minLength = BitMath.Min(span.Length, other.Length);
            comparer ??= Comparer<T>.Default;

            for (int i = 0; i < minLength; i++) {
                int c = comparer.Compare(span[i], other[i]);
                if (c != 0) {
                    return c;
                }
            }

            return span.Length.CompareTo(other.Length);
        }

        /// <summary>
        /// Determines whether the specified sequence appears at the start of the span.
        /// </summary>
        //[Intrinsic] // Unrolled and vectorized for half-constant input
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [OverloadResolutionPriority(-1)]
        public static bool StartsWith<T>(this ExSpan<T> span, ReadOnlyExSpan<T> value) where T : IEquatable<T>? =>
            StartsWith((ReadOnlyExSpan<T>)span, value);

        /// <summary>
        /// Determines whether the specified sequence appears at the start of the span.
        /// </summary>
        //[Intrinsic] // Unrolled and vectorized for half-constant input
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool StartsWith<T>(this ReadOnlyExSpan<T> span, ReadOnlyExSpan<T> value) where T : IEquatable<T>? {
            TSize valueLength = value.Length;
            if (TypeHelper.IsBitwiseEquatable<T>()) {
                return valueLength <= span.Length &&
                ExSpanHelpers.SequenceEqual(
                    ref Unsafe.As<T, byte>(ref ExMemoryMarshal.GetReference(span)),
                    ref Unsafe.As<T, byte>(ref ExMemoryMarshal.GetReference(value)),
                    ((uint)valueLength) * (nuint)Unsafe.SizeOf<T>());  // If this multiplication overflows, the span we got overflows the entire address range. There's no happy outcome for this api in such a case so we choose not to take the overhead of checking.
            }

            return valueLength <= span.Length && ExSpanHelpers.SequenceEqual(ref ExMemoryMarshal.GetReference(span), ref ExMemoryMarshal.GetReference(value), valueLength.ToUIntPtr());
        }

        /// <summary>
        /// Determines whether a specified sequence appears at the start of a read-only span.
        /// </summary>
        /// <param name="span">The source span.</param>
        /// <param name="value">The sequence to compare to the start of <paramref name="span"/>.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default <see cref="IEqualityComparer{T}"/> for the type of an element.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool StartsWith<T>(this ReadOnlyExSpan<T> span, ReadOnlyExSpan<T> value, IEqualityComparer<T>? comparer = null) =>
            value.Length <= span.Length &&
            SequenceEqual(span.Slice((TSize)0, value.Length), value, comparer);

        /// <summary>
        /// Determines whether the specified sequence appears at the end of the span.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        //[Intrinsic] // Unrolled and vectorized for half-constant input
        [OverloadResolutionPriority(-1)]
        public static bool EndsWith<T>(this ExSpan<T> span, ReadOnlyExSpan<T> value) where T : IEquatable<T>? =>
            EndsWith((ReadOnlyExSpan<T>)span, value);

        /// <summary>
        /// Determines whether the specified sequence appears at the end of the span.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        //[Intrinsic] // Unrolled and vectorized for half-constant input
        public static bool EndsWith<T>(this ReadOnlyExSpan<T> span, ReadOnlyExSpan<T> value) where T : IEquatable<T>? {
            TSize spanLength = span.Length;
            TSize valueLength = value.Length;
            if (TypeHelper.IsBitwiseEquatable<T>()) {
                return valueLength <= spanLength &&
                ExSpanHelpers.SequenceEqual(
                    ref Unsafe.As<T, byte>(ref Unsafe.Add(ref ExMemoryMarshal.GetReference(span), spanLength - valueLength /* force zero-extension */)),
                    ref Unsafe.As<T, byte>(ref ExMemoryMarshal.GetReference(value)),
                    (valueLength.ToUIntPtr() * (nuint)Unsafe.SizeOf<T>()));  // If this multiplication overflows, the span we got overflows the entire address range. There's no happy outcome for this api in such a case so we choose not to take the overhead of checking.
            }

            return valueLength <= spanLength &&
                ExSpanHelpers.SequenceEqual(
                    ref Unsafe.Add(ref ExMemoryMarshal.GetReference(span), spanLength - valueLength /* force zero-extension */),
                    ref ExMemoryMarshal.GetReference(value),
                    valueLength.ToUIntPtr());
        }

        /// <summary>
        /// Determines whether the specified sequence appears at the end of the read-only span.
        /// </summary>
        /// <param name="span">The source span.</param>
        /// <param name="value">The sequence to compare to the end of <paramref name="span"/>.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default <see cref="IEqualityComparer{T}"/> for the type of an element.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool EndsWith<T>(this ReadOnlyExSpan<T> span, ReadOnlyExSpan<T> value, IEqualityComparer<T>? comparer = null) =>
            value.Length <= span.Length &&
            SequenceEqual(span.Slice(span.Length - value.Length), value, comparer);

        /// <summary>
        /// Determines whether the specified value appears at the start of the span.
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="value">The value to compare.</param>
        /// <typeparam name="T">The type of elements in the span.</typeparam>
        /// <returns><see langword="true" /> if <paramref name="value" /> matches the beginning of <paramref name="span" />; otherwise, <see langword="false" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool StartsWith<T>(this ReadOnlyExSpan<T> span, T value) where T : IEquatable<T>? =>
            span.Length != (TSize)0 && (span[(TSize)0]?.Equals(value) ?? (object?)value is null);

        /// <summary>
        /// Determines whether the specified value appears at the start of the span.
        /// </summary>
        /// <typeparam name="T">The type of elements in the span.</typeparam>
        /// <param name="span">The span to search.</param>
        /// <param name="value">The value to compare.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default <see cref="IEqualityComparer{T}"/> for the type of an element.</param>
        /// <returns><see langword="true" /> if <paramref name="value" /> matches the beginning of <paramref name="span" />; otherwise, <see langword="false" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool StartsWith<T>(this ReadOnlyExSpan<T> span, T value, IEqualityComparer<T>? comparer = null) =>
            span.Length != (TSize)0 &&
            (comparer is null ? EqualityComparer<T>.Default.Equals(span[(TSize)0], value) : comparer.Equals(span[(TSize)0], value));

        /// <summary>
        /// Determines whether the specified value appears at the end of the span.
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="value">The value to compare.</param>
        /// <typeparam name="T">The type of the elements in the span.</typeparam>
        /// <returns><see langword="true" /> if <paramref name="value" /> matches the end of <paramref name="span" />; otherwise, <see langword="false" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool EndsWith<T>(this ReadOnlyExSpan<T> span, T value) where T : IEquatable<T>? =>
            span.Length != (TSize)0 && (span[span.Length-1]?.Equals(value) ?? (object?)value is null);

        /// <summary>
        /// Determines whether the specified value appears at the end of the span.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the span.</typeparam>
        /// <param name="span">The span to search.</param>
        /// <param name="value">The value to compare.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default <see cref="IEqualityComparer{T}"/> for the type of an element.</param>
        /// <returns><see langword="true" /> if <paramref name="value" /> matches the end of <paramref name="span" />; otherwise, <see langword="false" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool EndsWith<T>(this ReadOnlyExSpan<T> span, T value, IEqualityComparer<T>? comparer = null) =>
            span.Length != (TSize)0 &&
            (comparer is null ? EqualityComparer<T>.Default.Equals(span[span.Length - 1], value) : comparer.Equals(span[span.Length - 1], value));

        /// <summary>
        /// Reverses the sequence of the elements in the entire span.
        /// </summary>
        public static void Reverse<T>(this ExSpan<T> span) {
            if (span.Length > 1) {
                ExSpanHelpers.Reverse(ref ExMemoryMarshal.GetReference(span), (nuint)span.Length);
            }
        }

#if NOT_RELATED
        /// <summary>
        /// Creates a new memory over the target array.
        /// </summary>
        public static Memory<T> AsMemory<T>(this T[]? array) => new Memory<T>(array);

        /// <summary>
        /// Creates a new memory over the portion of the target array beginning
        /// at 'start' index and ending at 'end' index (exclusive).
        /// </summary>
        /// <param name="array">The target array.</param>
        /// <param name="start">The index at which to begin the memory.</param>
        /// <remarks>Returns default when <paramref name="array"/> is null.</remarks>
        /// <exception cref="ArrayTypeMismatchException">Thrown when <paramref name="array"/> is covariant and array's type is not exactly T[].</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the specified <paramref name="start"/> or end index is not in the range (&lt;0 or &gt;array.Length).
        /// </exception>
        public static Memory<T> AsMemory<T>(this T[]? array, int start) => new Memory<T>(array, start);

        /// <summary>
        /// Creates a new memory over the portion of the target array starting from
        /// 'startIndex' to the end of the array.
        /// </summary>
        public static Memory<T> AsMemory<T>(this T[]? array, Index startIndex) {
            if (array == null) {
                if (!startIndex.Equals(Index.Start))
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);

                return default;
            }

            int actualIndex = startIndex.GetOffset(array.Length);
            return new Memory<T>(array, actualIndex);
        }

        /// <summary>
        /// Creates a new memory over the portion of the target array beginning
        /// at 'start' index and ending at 'end' index (exclusive).
        /// </summary>
        /// <param name="array">The target array.</param>
        /// <param name="start">The index at which to begin the memory.</param>
        /// <param name="length">The number of items in the memory.</param>
        /// <remarks>Returns default when <paramref name="array"/> is null.</remarks>
        /// <exception cref="ArrayTypeMismatchException">Thrown when <paramref name="array"/> is covariant and array's type is not exactly T[].</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the specified <paramref name="start"/> or end index is not in the range (&lt;0 or &gt;Length).
        /// </exception>
        public static Memory<T> AsMemory<T>(this T[]? array, int start, int length) => new Memory<T>(array, start, length);

        /// <summary>
        /// Creates a new memory over the portion of the target array beginning at inclusive start index of the range
        /// and ending at the exclusive end index of the range.
        /// </summary>
        public static Memory<T> AsMemory<T>(this T[]? array, Range range) {
            if (array == null) {
                Index startIndex = range.Start;
                Index endIndex = range.End;
                if (!startIndex.Equals(Index.Start) || !endIndex.Equals(Index.Start))
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);

                return default;
            }

            (int start, int length) = range.GetOffsetAndLength(array.Length);
            return new Memory<T>(array, start, length);
        }

        /// <summary>
        /// Creates a new memory over the portion of the target array.
        /// </summary>
        public static Memory<T> AsMemory<T>(this ArraySegment<T> segment) => new Memory<T>(segment.Array, segment.Offset, segment.Count);

        /// <summary>
        /// Creates a new memory over the portion of the target array beginning
        /// at 'start' index and ending at 'end' index (exclusive).
        /// </summary>
        /// <param name="segment">The target array.</param>
        /// <param name="start">The index at which to begin the memory.</param>
        /// <remarks>Returns default when <paramref name="segment"/> is null.</remarks>
        /// <exception cref="ArrayTypeMismatchException">Thrown when <paramref name="segment"/> is covariant and array's type is not exactly T[].</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the specified <paramref name="start"/> or end index is not in the range (&lt;0 or &gt;segment.Count).
        /// </exception>
        public static Memory<T> AsMemory<T>(this ArraySegment<T> segment, int start) {
            if (((uint)start) > (uint)segment.Count)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.start);

            return new Memory<T>(segment.Array, segment.Offset + start, segment.Count - start);
        }

        /// <summary>
        /// Creates a new memory over the portion of the target array beginning
        /// at 'start' index and ending at 'end' index (exclusive).
        /// </summary>
        /// <param name="segment">The target array.</param>
        /// <param name="start">The index at which to begin the memory.</param>
        /// <param name="length">The number of items in the memory.</param>
        /// <remarks>Returns default when <paramref name="segment"/> is null.</remarks>
        /// <exception cref="ArrayTypeMismatchException">Thrown when <paramref name="segment"/> is covariant and array's type is not exactly T[].</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the specified <paramref name="start"/> or end index is not in the range (&lt;0 or &gt;segment.Count).
        /// </exception>
        public static Memory<T> AsMemory<T>(this ArraySegment<T> segment, int start, int length) {
            if (((uint)start) > (uint)segment.Count)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.start);
            if (((uint)length) > (uint)(segment.Count - start))
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.length);

            return new Memory<T>(segment.Array, segment.Offset + start, length);
        }

        /// <summary>
        /// Copies the contents of the array into the span. If the source
        /// and destinations overlap, this method behaves as if the original values in
        /// a temporary location before the destination is overwritten.
        /// </summary>
        ///<param name="source">The array to copy items from.</param>
        /// <param name="destination">The span to copy items into.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when the destination ExSpan is shorter than the source array.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyTo<T>(this T[]? source, ExSpan<T> destination) {
            new ReadOnlyExSpan<T>(source).CopyTo(destination);
        }

        /// <summary>
        /// Copies the contents of the array into the memory. If the source
        /// and destinations overlap, this method behaves as if the original values are in
        /// a temporary location before the destination is overwritten.
        /// </summary>
        ///<param name="source">The array to copy items from.</param>
        /// <param name="destination">The memory to copy items into.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when the destination is shorter than the source array.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyTo<T>(this T[]? source, Memory<T> destination) {
            source.CopyTo(destination.ExSpan);
        }
#endif // NOT_RELATED

        //
        //  Overlaps
        //  ========
        //
        //  The following methods can be used to determine if two sequences
        //  overlap in memory.
        //
        //  Two sequences overlap if they have positions in common and neither
        //  is empty. Empty sequences do not overlap with any other sequence.
        //
        //  If two sequences overlap, the element offset is the number of
        //  elements by which the second sequence is offset from the first
        //  sequence (i.e., second minus first). An exception is thrown if the
        //  number is not a whole number, which can happen when a sequence of a
        //  smaller type is cast to a sequence of a larger type with unsafe code
        //  or NonPortableCast. If the sequences do not overlap, the offset is
        //  meaningless and arbitrarily set to zero.
        //
        //  Implementation
        //  --------------
        //
        //  Implementing this correctly is quite tricky due of two problems:
        //
        //  * If the sequences refer to two different objects on the managed
        //    heap, the garbage collector can move them freely around or change
        //    their relative order in memory.
        //
        //  * The distance between two sequences can be greater than
        //    int.MaxValue (on a 32-bit system) or long.MaxValue (on a 64-bit
        //    system).
        //
        //  (For simplicity, the following text assumes a 32-bit system, but
        //  everything also applies to a 64-bit system if every 32 is replaced a
        //  64.)
        //
        //  The first problem is solved by calculating the distance with exactly
        //  one atomic operation. If the garbage collector happens to move the
        //  sequences afterwards and the sequences overlapped before, they will
        //  still overlap after the move and their distance hasn't changed. If
        //  the sequences did not overlap, the distance can change but the
        //  sequences still won't overlap.
        //
        //  The second problem is solved by making all addresses relative to the
        //  start of the first sequence and performing all operations in
        //  unsigned integer arithmetic modulo 2^32.
        //
        //  Example
        //  -------
        //
        //  Let's say there are two sequences, x and y. Let
        //
        //      ref T xRef = ExMemoryMarshal.GetReference(x)
        //      uint xLength = x.Length * Unsafe.SizeOf<T>()
        //      ref T yRef = ExMemoryMarshal.GetReference(y)
        //      uint yLength = y.Length * Unsafe.SizeOf<T>()
        //
        //  Visually, the two sequences are located somewhere in the 32-bit
        //  address space as follows:
        //
        //      [----------------------------------------------)                            normal address space
        //      0                                             2^32
        //                            [------------------)                                  first sequence
        //                            xRef            xRef + xLength
        //              [--------------------------)     .                                  second sequence
        //              yRef          .         yRef + yLength
        //              :             .            .     .
        //              :             .            .     .
        //                            .            .     .
        //                            .            .     .
        //                            .            .     .
        //                            [----------------------------------------------)      relative address space
        //                            0            .     .                          2^32
        //                            [------------------)             :                    first sequence
        //                            x1           .     x2            :
        //                            -------------)                   [-------------       second sequence
        //                                         y2                  y1
        //
        //  The idea is to make all addresses relative to xRef: Let x1 be the
        //  start address of x in this relative address space, x2 the end
        //  address of x, y1 the start address of y, and y2 the end address of
        //  y:
        //
        //      nuint x1 = 0
        //      nuint x2 = xLength
        //      nuint y1 = (nuint)Unsafe.ByteOffset(xRef, yRef)
        //      nuint y2 = y1 + yLength
        //
        //  xRef relative to xRef is 0.
        //
        //  x2 is simply x1 + xLength. This cannot overflow.
        //
        //  yRef relative to xRef is (yRef - xRef). If (yRef - xRef) is
        //  negative, casting it to an unsigned 32-bit integer turns it into
        //  (yRef - xRef + 2^32). So, in the example above, y1 moves to the right
        //  of x2.
        //
        //  y2 is simply y1 + yLength. Note that this can overflow, as in the
        //  example above, which must be avoided.
        //
        //  The two sequences do *not* overlap if y is entirely in the space
        //  right of x in the relative address space. (It can't be left of it!)
        //
        //          (y1 >= x2) && (y2 <= 2^32)
        //
        //  Inversely, they do overlap if
        //
        //          (y1 < x2) || (y2 > 2^32)
        //
        //  After substituting x2 and y2 with their respective definition:
        //
        //      == (y1 < xLength) || (y1 + yLength > 2^32)
        //
        //  Since yLength can't be greater than the size of the address space,
        //  the overflow can be avoided as follows:
        //
        //      == (y1 < xLength) || (y1 > 2^32 - yLength)
        //
        //  However, 2^32 cannot be stored in an unsigned 32-bit integer, so one
        //  more change is needed to keep doing everything with unsigned 32-bit
        //  integers:
        //
        //      == (y1 < xLength) || (y1 > -yLength)
        //
        //  Due to modulo arithmetic, this gives exactly same result *except* if
        //  yLength is zero, since 2^32 - 0 is 0 and not 2^32. So the case
        //  y.IsEmpty must be handled separately first.
        //

        /// <summary>
        /// Determines whether two sequences overlap in memory.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [OverloadResolutionPriority(-1)]
        public static bool Overlaps<T>(this ExSpan<T> span, ReadOnlyExSpan<T> other) =>
            Overlaps((ReadOnlyExSpan<T>)span, other);

        /// <summary>
        /// Determines whether two sequences overlap in memory and outputs the element offset.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [OverloadResolutionPriority(-1)]
        public static bool Overlaps<T>(this ExSpan<T> span, ReadOnlyExSpan<T> other, out int elementOffset) =>
            Overlaps((ReadOnlyExSpan<T>)span, other, out elementOffset);

        /// <summary>
        /// Determines whether two sequences overlap in memory.
        /// </summary>
        public static bool Overlaps<T>(this ReadOnlyExSpan<T> span, ReadOnlyExSpan<T> other) {
            if (span.IsEmpty || other.IsEmpty) {
                return false;
            }

            nint byteOffset = Unsafe.ByteOffset(
                ref ExMemoryMarshal.GetReference(span),
                ref ExMemoryMarshal.GetReference(other));

            return (nuint)byteOffset < (nuint)((nint)span.Length * Unsafe.SizeOf<T>()) ||
                    (nuint)byteOffset > (nuint)(-((nint)other.Length * Unsafe.SizeOf<T>()));
        }

        /// <summary>
        /// Determines whether two sequences overlap in memory and outputs the element offset.
        /// </summary>
        public static bool Overlaps<T>(this ReadOnlyExSpan<T> span, ReadOnlyExSpan<T> other, out int elementOffset) {
            if (span.IsEmpty || other.IsEmpty) {
                elementOffset = 0;
                return false;
            }

            nint byteOffset = Unsafe.ByteOffset(
                ref ExMemoryMarshal.GetReference(span),
                ref ExMemoryMarshal.GetReference(other));

            if ((nuint)byteOffset < (nuint)((nint)span.Length * Unsafe.SizeOf<T>()) ||
                (nuint)byteOffset > (nuint)(-((nint)other.Length * Unsafe.SizeOf<T>()))) {
                if (byteOffset % Unsafe.SizeOf<T>() != 0)
                    ThrowHelper.ThrowArgumentException_OverlapAlignmentMismatch();

                elementOffset = (int)(byteOffset / Unsafe.SizeOf<T>());
                return true;
            } else {
                elementOffset = 0;
                return false;
            }
        }

        /// <summary>
        /// Searches an entire sorted <see cref="ExSpan{T}"/> for a value
        /// using the specified <see cref="IComparable{T}"/> generic interface.
        /// </summary>
        /// <typeparam name="T">The element type of the span.</typeparam>
        /// <param name="span">The sorted <see cref="ExSpan{T}"/> to search.</param>
        /// <param name="comparable">The <see cref="IComparable{T}"/> to use when comparing.</param>
        /// <returns>
        /// The zero-based index of <paramref name="comparable"/> in the sorted <paramref name="span"/>,
        /// if <paramref name="comparable"/> is found; otherwise, a negative number that is the bitwise complement
        /// of the index of the next element that is larger than <paramref name="comparable"/> or, if there is
        /// no larger element, the bitwise complement of <see cref="ExSpan{T}.Length"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name = "comparable" /> is <see langword="null"/> .
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [OverloadResolutionPriority(-1)]
        public static TSize BinarySearch<T>(this ExSpan<T> span, IComparable<T> comparable) =>
            BinarySearch((ReadOnlyExSpan<T>)span, comparable);

        /// <summary>
        /// Searches an entire sorted <see cref="ExSpan{T}"/> for a value
        /// using the specified <typeparamref name="TComparable"/> generic type.
        /// </summary>
        /// <typeparam name="T">The element type of the span.</typeparam>
        /// <typeparam name="TComparable">The specific type of <see cref="IComparable{T}"/>.</typeparam>
        /// <param name="span">The sorted <see cref="ExSpan{T}"/> to search.</param>
        /// <param name="comparable">The <typeparamref name="TComparable"/> to use when comparing.</param>
        /// <returns>
        /// The zero-based index of <paramref name="comparable"/> in the sorted <paramref name="span"/>,
        /// if <paramref name="comparable"/> is found; otherwise, a negative number that is the bitwise complement
        /// of the index of the next element that is larger than <paramref name="comparable"/> or, if there is
        /// no larger element, the bitwise complement of <see cref="ExSpan{T}.Length"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name = "comparable" /> is <see langword="null"/> .
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [OverloadResolutionPriority(-1)]
        public static TSize BinarySearch<T, TComparable>(
            this ExSpan<T> span, TComparable comparable)
            where TComparable : IComparable<T>
#if ALLOWS_REF_STRUCT
            , allows ref struct
#endif
            =>
            BinarySearch((ReadOnlyExSpan<T>)span, comparable);

        /// <summary>
        /// Searches an entire sorted <see cref="ExSpan{T}"/> for the specified <paramref name="value"/>
        /// using the specified <typeparamref name="TComparer"/> generic type.
        /// </summary>
        /// <typeparam name="T">The element type of the span.</typeparam>
        /// <typeparam name="TComparer">The specific type of <see cref="IComparer{T}"/>.</typeparam>
        /// <param name="span">The sorted <see cref="ExSpan{T}"/> to search.</param>
        /// <param name="value">The object to locate. The value can be null for reference types.</param>
        /// <param name="comparer">The <typeparamref name="TComparer"/> to use when comparing.</param>
        /// <returns>
        /// The zero-based index of <paramref name="value"/> in the sorted <paramref name="span"/>,
        /// if <paramref name="value"/> is found; otherwise, a negative number that is the bitwise complement
        /// of the index of the next element that is larger than <paramref name="value"/> or, if there is
        /// no larger element, the bitwise complement of <see cref="ExSpan{T}.Length"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name = "comparer" /> is <see langword="null"/> .
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [OverloadResolutionPriority(-1)]
        public static TSize BinarySearch<T, TComparer>(
            this ExSpan<T> span, T value, TComparer comparer)
            where TComparer : IComparer<T>
#if ALLOWS_REF_STRUCT
            , allows ref struct
#endif
            =>
            BinarySearch((ReadOnlyExSpan<T>)span, value, comparer);

        /// <summary>
        /// Searches an entire sorted <see cref="ReadOnlyExSpan{T}"/> for a value
        /// using the specified <see cref="IComparable{T}"/> generic interface.
        /// </summary>
        /// <typeparam name="T">The element type of the span.</typeparam>
        /// <param name="span">The sorted <see cref="ReadOnlyExSpan{T}"/> to search.</param>
        /// <param name="comparable">The <see cref="IComparable{T}"/> to use when comparing.</param>
        /// <returns>
        /// The zero-based index of <paramref name="comparable"/> in the sorted <paramref name="span"/>,
        /// if <paramref name="comparable"/> is found; otherwise, a negative number that is the bitwise complement
        /// of the index of the next element that is larger than <paramref name="comparable"/> or, if there is
        /// no larger element, the bitwise complement of <see cref="ReadOnlyExSpan{T}.Length"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name = "comparable" /> is <see langword="null"/> .
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSize BinarySearch<T>(
            this ReadOnlyExSpan<T> span, IComparable<T> comparable) =>
            BinarySearch<T, IComparable<T>>(span, comparable);

        /// <summary>
        /// Searches an entire sorted <see cref="ReadOnlyExSpan{T}"/> for a value
        /// using the specified <typeparamref name="TComparable"/> generic type.
        /// </summary>
        /// <typeparam name="T">The element type of the span.</typeparam>
        /// <typeparam name="TComparable">The specific type of <see cref="IComparable{T}"/>.</typeparam>
        /// <param name="span">The sorted <see cref="ReadOnlyExSpan{T}"/> to search.</param>
        /// <param name="comparable">The <typeparamref name="TComparable"/> to use when comparing.</param>
        /// <returns>
        /// The zero-based index of <paramref name="comparable"/> in the sorted <paramref name="span"/>,
        /// if <paramref name="comparable"/> is found; otherwise, a negative number that is the bitwise complement
        /// of the index of the next element that is larger than <paramref name="comparable"/> or, if there is
        /// no larger element, the bitwise complement of <see cref="ReadOnlyExSpan{T}.Length"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name = "comparable" /> is <see langword="null"/> .
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSize BinarySearch<T, TComparable>(
            this ReadOnlyExSpan<T> span, TComparable comparable)
            where TComparable : IComparable<T>
#if ALLOWS_REF_STRUCT
            , allows ref struct
#endif
            {
            return ExSpanHelpers.BinarySearch(span, comparable);
        }

        /// <summary>
        /// Searches an entire sorted <see cref="ReadOnlyExSpan{T}"/> for the specified <paramref name="value"/>
        /// using the specified <typeparamref name="TComparer"/> generic type.
        /// </summary>
        /// <typeparam name="T">The element type of the span.</typeparam>
        /// <typeparam name="TComparer">The specific type of <see cref="IComparer{T}"/>.</typeparam>
        /// <param name="span">The sorted <see cref="ReadOnlyExSpan{T}"/> to search.</param>
        /// <param name="value">The object to locate. The value can be null for reference types.</param>
        /// <param name="comparer">The <typeparamref name="TComparer"/> to use when comparing.</param>
        /// <returns>
        /// The zero-based index of <paramref name="value"/> in the sorted <paramref name="span"/>,
        /// if <paramref name="value"/> is found; otherwise, a negative number that is the bitwise complement
        /// of the index of the next element that is larger than <paramref name="value"/> or, if there is
        /// no larger element, the bitwise complement of <see cref="ReadOnlyExSpan{T}.Length"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name = "comparer" /> is <see langword="null"/> .
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSize BinarySearch<T, TComparer>(
            this ReadOnlyExSpan<T> span, T value, TComparer comparer)
            where TComparer : IComparer<T>
#if ALLOWS_REF_STRUCT
            , allows ref struct
#endif
            {
            if (comparer is null)
                throw new ArgumentNullException(nameof(comparer));

            var comparable = new ExSpanHelpers.ComparerComparable<T, TComparer>(
                value, comparer);
            return BinarySearch(span, comparable);
        }

        /// <summary>
        /// Sorts the elements in the entire <see cref="ExSpan{T}" /> using the <see cref="IComparable{T}" /> implementation
        /// of each element of the <see cref= "ExSpan{T}" />
        /// </summary>
        /// <typeparam name="T">The type of the elements of the span.</typeparam>
        /// <param name="span">The <see cref="ExSpan{T}"/> to sort.</param>
        /// <exception cref="InvalidOperationException">
        /// One or more elements in <paramref name="span"/> do not implement the <see cref="IComparable{T}" /> interface.
        /// </exception>
        public static void Sort<T>(this ExSpan<T> span) =>
            Sort(span, (IComparer<T>?)null);

        /// <summary>
        /// Sorts the elements in the entire <see cref="ExSpan{T}" /> using the <typeparamref name="TComparer" />.
        /// </summary>
        /// <typeparam name="T">The type of the elements of the span.</typeparam>
        /// <typeparam name="TComparer">The type of the comparer to use to compare elements.</typeparam>
        /// <param name="span">The <see cref="ExSpan{T}"/> to sort.</param>
        /// <param name="comparer">
        /// The <see cref="IComparer{T}"/> implementation to use when comparing elements, or null to
        /// use the <see cref="IComparable{T}"/> interface implementation of each element.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="comparer"/> is null, and one or more elements in <paramref name="span"/> do not
        /// implement the <see cref="IComparable{T}" /> interface.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The implementation of <paramref name="comparer"/> caused an error during the sort.
        /// </exception>
        public static void Sort<T, TComparer>(this ExSpan<T> span, TComparer comparer) where TComparer : IComparer<T>? {
            if (span.Length > 1) {
                ExArraySortHelper<T>.Default.Sort(span, comparer); // value-type comparer will be boxed
            }
        }

        /// <summary>
        /// Sorts the elements in the entire <see cref="ExSpan{T}" /> using the specified <see cref="Comparison{T}" />.
        /// </summary>
        /// <typeparam name="T">The type of the elements of the span.</typeparam>
        /// <param name="span">The <see cref="ExSpan{T}"/> to sort.</param>
        /// <param name="comparison">The <see cref="Comparison{T}"/> to use when comparing elements.</param>
        /// <exception cref="ArgumentNullException"><paramref name="comparison"/> is null.</exception>
        public static void Sort<T>(this ExSpan<T> span, Comparison<T> comparison) {
            if (comparison == null)
                throw new ArgumentNullException(nameof(comparison));

            if (span.Length > 1) {
                ExArraySortHelper<T>.Sort(span, comparison);
            }
        }

        /// <summary>
        /// Sorts a pair of ExSpans (one containing the keys and the other containing the corresponding items)
        /// based on the keys in the first <see cref="ExSpan{TKey}" /> using the <see cref="IComparable{T}" />
        /// implementation of each key.
        /// </summary>
        /// <typeparam name="TKey">The type of the elements of the key span.</typeparam>
        /// <typeparam name="TValue">The type of the elements of the items span.</typeparam>
        /// <param name="keys">The span that contains the keys to sort.</param>
        /// <param name="items">The span that contains the items that correspond to the keys in <paramref name="keys"/>.</param>
        /// <exception cref="ArgumentException">
        /// The length of <paramref name="keys"/> isn't equal to the length of <paramref name="items"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// One or more elements in <paramref name="keys"/> do not implement the <see cref="IComparable{T}" /> interface.
        /// </exception>
        public static void Sort<TKey, TValue>(this ExSpan<TKey> keys, ExSpan<TValue> items) =>
            Sort(keys, items, (IComparer<TKey>?)null);

        /// <summary>
        /// Sorts a pair of ExSpans (one containing the keys and the other containing the corresponding items)
        /// based on the keys in the first <see cref="ExSpan{TKey}" /> using the specified comparer.
        /// </summary>
        /// <typeparam name="TKey">The type of the elements of the key span.</typeparam>
        /// <typeparam name="TValue">The type of the elements of the items span.</typeparam>
        /// <typeparam name="TComparer">The type of the comparer to use to compare elements.</typeparam>
        /// <param name="keys">The span that contains the keys to sort.</param>
        /// <param name="items">The span that contains the items that correspond to the keys in <paramref name="keys"/>.</param>
        /// <param name="comparer">
        /// The <see cref="IComparer{T}"/> implementation to use when comparing elements, or null to
        /// use the <see cref="IComparable{T}"/> interface implementation of each element.
        /// </param>
        /// <exception cref="ArgumentException">
        /// The length of <paramref name="keys"/> isn't equal to the length of <paramref name="items"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="comparer"/> is null, and one or more elements in <paramref name="keys"/> do not
        /// implement the <see cref="IComparable{T}" /> interface.
        /// </exception>
        public static void Sort<TKey, TValue, TComparer>(this ExSpan<TKey> keys, ExSpan<TValue> items, TComparer comparer) where TComparer : IComparer<TKey>? {
            if (keys.Length != items.Length)
                ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_SpansMustHaveSameLength);

            if (keys.Length > 1) {
                ExArraySortHelper<TKey, TValue>.Default.Sort(keys, items, comparer); // value-type comparer will be boxed
            }
        }

        /// <summary>
        /// Sorts a pair of ExSpans (one containing the keys and the other containing the corresponding items)
        /// based on the keys in the first <see cref="ExSpan{TKey}" /> using the specified comparison.
        /// </summary>
        /// <typeparam name="TKey">The type of the elements of the key span.</typeparam>
        /// <typeparam name="TValue">The type of the elements of the items span.</typeparam>
        /// <param name="keys">The span that contains the keys to sort.</param>
        /// <param name="items">The span that contains the items that correspond to the keys in <paramref name="keys"/>.</param>
        /// <param name="comparison">The <see cref="Comparison{T}"/> to use when comparing elements.</param>
        /// <exception cref="ArgumentNullException"><paramref name="comparison"/> is null.</exception>
        /// <exception cref="ArgumentException">
        /// The length of <paramref name="keys"/> isn't equal to the length of <paramref name="items"/>.
        /// </exception>
        public static void Sort<TKey, TValue>(this ExSpan<TKey> keys, ExSpan<TValue> items, Comparison<TKey> comparison) {
            if (comparison == null)
                throw new ArgumentNullException(nameof(comparison));
            if (keys.Length != items.Length)
                ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_SpansMustHaveSameLength);

            if (keys.Length > 1) {
                ExArraySortHelper<TKey, TValue>.Default.Sort(keys, items, new ComparisonComparer<TKey>(comparison));
            }
        }

        /// <summary>
        /// Replaces all occurrences of <paramref name="oldValue"/> with <paramref name="newValue"/>.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the span.</typeparam>
        /// <param name="span">The span in which the elements should be replaced.</param>
        /// <param name="oldValue">The value to be replaced with <paramref name="newValue"/>.</param>
        /// <param name="newValue">The value to replace all occurrences of <paramref name="oldValue"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Replace<T>(this ExSpan<T> span, T oldValue, T newValue) where T : IEquatable<T>? {
            nuint length = (nuint)span.Length;

            if (TypeHelper.IsBitwiseEquatable<T>()) {
                if (Unsafe.SizeOf<T>() == sizeof(byte)) {
                    ref byte src = ref Unsafe.As<T, byte>(ref ExMemoryMarshal.GetReference(span));
                    ExSpanHelpers.ReplaceValueType(
                        ref src,
                        ref src,
                        ExUnsafe.BitCast<T, byte>(oldValue),
                        ExUnsafe.BitCast<T, byte>(newValue),
                        length);
                    return;
                } else if (Unsafe.SizeOf<T>() == sizeof(ushort)) {
                    // Use ushort rather than short, as this avoids a sign-extending move.
                    ref ushort src = ref Unsafe.As<T, ushort>(ref ExMemoryMarshal.GetReference(span));
                    ExSpanHelpers.ReplaceValueType(
                        ref src,
                        ref src,
                        ExUnsafe.BitCast<T, ushort>(oldValue),
                        ExUnsafe.BitCast<T, ushort>(newValue),
                        length);
                    return;
                } else if (Unsafe.SizeOf<T>() == sizeof(int)) {
                    ref int src = ref Unsafe.As<T, int>(ref ExMemoryMarshal.GetReference(span));
                    ExSpanHelpers.ReplaceValueType(
                        ref src,
                        ref src,
                        ExUnsafe.BitCast<T, int>(oldValue),
                        ExUnsafe.BitCast<T, int>(newValue),
                        length);
                    return;
                } else if (Unsafe.SizeOf<T>() == sizeof(long)) {
                    ref long src = ref Unsafe.As<T, long>(ref ExMemoryMarshal.GetReference(span));
                    ExSpanHelpers.ReplaceValueType(
                        ref src,
                        ref src,
                        ExUnsafe.BitCast<T, long>(oldValue),
                        ExUnsafe.BitCast<T, long>(newValue),
                        length);
                    return;
                }
            }

            ref T src2 = ref ExMemoryMarshal.GetReference(span);
            ExSpanHelpers.Replace(ref src2, ref src2, oldValue, newValue, length);
        }

        /// <summary>
        /// Replaces all occurrences of <paramref name="oldValue"/> with <paramref name="newValue"/>.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the span.</typeparam>
        /// <param name="span">The span in which the elements should be replaced.</param>
        /// <param name="oldValue">The value to be replaced with <paramref name="newValue"/>.</param>
        /// <param name="newValue">The value to replace all occurrences of <paramref name="oldValue"/>.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default <see cref="IEqualityComparer{T}"/> for the type of an element.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Replace<T>(this ExSpan<T> span, T oldValue, T newValue, IEqualityComparer<T>? comparer = null) {
            if (TypeHelper.IsValueType<T>() && (comparer is null || comparer == EqualityComparer<T>.Default)) {
                if (TypeHelper.IsBitwiseEquatable<T>()) {
                    if (Unsafe.SizeOf<T>() == sizeof(byte)) {
                        ref byte src = ref Unsafe.As<T, byte>(ref ExMemoryMarshal.GetReference(span));
                        ExSpanHelpers.ReplaceValueType(
                            ref src,
                            ref src,
                            ExUnsafe.BitCast<T, byte>(oldValue),
                            ExUnsafe.BitCast<T, byte>(newValue),
                            (nuint)span.Length);
                        return;
                    } else if (Unsafe.SizeOf<T>() == sizeof(ushort)) {
                        // Use ushort rather than short, as this avoids a sign-extending move.
                        ref ushort src = ref Unsafe.As<T, ushort>(ref ExMemoryMarshal.GetReference(span));
                        ExSpanHelpers.ReplaceValueType(
                            ref src,
                            ref src,
                            ExUnsafe.BitCast<T, ushort>(oldValue),
                            ExUnsafe.BitCast<T, ushort>(newValue),
                            (nuint)span.Length);
                        return;
                    } else if (Unsafe.SizeOf<T>() == sizeof(int)) {
                        ref int src = ref Unsafe.As<T, int>(ref ExMemoryMarshal.GetReference(span));
                        ExSpanHelpers.ReplaceValueType(
                            ref src,
                            ref src,
                            ExUnsafe.BitCast<T, int>(oldValue),
                            ExUnsafe.BitCast<T, int>(newValue),
                            (nuint)span.Length);
                        return;
                    } else if (Unsafe.SizeOf<T>() == sizeof(long)) {
                        ref long src = ref Unsafe.As<T, long>(ref ExMemoryMarshal.GetReference(span));
                        ExSpanHelpers.ReplaceValueType(
                            ref src,
                            ref src,
                            ExUnsafe.BitCast<T, long>(oldValue),
                            ExUnsafe.BitCast<T, long>(newValue),
                            (nuint)span.Length);
                        return;
                    }
                }

                ReplaceDefaultComparer(span, oldValue, newValue);
                static void ReplaceDefaultComparer(ExSpan<T> span, T oldValue, T newValue) {
                    for (TSize i = 0; i < span.Length; i++) {
                        if (EqualityComparer<T>.Default.Equals(span[i], oldValue)) {
                            span[i] = newValue;
                        }
                    }
                }
            } else {
                ReplaceComparer(span, oldValue, newValue, comparer);
                static void ReplaceComparer(ExSpan<T> span, T oldValue, T newValue, IEqualityComparer<T>? comparer) {
                    comparer ??= EqualityComparer<T>.Default;
                    for (TSize i = 0; i < span.Length; i++) {
                        if (comparer.Equals(span[i], oldValue)) {
                            span[i] = newValue;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Copies <paramref name="source"/> to <paramref name="destination"/>, replacing all occurrences of <paramref name="oldValue"/> with <paramref name="newValue"/>.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the spans.</typeparam>
        /// <param name="source">The span to copy.</param>
        /// <param name="destination">The span into which the copied and replaced values should be written.</param>
        /// <param name="oldValue">The value to be replaced with <paramref name="newValue"/>.</param>
        /// <param name="newValue">The value to replace all occurrences of <paramref name="oldValue"/>.</param>
        /// <exception cref="ArgumentException">The <paramref name="destination"/> ExSpan was shorter than the <paramref name="source"/> span.</exception>
        /// <exception cref="ArgumentException">The <paramref name="source"/> and <paramref name="destination"/> were overlapping but not referring to the same starting location.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Replace<T>(this ReadOnlyExSpan<T> source, ExSpan<T> destination, T oldValue, T newValue) where T : IEquatable<T>? {
            nuint length = (nuint)source.Length;
            if (length == 0) {
                return;
            }

            if (length > (nuint)destination.Length) {
                ThrowHelper.ThrowArgumentException_DestinationTooShort();
            }

            ref T src = ref ExMemoryMarshal.GetReference(source);
            ref T dst = ref ExMemoryMarshal.GetReference(destination);

            nint byteOffset = Unsafe.ByteOffset(ref src, ref dst);
            if (byteOffset != 0 &&
                ((nuint)byteOffset < (nuint)((nint)source.Length * Unsafe.SizeOf<T>()) ||
                 (nuint)byteOffset > (nuint)(-((nint)destination.Length * Unsafe.SizeOf<T>())))) {
                ThrowHelper.ThrowArgumentException(ExceptionResource.InvalidOperation_SpanOverlappedOperation);
            }

            if (TypeHelper.IsBitwiseEquatable<T>()) {
                if (Unsafe.SizeOf<T>() == sizeof(byte)) {
                    ExSpanHelpers.ReplaceValueType(
                        ref Unsafe.As<T, byte>(ref src),
                        ref Unsafe.As<T, byte>(ref dst),
                        ExUnsafe.BitCast<T, byte>(oldValue),
                        ExUnsafe.BitCast<T, byte>(newValue),
                        length);
                    return;
                } else if (Unsafe.SizeOf<T>() == sizeof(ushort)) {
                    // Use ushort rather than short, as this avoids a sign-extending move.
                    ExSpanHelpers.ReplaceValueType(
                        ref Unsafe.As<T, ushort>(ref src),
                        ref Unsafe.As<T, ushort>(ref dst),
                        ExUnsafe.BitCast<T, ushort>(oldValue),
                        ExUnsafe.BitCast<T, ushort>(newValue),
                        length);
                    return;
                } else if (Unsafe.SizeOf<T>() == sizeof(int)) {
                    ExSpanHelpers.ReplaceValueType(
                        ref Unsafe.As<T, int>(ref src),
                        ref Unsafe.As<T, int>(ref dst),
                        ExUnsafe.BitCast<T, int>(oldValue),
                        ExUnsafe.BitCast<T, int>(newValue),
                        length);
                    return;
                } else if (Unsafe.SizeOf<T>() == sizeof(long)) {
                    ExSpanHelpers.ReplaceValueType(
                        ref Unsafe.As<T, long>(ref src),
                        ref Unsafe.As<T, long>(ref dst),
                        ExUnsafe.BitCast<T, long>(oldValue),
                        ExUnsafe.BitCast<T, long>(newValue),
                        length);
                    return;
                }
            }

            ExSpanHelpers.Replace(ref src, ref dst, oldValue, newValue, length);
        }

        /// <summary>
        /// Copies <paramref name="source"/> to <paramref name="destination"/>, replacing all occurrences of <paramref name="oldValue"/> with <paramref name="newValue"/>.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the spans.</typeparam>
        /// <param name="source">The span to copy.</param>
        /// <param name="destination">The span into which the copied and replaced values should be written.</param>
        /// <param name="oldValue">The value to be replaced with <paramref name="newValue"/>.</param>
        /// <param name="newValue">The value to replace all occurrences of <paramref name="oldValue"/>.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default <see cref="IEqualityComparer{T}"/> for the type of an element.</param>
        /// <exception cref="ArgumentException">The <paramref name="destination"/> ExSpan was shorter than the <paramref name="source"/> span.</exception>
        /// <exception cref="ArgumentException">The <paramref name="source"/> and <paramref name="destination"/> were overlapping but not referring to the same starting location.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Replace<T>(this ReadOnlyExSpan<T> source, ExSpan<T> destination, T oldValue, T newValue, IEqualityComparer<T>? comparer = null) {
            nuint length = (nuint)source.Length;
            if (length == 0) {
                return;
            }

            if (length > (nuint)destination.Length) {
                ThrowHelper.ThrowArgumentException_DestinationTooShort();
            }

            ref T src = ref ExMemoryMarshal.GetReference(source);
            ref T dst = ref ExMemoryMarshal.GetReference(destination);

            nint byteOffset = Unsafe.ByteOffset(ref src, ref dst);
            if (byteOffset != 0 &&
                ((nuint)byteOffset < (nuint)((nint)source.Length * Unsafe.SizeOf<T>()) ||
                 (nuint)byteOffset > (nuint)(-((nint)destination.Length * Unsafe.SizeOf<T>())))) {
                ThrowHelper.ThrowArgumentException(ExceptionResource.InvalidOperation_SpanOverlappedOperation);
            }

            if (TypeHelper.IsValueType<T>() && (comparer is null || comparer == EqualityComparer<T>.Default)) {
                if (TypeHelper.IsBitwiseEquatable<T>()) {
                    if (Unsafe.SizeOf<T>() == sizeof(byte)) {
                        ExSpanHelpers.ReplaceValueType(
                            ref Unsafe.As<T, byte>(ref src),
                            ref Unsafe.As<T, byte>(ref dst),
                            ExUnsafe.BitCast<T, byte>(oldValue),
                            ExUnsafe.BitCast<T, byte>(newValue),
                            length);
                        return;
                    } else if (Unsafe.SizeOf<T>() == sizeof(ushort)) {
                        // Use ushort rather than short, as this avoids a sign-extending move.
                        ExSpanHelpers.ReplaceValueType(
                            ref Unsafe.As<T, ushort>(ref src),
                            ref Unsafe.As<T, ushort>(ref dst),
                            ExUnsafe.BitCast<T, ushort>(oldValue),
                            ExUnsafe.BitCast<T, ushort>(newValue),
                            length);
                        return;
                    } else if (Unsafe.SizeOf<T>() == sizeof(int)) {
                        ExSpanHelpers.ReplaceValueType(
                            ref Unsafe.As<T, int>(ref src),
                            ref Unsafe.As<T, int>(ref dst),
                            ExUnsafe.BitCast<T, int>(oldValue),
                            ExUnsafe.BitCast<T, int>(newValue),
                            length);
                        return;
                    } else if (Unsafe.SizeOf<T>() == sizeof(long)) {
                        ExSpanHelpers.ReplaceValueType(
                            ref Unsafe.As<T, long>(ref src),
                            ref Unsafe.As<T, long>(ref dst),
                            ExUnsafe.BitCast<T, long>(oldValue),
                            ExUnsafe.BitCast<T, long>(newValue),
                            length);
                        return;
                    }
                }

                ReplaceDefaultComparer(source, destination, oldValue, newValue);
                static void ReplaceDefaultComparer(ReadOnlyExSpan<T> source, ExSpan<T> destination, T oldValue, T newValue) {
                    for (TSize i = 0; i < source.Length; i++) {
                        destination[i] = EqualityComparer<T>.Default.Equals(source[i], oldValue) ? newValue : source[i];
                    }
                }
            } else {
                ReplaceComparer(source, destination, oldValue, newValue, comparer);
                static void ReplaceComparer(ReadOnlyExSpan<T> source, ExSpan<T> destination, T oldValue, T newValue, IEqualityComparer<T>? comparer) {
                    comparer ??= EqualityComparer<T>.Default;
                    for (TSize i = 0; i < source.Length; i++) {
                        destination[i] = comparer.Equals(source[i], oldValue) ? newValue : source[i];
                    }
                }
            }
        }

#if NET8_0_OR_GREATER && TODO // [TODO why] SearchValues.IndexOfAny is internal
        /// <summary>
        /// Copies <paramref name="source"/> to <paramref name="destination"/>, replacing all occurrences of any of the
        /// elements in <paramref name="values"/> with <paramref name="newValue"/>.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the spans.</typeparam>
        /// <param name="source">The span to copy.</param>
        /// <param name="destination">The span into which the copied and replaced values should be written.</param>
        /// <param name="values">The values to be replaced with <paramref name="newValue"/>.</param>
        /// <param name="newValue">The value to replace all occurrences of any of the elements in <paramref name="values"/>.</param>
        /// <exception cref="ArgumentException">The <paramref name="destination"/> ExSpan was shorter than the <paramref name="source"/> span.</exception>
        /// <exception cref="ArgumentException">
        /// The <paramref name="source"/> and <paramref name="destination"/> were overlapping but not referring to the same starting location.
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="values"/> is <see langword="null"/>.</exception>
        public static void ReplaceAny<T>(this ReadOnlyExSpan<T> source, ExSpan<T> destination, SearchValues<T> values, T newValue) where T : IEquatable<T>? {
            if (source.Length > destination.Length) {
                ThrowHelper.ThrowArgumentException_DestinationTooShort();
            }

            if (!Unsafe.AreSame(ref source._reference, ref destination._reference) &&
                source.Overlaps(destination)) {
                ThrowHelper.ThrowArgumentException(ExceptionResource.InvalidOperation_SpanOverlappedOperation);
            }

            source.CopyTo(destination);
            ReplaceAny(destination.Slice(0, source.Length), values, newValue);
        }

        /// <summary>
        /// Replaces in <paramref name="span"/> all occurrences of any of the
        /// elements in <paramref name="values"/> with <paramref name="newValue"/>.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the spans.</typeparam>
        /// <param name="span">The span to edit.</param>
        /// <param name="values">The values to be replaced with <paramref name="newValue"/>.</param>
        /// <param name="newValue">The value to replace all occurrences of any of the elements in <paramref name="values"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="values"/> is <see langword="null"/>.</exception>
        public static void ReplaceAny<T>(this ExSpan<T> span, SearchValues<T> values, T newValue) where T : IEquatable<T>? {
            int pos;
            while ((pos = span.IndexOfAny(values)) >= 0) {
                span[pos] = newValue;
                span = span.Slice(pos + 1);
            }
        }

        /// <summary>
        /// Copies <paramref name="source"/> to <paramref name="destination"/>, replacing all occurrences of any of the
        /// elements other than those in <paramref name="values"/> with <paramref name="newValue"/>.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the spans.</typeparam>
        /// <param name="source">The span to copy.</param>
        /// <param name="destination">The span into which the copied and replaced values should be written.</param>
        /// <param name="values">The values to be excluded from replacement with <paramref name="newValue"/>.</param>
        /// <param name="newValue">The value to replace all occurrences of any elements other than those in <paramref name="values"/>.</param>
        /// <exception cref="ArgumentException">The <paramref name="destination"/> ExSpan was shorter than the <paramref name="source"/> span.</exception>
        /// <exception cref="ArgumentException">
        /// The <paramref name="source"/> and <paramref name="destination"/> were overlapping but not referring to the same starting location.
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="values"/> is <see langword="null"/>.</exception>
        public static void ReplaceAnyExcept<T>(this ReadOnlyExSpan<T> source, ExSpan<T> destination, SearchValues<T> values, T newValue) where T : IEquatable<T>? {
            if (source.Length > destination.Length) {
                ThrowHelper.ThrowArgumentException_DestinationTooShort();
            }

            if (!Unsafe.AreSame(ref source._reference, ref destination._reference) &&
                source.Overlaps(destination)) {
                ThrowHelper.ThrowArgumentException(ExceptionResource.InvalidOperation_SpanOverlappedOperation);
            }

            source.CopyTo(destination);
            ReplaceAnyExcept(destination.Slice(0, source.Length), values, newValue);
        }

        /// <summary>
        /// Replaces in <paramref name="span"/> all elements, other than those in <paramref name="values"/>, with <paramref name="newValue"/>.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the spans.</typeparam>
        /// <param name="span">The span to edit.</param>
        /// <param name="values">The values to be excluded from replacement with <paramref name="newValue"/>.</param>
        /// <param name="newValue">The value to replace all occurrences of any elements other than those in <paramref name="values"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="values"/> is <see langword="null"/>.</exception>
        public static void ReplaceAnyExcept<T>(this ExSpan<T> span, SearchValues<T> values, T newValue) where T : IEquatable<T>? {
            int pos;
            while ((pos = span.IndexOfAnyExcept(values)) >= 0) {
                span[pos] = newValue;
                span = span.Slice(pos + 1);
            }
        }
#endif // NET8_0_OR_GREATER

        /// <summary>Finds the length of any common prefix shared between <paramref name="span"/> and <paramref name="other"/>.</summary>
        /// <typeparam name="T">The type of the elements in the spans.</typeparam>
        /// <param name="span">The first sequence to compare.</param>
        /// <param name="other">The second sequence to compare.</param>
        /// <returns>The length of the common prefix shared by the two ExSpans.  If there's no shared prefix, 0 is returned.</returns>
        [OverloadResolutionPriority(-1)]
        public static TSize CommonPrefixLength<T>(this ExSpan<T> span, ReadOnlyExSpan<T> other) =>
            CommonPrefixLength((ReadOnlyExSpan<T>)span, other);

        /// <summary>Finds the length of any common prefix shared between <paramref name="span"/> and <paramref name="other"/>.</summary>
        /// <typeparam name="T">The type of the elements in the spans.</typeparam>
        /// <param name="span">The first sequence to compare.</param>
        /// <param name="other">The second sequence to compare.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default <see cref="IEqualityComparer{T}"/> for the type of an element.</param>
        /// <returns>The length of the common prefix shared by the two ExSpans.  If there's no shared prefix, 0 is returned.</returns>
        [OverloadResolutionPriority(-1)]
        public static TSize CommonPrefixLength<T>(this ExSpan<T> span, ReadOnlyExSpan<T> other, IEqualityComparer<T>? comparer) =>
            CommonPrefixLength((ReadOnlyExSpan<T>)span, other, comparer);

        /// <summary>Finds the length of any common prefix shared between <paramref name="span"/> and <paramref name="other"/>.</summary>
        /// <typeparam name="T">The type of the elements in the spans.</typeparam>
        /// <param name="span">The first sequence to compare.</param>
        /// <param name="other">The second sequence to compare.</param>
        /// <returns>The length of the common prefix shared by the two ExSpans.  If there's no shared prefix, 0 is returned.</returns>
        public static TSize CommonPrefixLength<T>(this ReadOnlyExSpan<T> span, ReadOnlyExSpan<T> other) {
            if (TypeHelper.IsBitwiseEquatable<T>()) {
                nuint length = BitMath.Min((nuint)span.Length, (nuint)other.Length);
                nuint size = (uint)Unsafe.SizeOf<T>();
                nuint index = ExSpanHelpers.CommonPrefixLength(
                    ref Unsafe.As<T, byte>(ref ExMemoryMarshal.GetReference(span)),
                    ref Unsafe.As<T, byte>(ref ExMemoryMarshal.GetReference(other)),
                    length * size);

                // A byte-wise comparison in CommonPrefixLength can be used for multi-byte types,
                // that are bitwise-equatable, too. In order to get the correct index in terms of type T
                // of the first mismatch, integer division by the size of T is used.
                //
                // Example for short:
                // index (byte-based):   b-1,  b,    b+1,    b+2,  b+3
                // index (short-based):  s-1,  s,            s+1
                // byte sequence 1:    { ..., [0x42, 0x43], [0x37, 0x38], ... }
                // byte sequence 2:    { ..., [0x42, 0x43], [0x37, 0xAB], ... }
                // So the mismatch is a byte-index b+3, which gives integer divided by the size of short:
                // 3 / 2 = 1, thus the expected index short-based.
                return (int)(index / size);
            }

            // Shrink one of the spans if necessary to ensure they're both the same length. We can then iterate until
            // the Length of one of them and at least have bounds checks removed from that one.
            SliceLongerSpanToMatchShorterLength(ref span, ref other);

            // Find the first element pairwise that is not equal, and return its index as the length
            // of the sequence before it that matches.
            for (int i = 0; i < span.Length; i++) {
                if (!EqualityComparer<T>.Default.Equals(span[i], other[i])) {
                    return i;
                }
            }

            return span.Length;
        }

        /// <summary>Determines the length of any common prefix shared between <paramref name="span"/> and <paramref name="other"/>.</summary>
        /// <typeparam name="T">The type of the elements in the sequences.</typeparam>
        /// <param name="span">The first sequence to compare.</param>
        /// <param name="other">The second sequence to compare.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default <see cref="IEqualityComparer{T}"/> for the type of an element.</param>
        /// <returns>The length of the common prefix shared by the two ExSpans.  If there's no shared prefix, 0 is returned.</returns>
        public static TSize CommonPrefixLength<T>(this ReadOnlyExSpan<T> span, ReadOnlyExSpan<T> other, IEqualityComparer<T>? comparer) {
            // If the comparer is null or the default, and T is a value type, we want to use EqualityComparer<T>.Default.Equals
            // directly to enable devirtualization.  The non-comparer overload already does so, so just use it.
            if (TypeHelper.IsValueType<T>() && (comparer is null || comparer == EqualityComparer<T>.Default)) {
                return CommonPrefixLength(span, other);
            }

            // Shrink one of the spans if necessary to ensure they're both the same length. We can then iterate until
            // the Length of one of them and at least have bounds checks removed from that one.
            SliceLongerSpanToMatchShorterLength(ref span, ref other);

            // Ensure we have a comparer, then compare the spans.
            comparer ??= EqualityComparer<T>.Default;
            for (TSize i = 0; i < span.Length; i++) {
                if (!comparer.Equals(span[i], other[i])) {
                    return i;
                }
            }

            return span.Length;
        }

        /// <summary>Determines if one ExSpan is longer than the other, and slices the longer one to match the length of the shorter.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SliceLongerSpanToMatchShorterLength<T>(ref ReadOnlyExSpan<T> span, ref ReadOnlyExSpan<T> other) {
            if (other.Length > span.Length) {
                other = other.Slice(0, span.Length);
            } else if (span.Length > other.Length) {
                span = span.Slice(0, other.Length);
            }
            Debug.Assert(span.Length == other.Length);
        }

#if NET8_0_OR_GREATER && TODO // [TODO why] SearchValues.IndexOfAny is internal
        /// <summary>
        /// Returns a type that allows for enumeration of each element within a split span
        /// using the provided separator character.
        /// </summary>
        /// <typeparam name="T">The type of the elements.</typeparam>
        /// <param name="source">The source span to be enumerated.</param>
        /// <param name="separator">The separator character to be used to split the provided span.</param>
        /// <returns>Returns a <see cref="ExSpanSplitEnumerator{T}"/>.</returns>
        public static ExSpanSplitEnumerator<T> Split<T>(this ReadOnlyExSpan<T> source, T separator) where T : IEquatable<T> =>
            new ExSpanSplitEnumerator<T>(source, separator);

        /// <summary>
        /// Returns a type that allows for enumeration of each element within a split span
        /// using the provided separator span.
        /// </summary>
        /// <typeparam name="T">The type of the elements.</typeparam>
        /// <param name="source">The source span to be enumerated.</param>
        /// <param name="separator">The separator ExSpan to be used to split the provided span.</param>
        /// <returns>Returns a <see cref="ExSpanSplitEnumerator{T}"/>.</returns>
        public static ExSpanSplitEnumerator<T> Split<T>(this ReadOnlyExSpan<T> source, ReadOnlyExSpan<T> separator) where T : IEquatable<T> =>
            new ExSpanSplitEnumerator<T>(source, separator, treatAsSingleSeparator: true);

        /// <summary>
        /// Returns a type that allows for enumeration of each element within a split span
        /// using any of the provided elements.
        /// </summary>
        /// <typeparam name="T">The type of the elements.</typeparam>
        /// <param name="source">The source span to be enumerated.</param>
        /// <param name="separators">The separators to be used to split the provided span.</param>
        /// <returns>Returns a <see cref="ExSpanSplitEnumerator{T}"/>.</returns>
        /// <remarks>
        /// If <typeparamref name="T"/> is <see cref="char"/> and if <paramref name="separators"/> is empty,
        /// all Unicode whitespace characters are used as the separators. This matches the behavior of when
        /// <see cref="string.Split(char[])"/> and related overloads are used with an empty separator array,
        /// or when <see cref="SplitAny(ReadOnlyExSpan{char}, ExSpan{Range}, ReadOnlyExSpan{char}, StringSplitOptions)"/>
        /// is used with an empty separator span.
        /// </remarks>
        public static ExSpanSplitEnumerator<T> SplitAny<T>(this ReadOnlyExSpan<T> source, [UnscopedRef] params ReadOnlyExSpan<T> separators) where T : IEquatable<T> =>
            new ExSpanSplitEnumerator<T>(source, separators);

        /// <summary>
        /// Returns a type that allows for enumeration of each element within a split span
        /// using the provided <see cref="ExSpanSplitEnumerator{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of the elements.</typeparam>
        /// <param name="source">The source span to be enumerated.</param>
        /// <param name="separators">The <see cref="ExSpanSplitEnumerator{T}"/> to be used to split the provided span.</param>
        /// <returns>Returns a <see cref="ExSpanSplitEnumerator{T}"/>.</returns>
        /// <remarks>
        /// Unlike <see cref="SplitAny{T}(ReadOnlyExSpan{T}, ReadOnlyExSpan{T})"/>, the <paramref name="separators"/> is not checked for being empty.
        /// An empty <paramref name="separators"/> will result in no separators being found, regardless of the type of <typeparamref name="T"/>,
        /// whereas <see cref="SplitAny{T}(ReadOnlyExSpan{T}, ReadOnlyExSpan{T})"/> will use all Unicode whitespace characters as separators if <paramref name="separators"/> is
        /// empty and <typeparamref name="T"/> is <see cref="char"/>.
        /// </remarks>
        public static ExSpanSplitEnumerator<T> SplitAny<T>(this ReadOnlyExSpan<T> source, SearchValues<T> separators) where T : IEquatable<T> =>
            new ExSpanSplitEnumerator<T>(source, separators);
#endif // NET8_0_OR_GREATER

#if NET8_0_OR_GREATER && TODO // [TODO why] NRange need System.Numerics.Tensors.dll
        /// <summary>
        /// Parses the source <see cref="ReadOnlyExSpan{Char}"/> for the specified <paramref name="separator"/>, populating the <paramref name="destination"/> ExSpan
        /// with <see cref="Range"/> instances representing the regions between the separators.
        /// </summary>
        /// <param name="source">The source span to parse.</param>
        /// <param name="destination">The destination ExSpan into which the resulting ranges are written.</param>
        /// <param name="separator">A character that delimits the regions in this instance.</param>
        /// <param name="options">A bitwise combination of the enumeration values that specifies whether to trim whitespace and include empty ranges.</param>
        /// <returns>The number of ranges written into <paramref name="destination"/>.</returns>
        /// <remarks>
        /// <para>
        /// Delimiter characters are not included in the elements of the returned array.
        /// </para>
        /// <para>
        /// If the <paramref name="destination"/> ExSpan is empty, or if the <paramref name="options"/> specifies <see cref="StringSplitOptions.RemoveEmptyEntries"/> and <paramref name="source"/> is empty,
        /// or if <paramref name="options"/> specifies both <see cref="StringSplitOptions.RemoveEmptyEntries"/> and <see cref="StringSplitOptions.TrimEntries"/> and the <paramref name="source"/> is
        /// entirely whitespace, no ranges are written to the destination.
        /// </para>
        /// <para>
        /// If the span does not contain <paramref name="separator"/>, or if <paramref name="destination"/>'s length is 1, a single range will be output containing the entire <paramref name="source"/>,
        /// subject to the processing implied by <paramref name="options"/>.
        /// </para>
        /// <para>
        /// If there are more regions in <paramref name="source"/> than will fit in <paramref name="destination"/>, the first <paramref name="destination"/> length minus 1 ranges are
        /// stored in <paramref name="destination"/>, and a range for the remainder of <paramref name="source"/> is stored in <paramref name="destination"/>.
        /// </para>
        /// </remarks>
        public static int Split(this ReadOnlyExSpan<char> source, ExSpan<Range> destination, char separator, StringSplitOptions options = StringSplitOptions.None) {
            string.CheckStringSplitOptions(options);

            return SplitCore(source, destination, new ReadOnlyExSpan<char>(in separator), default, isAny: true, options);
        }

        /// <summary>
        /// Parses the source <see cref="ReadOnlyExSpan{Char}"/> for the specified <paramref name="separator"/>, populating the <paramref name="destination"/> ExSpan
        /// with <see cref="Range"/> instances representing the regions between the separators.
        /// </summary>
        /// <param name="source">The source span to parse.</param>
        /// <param name="destination">The destination ExSpan into which the resulting ranges are written.</param>
        /// <param name="separator">A character that delimits the regions in this instance.</param>
        /// <param name="options">A bitwise combination of the enumeration values that specifies whether to trim whitespace and include empty ranges.</param>
        /// <returns>The number of ranges written into <paramref name="destination"/>.</returns>
        /// <remarks>
        /// <para>
        /// Delimiter characters are not included in the elements of the returned array.
        /// </para>
        /// <para>
        /// If the <paramref name="destination"/> ExSpan is empty, or if the <paramref name="options"/> specifies <see cref="StringSplitOptions.RemoveEmptyEntries"/> and <paramref name="source"/> is empty,
        /// or if <paramref name="options"/> specifies both <see cref="StringSplitOptions.RemoveEmptyEntries"/> and <see cref="StringSplitOptions.TrimEntries"/> and the <paramref name="source"/> is
        /// entirely whitespace, no ranges are written to the destination.
        /// </para>
        /// <para>
        /// If the span does not contain <paramref name="separator"/>, or if <paramref name="destination"/>'s length is 1, a single range will be output containing the entire <paramref name="source"/>,
        /// subject to the processing implied by <paramref name="options"/>.
        /// </para>
        /// <para>
        /// If there are more regions in <paramref name="source"/> than will fit in <paramref name="destination"/>, the first <paramref name="destination"/> length minus 1 ranges are
        /// stored in <paramref name="destination"/>, and a range for the remainder of <paramref name="source"/> is stored in <paramref name="destination"/>.
        /// </para>
        /// </remarks>
        public static int Split(this ReadOnlyExSpan<char> source, ExSpan<Range> destination, ReadOnlyExSpan<char> separator, StringSplitOptions options = StringSplitOptions.None) {
            string.CheckStringSplitOptions(options);

            // If the separator is an empty string, the whole input is considered the sole range.
            if (separator.IsEmpty) {
                if (!destination.IsEmpty) {
                    int startInclusive = 0, endExclusive = source.Length;

                    if ((options & StringSplitOptions.TrimEntries) != 0) {
                        (startInclusive, endExclusive) = TrimSplitEntry(source, startInclusive, endExclusive);
                    }

                    if (startInclusive != endExclusive || (options & StringSplitOptions.RemoveEmptyEntries) == 0) {
                        destination[0] = startInclusive..endExclusive;
                        return 1;
                    }
                }

                return 0;
            }

            return SplitCore(source, destination, separator, default, isAny: false, options);
        }

        /// <summary>
        /// Parses the source <see cref="ReadOnlyExSpan{Char}"/> for one of the specified <paramref name="separators"/>, populating the <paramref name="destination"/> ExSpan
        /// with <see cref="Range"/> instances representing the regions between the separators.
        /// </summary>
        /// <param name="source">The source span to parse.</param>
        /// <param name="destination">The destination ExSpan into which the resulting ranges are written.</param>
        /// <param name="separators">Any number of characters that may delimit the regions in this instance. If empty, all Unicode whitespace characters are used as the separators.</param>
        /// <param name="options">A bitwise combination of the enumeration values that specifies whether to trim whitespace and include empty ranges.</param>
        /// <returns>The number of ranges written into <paramref name="destination"/>.</returns>
        /// <remarks>
        /// <para>
        /// Delimiter characters are not included in the elements of the returned array.
        /// </para>
        /// <para>
        /// If the <paramref name="destination"/> ExSpan is empty, or if the <paramref name="options"/> specifies <see cref="StringSplitOptions.RemoveEmptyEntries"/> and <paramref name="source"/> is empty,
        /// or if <paramref name="options"/> specifies both <see cref="StringSplitOptions.RemoveEmptyEntries"/> and <see cref="StringSplitOptions.TrimEntries"/> and the <paramref name="source"/> is
        /// entirely whitespace, no ranges are written to the destination.
        /// </para>
        /// <para>
        /// If the span does not contain any of the <paramref name="separators"/>, or if <paramref name="destination"/>'s length is 1, a single range will be output containing the entire <paramref name="source"/>,
        /// subject to the processing implied by <paramref name="options"/>.
        /// </para>
        /// <para>
        /// If there are more regions in <paramref name="source"/> than will fit in <paramref name="destination"/>, the first <paramref name="destination"/> length minus 1 ranges are
        /// stored in <paramref name="destination"/>, and a range for the remainder of <paramref name="source"/> is stored in <paramref name="destination"/>.
        /// </para>
        /// </remarks>
        public static int SplitAny(this ReadOnlyExSpan<char> source, ExSpan<Range> destination, ReadOnlyExSpan<char> separators, StringSplitOptions options = StringSplitOptions.None) {
            string.CheckStringSplitOptions(options);

            // If the separators list is empty, whitespace is used as separators.  In that case, we want to ignore TrimEntries if specified,
            // since TrimEntries also impacts whitespace.  The TrimEntries flag must be left intact if we are constrained by count because we need to process last substring.
            if (separators.IsEmpty && destination.Length > source.Length) {
                options &= ~StringSplitOptions.TrimEntries;
            }

            return SplitCore(source, destination, separators, default, isAny: true, options);
        }

        /// <summary>
        /// Parses the source <see cref="ReadOnlyExSpan{Char}"/> for one of the specified <paramref name="separators"/>, populating the <paramref name="destination"/> ExSpan
        /// with <see cref="Range"/> instances representing the regions between the separators.
        /// </summary>
        /// <param name="source">The source span to parse.</param>
        /// <param name="destination">The destination ExSpan into which the resulting ranges are written.</param>
        /// <param name="separators">Any number of strings that may delimit the regions in this instance.  If empty, all Unicode whitespace characters are used as the separators.</param>
        /// <param name="options">A bitwise combination of the enumeration values that specifies whether to trim whitespace and include empty ranges.</param>
        /// <returns>The number of ranges written into <paramref name="destination"/>.</returns>
        /// <remarks>
        /// <para>
        /// Delimiter characters are not included in the elements of the returned array.
        /// </para>
        /// <para>
        /// If the <paramref name="destination"/> ExSpan is empty, or if the <paramref name="options"/> specifies <see cref="StringSplitOptions.RemoveEmptyEntries"/> and <paramref name="source"/> is empty,
        /// or if <paramref name="options"/> specifies both <see cref="StringSplitOptions.RemoveEmptyEntries"/> and <see cref="StringSplitOptions.TrimEntries"/> and the <paramref name="source"/> is
        /// entirely whitespace, no ranges are written to the destination.
        /// </para>
        /// <para>
        /// If the span does not contain any of the <paramref name="separators"/>, or if <paramref name="destination"/>'s length is 1, a single range will be output containing the entire <paramref name="source"/>,
        /// subject to the processing implied by <paramref name="options"/>.
        /// </para>
        /// <para>
        /// If there are more regions in <paramref name="source"/> than will fit in <paramref name="destination"/>, the first <paramref name="destination"/> length minus 1 ranges are
        /// stored in <paramref name="destination"/>, and a range for the remainder of <paramref name="source"/> is stored in <paramref name="destination"/>.
        /// </para>
        /// </remarks>
        public static int SplitAny(this ReadOnlyExSpan<char> source, ExSpan<Range> destination, ReadOnlyExSpan<string> separators, StringSplitOptions options = StringSplitOptions.None) {
            string.CheckStringSplitOptions(options);

            // If the separators list is empty, whitespace is used as separators.  In that case, we want to ignore TrimEntries if specified,
            // since TrimEntries also impacts whitespace.  The TrimEntries flag must be left intact if we are constrained by count because we need to process last substring.
            if (separators.IsEmpty && destination.Length > source.Length) {
                options &= ~StringSplitOptions.TrimEntries;
            }

            return SplitCore(source, destination, default, separators!, isAny: true, options);
        }

        /// <summary>Core implementation for all of the Split{Any}AsRanges methods.</summary>
        /// <param name="source">The source span to parse.</param>
        /// <param name="destination">The destination ExSpan into which the resulting ranges are written.</param>
        /// <param name="separatorOrSeparators">Either a single separator (one or more characters in length) or multiple individual 1-character separators.</param>
        /// <param name="stringSeparators">Strings to use as separators instead of <paramref name="separatorOrSeparators"/>.</param>
        /// <param name="isAny">true if the separators are a set; false if <paramref name="separatorOrSeparators"/> should be treated as a single separator.</param>
        /// <param name="options">A bitwise combination of the enumeration values that specifies whether to trim whitespace and include empty ranges.</param>
        /// <returns>The number of ranges written into <paramref name="destination"/>.</returns>
        /// <remarks>This implementation matches the various quirks of string.Split.</remarks>
        private static int SplitCore(
            ReadOnlyExSpan<char> source, ExSpan<Range> destination,
            ReadOnlyExSpan<char> separatorOrSeparators, ReadOnlyExSpan<string?> stringSeparators, bool isAny,
            StringSplitOptions options) {
            // If the destination is empty, there's nothing to do.
            if (destination.IsEmpty) {
                return 0;
            }

            bool keepEmptyEntries = (options & StringSplitOptions.RemoveEmptyEntries) == 0;
            bool trimEntries = (options & StringSplitOptions.TrimEntries) != 0;

            // If the input is empty, then we either return an empty range as the sole range, or if empty entries
            // are to be removed, we return nothing.
            if (source.Length == 0) {
                if (keepEmptyEntries) {
                    destination[0] = default;
                    return 1;
                }

                return 0;
            }

            int startInclusive = 0, endExclusive;

            // If the destination has only one slot, then we need to return the whole input, subject to the options.
            if (destination.Length == 1) {
                endExclusive = source.Length;
                if (trimEntries) {
                    (startInclusive, endExclusive) = TrimSplitEntry(source, startInclusive, endExclusive);
                }

                if (startInclusive != endExclusive || keepEmptyEntries) {
                    destination[0] = startInclusive..endExclusive;
                    return 1;
                }

                return 0;
            }

            scoped ValueListBuilder<int> separatorList = new ValueListBuilder<int>(stackalloc int[string.StackallocIntBufferSizeLimit]);
            scoped ValueListBuilder<int> lengthList = default;

            int separatorLength;
            int rangeCount = 0;
            if (!stringSeparators.IsEmpty) {
                lengthList = new ValueListBuilder<int>(stackalloc int[string.StackallocIntBufferSizeLimit]);
                string.MakeSeparatorListAny(source, stringSeparators, ref separatorList, ref lengthList);
                separatorLength = -1; // Will be set on each iteration of the loop
            } else if (isAny) {
                string.MakeSeparatorListAny(source, separatorOrSeparators, ref separatorList);
                separatorLength = 1;
            } else {
                string.MakeSeparatorList(source, separatorOrSeparators, ref separatorList);
                separatorLength = separatorOrSeparators.Length;
            }

            // Try to fill in all but the last slot in the destination.  The last slot is reserved for whatever remains
            // after the last discovered separator. If the options specify that empty entries are to be removed, then we
            // need to skip past all of those here as well, including any that occur at the beginning of the last entry,
            // which is why we enter the loop if remove empty entries is set, even if we've already added enough entries.
            int separatorIndex = 0;
            ExSpan<Range> destinationMinusOne = destination.Slice(0, destination.Length - 1);
            while (separatorIndex < separatorList.Length && (rangeCount < destinationMinusOne.Length || !keepEmptyEntries)) {
                endExclusive = separatorList[separatorIndex];
                if (separatorIndex < lengthList.Length) {
                    separatorLength = lengthList[separatorIndex];
                }
                separatorIndex++;

                // Trim off whitespace from the start and end of the range.
                int untrimmedEndEclusive = endExclusive;
                if (trimEntries) {
                    (startInclusive, endExclusive) = TrimSplitEntry(source, startInclusive, endExclusive);
                }

                // If the range is not empty or we're not ignoring empty ranges, store it.
                Debug.Assert(startInclusive <= endExclusive);
                if (startInclusive != endExclusive || keepEmptyEntries) {
                    // If we're not keeping empty entries, we may have entered the loop even if we'd
                    // already written enough ranges.  Now that we know this entry isn't empty, we
                    // need to validate there's still room remaining.
                    if ((uint)rangeCount >= (uint)destinationMinusOne.Length) {
                        break;
                    }

                    destinationMinusOne[rangeCount] = startInclusive..endExclusive;
                    rangeCount++;
                }

                // Reset to be just past the separator, and loop around to go again.
                startInclusive = untrimmedEndEclusive + separatorLength;
            }

            separatorList.Dispose();
            lengthList.Dispose();

            // Either we found at least destination.Length - 1 ranges or we didn't find any more separators.
            // If we still have a last destination slot available and there's anything left in the source,
            // put a range for the remainder of the source into the destination.
            if ((uint)rangeCount < (uint)destination.Length) {
                endExclusive = source.Length;
                if (trimEntries) {
                    (startInclusive, endExclusive) = TrimSplitEntry(source, startInclusive, endExclusive);
                }

                if (startInclusive != endExclusive || keepEmptyEntries) {
                    destination[rangeCount] = startInclusive..endExclusive;
                    rangeCount++;
                }
            }

            // Return how many ranges were written.
            return rangeCount;
        }

        /// <summary>Updates the starting and ending markers for a range to exclude whitespace.</summary>
        private static (int StartInclusive, int EndExclusive) TrimSplitEntry(ReadOnlyExSpan<char> source, int startInclusive, int endExclusive) {
            while (startInclusive < endExclusive && char.IsWhiteSpace(source[startInclusive])) {
                startInclusive++;
            }

            while (endExclusive > startInclusive && char.IsWhiteSpace(source[endExclusive - 1])) {
                endExclusive--;
            }

            return (startInclusive, endExclusive);
        }
#endif // NET8_0_OR_GREATER

        /// <summary>Counts the number of times the specified <paramref name="value"/> occurs in the <paramref name="span"/>.</summary>
        /// <typeparam name="T">The element type of the span.</typeparam>
        /// <param name="span">The span to search.</param>
        /// <param name="value">The value for which to search.</param>
        /// <returns>The number of times <paramref name="value"/> was found in the <paramref name="span"/>.</returns>
        [OverloadResolutionPriority(-1)]
        public static TSize Count<T>(this ExSpan<T> span, T value) where T : IEquatable<T>? =>
            Count((ReadOnlyExSpan<T>)span, value);

        /// <summary>Counts the number of times the specified <paramref name="value"/> occurs in the <paramref name="span"/>.</summary>
        /// <typeparam name="T">The element type of the span.</typeparam>
        /// <param name="span">The span to search.</param>
        /// <param name="value">The value for which to search.</param>
        /// <returns>The number of times <paramref name="value"/> was found in the <paramref name="span"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSize Count<T>(this ReadOnlyExSpan<T> span, T value) where T : IEquatable<T>? {
            if (TypeHelper.IsBitwiseEquatable<T>()) {
                if (Unsafe.SizeOf<T>() == sizeof(byte)) {
                    return ExSpanHelpers.CountValueType(
                        ref Unsafe.As<T, byte>(ref ExMemoryMarshal.GetReference(span)),
                        ExUnsafe.BitCast<T, byte>(value),
                        span.Length);
                } else if (Unsafe.SizeOf<T>() == sizeof(short)) {
                    return ExSpanHelpers.CountValueType(
                        ref Unsafe.As<T, short>(ref ExMemoryMarshal.GetReference(span)),
                        ExUnsafe.BitCast<T, short>(value),
                        span.Length);
                } else if (Unsafe.SizeOf<T>() == sizeof(int)) {
                    return ExSpanHelpers.CountValueType(
                        ref Unsafe.As<T, int>(ref ExMemoryMarshal.GetReference(span)),
                        ExUnsafe.BitCast<T, int>(value),
                        span.Length);
                } else if (Unsafe.SizeOf<T>() == sizeof(long)) {
                    return ExSpanHelpers.CountValueType(
                        ref Unsafe.As<T, long>(ref ExMemoryMarshal.GetReference(span)),
                        ExUnsafe.BitCast<T, long>(value),
                        span.Length);
                }
            }

            return ExSpanHelpers.Count(
                ref ExMemoryMarshal.GetReference(span),
                value,
                span.Length);
        }

        /// <summary>Counts the number of times the specified <paramref name="value"/> occurs in the <paramref name="span"/>.</summary>
        /// <typeparam name="T">The element type of the span.</typeparam>
        /// <param name="span">The span to search.</param>
        /// <param name="value">The value for which to search.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default <see cref="IEqualityComparer{T}"/> for the type of an element.</param>
        /// <returns>The number of times <paramref name="value"/> was found in the <paramref name="span"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSize Count<T>(this ReadOnlyExSpan<T> span, T value, IEqualityComparer<T>? comparer = null) {
            if (TypeHelper.IsValueType<T>() && (comparer is null || comparer == EqualityComparer<T>.Default)) {
                if (TypeHelper.IsBitwiseEquatable<T>()) {
                    if (Unsafe.SizeOf<T>() == sizeof(byte)) {
                        return ExSpanHelpers.CountValueType(
                            ref Unsafe.As<T, byte>(ref ExMemoryMarshal.GetReference(span)),
                            ExUnsafe.BitCast<T, byte>(value),
                            span.Length);
                    } else if (Unsafe.SizeOf<T>() == sizeof(short)) {
                        return ExSpanHelpers.CountValueType(
                            ref Unsafe.As<T, short>(ref ExMemoryMarshal.GetReference(span)),
                            ExUnsafe.BitCast<T, short>(value),
                            span.Length);
                    } else if (Unsafe.SizeOf<T>() == sizeof(int)) {
                        return ExSpanHelpers.CountValueType(
                            ref Unsafe.As<T, int>(ref ExMemoryMarshal.GetReference(span)),
                            ExUnsafe.BitCast<T, int>(value),
                            span.Length);
                    } else if (Unsafe.SizeOf<T>() == sizeof(long)) {
                        return ExSpanHelpers.CountValueType(
                            ref Unsafe.As<T, long>(ref ExMemoryMarshal.GetReference(span)),
                            ExUnsafe.BitCast<T, long>(value),
                            span.Length);
                    }
                }

                return CountDefaultComparer(span, value);
                static TSize CountDefaultComparer(ReadOnlyExSpan<T> span, T value) {
                    TSize count = 0;
                    for (TSize i = 0; i < span.Length; i++) {
                        if (EqualityComparer<T>.Default.Equals(span[i], value)) {
                            count++;
                        }
                    }

                    return count;
                }
            } else {
                return CountComparer(span, value, comparer);
                static TSize CountComparer(ReadOnlyExSpan<T> span, T value, IEqualityComparer<T>? comparer) {
                    comparer ??= EqualityComparer<T>.Default;

                    TSize count = 0;
                    for (TSize i = 0; i < span.Length; i++) {
                        if (comparer.Equals(span[i], value)) {
                            count++;
                        }
                    }

                    return count;
                }
            }
        }

        /// <summary>Counts the number of times the specified <paramref name="value"/> occurs in the <paramref name="span"/>.</summary>
        /// <typeparam name="T">The element type of the span.</typeparam>
        /// <param name="span">The span to search.</param>
        /// <param name="value">The value for which to search.</param>
        /// <returns>The number of times <paramref name="value"/> was found in the <paramref name="span"/>.</returns>
        [OverloadResolutionPriority(-1)]
        public static TSize Count<T>(this ExSpan<T> span, ReadOnlyExSpan<T> value) where T : IEquatable<T>? =>
            Count((ReadOnlyExSpan<T>)span, value);

        /// <summary>Counts the number of times the specified <paramref name="value"/> occurs in the <paramref name="span"/>.</summary>
        /// <typeparam name="T">The element type of the span.</typeparam>
        /// <param name="span">The span to search.</param>
        /// <param name="value">The value for which to search.</param>
        /// <returns>The number of times <paramref name="value"/> was found in the <paramref name="span"/>.</returns>
        public static TSize Count<T>(this ReadOnlyExSpan<T> span, ReadOnlyExSpan<T> value) where T : IEquatable<T>? {
            switch (value.Length) {
                case 0:
                    return 0;

                case 1:
                    return Count(span, value[0]);

                default:
                    TSize count = 0;

                    TSize pos;
                    while ((pos = span.IndexOf(value)) >= 0) {
                        span = span.Slice(pos + value.Length);
                        count++;
                    }

                    return count;
            }
        }

        /// <summary>Counts the number of times the specified <paramref name="value"/> occurs in the <paramref name="span"/>.</summary>
        /// <typeparam name="T">The element type of the span.</typeparam>
        /// <param name="span">The span to search.</param>
        /// <param name="value">The value for which to search.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default <see cref="IEqualityComparer{T}"/> for the type of an element.</param>
        /// <returns>The number of times <paramref name="value"/> was found in the <paramref name="span"/>.</returns>
        public static TSize Count<T>(this ReadOnlyExSpan<T> span, ReadOnlyExSpan<T> value, IEqualityComparer<T>? comparer = null) {
            switch (value.Length) {
                case 0:
                    return 0;

                case 1:
                    return Count(span, value[0], comparer);

                default:
                    TSize count = 0;

                    TSize pos;
                    while ((pos = span.IndexOf(value, comparer)) >= 0) {
                        span = span.Slice(pos + value.Length);
                        count++;
                    }

                    return count;
            }
        }

#if NET8_0_OR_GREATER && TODO // [TODO why] SearchValues.IndexOfAny is internal
        /// <summary>Counts the number of times any of the specified <paramref name="values"/> occurs in the <paramref name="span"/>.</summary>
        /// <typeparam name="T">The element type of the span.</typeparam>
        /// <param name="span">The span to search.</param>
        /// <param name="values">The set of values for which to search.</param>
        /// <returns>The number of times any of the <typeparamref name="T"/> elements in <paramref name="values"/> was found in the <paramref name="span"/>.</returns>
        /// <remarks>If <paramref name="values"/> is empty, 0 is returned.</remarks>
        /// <exception cref="ArgumentNullException"><paramref name="values"/> is <see langword="null"/>.</exception>
        public static TSize CountAny<T>(this ReadOnlyExSpan<T> span, SearchValues<T> values) where T : IEquatable<T>? {
            TSize count = 0;

            TSize pos;
            while ((pos = span.IndexOfAny(values)) >= 0) {
                count++;
                span = span.Slice(pos + 1);
            }

            return count;
        }
#endif // NET8_0_OR_GREATER

        /// <summary>Counts the number of times any of the specified <paramref name="values"/> occurs in the <paramref name="span"/>.</summary>
        /// <typeparam name="T">The element type of the span.</typeparam>
        /// <param name="span">The span to search.</param>
        /// <param name="values">The set of values for which to search.</param>
        /// <returns>The number of times any of the <typeparamref name="T"/> elements in <paramref name="values"/> was found in the <paramref name="span"/>.</returns>
        /// <remarks>If <paramref name="values"/> is empty, 0 is returned.</remarks>
        public static TSize CountAny<T>(this ReadOnlyExSpan<T> span, ReadOnlyExSpan<T> values) where T : IEquatable<T>? {
            TSize count = 0;

            TSize pos;
            while ((pos = span.IndexOfAny(values)) >= 0) {
                count++;
                span = span.Slice(pos + 1);
            }

            return count;
        }

#if PARAMS_COLLECTIONS
        /// <inheritdoc cref="CountAny{T}(ReadOnlyExSpan{T}, ReadOnlyExSpan{T})"/>
        [OverloadResolutionPriority(1)]
        public static TSize CountAny<T>(this ReadOnlyExSpan<T> span, params ReadOnlySpan<T> values) where T : IEquatable<T>? {
            return CountAny(span, values.AsReadOnlyExSpan());
        }
#endif

        /// <summary>Counts the number of times any of the specified <paramref name="values"/> occurs in the <paramref name="span"/>.</summary>
        /// <typeparam name="T">The element type of the span.</typeparam>
        /// <param name="span">The span to search.</param>
        /// <param name="values">The set of values for which to search.</param>
        /// <param name="comparer">
        /// The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the
        /// default <see cref="IEqualityComparer{T}"/> for the type of an element.
        /// </param>
        /// <returns>The number of times any of the <typeparamref name="T"/> elements in <paramref name="values"/> was found in the <paramref name="span"/>.</returns>
        /// <remarks>If <paramref name="values"/> is empty, 0 is returned.</remarks>
        public static TSize CountAny<T>(this ReadOnlyExSpan<T> span, ReadOnlyExSpan<T> values, IEqualityComparer<T>? comparer = null) {
            TSize count = 0;

            TSize pos;
            while ((pos = span.IndexOfAny(values, comparer)) >= 0) {
                count++;
                span = span.Slice(pos + 1);
            }

            return count;
        }


#if NET6_0_OR_GREATER
        /// <summary>Writes the specified interpolated string to the character span.</summary>
        /// <param name="destination">The span to which the interpolated string should be formatted.</param>
        /// <param name="handler">The interpolated string.</param>
        /// <param name="charsWritten">The number of characters written to the span.</param>
        /// <returns>true if the entire interpolated string could be formatted successfully; otherwise, false.</returns>
        public static bool TryWrite(this ExSpan<char> destination, [InterpolatedStringHandlerArgument(nameof(destination))] ref ExTryWriteInterpolatedStringHandler handler, out TSize charsWritten) {
            // The span argument isn't used directly in the method; rather, it'll be used by the compiler to create the handler.
            // We could validate here that span == handler._destination, but that doesn't seem necessary.
            if (handler._success) {
                charsWritten = handler._pos;
                return true;
            }

            charsWritten = 0;
            return false;
        }

        /// <summary>Writes the specified interpolated string to the character span.</summary>
        /// <param name="destination">The span to which the interpolated string should be formatted.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="handler">The interpolated string.</param>
        /// <param name="charsWritten">The number of characters written to the span.</param>
        /// <returns>true if the entire interpolated string could be formatted successfully; otherwise, false.</returns>
        public static bool TryWrite(this ExSpan<char> destination, IFormatProvider? provider, [InterpolatedStringHandlerArgument(nameof(destination), nameof(provider))] ref ExTryWriteInterpolatedStringHandler handler, out TSize charsWritten) =>
            // The provider is passed to the handler by the compiler, so the actual implementation of the method
            // is the same as the non-provider overload.
            TryWrite(destination, ref handler, out charsWritten);
#endif

#if NET8_0_OR_GREATER
        /// <summary>
        /// Writes the <see cref="CompositeFormat"/> string to the character span, substituting the format item or items
        /// with the string representation of the corresponding arguments.
        /// </summary>
        /// <typeparam name="TArg0">The type of the first object to format.</typeparam>
        /// <param name="destination">The span to which the string should be formatted.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">A <see cref="CompositeFormat"/>.</param>
        /// <param name="charsWritten">The number of characters written to the span.</param>
        /// <param name="arg0">The first object to format.</param>
        /// <returns><see langword="true"/> if the entire interpolated string could be formatted successfully; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="format"/> is null.</exception>
        /// <exception cref="FormatException">The index of a format item is greater than or equal to the number of supplied arguments.</exception>
        public static bool TryWrite<TArg0>(this ExSpan<char> destination, IFormatProvider? provider, CompositeFormat format, out TSize charsWritten, TArg0 arg0) {
            ArgumentNullException.ThrowIfNull(format);
            CompositeFormatHelper.ValidateNumberOfArgs(format, 1);
            return TryWrite(destination, provider, format, out charsWritten, arg0, 0, 0, default);
        }

        /// <summary>
        /// Writes the <see cref="CompositeFormat"/> string to the character span, substituting the format item or items
        /// with the string representation of the corresponding arguments.
        /// </summary>
        /// <typeparam name="TArg0">The type of the first object to format.</typeparam>
        /// <typeparam name="TArg1">The type of the second object to format.</typeparam>
        /// <param name="destination">The span to which the string should be formatted.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">A <see cref="CompositeFormat"/>.</param>
        /// <param name="charsWritten">The number of characters written to the span.</param>
        /// <param name="arg0">The first object to format.</param>
        /// <param name="arg1">The second object to format.</param>
        /// <returns><see langword="true"/> if the entire interpolated string could be formatted successfully; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="format"/> is null.</exception>
        /// <exception cref="FormatException">The index of a format item is greater than or equal to the number of supplied arguments.</exception>
        public static bool TryWrite<TArg0, TArg1>(this ExSpan<char> destination, IFormatProvider? provider, CompositeFormat format, out TSize charsWritten, TArg0 arg0, TArg1 arg1) {
            ArgumentNullException.ThrowIfNull(format);
            CompositeFormatHelper.ValidateNumberOfArgs(format, 2);
            return TryWrite(destination, provider, format, out charsWritten, arg0, arg1, 0, default);
        }

        /// <summary>
        /// Writes the <see cref="CompositeFormat"/> string to the character span, substituting the format item or items
        /// with the string representation of the corresponding arguments.
        /// </summary>
        /// <typeparam name="TArg0">The type of the first object to format.</typeparam>
        /// <typeparam name="TArg1">The type of the second object to format.</typeparam>
        /// <typeparam name="TArg2">The type of the third object to format.</typeparam>
        /// <param name="destination">The span to which the string should be formatted.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">A <see cref="CompositeFormat"/>.</param>
        /// <param name="charsWritten">The number of characters written to the span.</param>
        /// <param name="arg0">The first object to format.</param>
        /// <param name="arg1">The second object to format.</param>
        /// <param name="arg2">The third object to format.</param>
        /// <returns><see langword="true"/> if the entire interpolated string could be formatted successfully; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="format"/> is null.</exception>
        /// <exception cref="FormatException">The index of a format item is greater than or equal to the number of supplied arguments.</exception>
        public static bool TryWrite<TArg0, TArg1, TArg2>(this ExSpan<char> destination, IFormatProvider? provider, CompositeFormat format, out TSize charsWritten, TArg0 arg0, TArg1 arg1, TArg2 arg2) {
            ArgumentNullException.ThrowIfNull(format);
            CompositeFormatHelper.ValidateNumberOfArgs(format, 3);
            return TryWrite(destination, provider, format, out charsWritten, arg0, arg1, arg2, default);
        }

        /// <summary>
        /// Writes the <see cref="CompositeFormat"/> string to the character span, substituting the format item or items
        /// with the string representation of the corresponding arguments.
        /// </summary>
        /// <param name="destination">The span to which the string should be formatted.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">A <see cref="CompositeFormat"/>.</param>
        /// <param name="charsWritten">The number of characters written to the span.</param>
        /// <param name="args">An array of objects to format.</param>
        /// <returns><see langword="true"/> if the entire interpolated string could be formatted successfully; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="format"/> is null.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="args"/> is null.</exception>
        /// <exception cref="FormatException">The index of a format item is greater than or equal to the number of supplied arguments.</exception>
        public static bool TryWrite(this ExSpan<char> destination, IFormatProvider? provider, CompositeFormat format, out TSize charsWritten, params object?[] args) {
            ArgumentNullException.ThrowIfNull(format);
            ArgumentNullException.ThrowIfNull(args);
            return TryWrite(destination, provider, format, out charsWritten, (ReadOnlySpan<object?>)args);
        }

        /// <summary>
        /// Writes the <see cref="CompositeFormat"/> string to the character span, substituting the format item or items
        /// with the string representation of the corresponding arguments.
        /// </summary>
        /// <param name="destination">The span to which the string should be formatted.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">A <see cref="CompositeFormat"/>.</param>
        /// <param name="charsWritten">The number of characters written to the span.</param>
        /// <param name="args">A ExSpan of objects to format.</param>
        /// <returns><see langword="true"/> if the entire interpolated string could be formatted successfully; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="format"/> is null.</exception>
        /// <exception cref="FormatException">The index of a format item is greater than or equal to the number of supplied arguments.</exception>
        public static bool TryWrite(this ExSpan<char> destination, IFormatProvider? provider, CompositeFormat format, out TSize charsWritten,
#if PARAMS_COLLECTIONS
#endif // PARAMS_COLLECTIONS
            params
            ReadOnlySpan<object?> args) {
            ArgumentNullException.ThrowIfNull(format);
            CompositeFormatHelper.ValidateNumberOfArgs(format, args.Length);
            return args.Length switch {
                0 => TryWrite(destination, provider, format, out charsWritten, 0, 0, 0, args),
                1 => TryWrite(destination, provider, format, out charsWritten, args[0], 0, 0, args),
                2 => TryWrite(destination, provider, format, out charsWritten, args[0], args[1], 0, args),
                _ => TryWrite(destination, provider, format, out charsWritten, args[0], args[1], args[2], args),
            };
        }

        private static bool TryWrite<TArg0, TArg1, TArg2>(ExSpan<char> destination, IFormatProvider? provider, CompositeFormat format, out TSize charsWritten, TArg0 arg0, TArg1 arg1, TArg2 arg2, ReadOnlyExSpan<object?> args) {
            // Create the interpolated string handler.
            var handler = new ExTryWriteInterpolatedStringHandler(CompositeFormatHelper.GetLiteralLength(format), CompositeFormatHelper.GetFormattedCount(format), destination, provider, out bool shouldAppend);

            if (shouldAppend) {
                // Write each segment.
                foreach ((string? Literal, int ArgIndex, int Alignment, string? Format) segment in CompositeFormatHelper.GetSegments(format)) {
                    bool appended;
                    if (segment.Literal is string literal) {
                        appended = handler.AppendLiteral(literal);
                    } else {
                        int index = segment.ArgIndex;
                        switch (index) {
                            case 0:
                                appended = handler.AppendFormatted(arg0, segment.Alignment, segment.Format);
                                break;

                            case 1:
                                appended = handler.AppendFormatted(arg1, segment.Alignment, segment.Format);
                                break;

                            case 2:
                                appended = handler.AppendFormatted(arg2, segment.Alignment, segment.Format);
                                break;

                            default:
                                Debug.Assert(index > 2);
                                appended = handler.AppendFormatted(args[index], segment.Alignment, segment.Format);
                                break;
                        }
                    }

                    if (!appended) {
                        break;
                    }
                }
            }

            // Complete the operation.
            return TryWrite(destination, provider, ref handler, out charsWritten);
        }
#endif // NET8_0_OR_GREATER


    }
}
