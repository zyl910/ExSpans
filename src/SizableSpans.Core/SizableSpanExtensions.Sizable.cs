using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Zyl.SizableSpans {
    partial class SizableSpanExtensions {

        /// <summary>
        /// An conversion of a <see cref="ReadOnlySpan{T}"/> to a <see cref="ReadOnlySizableSpan{T}"/> (<see cref="ReadOnlySpan{T}"/> 到 <see cref="ReadOnlySizableSpan{T}"/> 的转换).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="span">The object to convert (要转换的对象).</param>
        /// <returns>a <see cref="ReadOnlySizableSpan{T}"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySizableSpan<T> AsReadOnlySizableSpan<T>(this ReadOnlySpan<T> span) {
            return (ReadOnlySizableSpan<T>)span;
        }

        /// <summary>
        /// An conversion of a <see cref="ReadOnlySizableSpan{T}"/> to a <see cref="ReadOnlySpan{T}"/>. The length will saturating limited to the maximum length it supports (<see cref="ReadOnlySizableSpan{T}"/> 到 <see cref="ReadOnlySpan{T}"/> 的转换. 长度会饱和限制为它所支持的最大长度).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="span">The object to convert (要转换的对象).</param>
        /// <returns>a <see cref="ReadOnlySpan{T}"/></returns>
        /// <seealso cref="MemoryMarshalHelper.GetSpanSaturatingLength"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<T> AsReadOnlySpan<T>(this ReadOnlySizableSpan<T> span) {
            return (ReadOnlySpan<T>)span;
        }

        /// <summary>
        /// An conversion of a <see cref="Span{T}"/> to a <see cref="SizableSpan{T}"/> (<see cref="Span{T}"/> 到 <see cref="SizableSpan{T}"/> 的转换).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="span">The object to convert (要转换的对象).</param>
        /// <returns>a <see cref="SizableSpan{T}"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SizableSpan<T> AsSizableSpan<T>(this Span<T> span) {
            return (SizableSpan<T>)span;
        }

        /// <summary>
        /// An conversion of a <see cref="SizableSpan{T}"/> to a <see cref="Span{T}"/>. The length will saturating limited to the maximum length it supports (<see cref="SizableSpan{T}"/> 到 <see cref="Span{T}"/> 的转换. 长度会饱和限制为它所支持的最大长度).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="span">The object to convert (要转换的对象).</param>
        /// <returns>a <see cref="Span{T}"/></returns>
        /// <seealso cref="MemoryMarshalHelper.GetSpanSaturatingLength"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> AsSpan<T>(this SizableSpan<T> span) {
            return (Span<T>)span;
        }

        /// <summary>
        /// Gets a value indicating whether this object is empty (返回一个值，该值指示当前源对象为空).
        /// </summary>
        /// <param name="source">Source object (源对象).</param>
        /// <returns><see langword="true"/> if this object is empty; otherwise, <see langword="false"/> (当前对象为空时为 <see langword="true"/>; 否则为 <see langword="false"/>).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEmpty(this ISizableLength source) {
            return (TSize)0 == source.Length;
        }

    }
}
