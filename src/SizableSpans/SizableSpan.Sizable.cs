#if NET7_0_OR_GREATER
#define STRUCT_REF_FIELD // C# 11 - ref fields and ref scoped variables. https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/ref-struct#ref-fields
#endif // NET7_0_OR_GREATER

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Zyl.SizableSpans.Extensions;
using Zyl.SizableSpans.Impl;

namespace Zyl.SizableSpans {
    partial struct SizableSpan<T> {

        /// <summary>
        /// Defines an implicit conversion of a <see cref="Span{T}"/> to a <see cref="SizableSpan{T}"/> (定义 <see cref="Span{T}"/> 到 <see cref="SizableSpan{T}"/> 的隐式转换).
        /// </summary>
        /// <param name="span">The object to convert (要转换的对象).</param>
        public static implicit operator SizableSpan<T>(Span<T> span) {
#if STRUCT_REF_FIELD
            return new SizableSpan<T>(ref span.GetPinnableReference(), span.NULength());
#else
            return new SizableSpan<T>(span, (TSize)0, span.NULength());
#endif
        }

        /// <summary>
        /// Defines an explicit conversion of a <see cref="SizableSpan{T}"/> to a <see cref="Span{T}"/>. The length will saturating limited to the maximum length it supports (定义 <see cref="SizableSpan{T}"/> 到 <see cref="Span{T}"/> 的显式转换. 长度会饱和限制为它所支持的最大长度).
        /// </summary>
        /// <param name="span">The object to convert (要转换的对象).</param>
        /// <seealso cref="MemoryMarshalHelper.GetSpanSaturatingLength"/>
        public static explicit operator Span<T>(SizableSpan<T> span) {
            if (TSize.Zero == span.Length) return Span<T>.Empty;
            int len = MemoryMarshalHelper.GetSpanSaturatingLength(span.Length);
#if STRUCT_REF_FIELD
            return MemoryMarshal.CreateSpan(ref span.GetPinnableReference(), len);
#else
            if (span._referenceSpan.IsEmpty) {
                unsafe {
                    return new Span<T>((void*)span._byteOffse, len);
                }
            } else if (TSize.Zero == span._byteOffse) {
                return span._referenceSpan;
            } else {
                int start = (int)((ulong)span._byteOffse / (uint)Unsafe.SizeOf<T>());
                return span._referenceSpan.Slice(start);
            }
#endif
        }

    }
}
