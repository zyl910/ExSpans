using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Zyl.VectorTraits.Numerics;

namespace Zyl.ExSpans.Buffers {
    /// <summary>
    /// Utility functions for pointers (指针的工具函数).
    /// </summary>
    public static partial class PointerUtil {

        /// <summary>
        /// Check if the alignment value is valid, an exception will be thrown if it is invalid. It must be a positive number and a power of 2 (检查对齐值是否有效, 无效时会抛出异常. 它必须是正数且为2的幂).
        /// </summary>
        /// <param name="alignment">The alignment value (对齐值).</param>
        /// <exception cref="ArgumentException">The alignment value is invalid.</exception>
        public static void CheckAlignmentValid(nint alignment) {
            bool flag = IsAlignmentValid(alignment);
            if (!flag) {
                throw new ArgumentException(string.Format("The alignment value({0}) is invalid.", (ulong)alignment), nameof(alignment));
            }
        }

        /// <summary>
        /// Check if the alignment value is valid, an exception will be thrown if it is invalid. It must be a positive number and a power of 2 (检查对齐值是否有效, 无效时会抛出异常. 它必须是正数且为2的幂).
        /// </summary>
        /// <param name="alignment">The alignment value (对齐值).</param>
        /// <exception cref="ArgumentException">The alignment value is invalid.</exception>
        [CLSCompliant(false)]
        public static void CheckAlignmentValid(nuint alignment) {
            bool flag = IsAlignmentValid(alignment);
            if (!flag) {
                throw new ArgumentException(string.Format("The alignment value({0}) is invalid.", (ulong)alignment), nameof(alignment));
            }
        }

        /// <summary>
        /// Check if the alignment value is valid or unused, an exception will be thrown if it is invalid. It must be a positive number and a power of 2, or 0 (检查对齐值是否有效, 无效时会抛出异常. 它必须是正数且为2的幂, 或0).
        /// </summary>
        /// <param name="alignment">The alignment value (对齐值).</param>
        /// <exception cref="ArgumentException">The alignment value is invalid.</exception>
        public static void CheckAlignmentValidOrUnused(nint alignment) {
            bool flag = IsAlignmentValidOrUnused(alignment);
            if (!flag) {
                throw new ArgumentException(string.Format("The alignment value({0}) is invalid.", (ulong)alignment), nameof(alignment));
            }
        }

        /// <summary>
        /// Check if the alignment value is valid or unused, an exception will be thrown if it is invalid. It must be a positive number and a power of 2, or 0 (检查对齐值是否有效, 无效时会抛出异常. 它必须是正数且为2的幂, 或0).
        /// </summary>
        /// <param name="alignment">The alignment value (对齐值).</param>
        /// <exception cref="ArgumentException">The alignment value is invalid.</exception>
        [CLSCompliant(false)]
        public static void CheckAlignmentValidOrUnused(nuint alignment) {
            bool flag = IsAlignmentValidOrUnused(alignment);
            if (!flag) {
                throw new ArgumentException(string.Format("The alignment value({0}) is invalid.", (ulong)alignment), nameof(alignment));
            }
        }

        /// <summary>
        /// Get the offset required to align the address to the target value (取得地址对齐到指定值所需的偏移量).
        /// </summary>
        /// <param name="address">The pointer address (指针地址).</param>
        /// <param name="alignment">Alignment value. Note that this method does not check if the alignment value is valid, please use <see cref="IsAlignmentValid(nint)"/> to check it first (对齐值. 注意本方法不会检查对齐值是否有效, 请先使用 IsAlignmentValid 来检查).</param>
        /// <returns>Returns the offset (返回偏移量).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nint GetAlignOffset(nint address, nint alignment) {
            return (nint)GetAlignOffset((nuint)address, (nuint)alignment);
        }

