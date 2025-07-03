using System;
using System.Collections.Generic;
using System.Text;

namespace Zyl.ExSpans.Buffers {
    partial class PointerUtil {

        /// <summary>
        /// Checks if the array length is valid, and throws an exception if it is not (检查数组长度是否有效, 无效时会抛出异常).
        /// </summary>
        /// <param name="length">Length of array (数组的长度).</param>
        /// <exception cref="ArgumentOutOfRangeException">The length parameter must be greater than or equal to 0. The length parameter out of array max length.</exception>
        /// <seealso cref="Array.MaxLength"/>
        /// <seealso cref="CheckArrayLengthInPool"/>
        public static void CheckArrayLength(nint length) {
            if (length < 0) {
                throw new ArgumentOutOfRangeException(nameof(length), "The length parameter must be greater than or equal to 0.");
            }
            if ((long)length > (long)int.MaxValue) {
                throw new ArgumentOutOfRangeException(nameof(length), string.Format("The length({0}) parameter out of array max length.", (long)length));
            }
#if NET6_0_OR_GREATER
            if ((long)length > (long)Array.MaxLength) {
                throw new ArgumentOutOfRangeException(nameof(length), string.Format("The length({0}) parameter out of array max length.", (long)length));
            }
#endif // NET6_0_OR_GREATER
        }

        /// <summary>
        /// Check if the length of the array in the array pool is valid, and throws an exception if it is not (检查数组池中的数组长度是否有效, 无效时会抛出异常).
        /// </summary>
        /// <param name="length">Length of array (数组的长度).</param>
        /// <param name="maxArrayLength">Maximum array length for array pool allocation. Defaults to <see cref="ExSpansGlobal.PoolMaxArrayLength"/> if it is 0 (数组池分配时的最大数组长度. 它为0时默认为 <see cref="ExSpansGlobal.PoolMaxArrayLength"/>).</param>
        /// <exception cref="ArgumentOutOfRangeException">The length parameter must be greater than or equal to 0. The length parameter out of array max length for array pool.</exception>
        /// <seealso cref="ExSpansGlobal.PoolMaxArrayLength"/>
        /// <seealso cref="CheckArrayLength"/>
        public static void CheckArrayLengthInPool(nint length, nint maxArrayLength = 0) {
            CheckArrayLength(length);
            if (maxArrayLength < 0) {
                throw new ArgumentOutOfRangeException(nameof(maxArrayLength), "The maxArrayLength parameter must be greater than or equal to 0.");
            }
            if (0 == maxArrayLength) {
                maxArrayLength = ExSpansGlobal.PoolMaxArrayLength;
            }
            if ((long)length > (long)maxArrayLength) {
                throw new ArgumentOutOfRangeException(nameof(length), string.Format("The length({0}) parameter out of array max length for array pool({1}).", (long)length, (long)maxArrayLength));
            }
        }

        /// <summary>
        /// Is the array length is valid (数组长度是否有效).
        /// </summary>
        /// <param name="length">Length of array (数组的长度).</param>
        /// <returns>Returns true if valid, false otherwise (有效时返回 true, 否则为 false)</returns>
        /// <seealso cref="Array.MaxLength"/>
        /// <seealso cref="IsArrayLengthValidInPool"/>
        public static bool IsArrayLengthValid(nint length) {
            if (length < 0) {
                return false;
            }
            if ((long)length > (long)int.MaxValue) {
                return false;
            }
#if NET6_0_OR_GREATER
            if ((long)length > (long)Array.MaxLength) {
                return false;
            }
#endif // NET6_0_OR_GREATER
            return true;
        }

        /// <summary>
        /// Is the length of the array in the array pool is valid (数组池中的数组长度是否有效).
        /// </summary>
        /// <param name="length">Length of array (数组的长度).</param>
        /// <param name="maxArrayLength">Maximum array length for array pool allocation. Defaults to <see cref="ExSpansGlobal.PoolMaxArrayLength"/> if it is 0 (数组池分配时的最大数组长度. 它为0时默认为 <see cref="ExSpansGlobal.PoolMaxArrayLength"/>).</param>
        /// <returns>Returns true if valid, false otherwise (有效时返回 true, 否则为 false)</returns>
        /// <exception cref="ArgumentOutOfRangeException">The maxArrayLength parameter must be greater than or equal to 0.</exception>
        /// <seealso cref="ExSpansGlobal.PoolMaxArrayLength"/>
        /// <seealso cref="IsArrayLengthValid"/>
        public static bool IsArrayLengthValidInPool(nint length, nint maxArrayLength = 0) {
            if (!IsArrayLengthValid(length)) {
                return false;
            }
            if (maxArrayLength < 0) {
                throw new ArgumentOutOfRangeException(nameof(maxArrayLength), "The maxArrayLength parameter must be greater than or equal to 0.");
            }
            if (0 == maxArrayLength) {
                maxArrayLength = ExSpansGlobal.PoolMaxArrayLength;
            }
            if ((long)length > (long)maxArrayLength) {
                return false;
            }
            return true;
        }

    }
}
