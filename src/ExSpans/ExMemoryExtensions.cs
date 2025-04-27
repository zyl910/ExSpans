//#undef INVOKE_SPAN_METHOD

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Zyl.ExSpans.Extensions;
using Zyl.ExSpans.Impl;
using Zyl.ExSpans.Reflection;

namespace Zyl.ExSpans {
    /// <summary>
    /// Provides extension methods for the span-related types, such as <see cref="ExSpan{T}"/> and <see cref="ReadOnlyExSpan{T}"/>. It can be regarded as the <see cref="MemoryExtensions"/> of <see cref="TSize"/> index range (提供跨度相关的类型的扩展方法，例如 <see cref="ExSpan{T}"/> 和 <see cref="ReadOnlyExSpan{T}"/>. 它可以被视为 <see cref="TSize"/> 索引范围的 MemoryExtensions).
    /// </summary>
    /// <remarks>
    /// <para>Commonly extension methods such as <see cref="ExSpanExtensions.AsExSpan{T}(T[])">AsExSpan</see> are located in <see cref="ExSpanExtensions"/> (AsExSpan 等常用扩展方法位于 ExSpanExtensions).</para>
    /// </remarks>
    public static partial class ExMemoryExtensions {

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
        /// Determines whether two read-only sequences are equal by comparing the elements using <see cref="IEquatable{T}.Equals(T)"/> (通过使用 <see cref="IEquatable{T}.Equals(T)"/> 比较元素, 确定两个只读序列是否相等).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="span">The first sequence to compare (要比较的第一个序列).</param>
        /// <param name="other">The second sequence to compare (要比较的第二个序列).</param>
        /// <returns>true if the two sequences are equal; otherwise, false (如果两个序列相等, 则 true; 否则 false).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        public static unsafe bool SequenceEqual<T>(this ReadOnlyExSpan<T> span, ReadOnlyExSpan<T> other, IEqualityComparer<T>? comparer = null) {
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