        /// <summary>
        /// Get the offset required to align the address to the target value (取得地址对齐到指定值所需的偏移量).
        /// </summary>
        /// <param name="address">The pointer address (指针地址).</param>
        /// <param name="alignment">Alignment value. Note that this method does not check if the alignment value is valid, please use <see cref="IsAlignmentValid(nuint)"/> to check it first (对齐值. 注意本方法不会检查对齐值是否有效, 请先使用 IsAlignmentValid 来检查).</param>
        /// <returns>Returns the offset (返回偏移量).</returns>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint GetAlignOffset(nuint address, nuint alignment) {
            nuint diff = address & (alignment - 1);
            if (0 == diff) return 0;
            return alignment - diff;
        }

        /// <summary>
        /// Get the offset required to align the address to the target value (取得地址对齐到指定值所需的偏移量).
        /// </summary>
        /// <param name="address">The pointer address (指针地址).</param>
        /// <param name="alignment">Alignment value. Note that this method does not check if the alignment value is valid, please use <see cref="IsAlignmentValid(nint)"/> to check it first (对齐值. 注意本方法不会检查对齐值是否有效, 请先使用 IsAlignmentValid 来检查).</param>
        /// <returns>Returns the offset (返回偏移量).</returns>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static nint GetAlignOffset(void* address, nint alignment) {
            return GetAlignOffset((nint)address, alignment);
        }

        /// <summary>
        /// Get the offset required to align the address to the target value (取得地址对齐到指定值所需的偏移量).
        /// </summary>
        /// <param name="address">The pointer address (指针地址).</param>
        /// <param name="alignment">Alignment value. Note that this method does not check if the alignment value is valid, please use <see cref="IsAlignmentValid(nuint)"/> to check it first (对齐值. 注意本方法不会检查对齐值是否有效, 请先使用 IsAlignmentValid 来检查).</param>
        /// <returns>Returns the offset (返回偏移量).</returns>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static nuint GetAlignOffset(void* address, nuint alignment) {
            return GetAlignOffset((nuint)address, alignment);
        }

        /// <summary>
        /// Get a enough number of items based on byte count (根据字节数, 获得足够的项目数量)
        /// </summary>
        /// <param name="byteCount">Total byte count (总字节数).</param>
        /// <param name="itemSize">The byte count of item (项目的字节数).</param>
        /// <returns>Returns a enough number of items (返回足够的项目数量)</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nint GetEnoughItemCount(nint byteCount, nint itemSize) {
            return (byteCount - 1 + itemSize) / itemSize;
        }

        /// <summary>
        /// Get a enough number of items based on byte count (根据字节数, 获得足够的项目数量)
        /// </summary>
        /// <param name="byteCount">Total byte count (总字节数).</param>
        /// <param name="itemSize">The byte count of item (项目的字节数).</param>
        /// <returns>Returns a enough number of items (返回足够的项目数量)</returns>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint GetEnoughItemCount(nuint byteCount, nuint itemSize) {
            return (byteCount - 1 + itemSize) / itemSize;
        }

        /// <summary>
        /// Is the address aligned (地址是否已对齐).
        /// </summary>
        /// <param name="address">The pointer address (指针地址).</param>
        /// <param name="alignment">Alignment value. Note that this method does not check if the alignment value is valid, please use <see cref="IsAlignmentValid(nint)"/> to check it first (对齐值. 注意本方法不会检查对齐值是否有效, 请先使用 IsAlignmentValid 来检查).</param>
        /// <returns>Returns true if aligned, false otherwise (已对齐时返回 true, 否则为 false)</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAddressAligned(nint address, nint alignment) {
            return IsAddressAligned((nuint)address, (nuint)alignment);
        }

        /// <summary>
        /// Is the address aligned (地址是否已对齐).
        /// </summary>
        /// <param name="address">The pointer address (指针地址).</param>
        /// <param name="alignment">Alignment value. Note that this method does not check if the alignment value is valid, please use <see cref="IsAlignmentValid(nuint)"/> to check it first (对齐值. 注意本方法不会检查对齐值是否有效, 请先使用 IsAlignmentValid 来检查).</param>
        /// <returns>Returns true if aligned, false otherwise (已对齐时返回 true, 否则为 false)</returns>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAddressAligned(nuint address, nuint alignment) {
            return 0 == (address & alignment - 1);
        }

        /// <summary>
        /// Is the address aligned (地址是否已对齐).
        /// </summary>
        /// <param name="address">The pointer address (指针地址).</param>
        /// <param name="alignment">Alignment value. Note that this method does not check if the alignment value is valid, please use <see cref="IsAlignmentValid(nint)"/> to check it first (对齐值. 注意本方法不会检查对齐值是否有效, 请先使用 IsAlignmentValid 来检查).</param>
        /// <returns>Returns true if aligned, false otherwise (已对齐时返回 true, 否则为 false)</returns>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static bool IsAddressAligned(void* address, nint alignment) {
            return IsAddressAligned((nint)address, alignment);
        }

        /// <summary>
        /// Is the address aligned (地址是否已对齐).
        /// </summary>
        /// <param name="address">The pointer address (指针地址).</param>
        /// <param name="alignment">Alignment value. Note that this method does not check if the alignment value is valid, please use <see cref="IsAlignmentValid(nuint)"/> to check it first (对齐值. 注意本方法不会检查对齐值是否有效, 请先使用 IsAlignmentValid 来检查).</param>
        /// <returns>Returns true if aligned, false otherwise (已对齐时返回 true, 否则为 false)</returns>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static bool IsAddressAligned(void* address, nuint alignment) {
            return IsAddressAligned((nuint)address, alignment);
        }

        /// <summary>
        /// Is the address aligned or unused alignment value. When alignment is 0~1, no alignment value is used and always returns true (地址是否已对齐, 或不使用对齐值. 当 alignment 为 0~1时, 不使用对齐值, 总是返回 true).
        /// </summary>
        /// <param name="address">The pointer address (指针地址).</param>
        /// <param name="alignment">Alignment value. Note that this method does not check if the alignment value is valid, please use <see cref="IsAlignmentValid(nint)"/> to check it first (对齐值. 注意本方法不会检查对齐值是否有效, 请先使用 IsAlignmentValid 来检查).</param>
        /// <returns>Returns true if aligned, false otherwise (已对齐时返回 true, 否则为 false)</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAddressAlignedOrUnused(nint address, nint alignment) {
            return IsAddressAlignedOrUnused((nuint)address, (nuint)alignment);
        }

        /// <summary>
        /// Is the address aligned or unused alignment value. When alignment is 0~1, no alignment value is used and always returns true (地址是否已对齐, 或不使用对齐值. 当 alignment 为 0~1时, 不使用对齐值, 总是返回 true).
        /// </summary>
        /// <param name="address">The pointer address (指针地址).</param>
        /// <param name="alignment">Alignment value. Note that this method does not check if the alignment value is valid, please use <see cref="IsAlignmentValid(nint)"/> to check it first (对齐值. 注意本方法不会检查对齐值是否有效, 请先使用 IsAlignmentValid 来检查).</param>
        /// <returns>Returns true if aligned, false otherwise (已对齐时返回 true, 否则为 false)</returns>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAddressAlignedOrUnused(nuint address, nuint alignment) {
            return alignment <= 1 || 0 == (address & alignment - 1);
        }

        /// <summary>
        /// Is the address aligned or unused alignment value. When alignment is 0~1, no alignment value is used and always returns true (地址是否已对齐, 或不使用对齐值. 当 alignment 为 0~1时, 不使用对齐值, 总是返回 true).
        /// </summary>
        /// <param name="address">The pointer address (指针地址).</param>
        /// <param name="alignment">Alignment value. Note that this method does not check if the alignment value is valid, please use <see cref="IsAlignmentValid(nint)"/> to check it first (对齐值. 注意本方法不会检查对齐值是否有效, 请先使用 IsAlignmentValid 来检查).</param>
        /// <returns>Returns true if aligned, false otherwise (已对齐时返回 true, 否则为 false)</returns>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static bool IsAddressAlignedOrUnused(void* address, nint alignment) {
            return IsAddressAlignedOrUnused((nint)address, alignment);
        }

        /// <summary>
        /// Is the address aligned or unused alignment value. When alignment is 0~1, no alignment value is used and always returns true (地址是否已对齐, 或不使用对齐值. 当 alignment 为 0~1时, 不使用对齐值, 总是返回 true).
        /// </summary>
        /// <param name="address">The pointer address (指针地址).</param>
        /// <param name="alignment">Alignment value. Note that this method does not check if the alignment value is valid, please use <see cref="IsAlignmentValid(nint)"/> to check it first (对齐值. 注意本方法不会检查对齐值是否有效, 请先使用 IsAlignmentValid 来检查).</param>
        /// <returns>Returns true if aligned, false otherwise (已对齐时返回 true, 否则为 false)</returns>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static bool IsAddressAlignedOrUnused(void* address, nuint alignment) {
            return IsAddressAlignedOrUnused((nuint)address, alignment);
        }

        /// <summary>
        /// Whether the alignment value is unused. If it is less than or equal to 1, it is unused. Note that this method does not check if it is valid (对齐值是否不使用. 它小于等于1时表示不使用. 注意本方法不会检查它是否有效).
        /// </summary>
        /// <param name="alignment">The alignment value (对齐值).</param>
        /// <returns>Returns true if unused, false otherwise (不使用时返回 true, 否则为 false)</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAlignmentUnused(nint alignment) {
            return alignment <= 1;
        }

        /// <summary>
        /// Whether the alignment value is unused. If it is less than or equal to 1, it is unused. Note that this method does not check if it is valid (对齐值是否不使用. 它小于等于1时表示不使用. 注意本方法不会检查它是否有效).
        /// </summary>
        /// <param name="alignment">The alignment value (对齐值).</param>
        /// <returns>Returns true if unused, false otherwise (不使用时返回 true, 否则为 false)</returns>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAlignmentUnused(nuint alignment) {
            return alignment <= 1;
        }

        /// <summary>
        /// Whether the alignment value is used. If it is greater than 1, it is used. Note that this method does not check if it is valid (对齐值是否已使用. 它大于1时表示已使用. 注意本方法不会检查它是否有效).
        /// </summary>
        /// <param name="alignment">The alignment value (对齐值).</param>
        /// <returns>Returns true if used, false otherwise (已使用时返回 true, 否则为 false)</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAlignmentUsed(nint alignment) {
            return alignment > 1;
        }

        /// <summary>
        /// Whether the alignment value is used. If it is greater than 1, it is used. Note that this method does not check if it is valid (对齐值是否已使用. 它大于1时表示已使用. 注意本方法不会检查它是否有效).
        /// </summary>
        /// <param name="alignment">The alignment value (对齐值).</param>
        /// <returns>Returns true if used, false otherwise (已使用时返回 true, 否则为 false)</returns>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAlignmentUsed(nuint alignment) {
            return alignment > 1;
        }

        /// <summary>
        /// Is the alignment value valid. It must be a positive number and a power of 2 (对齐值是否有效. 它必须是正数且为2的幂).
        /// </summary>
        /// <param name="alignment">The alignment value (对齐值).</param>
        /// <returns>Returns true if valid, false otherwise (有效时返回 true, 否则为 false)</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAlignmentValid(nint alignment) {
            if (alignment < 0) return false;
            return IsAlignmentValid((nuint)alignment);
        }

        /// <summary>
        /// Is the alignment value valid. It must be a positive number and a power of 2 (对齐值是否有效. 它必须是正数且为2的幂).
        /// </summary>
        /// <param name="alignment">The alignment value (对齐值).</param>
        /// <returns>Returns true if valid, false otherwise (有效时返回 true, 否则为 false)</returns>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAlignmentValid(nuint alignment) {
            return MathBitOperations.IsPow2(alignment);
        }

        /// <summary>
        /// Is the alignment value valid or unused. It must be a positive number and a power of 2, or 0 (对齐值是否有效或不使用. 它必须是正数且为2的幂, 或0).
        /// </summary>
        /// <param name="alignment">The alignment value (对齐值).</param>
        /// <returns>Returns true if valid, false otherwise (有效时返回 true, 否则为 false)</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAlignmentValidOrUnused(nint alignment) {
            if (alignment < 0) return false;
            return IsAlignmentValidOrUnused((nuint)alignment);
        }

        /// <summary>
        /// Is the alignment value valid or unused. It must be a positive number and a power of 2, or 0 (对齐值是否有效或不使用. 它必须是正数且为2的幂, 或0).
        /// </summary>
        /// <param name="alignment">The alignment value (对齐值).</param>
        /// <returns>Returns true if valid, false otherwise (有效时返回 true, 否则为 false)</returns>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAlignmentValidOrUnused(nuint alignment) {
            if (alignment == 0) return true;
            return MathBitOperations.IsPow2(alignment);
        }

    }
}
