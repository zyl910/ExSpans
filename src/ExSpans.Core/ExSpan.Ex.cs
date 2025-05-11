#if NET7_0_OR_GREATER
#define STRUCT_REF_FIELD // C# 11 - ref fields and ref scoped variables. https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/ref-struct#ref-fields
#endif // NET7_0_OR_GREATER

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Zyl.ExSpans.Extensions;
using Zyl.ExSpans.Impl;

namespace Zyl.ExSpans {
    partial struct ExSpan<T> {

        /// <summary>
        /// Defines an implicit conversion of a <see cref="Span{T}"/> to a <see cref="ExSpan{T}"/> (定义 <see cref="Span{T}"/> 到 <see cref="ExSpan{T}"/> 的隐式转换).
        /// </summary>
        /// <param name="span">The object to convert (要转换的对象).</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ExSpan<T>(Span<T> span) {
#if STRUCT_REF_FIELD
            return new ExSpan<T>(ref span.GetPinnableReference(), span.ExLength());
#else
            return new ExSpan<T>(span, (TSize)0, span.ExLength());
#endif
        }

        /// <summary>
        /// Defines an explicit conversion of a <see cref="ExSpan{T}"/> to a <see cref="Span{T}"/>. The length will saturating limited to the maximum length it supports (定义 <see cref="ExSpan{T}"/> 到 <see cref="Span{T}"/> 的显式转换. 长度会饱和限制为它所支持的最大长度).
        /// </summary>
        /// <param name="span">The object to convert (要转换的对象).</param>
        /// <seealso cref="MemoryMarshalHelper.GetSpanSaturatingLength"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Span<T>(ExSpan<T> span) {
            if ((TSize)0 == span.Length) return Span<T>.Empty;
            int len = MemoryMarshalHelper.GetSpanSaturatingLength(span.Length);
#if STRUCT_REF_FIELD
            return MemoryMarshal.CreateSpan(ref span.GetPinnableReference(), len);
#else
            if (span._referenceSpan.IsEmpty) {
                unsafe {
                    return new Span<T>((void*)span._byteOffset, len);
                }
            } else if ((TSize)0 == span._byteOffset) {
                if (len >= span._referenceSpan.Length) {
                    return span._referenceSpan;
                } else {
                    return span._referenceSpan.Slice(0, len);
                }
            } else {
                int start = (int)((ulong)span._byteOffset / (uint)Unsafe.SizeOf<T>());
                return span._referenceSpan.Slice(start, len);
            }
#endif
        }

        /// <summary>
        /// Forms a slice out of the given Ex span, ending at 'TSize' (从尾部指定索引处的扩展跨度形成切片).
        /// </summary>
        /// <param name="end">The zero-based index at which to begin this slice (从零开始的切片尾部索引).</param>
        /// <returns>Returns the new Ex span (返回新的扩展跨度).</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the specified <paramref name="end"/> index is not in range (&lt;0 or &gt;Length).
        /// </exception>
        [MyCLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ExSpan<T> LastSlice(TSize end) {
            TSize len = _length - end;
            return Slice(0, len);
        }

        /// <summary>
        /// Forms a slice out of the given Ex span, beginning at 'end', of given length (从尾部指定长度的指定索引处开始的当前扩展跨度形成切片)
        /// </summary>
        /// <param name="end">The zero-based index at which to begin this slice (从零开始的切片尾部索引).</param>
        /// <param name="length">The desired length for the slice (exclusive) (切片所需的长度).</param>
        /// <returns>Returns the new Ex span (返回新的扩展跨度).</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the specified <paramref name="end"/> or end index is not in range (&lt;0 or &gt;Length).
        /// </exception>
        [MyCLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ExSpan<T> LastSlice(TSize end, TSize length) {
            TSize start = _length - end - length;
            return Slice(start, length);
        }

    }
}
