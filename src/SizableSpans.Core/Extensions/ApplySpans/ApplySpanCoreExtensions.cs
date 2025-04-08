using System;
using System.Collections.Generic;
using System.Text;
using Zyl.SizableSpans.Reflection;

namespace Zyl.SizableSpans.Extensions.ApplySpans {

    /// <summary>
    /// Provides commonly used extension methods for the span-related types, such as <see cref="Span{T}"/> and <see cref="ReadOnlySpan{T}"/> (提供跨度相关的类型的常用的扩展方法，例如 <see cref="Span{T}"/> 和 <see cref="ReadOnlySpan{T}"/>).
    /// </summary>
    public static class ApplySpanCoreExtensions {

        /// <summary>
        /// Convert items data append string. The headerLength parameter uses the value of <see cref="SizableMemoryMarshal.SpanViewLength"/> (将各项数据转追加字符串. headerLength 参数使用 <see cref="SizableMemoryMarshal.SpanViewLength"/> 的值).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="source">The source data (源数据).</param>
        /// <param name="output">The output <see cref="StringBuilder"/> (输出的 <see cref="StringBuilder"/>).</param>
        /// <param name="itemFormater">The formater of each item (各项的格式化器). Default value is <see cref="ItemFormaters.Default">ItemFormaters.Default</see>. Prototype is `string func(TSize index, T value)`.</param>
        /// <param name="stringFlags">Flags for convert items data into string (各项数据转字符串的标志).</param>
        /// <param name="nameFlags">Flags for type name (类型名的标志).</param>
        /// <seealso cref="ItemFormaters"/>
        [CLSCompliant(false)]
        public static void ItemsAppendString<T>(this ReadOnlySpan<T> source, StringBuilder output, Func<TSize, T, string>? itemFormater = null, ItemsToStringFlags stringFlags = ItemsToStringFlags.Default, TypeNameFlags nameFlags = TypeNameFlags.Default) {
            ItemsAppendString(source, output, (TSize)SizableMemoryMarshal.SpanViewLength, default, itemFormater, stringFlags, nameFlags);
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
        [CLSCompliant(false)]
        public static void ItemsAppendString<T>(this ReadOnlySpan<T> source, StringBuilder output, TSize headerLength, TSize footerLength = default, Func<TSize, T, string>? itemFormater = null, ItemsToStringFlags stringFlags = ItemsToStringFlags.Default, TypeNameFlags nameFlags = TypeNameFlags.Default) {
            if (!stringFlags.HasFlag(ItemsToStringFlags.HideType)) {
                TypeNameUtil.AppendName(output, typeof(ReadOnlySpan<T>), nameFlags, null, typeof(T));
            }
            SizableSpanExtensions.ItemsAppendStringUnsafe(in source.GetPinnableReference(), (TSize)source.Length, output, headerLength, footerLength, itemFormater, stringFlags);
        }

        /// <inheritdoc cref="ItemsAppendString{T}(ReadOnlySpan{T}, Func{nuint, T, string}?, ItemsToStringFlags, TypeNameFlags)"/>
        [CLSCompliant(false)]
        public static void ItemsAppendString<T>(this Span<T> source, StringBuilder output, Func<TSize, T, string>? itemFormater = null, ItemsToStringFlags stringFlags = ItemsToStringFlags.Default, TypeNameFlags nameFlags = TypeNameFlags.Default) {
            ItemsAppendString(source, output, (TSize)SizableMemoryMarshal.SpanViewLength, default, itemFormater, stringFlags, nameFlags);
        }

        /// <inheritdoc cref="ItemsAppendString{T}(ReadOnlySpan{T}, nuint, nuint, Func{nuint, T, string}?, ItemsToStringFlags, TypeNameFlags)"/>
        [CLSCompliant(false)]
        public static void ItemsAppendString<T>(this Span<T> source, StringBuilder output, TSize headerLength, TSize footerLength = default, Func<TSize, T, string>? itemFormater = null, ItemsToStringFlags stringFlags = ItemsToStringFlags.Default, TypeNameFlags nameFlags = TypeNameFlags.Default) {
            if (!stringFlags.HasFlag(ItemsToStringFlags.HideType)) {
                TypeNameUtil.AppendName(output, typeof(Span<T>), nameFlags, null, typeof(T));
            }
            SizableSpanExtensions.ItemsAppendStringUnsafe(in source.GetPinnableReference(), (TSize)source.Length, output, headerLength, footerLength, itemFormater, stringFlags);
        }


        /// <summary>
        /// Convert items data append string to action. The headerLength parameter uses the value of <see cref="SizableMemoryMarshal.SpanViewLength"/> (将各项数据转追加字符串到动作. headerLength 参数使用 <see cref="SizableMemoryMarshal.SpanViewLength"/> 的值).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="source">The source data (源数据).</param>
        /// <param name="output">The output action (输出动作).</param>
        /// <param name="itemFormater">The formater of each item (各项的格式化器). Default value is <see cref="ItemFormaters.Default">ItemFormaters.Default</see>. Prototype is `string func(TSize index, T value)`.</param>
        /// <param name="stringFlags">Flags for convert items data into string (各项数据转字符串的标志).</param>
        /// <param name="nameFlags">Flags for type name (类型名的标志).</param>
        /// <seealso cref="ItemFormaters"/>
        [CLSCompliant(false)]
        public static void ItemsAppendStringTo<T>(this ReadOnlySpan<T> source, Action<string> output, Func<TSize, T, string>? itemFormater = null, ItemsToStringFlags stringFlags = ItemsToStringFlags.Default, TypeNameFlags nameFlags = TypeNameFlags.Default) {
            ItemsAppendStringTo(source, output, (TSize)SizableMemoryMarshal.SpanViewLength, default, itemFormater, stringFlags, nameFlags);
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
        [CLSCompliant(false)]
        public static void ItemsAppendStringTo<T>(this ReadOnlySpan<T> source, Action<string> output, TSize headerLength, TSize footerLength = default, Func<TSize, T, string>? itemFormater = null, ItemsToStringFlags stringFlags = ItemsToStringFlags.Default, TypeNameFlags nameFlags = TypeNameFlags.Default) {
            if (!stringFlags.HasFlag(ItemsToStringFlags.HideType)) {
                TypeNameUtil.AppendNameTo(output, typeof(ReadOnlySpan<T>), nameFlags, null, typeof(T));
            }
            SizableSpanExtensions.ItemsAppendStringToUnsafe(in source.GetPinnableReference(), (TSize)source.Length, output, headerLength, footerLength, itemFormater, stringFlags);
        }

        /// <inheritdoc cref="ItemsAppendStringTo{T}(ReadOnlySpan{T}, Func{nuint, T, string}?, ItemsToStringFlags, TypeNameFlags)"/>
        [CLSCompliant(false)]
        public static void ItemsAppendStringTo<T>(this Span<T> source, Action<string> output, Func<TSize, T, string>? itemFormater = null, ItemsToStringFlags stringFlags = ItemsToStringFlags.Default, TypeNameFlags nameFlags = TypeNameFlags.Default) {
            ItemsAppendStringTo(source, output, (TSize)SizableMemoryMarshal.SpanViewLength, default, itemFormater, stringFlags, nameFlags);
        }

        /// <inheritdoc cref="ItemsAppendStringTo{T}(ReadOnlySpan{T}, nuint, nuint, Func{nuint, T, string}?, ItemsToStringFlags, TypeNameFlags)"/>
        [CLSCompliant(false)]
        public static void ItemsAppendStringTo<T>(this Span<T> source, Action<string> output, TSize headerLength, TSize footerLength = default, Func<TSize, T, string>? itemFormater = null, ItemsToStringFlags stringFlags = ItemsToStringFlags.Default, TypeNameFlags nameFlags = TypeNameFlags.Default) {
            if (!stringFlags.HasFlag(ItemsToStringFlags.HideType)) {
                TypeNameUtil.AppendNameTo(output, typeof(Span<T>), nameFlags, null, typeof(T));
            }
            SizableSpanExtensions.ItemsAppendStringToUnsafe(in source.GetPinnableReference(), (TSize)source.Length, output, headerLength, footerLength, itemFormater, stringFlags);
        }

        /// <summary>
        /// Convert items data into string. The headerLength parameter uses the value of <see cref="SizableMemoryMarshal.SpanViewLength"/> (将各项数据转为字符串. headerLength 参数使用 <see cref="SizableMemoryMarshal.SpanViewLength"/> 的值).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="source">The source data (源数据).</param>
        /// <param name="itemFormater">The formater of each item (各项的格式化器). Default value is <see cref="ItemFormaters.Default">ItemFormaters.Default</see>. Prototype is `string func(TSize index, T value)`.</param>
        /// <param name="stringFlags">Flags for convert items data into string (各项数据转字符串的标志).</param>
        /// <param name="nameFlags">Flags for type name (类型名的标志).</param>
        /// <returns>A formatted string (格式化后的字符串).</returns>
        /// <seealso cref="ItemFormaters"/>
        [CLSCompliant(false)]
        public static string ItemsToString<T>(this ReadOnlySpan<T> source, Func<TSize, T, string>? itemFormater = null, ItemsToStringFlags stringFlags = ItemsToStringFlags.Default, TypeNameFlags nameFlags = TypeNameFlags.Default) {
            return ItemsToString(source, (TSize)SizableMemoryMarshal.SpanViewLength, default, itemFormater, stringFlags, nameFlags);
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
        [CLSCompliant(false)]
        public static string ItemsToString<T>(this ReadOnlySpan<T> source, TSize headerLength, TSize footerLength = default, Func<TSize, T, string>? itemFormater = null, ItemsToStringFlags stringFlags = ItemsToStringFlags.Default, TypeNameFlags nameFlags = TypeNameFlags.Default) {
            StringBuilder stringBuilder = new StringBuilder();
            ItemsAppendString(source, stringBuilder, headerLength, footerLength, itemFormater, stringFlags);
            return stringBuilder.ToString();
        }

        /// <inheritdoc cref="ItemsToString{T}(ReadOnlySpan{T}, Func{nuint, T, string}?, ItemsToStringFlags, TypeNameFlags)"/>
        [CLSCompliant(false)]
        public static string ItemsToString<T>(this Span<T> source, Func<TSize, T, string>? itemFormater = null, ItemsToStringFlags stringFlags = ItemsToStringFlags.Default, TypeNameFlags nameFlags = TypeNameFlags.Default) {
            return ItemsToString(source, (TSize)SizableMemoryMarshal.SpanViewLength, default, itemFormater, stringFlags, nameFlags);
        }

        /// <inheritdoc cref="ItemsToString{T}(ReadOnlySpan{T}, nuint, nuint, Func{nuint, T, string}?, ItemsToStringFlags, TypeNameFlags)"/>
        [CLSCompliant(false)]
        public static string ItemsToString<T>(this Span<T> source, TSize headerLength, TSize footerLength = default, Func<TSize, T, string>? itemFormater = null, ItemsToStringFlags stringFlags = ItemsToStringFlags.Default, TypeNameFlags nameFlags = TypeNameFlags.Default) {
            StringBuilder stringBuilder = new StringBuilder();
            ItemsAppendString(source, stringBuilder, headerLength, footerLength, itemFormater, stringFlags);
            return stringBuilder.ToString();
        }

    }

}
