using System;
using System.Collections.Generic;
using System.Text;

namespace Zyl.SizableSpans {
    /// <summary>
    /// The formaters of each item (各项的格式化器集).
    /// </summary>
    /// <seealso cref="SizableSpanExtensions.ItemsToString{T}(ReadOnlySizableSpan{T}, Func{nuint, T, string}?, bool)"/>
    public static class ItemFormaters {

        /// <summary>
        /// The formater of each item - Default (各项的格式化器 - 默认).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="index">Item index (项的索引).</param>
        /// <param name="value">Item value (项的值).</param>
        /// <returns>A formatted string (格式化后的字符串).</returns>
        [MyCLSCompliant(false)]
        public static string Default<T>(TSize index, T value) {
            _ = index;
            return value?.ToString() ?? "";
            //return $"{value}";
        }

        /// <summary>
        /// The formater of each item - Hexadecimal (各项的格式化器 - 十六进制).
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <param name="index">Item index (项的索引).</param>
        /// <param name="value">Item value (项的值).</param>
        /// <returns>A formatted string (格式化后的字符串).</returns>
        [MyCLSCompliant(false)]
        public static string Hex<T>(TSize index, T value) {
            _ = index;
            return string.Format("0x{0:X}", value);
        }

    }
}
