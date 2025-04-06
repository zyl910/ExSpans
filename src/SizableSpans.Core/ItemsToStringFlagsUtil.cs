using System;
using System.Collections.Generic;
using System.Text;

namespace Zyl.SizableSpans {

    /// <summary>
    /// Flags for convert items data into string (各项数据转字符串的标志).
    /// </summary>
    [Flags]
    public enum ItemsToStringFlags {
        /// <summary>Default (默认).</summary>
        Default = 0,
        /// <summary>Hide type name (隐藏类型名).</summary>
        HideType = 1 << 0, // 1
        /// <summary>Hide length (隐藏长度).</summary>
        HideLength = 1 << 1, // 2
        /// <summary>Hide brace (隐藏大括号). The brace are `{` or `}` symbols.</summary>
        HideBrace = 1 << 2, // 4
    };

    /// <summary>
    /// The utilities of flags for convert items data into string (各项数据转字符串的标志的工具)
    /// </summary>
    /// <seealso cref="ItemsToStringFlags"/>
    public static class ItemsToStringFlagsUtil {
        /// <summary>
        /// All flags of <see cref="ItemsToStringFlags"/> (ItemsToStringFlags的所有标志).
        /// </summary>
        public static readonly ItemsToStringFlags All = ItemsToStringFlags.HideType | ItemsToStringFlags.HideLength | ItemsToStringFlags.HideBrace;
    }

}
