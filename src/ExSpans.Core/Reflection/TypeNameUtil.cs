#if NET9_0_OR_GREATER
#define ALLOWS_REF_STRUCT // C# 13 - ref struct interface; allows ref struct. https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-13#ref-struct-interfaces
#endif // NET9_0_OR_GREATER

using System;
using System.Collections.Generic;
using System.Text;

namespace Zyl.ExSpans.Reflection {

    /// <summary>
    /// Flags for type name (类型名的标志).
    /// </summary>
    [Flags]
    public enum TypeNameFlags {
        /// <summary>Default (默认).</summary>
        Default = 0,
        /// <summary>Raw type name (原始类型名). Returns <see cref="Type.FullName"/> or <see cref="Type.Name"/>.</summary>
        Raw = 1 << 0, // 1
        /// <summary>Do not use keyword type names. For example, instead of `long`, use `Int64` (不使用关键字类型名. 例如不使用 `long`, 而是使用 `Int64`).</summary>
        NoKeyword = 1 << 1, // 2
        /// <summary>Show <see cref="Nullable{T}"/>. When false, it will be displayed as <c>T?</c> (显示 <see cref="Nullable{T}"/>. 为 false 时会显示成 <c>T?</c>).</summary>
        ShowNullable = 1 << 2, // 4
        /// <summary>Show namespace (显示名称空间).</summary>
        ShowNamespace = 1 << 3, // 8
        /// <summary>Whether generic subtypes show namespaces (泛型子类型是否显示名称空间).</summary>
        SubShowNamespace = 1 << 4, // 16
    };

    /// <summary>
    /// The utilities of type name (类型名的工具)
    /// </summary>
    /// <seealso cref="ItemsToStringFlags"/>
    public static class TypeNameUtil {

        private static readonly Dictionary<string, string> _aliases = new Dictionary<string, string> {
            { typeof(int).FullName!, "int" },
            { typeof(uint).FullName!, "uint" },
            { typeof(long).FullName!, "long" },
            { typeof(ulong).FullName!, "ulong" },
            { typeof(short).FullName!, "short" },
            { typeof(ushort).FullName!, "ushort" },
            { typeof(byte).FullName!, "byte" },
            { typeof(sbyte).FullName!, "sbyte" },
            { typeof(char).FullName!, "char" },
            { typeof(float).FullName!, "float" },
            { typeof(double).FullName!, "double" },
            { typeof(decimal).FullName!, "decimal" },
            { typeof(object).FullName!, "object" },
            { typeof(bool).FullName!, "bool" },
            { typeof(string).FullName!, "string" },
            { typeof(void).FullName!, "void" }
        };

        /// <summary>
        /// Convert the flags of a generic subtype to the current flags (将泛型子类型的标志转为当前标志).
        /// </summary>
        /// <param name="flags">The flags of a generic subtype (泛型子类型的标志)</param>
        /// <returns>The current flags (当前标志)</returns>
        public static TypeNameFlags FromSub(TypeNameFlags flags) {
            TypeNameFlags rt = flags & ~TypeNameFlags.ShowNamespace;
            if (TypeNameFlags.SubShowNamespace == (TypeNameFlags.SubShowNamespace & flags)) rt |= TypeNameFlags.ShowNamespace;
            return rt;
        }

        /// <summary>
        /// Append type name (追加类型名)
        /// </summary>
        /// <typeparam name="T">The target type (目标类型).</typeparam>
        /// <param name="output">The output <see cref="StringBuilder"/> (输出的 <see cref="StringBuilder"/>).</param>
        /// <param name="flags">The flags (标志).</param>
        public static void AppendName<T>(StringBuilder output, TypeNameFlags flags = TypeNameFlags.Default)
#if ALLOWS_REF_STRUCT
                where T : allows ref struct
#endif // ALLOWS_REF_STRUCT
                {
            AppendName(output, typeof(T), flags);
        }

        /// <summary>
        /// Append type name (追加类型名)
        /// </summary>
        /// <param name="output">The output <see cref="StringBuilder"/> (输出的 <see cref="StringBuilder"/>).</param>
        /// <param name="atype">The target type (目标类型).</param>
        /// <param name="flags">The flags (标志).</param>
        /// <param name="typeFallback">The fallback type (回退类型). Only used for .NET Standard 1.1, as it cannot obtain generic arguments, more information needs to be passed through this parameter (仅用于 .NET Standard 1.1，因它不能获取泛型参数, 于是需要借助该参数传递更多信息).</param>
        /// <param name="typeArguments">The type arguments (类型参数列表). Only used for .NET Standard 1.1, as it cannot obtain generic arguments, more information needs to be passed through this parameter (仅用于 .NET Standard 1.1，因它不能获取泛型参数, 于是需要借助该参数传递更多信息).</param>
        public static void AppendName(StringBuilder output, Type atype, TypeNameFlags flags = TypeNameFlags.Default, Type? typeFallback = null, params Type[] typeArguments) {
            // AppendNameTo((str) => output.Append(str), atype, flags, typeFallback, typeArguments); // OK.
            AppendNameTo(delegate (string str) {
                output.Append(str);
            }, atype, flags, typeFallback, typeArguments);
        }

        /// <summary>
        /// Append type name to action (将类型名追加到动作)
        /// </summary>
        /// <typeparam name="T">The target type (目标类型).</typeparam>
        /// <param name="output">The output action (输出动作).</param>
        /// <param name="flags">The flags (标志).</param>
        public static void AppendNameTo<T>(Action<string> output, TypeNameFlags flags = TypeNameFlags.Default)
#if ALLOWS_REF_STRUCT
                where T : allows ref struct
#endif // ALLOWS_REF_STRUCT
                {
            AppendNameTo(output, typeof(T), flags);
        }

