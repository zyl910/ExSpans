#if NET9_0_OR_GREATER
#define ALLOWS_REF_STRUCT // C# 13 - ref struct interface; allows ref struct. https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-13#ref-struct-interfaces
#endif // NET9_0_OR_GREATER

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Zyl.ExSpans.Extensions;
using Zyl.ExSpans.Impl;
using Zyl.ExSpans.Reflection;

namespace Zyl.ExSpans {
    partial class ExSpanExtensions {

        /// <summary>
        /// An conversion of a <see cref="ReadOnlySpan{T}"/> to a <see cref="ReadOnlyExSpan{T}"/> (<see cref="ReadOnlySpan{T}"/> 到 <see cref="ReadOnlyExSpan{T}"/> 的转换).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="span">The object to convert (要转换的对象).</param>
        /// <returns>a <see cref="ReadOnlyExSpan{T}"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyExSpan<T> AsReadOnlyExSpan<T>(this ReadOnlySpan<T> span) {
            return (ReadOnlyExSpan<T>)span;
        }

        /// <summary>
        /// An conversion of a <see cref="ExSpan{T}"/> to a <see cref="ReadOnlyExSpan{T}"/> (<see cref="ExSpan{T}"/> 到 <see cref="ReadOnlyExSpan{T}"/> 的转换).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="span">The object to convert (要转换的对象).</param>
        /// <returns>a <see cref="ReadOnlyExSpan{T}"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyExSpan<T> AsReadOnlyExSpan<T>(this ExSpan<T> span) {
            return (ReadOnlyExSpan<T>)span;
        }

        /// <summary>
        /// An conversion of a <see cref="ReadOnlyExSpan{T}"/> to a <see cref="ReadOnlySpan{T}"/>. The length will saturating limited to the maximum length it supports (<see cref="ReadOnlyExSpan{T}"/> 到 <see cref="ReadOnlySpan{T}"/> 的转换. 长度会饱和限制为它所支持的最大长度).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="span">The object to convert (要转换的对象).</param>
        /// <returns>a <see cref="ReadOnlySpan{T}"/></returns>
        /// <seealso cref="MemoryMarshalHelper.GetSpanSaturatingLength"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<T> AsReadOnlySpan<T>(this ReadOnlyExSpan<T> span) {
            return (ReadOnlySpan<T>)span;
        }

        /// <summary>
        /// An conversion of a <see cref="ReadOnlyExSpan{T}"/> to a <see cref="ReadOnlySpan{T}"/>, beginning at 'start'. The length will saturating limited to the maximum length it supports (<see cref="ReadOnlyExSpan{T}"/> 到 <see cref="ReadOnlySpan{T}"/> 的转换, 从指定索引处开始. 长度会饱和限制为它所支持的最大长度).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="span">The object to convert (要转换的对象).</param>
        /// <param name="start">The zero-based index at which to begin this slice (从零开始切片的索引).</param>
        /// <returns>a <see cref="ReadOnlySpan{T}"/></returns>
        /// <seealso cref="MemoryMarshalHelper.GetSpanSaturatingLength"/>
        [MyCLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<T> AsReadOnlySpan<T>(this ReadOnlyExSpan<T> span, TSize start) {
            return (ReadOnlySpan<T>)span.Slice(start);
        }

        /// <summary>
        /// An conversion of a <see cref="ReadOnlyExSpan{T}"/> to a <see cref="ReadOnlySpan{T}"/>, beginning at 'start', of given length (<see cref="ReadOnlyExSpan{T}"/> 到 <see cref="ReadOnlySpan{T}"/> 的转换, 从指定索引处开始, 且使用指定长度).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="span">The object to convert (要转换的对象).</param>
        /// <param name="start">The zero-based index at which to begin this slice (从零开始切片的索引).</param>
        /// <param name="length"></param>
        /// <returns>a <see cref="ReadOnlySpan{T}"/></returns>
        [MyCLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<T> AsReadOnlySpan<T>(this ReadOnlyExSpan<T> span, TSize start, int length) {
            if (length < 0)
                ThrowHelper.ThrowArgumentOutOfRangeException();
            return (ReadOnlySpan<T>)span.Slice(start, (TSize)(uint)length);
        }

        /// <summary>
        /// An conversion of a <see cref="Span{T}"/> to a <see cref="ExSpan{T}"/> (<see cref="Span{T}"/> 到 <see cref="ExSpan{T}"/> 的转换).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="span">The object to convert (要转换的对象).</param>
        /// <returns>a <see cref="ExSpan{T}"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ExSpan<T> AsExSpan<T>(this Span<T> span) {
            return (ExSpan<T>)span;
        }

        /// <summary>
        /// An conversion of a <see cref="ExSpan{T}"/> to a <see cref="Span{T}"/>. The length will saturating limited to the maximum length it supports (<see cref="ExSpan{T}"/> 到 <see cref="Span{T}"/> 的转换. 长度会饱和限制为它所支持的最大长度).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="span">The object to convert (要转换的对象).</param>
        /// <returns>a <see cref="Span{T}"/></returns>
        /// <seealso cref="MemoryMarshalHelper.GetSpanSaturatingLength"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> AsSpan<T>(this ExSpan<T> span) {
            return (Span<T>)span;
        }

        /// <summary>
        /// An conversion of a <see cref="ExSpan{T}"/> to a <see cref="Span{T}"/>, beginning at 'start'. The length will saturating limited to the maximum length it supports (<see cref="ExSpan{T}"/> 到 <see cref="Span{T}"/> 的转换, 从指定索引处开始. 长度会饱和限制为它所支持的最大长度).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="span">The object to convert (要转换的对象).</param>
        /// <param name="start">The zero-based index at which to begin this slice (从零开始切片的索引).</param>
        /// <returns>a <see cref="Span{T}"/></returns>
        /// <seealso cref="MemoryMarshalHelper.GetSpanSaturatingLength"/>
        [MyCLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> AsSpan<T>(this ExSpan<T> span, TSize start) {
            return (Span<T>)span.Slice(start);
        }

        /// <summary>
        /// An conversion of a <see cref="ExSpan{T}"/> to a <see cref="Span{T}"/>, beginning at 'start', of given length (<see cref="ExSpan{T}"/> 到 <see cref="Span{T}"/> 的转换, 从指定索引处开始, 且使用指定长度).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="span">The object to convert (要转换的对象).</param>
        /// <param name="start">The zero-based index at which to begin this slice (从零开始切片的索引).</param>
        /// <param name="length"></param>
        /// <returns>a <see cref="Span{T}"/></returns>
        [MyCLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> AsSpan<T>(this ExSpan<T> span, TSize start, int length) {
            if (length < 0)
                ThrowHelper.ThrowArgumentOutOfRangeException();
            return (Span<T>)span.Slice(start, (TSize)(uint)length);
        }

        /// <summary>
        /// Gets a value indicating whether this object is empty (返回一个值，该值指示当前源对象为空).
        /// </summary>
        /// <param name="source">Source object (源对象).</param>
        /// <returns><see langword="true"/> if this object is empty; otherwise, <see langword="false"/> (当前对象为空时为 <see langword="true"/>; 否则为 <see langword="false"/>).</returns>
        [MyCLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEmpty(this IExLength source) {
            return (TSize)0 == source.Length;
        }

        /// <summary>
        /// Convert items data append string. The headerLength parameter uses the value of <see cref="ExMemoryMarshal.SpanViewLength"/> (将各项数据转追加字符串. headerLength 参数使用 <see cref="ExMemoryMarshal.SpanViewLength"/> 的值).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="source">The source data (源数据).</param>
        /// <param name="output">The output <see cref="StringBuilder"/> (输出的 <see cref="StringBuilder"/>).</param>
        /// <param name="itemFormater">The formater of each item (各项的格式化器). Default value is <see cref="ItemFormaters.Default">ItemFormaters.Default</see>. Prototype is `string func(TSize index, T value)`.</param>
        /// <param name="stringFlags">Flags for convert items data into string (各项数据转字符串的标志).</param>
        /// <param name="nameFlags">Flags for type name (类型名的标志).</param>
        /// <seealso cref="ItemFormaters"/>
        [MyCLSCompliant(false)]
        public static void ItemsAppendString<T>(this ReadOnlyExSpan<T> source, StringBuilder output, Func<TSize, T, string>? itemFormater = null, ItemsToStringFlags stringFlags = ItemsToStringFlags.Default, TypeNameFlags nameFlags = TypeNameFlags.Default) {
            ItemsAppendString(source, output, (TSize)ExMemoryMarshal.SpanViewLength, default, itemFormater, stringFlags, nameFlags);
        }

        /// <summary>
        /// Convert items data append string. It has the <paramref name="headerLength"/>, <paramref name="footerLength"/> parameter (将各项数据追加字符串. 它具有 <paramref name="headerLength"/>, <paramref name="footerLength"/> 参数).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="source">The source data (源数据).</param>
        /// <param name="output">The output <see cref="StringBuilder"/> (输出的 <see cref="StringBuilder"/>).</param>
        /// <param name="headerLength">The max length of header data (头部的最大长度).</param>
        /// <param name="footerLength">The max length of footer data (尾部的最大长度).</param>
        /// <param name="itemFormater">The formater of each item (各项的格式化器). Default value is <see cref="ItemFormaters.Default">ItemFormaters.Default</see>. Prototype is `string func(TSize index, T value)`.</param>
        /// <param name="stringFlags">Flags for convert items data into string (各项数据转字符串的标志).</param>
        /// <param name="nameFlags">Flags for type name (类型名的标志).</param>
        /// <seealso cref="ItemFormaters"/>
        [MyCLSCompliant(false)]
        public static void ItemsAppendString<T>(this ReadOnlyExSpan<T> source, StringBuilder output, TSize headerLength, TSize footerLength = default, Func<TSize, T, string>? itemFormater = null, ItemsToStringFlags stringFlags = ItemsToStringFlags.Default, TypeNameFlags nameFlags = TypeNameFlags.Default) {
            if (!stringFlags.HasFlag(ItemsToStringFlags.HideType)) {
                TypeNameUtil.AppendName(output, typeof(ReadOnlyExSpan<T>), nameFlags, null, typeof(T));
            }
            ItemsAppendStringUnsafe(in source.GetPinnableReadOnlyReference(), source.Length, output, headerLength, footerLength, itemFormater, stringFlags);
        }

        /// <inheritdoc cref="ItemsAppendString{T}(ReadOnlyExSpan{T}, StringBuilder, Func{TSize, T, string}?, ItemsToStringFlags, TypeNameFlags)"/>
        [MyCLSCompliant(false)]
        public static void ItemsAppendString<T>(this ExSpan<T> source, StringBuilder output, Func<TSize, T, string>? itemFormater = null, ItemsToStringFlags stringFlags = ItemsToStringFlags.Default, TypeNameFlags nameFlags = TypeNameFlags.Default) {
            ItemsAppendString(source, output, (TSize)ExMemoryMarshal.SpanViewLength, default, itemFormater, stringFlags, nameFlags);
        }

        /// <inheritdoc cref="ItemsAppendString{T}(ReadOnlyExSpan{T}, StringBuilder, TSize, TSize, Func{TSize, T, string}?, ItemsToStringFlags, TypeNameFlags)"/>
        [MyCLSCompliant(false)]
        public static void ItemsAppendString<T>(this ExSpan<T> source, StringBuilder output, TSize headerLength, TSize footerLength = default, Func<TSize, T, string>? itemFormater = null, ItemsToStringFlags stringFlags = ItemsToStringFlags.Default, TypeNameFlags nameFlags = TypeNameFlags.Default) {
            if (!stringFlags.HasFlag(ItemsToStringFlags.HideType)) {
                TypeNameUtil.AppendName(output, typeof(ExSpan<T>), nameFlags, null, typeof(T));
            }
            ItemsAppendStringUnsafe(in source.GetPinnableReadOnlyReference(), source.Length, output, headerLength, footerLength, itemFormater, stringFlags);
        }

        /// <summary>
        /// Convert items data append string. The headerLength parameter uses the value of <see cref="ExMemoryMarshal.SpanViewLength"/> (将各项数据转追加字符串. headerLength 参数使用 <see cref="ExMemoryMarshal.SpanViewLength"/> 的值).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <typeparam name="TSpan">The type of span (跨度的类型).</typeparam>
        /// <param name="source">The source data (源数据).</param>
        /// <param name="typeSample">Sample of type. Only its type is referenced, not its data. (类型的样例. 仅参考它的类型，不使用它的数据).</param>
        /// <param name="output">The output <see cref="StringBuilder"/> (输出的 <see cref="StringBuilder"/>).</param>
        /// <param name="itemFormater">The formater of each item (各项的格式化器). Default value is <see cref="ItemFormaters.Default">ItemFormaters.Default</see>. Prototype is `string func(TSize index, T value)`.</param>
        /// <param name="stringFlags">Flags for convert items data into string (各项数据转字符串的标志).</param>
        /// <param name="nameFlags">Flags for type name (类型名的标志).</param>
        /// <returns>A formatted string (格式化后的字符串).</returns>
        [MyCLSCompliant(false)]
        public static void ItemsAppendString<T, TSpan>(this TSpan source, in T typeSample, StringBuilder output, Func<TSize, T, string>? itemFormater = null, ItemsToStringFlags stringFlags = ItemsToStringFlags.Default, TypeNameFlags nameFlags = TypeNameFlags.Default)
                where TSpan : IReadOnlyExSpanBase<T>
#if ALLOWS_REF_STRUCT
                , allows ref struct
#endif // ALLOWS_REF_STRUCT
                {
            ItemsAppendString(source, in typeSample, output, (TSize)ExMemoryMarshal.SpanViewLength, default, itemFormater, stringFlags, nameFlags);
        }

        /// <summary>
        /// Convert items data append string. It has the <paramref name="headerLength"/>, <paramref name="footerLength"/> parameter (将各项数据追加字符串. 它具有 <paramref name="headerLength"/>, <paramref name="footerLength"/> 参数).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <typeparam name="TSpan">The type of span (跨度的类型).</typeparam>
        /// <param name="source">The source data (源数据).</param>
        /// <param name="typeSample">Sample of type. Only its type is referenced, not its data. (类型的样例. 仅参考它的类型，不使用它的数据).</param>
        /// <param name="output">The output <see cref="StringBuilder"/> (输出的 <see cref="StringBuilder"/>).</param>
        /// <param name="headerLength">The max length of header data (头部的最大长度).</param>
        /// <param name="footerLength">The max length of footer data (尾部的最大长度).</param>
        /// <param name="itemFormater">The formater of each item (各项的格式化器). Default value is <see cref="ItemFormaters.Default">ItemFormaters.Default</see>. Prototype is `string func(TSize index, T value)`.</param>
        /// <param name="stringFlags">Flags for convert items data into string (各项数据转字符串的标志).</param>
        /// <param name="nameFlags">Flags for type name (类型名的标志).</param>
        /// <seealso cref="ItemFormaters"/>
        [MyCLSCompliant(false)]
        public static void ItemsAppendString<T, TSpan>(this TSpan source, in T typeSample, StringBuilder output, TSize headerLength, TSize footerLength = default, Func<TSize, T, string>? itemFormater = null, ItemsToStringFlags stringFlags = ItemsToStringFlags.Default, TypeNameFlags nameFlags = TypeNameFlags.Default)
                where TSpan : IReadOnlyExSpanBase<T>
#if ALLOWS_REF_STRUCT
                , allows ref struct
#endif // ALLOWS_REF_STRUCT
                {
            _ = typeSample;
            if (!stringFlags.HasFlag(ItemsToStringFlags.HideType)) {
                TypeNameUtil.AppendName(output, typeof(TSpan), nameFlags, typeof(IReadOnlyExSpanBase<T>), typeof(T));
            }
            ItemsAppendStringUnsafe(in source.GetPinnableReadOnlyReference(), source.Length, output, headerLength, footerLength, itemFormater, stringFlags);
        }

        /// <summary>
        /// Unsafe convert items data append string. The headerLength parameter uses the value of <see cref="ExMemoryMarshal.SpanViewLength"/> (非安全的将各项数据转追加字符串. headerLength 参数使用 <see cref="ExMemoryMarshal.SpanViewLength"/> 的值).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="source">The reference to source data (源数据的引用).</param>
        /// <param name="length">The length of source data (源数据的长度).</param>
        /// <param name="output">The output <see cref="StringBuilder"/> (输出的 <see cref="StringBuilder"/>).</param>
        /// <param name="itemFormater">The formater of each item (各项的格式化器). Default value is <see cref="ItemFormaters.Default">ItemFormaters.Default</see>. Prototype is `string func(TSize index, T value)`.</param>
        /// <param name="stringFlags">Flags for convert items data into string (各项数据转字符串的标志). <see cref="ItemsToStringFlags.HideType">HideType</see> flag is invalid, please output the type name by yourself (<see cref="ItemsToStringFlags.HideType">HideType</see> 标志无效, 请自行输出类型名称). </param>
        /// <seealso cref="ItemFormaters"/>
        internal static void ItemsAppendStringUnsafe<T>(ref readonly T source, TSize length, StringBuilder output, Func<TSize, T, string>? itemFormater = null, ItemsToStringFlags stringFlags = ItemsToStringFlags.Default) {
            ItemsAppendStringUnsafe(in source, length, output, (TSize)ExMemoryMarshal.SpanViewLength, default, itemFormater, stringFlags);
        }

        /// <summary>
        /// Unsafe convert items data append string. It has the <paramref name="headerLength"/>, <paramref name="footerLength"/> parameter (非安全的将各项数据追加字符串. 它具有 <paramref name="headerLength"/>, <paramref name="footerLength"/> 参数).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="source">The reference to source data (源数据的引用).</param>
        /// <param name="length">The length of source data (源数据的长度).</param>
        /// <param name="output">The output <see cref="StringBuilder"/> (输出的 <see cref="StringBuilder"/>).</param>
        /// <param name="headerLength">The max length of header data (头部的最大长度).</param>
        /// <param name="footerLength">The max length of footer data (尾部的最大长度).</param>
        /// <param name="itemFormater">The formater of each item (各项的格式化器). Default value is <see cref="ItemFormaters.Default">ItemFormaters.Default</see>. Prototype is `string func(TSize index, T value)`.</param>
        /// <param name="stringFlags">Flags for convert items data into string (各项数据转字符串的标志). <see cref="ItemsToStringFlags.HideType">HideType</see> flag is invalid, please output the type name by yourself (<see cref="ItemsToStringFlags.HideType">HideType</see> 标志无效, 请自行输出类型名称). </param>
        /// <seealso cref="ItemFormaters"/>
        internal static void ItemsAppendStringUnsafe<T>(ref readonly T source, TSize length, StringBuilder output, TSize headerLength, TSize footerLength = default, Func<TSize, T, string>? itemFormater = null, ItemsToStringFlags stringFlags = ItemsToStringFlags.Default) {
            //ItemsAppendStringToUnsafe(in source, length, (str) => output.Append(str), headerLength, footerLength, itemFormater, stringFlags); // OK.
            ItemsAppendStringToUnsafe(in source, length, delegate (string str) {
                output.Append(str);
            }, headerLength, footerLength, itemFormater, stringFlags);
        }

        /// <summary>
        /// Convert items data append string to action. The headerLength parameter uses the value of <see cref="ExMemoryMarshal.SpanViewLength"/> (将各项数据转追加字符串到动作. headerLength 参数使用 <see cref="ExMemoryMarshal.SpanViewLength"/> 的值).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="source">The source data (源数据).</param>
        /// <param name="output">The output action (输出动作).</param>
        /// <param name="itemFormater">The formater of each item (各项的格式化器). Default value is <see cref="ItemFormaters.Default">ItemFormaters.Default</see>. Prototype is `string func(TSize index, T value)`.</param>
        /// <param name="stringFlags">Flags for convert items data into string (各项数据转字符串的标志).</param>
        /// <param name="nameFlags">Flags for type name (类型名的标志).</param>
        /// <seealso cref="ItemFormaters"/>
        [MyCLSCompliant(false)]
        public static void ItemsAppendStringTo<T>(this ReadOnlyExSpan<T> source, Action<string> output, Func<TSize, T, string>? itemFormater = null, ItemsToStringFlags stringFlags = ItemsToStringFlags.Default, TypeNameFlags nameFlags = TypeNameFlags.Default) {
            ItemsAppendStringTo(source, output, (TSize)ExMemoryMarshal.SpanViewLength, default, itemFormater, stringFlags, nameFlags);
        }

        /// <summary>
        /// Convert items data append string to action. It has the <paramref name="headerLength"/>, <paramref name="footerLength"/> parameter (将各项数据转追加字符串到动作. 它具有 <paramref name="headerLength"/>, <paramref name="footerLength"/> 参数).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="source">The source data (源数据).</param>
        /// <param name="output">The output action (输出动作).</param>
        /// <param name="headerLength">The max length of header data (头部的最大长度).</param>
        /// <param name="footerLength">The max length of footer data (尾部的最大长度).</param>
        /// <param name="itemFormater">The formater of each item (各项的格式化器). Default value is <see cref="ItemFormaters.Default">ItemFormaters.Default</see>. Prototype is `string func(TSize index, T value)`.</param>
        /// <param name="stringFlags">Flags for convert items data into string (各项数据转字符串的标志).</param>
        /// <param name="nameFlags">Flags for type name (类型名的标志).</param>
        /// <seealso cref="ItemFormaters"/>
        [MyCLSCompliant(false)]
        public static void ItemsAppendStringTo<T>(this ReadOnlyExSpan<T> source, Action<string> output, TSize headerLength, TSize footerLength = default, Func<TSize, T, string>? itemFormater = null, ItemsToStringFlags stringFlags = ItemsToStringFlags.Default, TypeNameFlags nameFlags = TypeNameFlags.Default) {
            if (!stringFlags.HasFlag(ItemsToStringFlags.HideType)) {
                TypeNameUtil.AppendNameTo(output, typeof(ReadOnlyExSpan<T>), nameFlags, null, typeof(T));
            }
            ItemsAppendStringToUnsafe(in source.GetPinnableReadOnlyReference(), source.Length, output, headerLength, footerLength, itemFormater, stringFlags);
        }

        /// <inheritdoc cref="ItemsAppendStringTo{T}(ReadOnlyExSpan{T}, Action{string}, Func{TSize, T, string}?, ItemsToStringFlags, TypeNameFlags)"/>
        [MyCLSCompliant(false)]
        public static void ItemsAppendStringTo<T>(this ExSpan<T> source, Action<string> output, Func<TSize, T, string>? itemFormater = null, ItemsToStringFlags stringFlags = ItemsToStringFlags.Default, TypeNameFlags nameFlags = TypeNameFlags.Default) {
            ItemsAppendStringTo(source, output, (TSize)ExMemoryMarshal.SpanViewLength, default, itemFormater, stringFlags, nameFlags);
        }

        /// <inheritdoc cref="ItemsAppendStringTo{T}(ReadOnlyExSpan{T}, Action{string}, TSize, TSize, Func{TSize, T, string}?, ItemsToStringFlags, TypeNameFlags)"/>
        [MyCLSCompliant(false)]
        public static void ItemsAppendStringTo<T>(this ExSpan<T> source, Action<string> output, TSize headerLength, TSize footerLength = default, Func<TSize, T, string>? itemFormater = null, ItemsToStringFlags stringFlags = ItemsToStringFlags.Default, TypeNameFlags nameFlags = TypeNameFlags.Default) {
            if (!stringFlags.HasFlag(ItemsToStringFlags.HideType)) {
                TypeNameUtil.AppendNameTo(output, typeof(ExSpan<T>), nameFlags, null, typeof(T));
            }
            ItemsAppendStringToUnsafe(in source.GetPinnableReadOnlyReference(), source.Length, output, headerLength, footerLength, itemFormater, stringFlags);
        }

        /// <summary>
        /// Convert items data append string to action. The headerLength parameter uses the value of <see cref="ExMemoryMarshal.SpanViewLength"/> (将各项数据转追加字符串到动作. headerLength 参数使用 <see cref="ExMemoryMarshal.SpanViewLength"/> 的值).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <typeparam name="TSpan">The type of span (跨度的类型).</typeparam>
        /// <param name="source">The source data (源数据).</param>
        /// <param name="typeSample">Sample of type. Only its type is referenced, not its data. (类型的样例. 仅参考它的类型，不使用它的数据).</param>
        /// <param name="output">The output action (输出动作).</param>
        /// <param name="itemFormater">The formater of each item (各项的格式化器). Default value is <see cref="ItemFormaters.Default">ItemFormaters.Default</see>. Prototype is `string func(TSize index, T value)`.</param>
        /// <param name="stringFlags">Flags for convert items data into string (各项数据转字符串的标志).</param>
        /// <param name="nameFlags">Flags for type name (类型名的标志).</param>
        /// <returns>A formatted string (格式化后的字符串).</returns>
        [MyCLSCompliant(false)]
        public static void ItemsAppendStringTo<T, TSpan>(this TSpan source, in T typeSample, Action<string> output, Func<TSize, T, string>? itemFormater = null, ItemsToStringFlags stringFlags = ItemsToStringFlags.Default, TypeNameFlags nameFlags = TypeNameFlags.Default)
                where TSpan : IReadOnlyExSpanBase<T>
#if ALLOWS_REF_STRUCT
                , allows ref struct
#endif // ALLOWS_REF_STRUCT
                {
            ItemsAppendStringTo(source, in typeSample, output, (TSize)ExMemoryMarshal.SpanViewLength, default, itemFormater, stringFlags, nameFlags);
        }

        /// <summary>
        /// Convert items data append string to action. It has the <paramref name="headerLength"/>, <paramref name="footerLength"/> parameter (将各项数据转追加字符串到动作. 它具有 <paramref name="headerLength"/>, <paramref name="footerLength"/> 参数).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <typeparam name="TSpan">The type of span (跨度的类型).</typeparam>
        /// <param name="source">The source data (源数据).</param>
        /// <param name="typeSample">Sample of type. Only its type is referenced, not its data. (类型的样例. 仅参考它的类型，不使用它的数据).</param>
        /// <param name="output">The output action (输出动作).</param>
        /// <param name="headerLength">The max length of header data (头部的最大长度).</param>
        /// <param name="footerLength">The max length of footer data (尾部的最大长度).</param>
        /// <param name="itemFormater">The formater of each item (各项的格式化器). Default value is <see cref="ItemFormaters.Default">ItemFormaters.Default</see>. Prototype is `string func(TSize index, T value)`.</param>
        /// <param name="stringFlags">Flags for convert items data into string (各项数据转字符串的标志).</param>
        /// <param name="nameFlags">Flags for type name (类型名的标志).</param>
        /// <seealso cref="ItemFormaters"/>
        [MyCLSCompliant(false)]
        public static void ItemsAppendStringTo<T, TSpan>(this TSpan source, in T typeSample, Action<string> output, TSize headerLength, TSize footerLength = default, Func<TSize, T, string>? itemFormater = null, ItemsToStringFlags stringFlags = ItemsToStringFlags.Default, TypeNameFlags nameFlags = TypeNameFlags.Default)
                where TSpan : IReadOnlyExSpanBase<T>
#if ALLOWS_REF_STRUCT
                , allows ref struct
#endif // ALLOWS_REF_STRUCT
                {
            _ = typeSample;
            if (!stringFlags.HasFlag(ItemsToStringFlags.HideType)) {
                TypeNameUtil.AppendNameTo(output, typeof(TSpan), nameFlags, typeof(IReadOnlyExSpanBase<T>), typeof(T));
            }
            ItemsAppendStringToUnsafe(in source.GetPinnableReadOnlyReference(), source.Length, output, headerLength, footerLength, itemFormater, stringFlags);
        }

        /// <summary>
        /// Unsafe convert items data append string to action. The headerLength parameter uses the value of <see cref="ExMemoryMarshal.SpanViewLength"/> (非安全的将各项数据转追加字符串到动作. headerLength 参数使用 <see cref="ExMemoryMarshal.SpanViewLength"/> 的值).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="source">The reference to source data (源数据的引用).</param>
        /// <param name="length">The length of source data (源数据的长度).</param>
        /// <param name="output">The output action (输出动作).</param>
        /// <param name="itemFormater">The formater of each item (各项的格式化器). Default value is <see cref="ItemFormaters.Default">ItemFormaters.Default</see>. Prototype is `string func(TSize index, T value)`.</param>
        /// <param name="stringFlags">Flags for convert items data into string (各项数据转字符串的标志). <see cref="ItemsToStringFlags.HideType">HideType</see> flag is invalid, please output the type name by yourself (<see cref="ItemsToStringFlags.HideType">HideType</see> 标志无效, 请自行输出类型名称). </param>
        /// <seealso cref="ItemFormaters"/>
        internal static void ItemsAppendStringToUnsafe<T>(ref readonly T source, TSize length, Action<string> output, Func<TSize, T, string>? itemFormater = null, ItemsToStringFlags stringFlags = ItemsToStringFlags.Default) {
            ItemsAppendStringToUnsafe(in source, length, output, (TSize)ExMemoryMarshal.SpanViewLength, default, itemFormater, stringFlags);
        }

        /// <summary>
        /// Unsafe convert items data append string to action. It has the <paramref name="headerLength"/>, <paramref name="footerLength"/> parameter (非安全的将各项数据转追加字符串到动作. 它具有 <paramref name="headerLength"/>, <paramref name="footerLength"/> 参数).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="source">The reference to source data (源数据的引用).</param>
        /// <param name="length">The length of source data (源数据的长度).</param>
        /// <param name="output">The output action (输出动作).</param>
        /// <param name="headerLength">The max length of header data (头部的最大长度).</param>
        /// <param name="footerLength">The max length of footer data (尾部的最大长度).</param>
        /// <param name="itemFormater">The formater of each item (各项的格式化器). Default value is <see cref="ItemFormaters.Default">ItemFormaters.Default</see>. Prototype is `string func(TSize index, T value)`.</param>
        /// <param name="stringFlags">Flags for convert items data into string (各项数据转字符串的标志). <see cref="ItemsToStringFlags.HideType">HideType</see> flag is invalid, please output the type name by yourself (<see cref="ItemsToStringFlags.HideType">HideType</see> 标志无效, 请自行输出类型名称). </param>
        /// <seealso cref="ItemFormaters"/>
        internal static void ItemsAppendStringToUnsafe<T>(ref readonly T source, TSize length, Action<string> output, TSize headerLength, TSize footerLength = default, Func<TSize, T, string>? itemFormater = null, ItemsToStringFlags stringFlags = ItemsToStringFlags.Default) {
            const string separator = ", ";

            // Output length.
            bool hideLength = ItemsToStringFlags.HideLength == (ItemsToStringFlags.HideLength & stringFlags);
            if (hideLength) {
                // hide.
            } else {
                output($"[{length}]");
            }

            // For consistency, isNoItems is no longer used.
            //bool isNoItems = ((TSize)0 == length) || (((TSize)0 == headerLength) && ((TSize)0 == footerLength));
            //if (isNoItems) {
            //    return;
            //}

            // Output body.
            itemFormater ??= ItemFormaters.Default;
            TSize zero = default;
            TSize headerCount = default;
            TSize footerCount = default;
            TSize footerStart = default;
            if (length <= headerLength) {
                headerCount = length;
            } else {
                headerCount = headerLength;
                footerStart = length - footerLength;
                if (footerStart < headerCount) footerStart = headerCount;
                footerCount = length - footerStart;
            }
            // Output before.
            bool hideBrace = ItemsToStringFlags.HideBrace == (ItemsToStringFlags.HideBrace & stringFlags);
            if (!hideBrace) {
                output("{");
            }
            // Output header.
            ref T p0 = ref Unsafe.AsRef(in source);
            ref T p = ref p0;
            for (TSize i = zero; i < headerCount; i += 1) {
                if (i > zero) {
                    output(separator);
                }
                T value = p;
                string str = itemFormater(i, value);
                output(str);
                // Next.
                p = ref Unsafe.Add(ref p, 1);
            }
            // Output ellipsis.
            if (headerCount < length && headerCount != footerStart) {
                output(separator);
                output("...");
            }
            // Output footer.
            if (footerCount > zero) {
                output(separator);
                // Output.
                p = ref ExUnsafe.Add(ref p0, footerStart);
                for (TSize i = zero; i < footerCount; i += 1) {
                    if (i > zero) {
                        output(separator);
                    }
                    T value = p;
                    string str = itemFormater(i, value);
                    output(str);
                    // Next.
                    p = ref Unsafe.Add(ref p, 1);
                }
            }
            // Output after.
            if (!hideBrace) {
                output("}");
            }
        }

        /// <summary>
        /// Convert items data into string. The headerLength parameter uses the value of <see cref="ExMemoryMarshal.SpanViewLength"/> (将各项数据转为字符串. headerLength 参数使用 <see cref="ExMemoryMarshal.SpanViewLength"/> 的值).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="source">The source data (源数据).</param>
        /// <param name="itemFormater">The formater of each item (各项的格式化器). Default value is <see cref="ItemFormaters.Default">ItemFormaters.Default</see>. Prototype is `string func(TSize index, T value)`.</param>
        /// <param name="stringFlags">Flags for convert items data into string (各项数据转字符串的标志).</param>
        /// <param name="nameFlags">Flags for type name (类型名的标志).</param>
        /// <returns>A formatted string (格式化后的字符串).</returns>
        /// <seealso cref="ItemFormaters"/>
        [MyCLSCompliant(false)]
        public static string ItemsToString<T>(this ReadOnlyExSpan<T> source, Func<TSize, T, string>? itemFormater = null, ItemsToStringFlags stringFlags = ItemsToStringFlags.Default, TypeNameFlags nameFlags = TypeNameFlags.Default) {
            return ItemsToString(source, (TSize)ExMemoryMarshal.SpanViewLength, default, itemFormater, stringFlags, nameFlags);
        }

        /// <summary>
        /// Convert items data into string. It has the <paramref name="headerLength"/>, <paramref name="footerLength"/> parameter (将各项数据转为字符串. 它具有 <paramref name="headerLength"/>, <paramref name="footerLength"/> 参数).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="source">The source data (源数据).</param>
        /// <param name="headerLength">The max length of header data (头部的最大长度).</param>
        /// <param name="footerLength">The max length of footer data (尾部的最大长度).</param>
        /// <param name="itemFormater">The formater of each item (各项的格式化器). Default value is <see cref="ItemFormaters.Default">ItemFormaters.Default</see>. Prototype is `string func(TSize index, T value)`.</param>
        /// <param name="stringFlags">Flags for convert items data into string (各项数据转字符串的标志).</param>
        /// <param name="nameFlags">Flags for type name (类型名的标志).</param>
        /// <returns>A formatted string (格式化后的字符串).</returns>
        /// <seealso cref="ItemFormaters"/>
        [MyCLSCompliant(false)]
        public static string ItemsToString<T>(this ReadOnlyExSpan<T> source, TSize headerLength, TSize footerLength = default, Func<TSize, T, string>? itemFormater = null, ItemsToStringFlags stringFlags = ItemsToStringFlags.Default, TypeNameFlags nameFlags = TypeNameFlags.Default) {
            StringBuilder stringBuilder = new StringBuilder();
            ItemsAppendString(source, stringBuilder, headerLength, footerLength, itemFormater, stringFlags);
            return stringBuilder.ToString();
        }

        /// <inheritdoc cref="ItemsToString{T}(ReadOnlyExSpan{T}, Func{TSize, T, string}?, ItemsToStringFlags, TypeNameFlags)"/>
        [MyCLSCompliant(false)]
        public static string ItemsToString<T>(this ExSpan<T> source, Func<TSize, T, string>? itemFormater = null, ItemsToStringFlags stringFlags = ItemsToStringFlags.Default, TypeNameFlags nameFlags = TypeNameFlags.Default) {
            return ItemsToString(source, (TSize)ExMemoryMarshal.SpanViewLength, default, itemFormater, stringFlags, nameFlags);
        }

        /// <inheritdoc cref="ItemsToString{T}(ReadOnlyExSpan{T}, TSize, TSize, Func{TSize, T, string}?, ItemsToStringFlags, TypeNameFlags)"/>
        [MyCLSCompliant(false)]
        public static string ItemsToString<T>(this ExSpan<T> source, TSize headerLength, TSize footerLength = default, Func<TSize, T, string>? itemFormater = null, ItemsToStringFlags stringFlags = ItemsToStringFlags.Default, TypeNameFlags nameFlags = TypeNameFlags.Default) {
            StringBuilder stringBuilder = new StringBuilder();
            ItemsAppendString(source, stringBuilder, headerLength, footerLength, itemFormater, stringFlags);
            return stringBuilder.ToString();
        }

        // Output.WriteLine("TSpan without typeSample: {0}", span.ItemsToString()); // CS0411 The type arguments for method 'ExSpanExtensions.ItemsToString<T, TSpan>(TSpan, bool)' cannot be inferred from the usage. Try specifying the type arguments explicitly.
        // Output.WriteLine("TSpan without typeSample: {0}", span.ItemsToString<int, ExSpan<int>>()); // OK. But the code is too long. So it was decided to disable it.
        //public static string ItemsToString<T, TSpan>(this TSpan span, ItemsToStringFlags stringFlags = ItemsToStringFlags.Default)
        //        where TSpan : IReadOnlyExSpanBase<T>, allows ref struct {
        //    return ItemsToString(span, span.GetPinnableReadOnlyReference(), null, stringFlags);
        //}

        //Output.WriteLine("TSpan use itemFormater: {0}", span.ItemsToString(ItemFormaters.Hex)); // CS0411 The type arguments for method 'ExSpanExtensions.ItemsToString<T, TSpan>(TSpan, Func<TSize, T, string>?, bool)' cannot be inferred from the usage. Try specifying the type arguments explicitly.
        //public static string ItemsToString<T, TSpan>(this TSpan source, Func<TSize, T, string>? itemFormater = null, ItemsToStringFlags stringFlags = ItemsToStringFlags.Default)
        //        where TSpan : IReadOnlyExSpanBase<T>, allows ref struct {
        //    return ItemsToString(source, source.GetPinnableReadOnlyReference(), (TSize)ExMemoryMarshal.SpanViewLength, default, itemFormater, stringFlags);
        //}

        /// <summary>
        /// Convert items data into string. It has the <paramref name="typeSample"/> parameter. The headerLength parameter uses the value of <see cref="ExMemoryMarshal.SpanViewLength"/> (将各项数据转为字符串. 它具有 <paramref name="typeSample"/> 参数. headerLength 参数使用 <see cref="ExMemoryMarshal.SpanViewLength"/> 的值).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <typeparam name="TSpan">The type of span (跨度的类型).</typeparam>
        /// <param name="source">The source data (源数据).</param>
        /// <param name="typeSample">Sample of type. Only its type is referenced, not its data. (类型的样例. 仅参考它的类型，不使用它的数据).</param>
        /// <param name="itemFormater">The formater of each item (各项的格式化器). Default value is <see cref="ItemFormaters.Default">ItemFormaters.Default</see>. Prototype is `string func(TSize index, T value)`.</param>
        /// <param name="stringFlags">Flags for convert items data into string (各项数据转字符串的标志).</param>
        /// <param name="nameFlags">Flags for type name (类型名的标志).</param>
        /// <returns>A formatted string (格式化后的字符串).</returns>
        /// <seealso cref="ItemFormaters"/>
        [MyCLSCompliant(false)]
        public static string ItemsToString<T, TSpan>(this TSpan source, in T typeSample, Func<TSize, T, string>? itemFormater = null, ItemsToStringFlags stringFlags = ItemsToStringFlags.Default, TypeNameFlags nameFlags = TypeNameFlags.Default)
                where TSpan : IReadOnlyExSpanBase<T>
#if ALLOWS_REF_STRUCT
                , allows ref struct
#endif // ALLOWS_REF_STRUCT
                {
            return ItemsToString(source, in typeSample, (TSize)ExMemoryMarshal.SpanViewLength, default, itemFormater, stringFlags, nameFlags);
        }

        /// <summary>
        /// Convert items data into string. It has the <paramref name="typeSample"/>, <paramref name="headerLength"/>, <paramref name="footerLength"/> parameter (将各项数据转为字符串. 它具有 <paramref name="typeSample"/>, <paramref name="headerLength"/>, <paramref name="footerLength"/> 参数).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <typeparam name="TSpan">The type of span (跨度的类型).</typeparam>
        /// <param name="source">The source data (源数据).</param>
        /// <param name="typeSample">Sample of type. Only its type is referenced, not its data. (类型的样例. 仅参考它的类型，不使用它的数据).</param>
        /// <param name="headerLength">The max length of header data (头部的最大长度).</param>
        /// <param name="footerLength">The max length of footer data (尾部的最大长度).</param>
        /// <param name="itemFormater">The formater of each item (各项的格式化器). Default value is <see cref="ItemFormaters.Default">ItemFormaters.Default</see>. Prototype is `string func(TSize index, T value)`.</param>
        /// <param name="stringFlags">Flags for convert items data into string (各项数据转字符串的标志).</param>
        /// <param name="nameFlags">Flags for type name (类型名的标志).</param>
        /// <returns>A formatted string (格式化后的字符串).</returns>
        /// <seealso cref="ItemFormaters"/>
        [MyCLSCompliant(false)]
        public static string ItemsToString<T, TSpan>(this TSpan source, in T typeSample, TSize headerLength, TSize footerLength = default, Func<TSize, T, string>? itemFormater = null, ItemsToStringFlags stringFlags = ItemsToStringFlags.Default, TypeNameFlags nameFlags = TypeNameFlags.Default)
                where TSpan : IReadOnlyExSpanBase<T>
#if ALLOWS_REF_STRUCT
                , allows ref struct
#endif // ALLOWS_REF_STRUCT
                {
            _ = typeSample;
            StringBuilder stringBuilder = new StringBuilder();
            ItemsAppendString(source, in typeSample, stringBuilder, headerLength, footerLength, itemFormater, stringFlags);
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Unsafe convert items data into string. The headerLength parameter uses the value of <see cref="ExMemoryMarshal.SpanViewLength"/> (非安全的将各项数据转为字符串. headerLength 参数使用 <see cref="ExMemoryMarshal.SpanViewLength"/> 的值).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="source">The reference to source data (源数据的引用).</param>
        /// <param name="length">The length of source data (源数据的长度).</param>
        /// <param name="itemFormater">The formater of each item (各项的格式化器). Default value is <see cref="ItemFormaters.Default">ItemFormaters.Default</see>. Prototype is `string func(TSize index, T value)`.</param>
        /// <param name="stringFlags">Flags for convert items data into string (各项数据转字符串的标志). <see cref="ItemsToStringFlags.HideType">HideType</see> flag is invalid, please output the type name by yourself (<see cref="ItemsToStringFlags.HideType">HideType</see> 标志无效, 请自行输出类型名称). </param>
        /// <returns>A formatted string (格式化后的字符串).</returns>
        /// <seealso cref="ItemFormaters"/>
        internal static string ItemsToStringUnsafe<T>(ref readonly T source, TSize length, Func<TSize, T, string>? itemFormater = null, ItemsToStringFlags stringFlags = ItemsToStringFlags.Default) {
            return ItemsToStringUnsafe(in source, length, (TSize)ExMemoryMarshal.SpanViewLength, default, itemFormater, stringFlags);
        }

        /// <summary>
        /// Unsafe convert items data into string. It has the <paramref name="headerLength"/>, <paramref name="footerLength"/> parameter (非安全的将各项数据转为字符串. 它具有 <paramref name="headerLength"/>, <paramref name="footerLength"/> 参数).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="source">The reference to source data (源数据的引用).</param>
        /// <param name="length">The length of source data (源数据的长度).</param>
        /// <param name="headerLength">The max length of header data (头部的最大长度).</param>
        /// <param name="footerLength">The max length of footer data (尾部的最大长度).</param>
        /// <param name="itemFormater">The formater of each item (各项的格式化器). Default value is <see cref="ItemFormaters.Default">ItemFormaters.Default</see>. Prototype is `string func(TSize index, T value)`.</param>
        /// <param name="stringFlags">Flags for convert items data into string (各项数据转字符串的标志). <see cref="ItemsToStringFlags.HideType">HideType</see> flag is invalid, please output the type name by yourself (<see cref="ItemsToStringFlags.HideType">HideType</see> 标志无效, 请自行输出类型名称). </param>
        /// <returns>A formatted string (格式化后的字符串).</returns>
        /// <seealso cref="ItemFormaters"/>
        internal static string ItemsToStringUnsafe<T>(ref readonly T source, TSize length, TSize headerLength, TSize footerLength = default, Func<TSize, T, string>? itemFormater = null, ItemsToStringFlags stringFlags = ItemsToStringFlags.Default) {
            StringBuilder stringBuilder = new StringBuilder();
            ItemsAppendStringUnsafe(in source, length, stringBuilder, headerLength, footerLength, itemFormater, stringFlags);
            return stringBuilder.ToString();
        }

    }
}
