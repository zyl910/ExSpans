#if NET7_0_OR_GREATER
#define GENERIC_MATH // C# 11 - Generic math support. https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-11#generic-math-support
#endif // NET7_0_OR_GREATER

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Zyl.SizableSpans.Impl;

namespace Zyl.SizableSpans.Extensions {
    /// <summary>
    /// Extensions of <see cref="IntPtr"/> types (<see cref="IntPtr"/> 类型的扩展)
    /// </summary>
    public static class IntPtrExtensions {

        /// <summary>
        /// Add (加法). <c>left + right</c>.
        /// </summary>
        /// <param name="left">Left value (左值).</param>
        /// <param name="right">Right value (右值).</param>
        /// <returns>The sum of left and right (左值与右值的和).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nint Add(this nint left, nint right) {
            return left + right;
        }

        /// <summary>
        /// Add (加法). <c>left + right</c>.
        /// </summary>
        /// <param name="left">Left value (左值).</param>
        /// <param name="right">Right value (右值).</param>
        /// <returns>The sum of left and right (左值与右值的和).</returns>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint Add(this nuint left, nuint right) {
            return left + right;
        }

//        /// <summary>
//        /// Indicates whether the current object is equal to another object of the same type (指示当前对象是否等于同一类型的另一个对象).
//        /// </summary>
//        /// <param name="left">Left value (左值).</param>
//        /// <param name="right">Right value (右值).</param>
//        /// <returns><see langword="true"/> if equal; otherwise, <see langword="false"/></returns>
//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        public static bool Equals(this nint left, nint right) {
//#if GENERIC_MATH
//            return left == right;
//#else
//            unsafe {
//                return (void*)left == (void*)right;
//            }
//#endif
//        }

