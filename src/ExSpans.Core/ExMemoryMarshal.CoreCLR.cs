using System;
using System.Runtime.CompilerServices;

namespace Zyl.ExSpans {
    partial class ExMemoryMarshal {
        /// <summary>
        /// Returns a reference to the 0th element of <paramref name="array"/>. If the array is empty, returns a null reference. Such a reference may be used for pinning but must never be dereferenced (返回对 array 中第 0 个元素的引用。 如果数组为空，则返回对 null 引用。 此类引用可用于固定，但绝不能取消引用).
        /// </summary>
        /// <remarks>
        /// This method does not perform array variance checks. The caller must manually perform any array variance checks if the caller wishes to write to the returned reference (此方法不执行数组差异检查。 如果调用方希望写入返回的引用，则调用方必须手动执行任何数组差异检查).
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T GetArrayDataReference<T>(T[] array) {
            if (null == array || array.Length <= 0) {
                return ref Unsafe.NullRef<T>();
            }
            return ref array[0];
        }

        /*

        /// <summary>
        /// Returns a reference to the 0th element of <paramref name="array"/>. If the array is empty, returns a reference to where the 0th element
        /// would have been stored. Such a reference may be used for pinning but must never be dereferenced.
        /// </summary>
        /// <exception cref="NullReferenceException"><paramref name="array"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// The caller must manually reinterpret the returned <em>ref byte</em> as a ref to the array's underlying elemental type,
        /// perhaps utilizing an API such as <em>System.Runtime.CompilerServices.Unsafe.As</em> to assist with the reinterpretation.
        /// This technique does not perform array variance checks. The caller must manually perform any array variance checks
        /// if the caller wishes to write to the returned reference.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref byte GetArrayDataReference(Array array) {
            // If needed, we can save one or two instructions per call by marking this method as intrinsic and asking the JIT
            // to special-case arrays of known type and dimension.

            // See comment on RawArrayData (in RuntimeHelpers.CoreCLR.cs) for details
            return ref Unsafe.AddByteOffset(ref Unsafe.As<RawData>(array).Data, (nuint)RuntimeHelpers.GetMethodTable(array)->BaseSize - (nuint)(2 * sizeof(IntPtr)));
        }

        */
    }
}
