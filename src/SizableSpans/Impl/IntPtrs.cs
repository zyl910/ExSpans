#if NET7_0_OR_GREATER
#define GENERIC_MATH // C# 11 - Generic math support. https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-11#generic-math-support
#endif // NET7_0_OR_GREATER

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Zyl.SizableSpans.Impl {
    /// <summary>
    /// <see cref="IntPtr"/> utils (IntPtr 的工具方法).
    /// </summary>
    public static class IntPtrs {

        /// <summary>
        /// Add (加法).
        /// </summary>
        /// <param name="left">Left value (左值).</param>
        /// <param name="right">Right value (右值).</param>
        /// <returns>The sum of left and right (左值与右值的和).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nint Add(nint left, nint right) {
#if GENERIC_MATH
            return left + right;
#else
            unsafe {
                //return (nint)((void*)left + (void*)right);
                return (nint)Unsafe.AsPointer( ref SizableUnsafe.Add(ref Unsafe.AsRef<byte>((void*)left), right));
            }
#endif
        }

        /// <summary>
        /// Add (加法).
        /// </summary>
        /// <param name="left">Left value (左值).</param>
        /// <param name="right">Right value (右值).</param>
        /// <returns>The sum of left and right (左值与右值的和).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint Add(nuint left, nuint right) {
#if GENERIC_MATH
            return left + right;
#else
            unsafe {
                return (nuint)Unsafe.AsPointer(ref SizableUnsafe.Add(ref Unsafe.AsRef<byte>((void*)left), right));
            }
#endif
        }

//        /// <summary>
//        /// Indicates whether the current object is equal to another object of the same type (指示当前对象是否等于同一类型的另一个对象).
//        /// </summary>
//        /// <param name="left">Left value (左值).</param>
//        /// <param name="right">Right value (右值).</param>
//        /// <returns><see langword="true"/> if equal; otherwise, <see langword="false"/></returns>
//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        public static bool Equals(nint left, nint right) {
//#if GENERIC_MATH
//            return left < right;
//#else
//            unsafe {
//                return (void*)left < (void*)right;
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
//        public static bool Equals(nuint left, nuint right) {
//#if GENERIC_MATH
//            return left < right;
//#else
//            unsafe {
//                return (void*)left < (void*)right;
//            }
//#endif
//        }

        /// <summary>
        /// Greater than (大于比较).
        /// </summary>
        /// <param name="left">Left value (左值).</param>
        /// <param name="right">Right value (右值).</param>
        /// <returns><see langword="true"/> if greater; otherwise, <see langword="false"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GreaterThan(nint left, nint right) {
#if GENERIC_MATH
            return left > right;
#else
            unsafe {
                return (void*)left > (void*)right;
            }
#endif
        }

        /// <summary>
        /// Greater than (大于比较).
        /// </summary>
        /// <param name="left">Left value (左值).</param>
        /// <param name="right">Right value (右值).</param>
        /// <returns><see langword="true"/> if greater; otherwise, <see langword="false"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GreaterThan(nuint left, nuint right) {
#if GENERIC_MATH
            return left > right;
#else
            unsafe {
                return (void*)left > (void*)right;
            }
#endif
        }

        /// <summary>
        /// Greater or equal than (大于或等于).
        /// </summary>
        /// <param name="left">Left value (左值).</param>
        /// <param name="right">Right value (右值).</param>
        /// <returns><see langword="true"/> if greater or equal; otherwise, <see langword="false"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GreaterThanOrEqual(nint left, nint right) {
#if GENERIC_MATH
            return left >= right;
#else
            unsafe {
                return (void*)left >= (void*)right;
            }
#endif
        }

        /// <summary>
        /// Greater or equal than (大于或等于).
        /// </summary>
        /// <param name="left">Left value (左值).</param>
        /// <param name="right">Right value (右值).</param>
        /// <returns><see langword="true"/> if greater or equal; otherwise, <see langword="false"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GreaterThanOrEqual(nuint left, nuint right) {
