//#undef INVOKE_SPAN_METHOD

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Zyl.SizableSpans.Extensions;
using Zyl.SizableSpans.Impl;
using Zyl.SizableSpans.Reflection;

namespace Zyl.SizableSpans {
    /// <summary>
    /// Provides extension methods for the span-related types, such as <see cref="SizableSpan{T}"/> and <see cref="ReadOnlySizableSpan{T}"/>. It can be regarded as the <see cref="MemoryExtensions"/> of <see cref="TSize"/> index range (提供跨度相关的类型的扩展方法，例如 <see cref="SizableSpan{T}"/> 和 <see cref="ReadOnlySizableSpan{T}"/>. 它可以被视为 <see cref="TSize"/> 索引范围的 MemoryExtensions).
    /// </summary>
    /// <remarks>
    /// <para>Commonly extension methods such as <see cref="SizableSpanExtensions.AsSizableSpan{T}(T[])">AsSizableSpan</see> are located in <see cref="SizableSpanExtensions"/> (AsSizableSpan 等常用扩展方法位于 SizableSpanExtensions).</para>
    /// </remarks>
    public static partial class SizableMemoryExtensions {

        /// <summary>
        /// Determines whether two sequences are equal by comparing the elements using IEquatable{T}.Equals(T).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [OverloadResolutionPriority(-1)]
        public static bool SequenceEqual<T>(this SizableSpan<T> span, ReadOnlySizableSpan<T> other) where T : IEquatable<T>? =>
            SequenceEqual((ReadOnlySizableSpan<T>)span, other);

        /// <summary>
        /// Determines whether two sequences are equal by comparing the elements using IEquatable{T}.Equals(T).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool SequenceEqual<T>(this ReadOnlySizableSpan<T> span, ReadOnlySizableSpan<T> other) where T : IEquatable<T>? {
            if (span.Length != other.Length) {
                return false;
            }
            TSize length = span.Length;
#if INVOKE_SPAN_METHOD && NET6_0_OR_GREATER
            int blockSize = SizableMemoryMarshal.ArrayMaxLengthSafe / Unsafe.SizeOf<T>();
            if (blockSize < 1) blockSize = 1;
            TSize blockSizeN = (TSize)blockSize;
            TSize index = (TSize)0;
            while (index.LessThan(length)) {
                TSize count = length.Subtract(index);
                TSize indexNext;
                bool rt;
                if (count.LessThan(blockSizeN)) {
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
#else
            if (TypeHelper.IsBitwiseEquatable<T>()) {
                return SizableSpanHelpers.SequenceEqual(
                        ref Unsafe.As<T, byte>(ref SizableMemoryMarshal.GetReference(span)),
                        ref Unsafe.As<T, byte>(ref SizableMemoryMarshal.GetReference(other)),
                        (nuint)((ulong)length * (ulong)Unsafe.SizeOf<T>()));  // // If this multiplication overflows, the Span we got overflows the entire address range. There's no happy outcome for this API in such a case so we choose not to take the overhead of checking.
            }
            return SizableSpanHelpers.SequenceEqual<T>(ref SizableMemoryMarshal.GetReference(span), ref SizableMemoryMarshal.GetReference(other), length.ToUIntPtr());
#endif // INVOKE_SPAN_METHOD && NET6_0_OR_GREATER
        }

        /// <summary>
        /// Determines whether two sequences are equal by comparing the elements using an <see cref="IEqualityComparer{T}"/>.
        /// </summary>
        /// <param name="span">The first sequence to compare.</param>
        /// <param name="other">The second sequence to compare.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default <see cref="IEqualityComparer{T}"/> for the type of an element.</param>
        /// <returns>true if the two sequences are equal; otherwise, false.</returns>
        [OverloadResolutionPriority(-1)]
        public static bool SequenceEqual<T>(this SizableSpan<T> span, ReadOnlySizableSpan<T> other, IEqualityComparer<T>? comparer = null) =>
            SequenceEqual((ReadOnlySizableSpan<T>)span, other, comparer);

        /// <summary>
        /// Determines whether two sequences are equal by comparing the elements using an <see cref="IEqualityComparer{T}"/>.
        /// </summary>
        /// <param name="span">The first sequence to compare.</param>
        /// <param name="other">The second sequence to compare.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default <see cref="IEqualityComparer{T}"/> for the type of an element.</param>
        /// <returns>true if the two sequences are equal; otherwise, false.</returns>
        public static unsafe bool SequenceEqual<T>(this ReadOnlySizableSpan<T> span, ReadOnlySizableSpan<T> other, IEqualityComparer<T>? comparer = null) {
            // If the spans differ in length, they're not equal.
            if (span.Length != other.Length) {
                return false;
            }
            TSize length = span.Length;
#if INVOKE_SPAN_METHOD && NET6_0_OR_GREATER
            int blockSize = SizableMemoryMarshal.ArrayMaxLengthSafe / Unsafe.SizeOf<T>();
            if (blockSize < 1) blockSize = 1;
            TSize blockSizeN = (TSize)blockSize;
            TSize index = (TSize)0;
            while (index.LessThan(length)) {
                TSize count = length.Subtract(index);
                TSize indexNext;
                bool rt;
                if (count.LessThan(blockSizeN)) {
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
#else
            if (TypeHelper.IsValueType(typeof(T))) {
                if (comparer is null || comparer == EqualityComparer<T>.Default) {
                    // If no comparer was supplied and the type is bitwise equatable, take the fast path doing a bitwise comparison.
                    if (TypeHelper.IsBitwiseEquatable<T>()) {
                        return SizableSpanHelpers.SequenceEqual(
                            ref Unsafe.As<T, byte>(ref SizableMemoryMarshal.GetReference(span)),
                            ref Unsafe.As<T, byte>(ref SizableMemoryMarshal.GetReference(other)),
                            ((uint)span.Length) * (nuint)Unsafe.SizeOf<T>());  // If this multiplication overflows, the Span we got overflows the entire address range. There's no happy outcome for this API in such a case so we choose not to take the overhead of checking.
                    }

                    // Otherwise, compare each element using EqualityComparer<T>.Default.Equals in a way that will enable it to devirtualize.
                    for (TSize i = (TSize)0; i.LessThan(length); i+=1) {
                        if (!EqualityComparer<T>.Default.Equals(span[i], other[i])) {
                            return false;
                        }
                    }

                    return true;
                }
            }

            // Use the comparer to compare each element.
            comparer ??= EqualityComparer<T>.Default;
            for (TSize i = (TSize)0; i.LessThan(length); i += 1) {
                if (!comparer.Equals(span[i], other[i])) {
                    return false;
                }
            }

            return true;
#endif // INVOKE_SPAN_METHOD && NET6_0_OR_GREATER
        }

    }
}