//        /// <summary>
//        /// Indicates whether the current object is equal to another object of the same type (指示当前对象是否等于同一类型的另一个对象).
//        /// </summary>
//        /// <param name="left">Left value (左值).</param>
//        /// <param name="right">Right value (右值).</param>
//        /// <returns><see langword="true"/> if equal; otherwise, <see langword="false"/></returns>
//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        public static bool Equals(this nuint left, nuint right) {
//#if GENERIC_MATH
//            return left == right;
//#else
//            unsafe {
//                return (void*)left == (void*)right;
//            }
//#endif
//        }

        /// <summary>
        /// Greater than (大于比较). <c>left &gt; right</c>.
        /// </summary>
        /// <param name="left">Left value (左值).</param>
        /// <param name="right">Right value (右值).</param>
        /// <returns><see langword="true"/> if greater; otherwise, <see langword="false"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GreaterThan(this nint left, nint right) {
            return left > right;
        }

        /// <summary>
        /// Greater than (大于比较). <c>left &gt; right</c>.
        /// </summary>
        /// <param name="left">Left value (左值).</param>
        /// <param name="right">Right value (右值).</param>
        /// <returns><see langword="true"/> if greater; otherwise, <see langword="false"/></returns>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GreaterThan(this nuint left, nuint right) {
            return left > right;
        }

        /// <summary>
        /// Greater or equal than (大于或等于). <c>left &gt;= right</c>.
        /// </summary>
        /// <param name="left">Left value (左值).</param>
        /// <param name="right">Right value (右值).</param>
        /// <returns><see langword="true"/> if greater or equal; otherwise, <see langword="false"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GreaterThanOrEqual(this nint left, nint right) {
            return left >= right;
        }

        /// <summary>
        /// Greater or equal than (大于或等于). <c>left &gt;= right</c>.
        /// </summary>
        /// <param name="left">Left value (左值).</param>
        /// <param name="right">Right value (右值).</param>
        /// <returns><see langword="true"/> if greater or equal; otherwise, <see langword="false"/></returns>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GreaterThanOrEqual(this nuint left, nuint right) {
            return left >= right;
        }

        /// <summary>
        /// Less than (小于比较). <c>left &lt; right</c>.
        /// </summary>
        /// <param name="left">Left value (左值).</param>
        /// <param name="right">Right value (右值).</param>
        /// <returns><see langword="true"/> if less; otherwise, <see langword="false"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool LessThan(this nint left, nint right) {
            return left < right;
        }

        /// <summary>
        /// Less than (小于比较). <c>left &lt; right</c>.
        /// </summary>
        /// <param name="left">Left value (左值).</param>
        /// <param name="right">Right value (右值).</param>
        /// <returns><see langword="true"/> if less; otherwise, <see langword="false"/></returns>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool LessThan(this nuint left, nuint right) {
            return left < right;
        }

        /// <summary>
        /// Less or equal than (小于或等于). <c>left &lt;= right</c>.
        /// </summary>
        /// <param name="left">Left value (左值).</param>
        /// <param name="right">Right value (右值).</param>
        /// <returns><see langword="true"/> if less or equal; otherwise, <see langword="false"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool LessThanOrEqual(this nint left, nint right) {
            return left <= right;
        }

        /// <summary>
        /// Less or equal than (小于或等于). <c>left &lt;= right</c>.
        /// </summary>
        /// <param name="left">Left value (左值).</param>
        /// <param name="right">Right value (右值).</param>
        /// <returns><see langword="true"/> if less or equal; otherwise, <see langword="false"/></returns>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool LessThanOrEqual(this nuint left, nuint right) {
            return left <= right;
        }

        /// <summary>
        /// Multiply (乘法). <c>left * right</c>.
        /// </summary>
        /// <param name="left">Left value (左值).</param>
        /// <param name="right">Right value (右值).</param>
        /// <returns>The product of left and right (左值与右值的积).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nint Multiply(this nint left, nint right) {
            return (nint)(left * right);
        }

        /// <summary>
        /// Multiply (乘法). <c>left * right</c>.
        /// </summary>
        /// <param name="left">Left value (左值).</param>
        /// <param name="right">Right value (右值).</param>
        /// <returns>The product of left and right (左值与右值的积).</returns>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint Multiply(this nuint left, nuint right) {
            return (nuint)(left * right);
        }

        /// <summary>
        /// Multiply - Checked(乘法 - 检查). <c>checked(left * right)</c>.
        /// </summary>
        /// <param name="left">Left value (左值).</param>
        /// <param name="right">Right value (右值).</param>
        /// <returns>The product of left and right (左值与右值的积).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nint MultiplyChecked(this nint left, nint right) {
            return checked((nint)(left * right));
        }

        /// <summary>
        /// Multiply - Checked(乘法 - 检查). <c>checked(left * right)</c>.
        /// </summary>
        /// <param name="left">Left value (左值).</param>
        /// <param name="right">Right value (右值).</param>
        /// <returns>The product of left and right (左值与右值的积).</returns>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint MultiplyChecked(this nuint left, nuint right) {
            return checked((nuint)(left * right));
        }

        /// <summary>
        /// Subtract (减法). <c>left - right</c>.
        /// </summary>
        /// <param name="left">Left value (左值).</param>
        /// <param name="right">Right value (右值).</param>
        /// <returns>The difference of left and right (左值与右值的差).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nint Subtract(this nint left, nint right) {
            return left - right;
        }

        /// <summary>
        /// Subtract (减法). <c>left - right</c>.
        /// </summary>
        /// <param name="left">Left value (左值).</param>
        /// <param name="right">Right value (右值).</param>
        /// <returns>The difference of left and right (左值与右值的差).</returns>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint Subtract(this nuint left, nuint right) {
            return left - right;
        }

        /// <summary>
        /// Convert <see cref="UIntPtr"/> saturating to <see cref="Int32"/> (将 <see cref="UIntPtr"/> 饱和转换为 <see cref="Int32"/>).
        /// </summary>
        /// <param name="source">Source value (源值).</param>
        /// <returns>A value after saturating convert (饱和转换后的值).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SaturatingToInt32(this nint source) {
            int dst = source.LessThan(int.MinValue) ? int.MinValue : (source.LessThan(int.MaxValue) ? (int)source : int.MaxValue);
            return dst;
        }

        /// <summary>
        /// Convert <see cref="UIntPtr"/> saturating to <see cref="Int32"/> (将 <see cref="UIntPtr"/> 饱和转换为 <see cref="Int32"/>).
        /// </summary>
        /// <param name="source">Source value (源值).</param>
        /// <returns>A value after saturating convert (饱和转换后的值).</returns>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SaturatingToInt32(this nuint source) {
            int dst = source.LessThan((uint)int.MaxValue) ? (int)source : int.MaxValue;
            return dst;
        }

        /// <summary>
        /// Convert <see cref="UInt64"/> saturating to <see cref="IntPtr"/> (将 <see cref="UInt64"/> 饱和转换为 <see cref="IntPtr"/>).
        /// </summary>
        /// <param name="source">Source value (源值).</param>
        /// <returns>A value after saturating convert (饱和转换后的值).</returns>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nint SaturatingToIntPtr(this ulong source) {
            nint dst;
            if (SizableMemoryMarshal.Is64BitProcess) {
                dst = (nint)((source < long.MaxValue) ? source : long.MaxValue);
            } else {
                dst = (nint)((source < int.MaxValue) ? source : int.MaxValue);
            }
            return dst;
        }

        /// <summary>
        /// Convert <see cref="UInt64"/> saturating to <see cref="UIntPtr"/> (将 <see cref="UInt64"/> 饱和转换为 <see cref="UIntPtr"/>).
        /// </summary>
        /// <param name="source">Source value (源值).</param>
        /// <returns>A value after saturating convert (饱和转换后的值).</returns>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint SaturatingToUIntPtr(this ulong source) {
            nuint dst;
            if (SizableMemoryMarshal.Is64BitProcess) {
                dst = (nuint)source;
            } else {
                dst = (nuint)((source < uint.MaxValue) ? source : uint.MaxValue);
            }
            return dst;
        }

        /// <summary>
        /// Convert <see cref="UInt64"/> saturating to TSize (将 <see cref="UInt64"/> 饱和转换为 TSize).
        /// </summary>
        /// <param name="source">Source value (源值).</param>
        /// <returns>A value after saturating convert (饱和转换后的值).</returns>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSize SaturatingToTSize(this ulong source) {
