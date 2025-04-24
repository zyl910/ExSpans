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
    partial struct ReadOnlyExSpan<T> {

        /// <summary>
        /// Defines an implicit conversion of a <see cref="ReadOnlySpan{T}"/> to a <see cref="ReadOnlyExSpan{T}"/> (定义 <see cref="ReadOnlySpan{T}"/> 到 <see cref="ReadOnlyExSpan{T}"/> 的隐式转换).
        /// </summary>
        /// <param name="span">The object to convert (要转换的对象).</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlyExSpan<T>(ReadOnlySpan<T> span) {
#if STRUCT_REF_FIELD
            return new ReadOnlyExSpan<T>(ref Unsafe.AsRef(in span.GetPinnableReference()), span.ExLength());
#else
            return new ReadOnlyExSpan<T>(span, (TSize)0, span.ExLength());
#endif
        }

        /// <summary>
        /// Defines an explicit conversion of a <see cref="ReadOnlyExSpan{T}"/> to a <see cref="ReadOnlySpan{T}"/>. The length will saturating limited to the maximum length it supports (定义 <see cref="ReadOnlyExSpan{T}"/> 到 <see cref="ReadOnlySpan{T}"/> 的显式转换. 长度会饱和限制为它所支持的最大长度).
        /// </summary>
        /// <param name="span">The object to convert (要转换的对象).</param>
        /// <seealso cref="MemoryMarshalHelper.GetSpanSaturatingLength"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator ReadOnlySpan<T>(ReadOnlyExSpan<T> span) {
            if (TSize.Zero == span.Length) return ReadOnlySpan<T>.Empty;
            int len = MemoryMarshalHelper.GetSpanSaturatingLength(span.Length);
#if STRUCT_REF_FIELD
            return MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef(in span.GetPinnableReference()), len);
#else
            if (span._referenceSpan.IsEmpty) {
                unsafe {
                    return new ReadOnlySpan<T>((void*)span._byteOffset, len);
                }
            } else if (TSize.Zero == span._byteOffset) {
                return span._referenceSpan;
            } else {
                int start = (int)((ulong)span._byteOffset / (uint)Unsafe.SizeOf<T>());
                return span._referenceSpan.Slice(start);
            }
#endif
        }

    }
}
