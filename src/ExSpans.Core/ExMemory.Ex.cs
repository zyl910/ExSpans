using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Zyl.ExSpans.Buffers;

namespace Zyl.ExSpans {
    partial struct ExMemory<T> {

        /// <summary>
        /// Returns a <see cref="ExSpan{T}"/> from the memory (从内存获取 <see cref="ExSpan{T}"/>).
        /// </summary>
        public ExSpan<T> ExSpan {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get {
                object? tmpObject = _object;
                if (tmpObject != null) {
                    if (tmpObject is ExMemoryManager<T> mgr) {
                        nint indexFix = _index & ReadOnlyExMemory<T>.RemoveFlagsBitMask;
                        return mgr.GetExSpan().Slice(indexFix, _length);
                    }
                }
                return GetSpanCore();
            }
        }

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
