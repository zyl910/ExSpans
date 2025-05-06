#if NET9_0_OR_GREATER
#define ALLOWS_REF_STRUCT // C# 13 - ref struct interface; allows ref struct. https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-13#ref-struct-interfaces
#endif // NET9_0_OR_GREATER

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Zyl.ExSpans.Reflection {
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
#if ALLOWS_REF_STRUCT
                where T : allows ref struct
#endif // ALLOWS_REF_STRUCT
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
#if ALLOWS_REF_STRUCT
                where T : allows ref struct
#endif // ALLOWS_REF_STRUCT
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
#if ALLOWS_REF_STRUCT
                where T : allows ref struct
#endif // ALLOWS_REF_STRUCT
                {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_0_OR_GREATER
            bool isBlittable = !RuntimeHelpers.IsReferenceOrContainsReferences<T>();
#else
            bool isBlittable = IsPrimitive<T>();
#endif
            return isBlittable;
        }

        /// <summary>
        /// Is BitwiseEquatable types (是按位相等的类型)
        /// </summary>
        /// <typeparam name="T">The element type (元素的类型).</typeparam>
        /// <returns>true is BitwiseEquatable types; otherwise is false.</returns>
        /// <remarks>
        /// <para>Due to the fact that the method was not publicly available at the time of execution, it will now rolled back to call the <see cref="IsPrimitive{T}()"/> method (由于运行时尚未公开该方法, 目前会回退为调用 IsPrimitive 方法)</para>
        /// <para>BitwiseEquatable types: https://github.com/dotnet/runtime/issues/46017</para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBitwiseEquatable<T>()
#if ALLOWS_REF_STRUCT
                where T : allows ref struct
#endif // ALLOWS_REF_STRUCT
                {
#if NETX_0_OR_GREATER
            return RuntimeHelpers.IsBitwiseEquatable<T>();
#else
            return IsPrimitive<T>();
#endif
        }

        /// <summary>
        /// Is enum types (是否为枚举类型).
        /// </summary>
        /// <typeparam name="T">The type (类型).</typeparam>
        /// <returns>true if the Type is the enum types; otherwise, false (类型是枚举类型就返回 true; 否则返回 false).</returns>
        /// <seealso cref="Type.IsEnum"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEnum<T>()
#if ALLOWS_REF_STRUCT
                where T : allows ref struct
#endif // ALLOWS_REF_STRUCT
                {
            return IsEnum(typeof(T));
        }

        /// <summary>
        /// Is enum types (是否为枚举类型).
        /// </summary>
        /// <param name="atype">The type (类型).</param>
        /// <returns>true if the Type is the enum types; otherwise, false (类型是枚举类型就返回 true; 否则返回 false).</returns>
        /// <seealso cref="Type.IsEnum"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEnum(Type atype) {
#if NETSTANDARD2_0_OR_GREATER || NETCOREAPP1_0_OR_GREATER || NET20_OR_GREATER
            return atype.IsEnum;
#else
            return atype.GetTypeInfo().IsEnum;
#endif
        }

        /// <summary>
        /// Is generic types (是否为泛型类型).
        /// </summary>
        /// <typeparam name="T">The type (类型).</typeparam>
        /// <returns>true if the Type is the generic types; otherwise, false (类型是泛型类型就返回 true; 否则返回 false).</returns>
        /// <seealso cref="Type.IsGenericType"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsGenericType<T>()
#if ALLOWS_REF_STRUCT
                where T : allows ref struct
#endif // ALLOWS_REF_STRUCT
                {
            return IsGenericType(typeof(T));
        }

        /// <summary>
        /// Is generic types (是否为泛型类型).
        /// </summary>
        /// <param name="atype">The type (类型).</param>
        /// <returns>true if the Type is the generic types; otherwise, false (类型是泛型类型就返回 true; 否则返回 false).</returns>
        /// <seealso cref="Type.IsGenericType"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsGenericType(Type atype) {
#if NETSTANDARD2_0_OR_GREATER || NETCOREAPP1_0_OR_GREATER || NET20_OR_GREATER
            return atype.IsGenericType;
#else
            return atype.GetTypeInfo().IsGenericType;
#endif
        }

        /// <summary>
        /// Is primitive types (是否为基元类型).
        /// </summary>
        /// <typeparam name="T">The type (类型).</typeparam>
        /// <returns>true if the Type is the primitive types; otherwise, false (类型是基元类型就返回 true; 否则返回 false).</returns>
        /// <seealso cref="Type.IsPrimitive"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPrimitive<T>()
#if ALLOWS_REF_STRUCT
                where T : allows ref struct
#endif // ALLOWS_REF_STRUCT
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
        /// Returns a value that indicates whether the specified type is a reference type or a value type that contains references or by-refs (返回一个值，该值指示指定的类型是引用类型还是包含引用或 by-refs 的值类型).
        /// </summary>
        /// <typeparam name="T">The type (类型).</typeparam>
        /// <returns>true if the given type is a reference type or a value type that contains references or by-refs; otherwise, false (类型是是引用类型还是包含引用或 by-refs 的值类型就返回 true; 否则返回 false).</returns>
        /// <remarks>
        /// <para>In .NET Standard 1.1~2.0, due to the inability to accurately determine, we rolled back to call the !<see cref="IsValueType{T}()"/> method (在 .NET Standard 1.1~2.0 时, 因无法准确判断, 于是回退为调用 !IsValueType 方法)</para>
        /// </remarks>
        /// <seealso cref="RuntimeHelpers.IsReferenceOrContainsReferences{T}"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsReferenceOrContainsReferences<T>()
#if ALLOWS_REF_STRUCT
                where T : allows ref struct
#endif // ALLOWS_REF_STRUCT
                {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_0_OR_GREATER
            return RuntimeHelpers.IsReferenceOrContainsReferences<T>();
#else
            return !IsValueType<T>();
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
#if ALLOWS_REF_STRUCT
                where T : allows ref struct
#endif // ALLOWS_REF_STRUCT
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