#if GENERIC_MATH
            return left >= right;
#else
            unsafe {
                return (void*)left >= (void*)right;
            }
#endif
        }

        /// <summary>
        /// Less than (小于比较).
        /// </summary>
        /// <param name="left">Left value (左值).</param>
        /// <param name="right">Right value (右值).</param>
        /// <returns><see langword="true"/> if less; otherwise, <see langword="false"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool LessThan(nint left, nint right) {
#if GENERIC_MATH
            return left < right;
#else
            unsafe {
                return (void*)left < (void*)right;
            }
#endif
        }

        /// <summary>
        /// Less than (小于比较).
        /// </summary>
        /// <param name="left">Left value (左值).</param>
        /// <param name="right">Right value (右值).</param>
        /// <returns><see langword="true"/> if less; otherwise, <see langword="false"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool LessThan(nuint left, nuint right) {
#if GENERIC_MATH
            return left < right;
#else
            unsafe {
                return (void*)left < (void*)right;
            }
#endif
        }

        /// <summary>
        /// Less or equal than (小于或等于).
        /// </summary>
        /// <param name="left">Left value (左值).</param>
        /// <param name="right">Right value (右值).</param>
        /// <returns><see langword="true"/> if less or equal; otherwise, <see langword="false"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool LessThanOrEqual(nint left, nint right) {
#if GENERIC_MATH
            return left <= right;
#else
            unsafe {
                return (void*)left <= (void*)right;
            }
#endif
        }

        /// <summary>
        /// Less or equal than (小于或等于).
        /// </summary>
        /// <param name="left">Left value (左值).</param>
        /// <param name="right">Right value (右值).</param>
        /// <returns><see langword="true"/> if less or equal; otherwise, <see langword="false"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool LessThanOrEqual(nuint left, nuint right) {
#if GENERIC_MATH
            return left <= right;
#else
            unsafe {
                return (void*)left <= (void*)right;
            }
#endif
        }


        /// <summary>
        /// Subtract (减法).
        /// </summary>
        /// <param name="left">Left value (左值).</param>
        /// <param name="right">Right value (右值).</param>
        /// <returns>The difference of left and right (左值与右值的差).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nint Subtract(nint left, nint right) {
#if GENERIC_MATH
            return left - right;
#else
            unsafe {
                //return (nint)((void*)left + (void*)right);
                return (nint)Unsafe.AsPointer(ref SizableUnsafe.Subtract(ref Unsafe.AsRef<byte>((void*)left), right));
            }
#endif
        }

        /// <summary>
        /// Subtract (减法).
        /// </summary>
        /// <param name="left">Left value (左值).</param>
        /// <param name="right">Right value (右值).</param>
        /// <returns>The difference of left and right (左值与右值的差).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint Subtract(nuint left, nuint right) {
#if GENERIC_MATH
            return left - right;
#else
            unsafe {
                return (nuint)Unsafe.AsPointer(ref SizableUnsafe.Subtract(ref Unsafe.AsRef<byte>((void*)left), right));
            }
#endif
        }

        /// <summary>
        /// The <see cref="UIntPtr"/> to <see cref="IntPtr"/>.
        /// </summary>
        /// <param name="source">The source (源).</param>
        /// <returns><see cref="IntPtr"/> value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nint ToIntPtr(nuint source) {
#if NET7_0_OR_GREATER
            return (nint)source;
#else
            //return (nint)(void*)source;
            return Unsafe.As<nuint, nint>(ref source);
#endif // NET7_0_OR_GREATER
        }

        /// <summary>
        /// The <see cref="IntPtr"/> to <see cref="UIntPtr"/>.
        /// </summary>
        /// <param name="source">The source (源).</param>
        /// <returns><see cref="UIntPtr"/> value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint ToUIntPtr(nint source) {
#if NET7_0_OR_GREATER
            return (nuint)source;
#else
            //return (nuint)(void*)source;
            return Unsafe.As<nint, nuint>(ref source);
#endif // NET7_0_OR_GREATER
        }

    }
}
