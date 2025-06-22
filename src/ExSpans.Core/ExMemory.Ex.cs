using System;
using System.Collections.Generic;
using System.Text;

namespace Zyl.ExSpans {
    partial struct ExMemory<T> {

        /// <summary>
        /// Returns a value that indicates whether two <see cref="ExMemory{T}"/> instances are not equal (返回一个值，该值指示两个 <see cref="ExMemory{T}"/> 实例是否不相等).
        /// </summary>
        public static bool operator !=(ExMemory<T> left, ExMemory<T> right) => !(left == right);

        /// <summary>
        /// Returns a value that indicates whether two <see cref="ExMemory{T}"/> instances are equal (返回一个值，该值指示两个 <see cref="ExMemory{T}"/> 实例是否相等).
        /// </summary>
        public static bool operator ==(ExMemory<T> left, ExMemory<T> right) {
            return
                left._object == right._object &&
                left._index == right._index &&
                left._length == right._length;
        }

    }
}