        /// <summary>
        /// Append type name to action (将类型名追加到动作)
        /// </summary>
        /// <param name="output">The output action (输出动作).</param>
        /// <param name="atype">The target type (目标类型).</param>
        /// <param name="flags">The flags (标志).</param>
        /// <param name="typeFallback">The fallback type (回退类型). Only used for .NET Standard 1.1, as it cannot obtain generic arguments, more information needs to be passed through this parameter (仅用于 .NET Standard 1.1，因它不能获取泛型参数, 于是需要借助该参数传递更多信息).</param>
        /// <param name="typeArguments">The type arguments (类型参数列表). Only used for .NET Standard 1.1, as it cannot obtain generic arguments, more information needs to be passed through this parameter (仅用于 .NET Standard 1.1，因它不能获取泛型参数, 于是需要借助该参数传递更多信息).</param>
        public static void AppendNameTo(Action<string> output, Type atype, TypeNameFlags flags = TypeNameFlags.Default, Type? typeFallback = null, params Type[] typeArguments) {
            if (null == atype) return;
            bool noKeyword = flags.HasFlag(TypeNameFlags.NoKeyword);
            bool showNullable = flags.HasFlag(TypeNameFlags.ShowNullable);
            bool showNamespace = flags.HasFlag(TypeNameFlags.ShowNamespace);
            if (flags.HasFlag(TypeNameFlags.Raw)) {
                if (showNamespace) {
                    output(atype.FullName ?? atype.Name);
                } else {
                    output(atype.Name);
                }
                return;
            }
            // Array.
            if (atype.IsArray) {
                AppendNameTo(output, atype.GetElementType()!, flags);
                output("[");
                output("]");
                return;
            }
            // Pointer.
            if (atype.IsPointer) {
                AppendNameTo(output, atype.GetElementType()!, flags);
                output("*");
                return;
            }
            // GenericType.
            bool needShowName = true;
            if (TypeHelper.IsGenericType(atype)) {
                needShowName = false;
                TypeNameFlags flagsSub = FromSub(flags);
#if NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_0_OR_GREATER || NET20_OR_GREATER
                _ = typeFallback;
                _ = typeArguments;
                Type[] typeArgumentsCur = atype.GetGenericArguments();
                bool simpleNullable = !showNullable && typeof(Nullable<>).Equals(atype.GetGenericTypeDefinition()) && typeArgumentsCur.Length > 0;
                if (simpleNullable) {
                    AppendNameTo(output, typeArgumentsCur[0], flagsSub);
                    output("?");
                } else {
                    if (showNamespace) {
                        output(TypeHelper.GetFullBaseName(atype));
                    } else {
                        output(TypeHelper.GetBaseName(atype));
                    }
                    if (null != typeArgumentsCur && typeArgumentsCur.Length > 0) {
                        output("<");
                        for (int i = 0; i < typeArgumentsCur.Length; i++) {
                            if (i > 0) {
                                output(", ");
                            }
                            AppendNameTo(output, typeArgumentsCur[i], flagsSub);
                        }
                        output(">");
                    }
                }
#else
                var atype2 = typeFallback ?? atype;
                if (showNamespace) {
                    output(TypeHelper.GetFullBaseName(atype2));
                } else {
                    output(TypeHelper.GetBaseName(atype2));
                }
                if (null != typeArguments && typeArguments.Length > 0) {
                    output("<");
                    for (int i = 0; i < typeArguments.Length; i++) {
                        if (i > 0) {
                            output(", ");
                        }
                        AppendNameTo(output, typeArguments[i], flagsSub);
                    }
                    output(">");
                }
#endif
            }
            // needShowName.
            if (needShowName) {
                if (!noKeyword && _aliases.TryGetValue(atype.FullName ?? "", out string? alias)) {
                    output(alias);
                } else {
                    if (showNamespace) {
                        output(atype.FullName ?? atype.Name);
                    } else {
                        output(atype.Name);
                    }
                }
            }
        }

        /// <summary>
        /// Get type name (取得类型名).
        /// </summary>
        /// <typeparam name="T">The target type (目标类型).</typeparam>
        /// <param name="flags">The flags (标志).</param>
        /// <returns>The type name (类型名).</returns>
        public static string GetName<T>(TypeNameFlags flags = TypeNameFlags.Default)
#if ALLOWS_REF_STRUCT
                where T : allows ref struct
#endif // ALLOWS_REF_STRUCT
                {
            return GetName(typeof(T), flags);
        }

        /// <summary>
        /// Get type name (取得类型名).
        /// </summary>
        /// <param name="atype">The target type (目标类型).</param>
        /// <param name="flags">The flags (标志).</param>
        /// <param name="typeFallback">The fallback type (回退类型). Only used for .NET Standard 1.1, as it cannot obtain generic arguments, more information needs to be passed through this parameter (仅用于 .NET Standard 1.1，因它不能获取泛型参数, 于是需要借助该参数传递更多信息).</param>
        /// <param name="typeArguments">The type arguments (类型参数列表). Only used for .NET Standard 1.1, as it cannot obtain generic arguments, more information needs to be passed through this parameter (仅用于 .NET Standard 1.1，因它不能获取泛型参数, 于是需要借助该参数传递更多信息).</param>
        /// <returns>The type name (类型名).</returns>
        public static string GetName(Type atype, TypeNameFlags flags = TypeNameFlags.Default, Type? typeFallback = null, params Type[] typeArguments) {
            StringBuilder stringBuilder = new StringBuilder();
            AppendName(stringBuilder, atype, flags, typeFallback, typeArguments);
            return stringBuilder.ToString();
        }

    }

}