#if SIZE_UINTPTR
            return SaturatingToUIntPtr(source);
#else
            return SaturatingToIntPtr(source);
#endif // SIZE_UINTPTR
        }

        /// <summary>
        /// The <see cref="IntPtr"/> to <see cref="IntPtr"/>.
        /// </summary>
        /// <param name="source">The source (源).</param>
        /// <returns><see cref="IntPtr"/> value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nint ToIntPtr(this nint source) {
            return source;
        }

        /// <summary>
        /// The <see cref="UIntPtr"/> to <see cref="IntPtr"/>.
        /// </summary>
        /// <param name="source">The source (源).</param>
        /// <returns><see cref="IntPtr"/> value.</returns>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nint ToIntPtr(this nuint source) {
#if GENERIC_MATH
            return (nint)source;
#else
            //return (nint)(void*)source;
            return Unsafe.As<nuint, nint>(ref source);
#endif // GENERIC_MATH
        }

        /// <summary>
        /// The <see cref="IntPtr"/> to <see cref="UIntPtr"/>.
        /// </summary>
        /// <param name="source">The source (源).</param>
        /// <returns><see cref="UIntPtr"/> value.</returns>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint ToUIntPtr(this nint source) {
#if GENERIC_MATH
            return (nuint)source;
#else
            //return (nuint)(void*)source;
            return Unsafe.As<nint, nuint>(ref source);
#endif // GENERIC_MATH
        }

        /// <summary>
        /// The <see cref="UIntPtr"/> to <see cref="UIntPtr"/>.
        /// </summary>
        /// <param name="source">The source (源).</param>
        /// <returns><see cref="UIntPtr"/> value.</returns>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint ToUIntPtr(this nuint source) {
            return source;
        }

    }
}
