#if NET9_0_OR_GREATER
#define STRUCT_WHERE_ALLOWS_REF // C# 13 - ref struct interface; allows ref struct. https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-13#ref-struct-interfaces
#endif // NET9_0_OR_GREATER

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Zyl.SizableSpans.Impl {
    /// <summary>
    /// <see cref="Type"/> Helper.
    /// </summary>
    public static class TypeHelper {

        /// <summary>
        /// Get the base name of the type (取得类型的基本名).
        /// </summary>
        /// <typeparam name="T">The type (类型).</typeparam>
        /// <returns>The base name (基本名).</returns>
        public static string GetBaseName<T>()
#if STRUCT_WHERE_ALLOWS_REF
                where T : allows ref struct
#endif // STRUCT_WHERE_ALLOWS_REF
                {
            return GetBaseName(typeof(T));
        }

        /// <summary>
        /// Get the base name of the type (取得类型的基本名).
        /// </summary>
        /// <param name="atype">The type (类型).</param>
        /// <returns>Full base name (返回完整基本名).</returns>
        public static string GetBaseName(Type atype) {
            string name = atype.Name;
            int m = name.IndexOf('`');
            if (m >= 0) name = name.Substring(0, m);
            return name;
        }

        /// <summary>
        /// Get the namespace and base name of the type (取得类型的名称空间与基本名).
        /// </summary>
        /// <typeparam name="T">The type (类型).</typeparam>
        /// <returns>The full base name (完整基本名).</returns>
        public static string GetFullBaseName<T>()
#if STRUCT_WHERE_ALLOWS_REF
                where T : allows ref struct
#endif // STRUCT_WHERE_ALLOWS_REF
                {
            return GetFullBaseName(typeof(T));
        }

        /// <summary>
        /// Get the namespace and base name of the type (取得类型的名称空间与基本名).
        /// </summary>
        /// <param name="atype">The type (类型).</param>
        /// <returns>The full base name (完整基本名).</returns>
        public static string GetFullBaseName(Type atype) {
            return atype.Namespace + "." + GetBaseName(atype);
        }

        /// <summary>
        /// Is blittable types (是可直接按位复制的类型)
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <returns>true is blittable types; otherwise is false.</returns>
        /// <remarks>
        /// <para>In .NET Standard 1.1~2.0, due to the inability to accurately determine, we rolled back to call the <see cref="IsPrimitive{T}()"/> method (在 .NET Standard 1.1~2.0 时, 因无法准确判断, 于是回退为调用 IsPrimitive 方法)</para>
        /// <para>Blittable types: https://learn.microsoft.com/en-us/dotnet/framework/interop/blittable-and-non-blittable-types</para>
        /// </remarks>
        /// <seealso cref="RuntimeHelpers.IsReferenceOrContainsReferences{T}"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBlittable<T>()
#if STRUCT_WHERE_ALLOWS_REF
                where T : allows ref struct
#endif // STRUCT_WHERE_ALLOWS_REF
                {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_0_OR_GREATER
            bool isBlittable = !RuntimeHelpers.IsReferenceOrContainsReferences<T>();
#else
            bool isBlittable = IsPrimitive<T>();
#endif
            return isBlittable;
        }

        /// <summary>
        /// Is primitive types (是否为基元类型).
        /// </summary>
        /// <typeparam name="T">The type (类型).</typeparam>
        /// <returns>true if the Type is the primitive types; otherwise, false (类型是基元类型就返回 true; 否则返回 false).</returns>
        /// <seealso cref="Type.IsPrimitive"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPrimitive<T>()
#if STRUCT_WHERE_ALLOWS_REF
                where T : allows ref struct
#endif // STRUCT_WHERE_ALLOWS_REF
                {
            return IsPrimitive(typeof(T));
        }

        /// <summary>
        /// Is primitive types (是否为基元类型).
        /// </summary>
        /// <param name="atype">The type (类型).</param>
        /// <returns>true if the Type is the primitive types; otherwise, false (类型是基元类型就返回 true; 否则返回 false).</returns>
        /// <seealso cref="Type.IsPrimitive"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPrimitive(Type atype) {
#if NETSTANDARD2_0_OR_GREATER || NETCOREAPP1_0_OR_GREATER || NET20_OR_GREATER
            return atype.IsPrimitive;
#else
            return atype.GetTypeInfo().IsPrimitive;
#endif
        }

        /// <summary>
        /// Is value types (是否为值类型).
        /// </summary>
        /// <typeparam name="T">The type (类型).</typeparam>
        /// <returns>true if the Type is the value types; otherwise, false (类型是值类型就返回 true; 否则返回 false).</returns>
        /// <seealso cref="Type.IsValueType"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValueType<T>()
#if STRUCT_WHERE_ALLOWS_REF
                where T : allows ref struct
#endif // STRUCT_WHERE_ALLOWS_REF
                {
            return IsValueType(typeof(T));
        }

        /// <summary>
        /// Is value types (是否为值类型).
        /// </summary>
        /// <param name="atype">The type (类型).</param>
        /// <returns>true if the Type is the value types; otherwise, false (类型是值类型就返回 true; 否则返回 false).</returns>
        /// <seealso cref="Type.IsValueType"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValueType(Type atype) {
#if NETSTANDARD2_0_OR_GREATER || NETCOREAPP1_0_OR_GREATER || NET20_OR_GREATER
            return atype.IsValueType;
#else
            return atype.GetTypeInfo().IsValueType;
#endif
        }

    }
}
