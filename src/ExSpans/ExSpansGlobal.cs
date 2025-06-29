#if SIZE_UINTPTR
global using TSize = nuint; //System.UIntPtr;
global using TSize32 = System.UInt32;
global using MyCLSCompliantAttribute = System.CLSCompliantAttribute;
#else
global using TSize = nint; //System.IntPtr;
global using TSize32 = System.Int32;
global using MyCLSCompliantAttribute = Zyl.ExSpans.Impl.FakeCLSCompliantAttribute;
#endif // SIZE_UINTPTR
global using TUSize = nuint; //System.UIntPtr;

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Zyl.ExSpans;
using Zyl.ExSpans.Extensions;

[assembly: CLSCompliant(true)]

// -- Zyl.ExSpans
[assembly: TypeForwardedToAttribute(typeof(ReadOnlyExSpan<>))]
[assembly: TypeForwardedToAttribute(typeof(ExMemoryMarshal))]
[assembly: TypeForwardedToAttribute(typeof(ExSpan<>))]
[assembly: TypeForwardedToAttribute(typeof(ExSpanExtensions))]

// -- Zyl.ExSpans.Extensions
[assembly: TypeForwardedToAttribute(typeof(IntPtrExtensions))]
[assembly: TypeForwardedToAttribute(typeof(ExLengthExtensions))]

namespace Zyl.ExSpans {

    /// <summary>
    /// ExSpans global initializer (全局初始化器). It is used to initialize data ahead of time to improve performance (它用于提前初始化数据, 提高运行性能).
    /// </summary>
    public static class ExSpansGlobal {
        private static bool m_Inited = false;
        private static readonly int m_InitCheckSum;

        /// <summary>
        /// Get init check sum.
        /// </summary>
        public static int InitCheckSum { get => m_InitCheckSum; }

        /// <summary>
        /// Maximum array length for array pool allocation (数组池分配时的最大数组长度). Environment variable name is `EXSPANS_POOLMAXARRAYLENGTH`.
        /// </summary>
        public static nint PoolMaxArrayLength { get; private set; } = 16 * 1024 * 1024;  // 16MB.

        /// <summary>
        /// Do initialize (进行初始化).
        /// </summary>
        public static void Init() {
            if (m_Inited) return;
            m_Inited = true;
            // Initialize on static constructor.
            // done.
            Debug.WriteLine("ExSpansGlobal initialize done.");
#if (NETSTANDARD1_1)
#else
            Trace.WriteLine("ExSpansGlobal initialize done.");
#endif
        }

        static ExSpansGlobal() {
            // == EnvironmentVariable
#if NETSTANDARD1_3_OR_GREATER || NETCOREAPP2_1_OR_GREATER || NET40_OR_GREATER
            try {
                string key;
                string? str;
                long num;
                // EXSPANS_POOLMAXARRAYLENGTH
                key = "EXSPANS_POOLMAXARRAYLENGTH";
                str = Environment.GetEnvironmentVariable(key);
                Debug.WriteLine("EnvironmentVariable - {0}: {1}", key, str);
                if (!string.IsNullOrEmpty(str) && long.TryParse(str, out num)) {
                    bool isOk = false;
                    if (long.TryParse(str, out num)) {
                        isOk = (num >= 0) && (num < int.MaxValue)
#if NET6_0_OR_GREATER
                            && (num < Array.MaxLength)
#endif // NET6_0_OR_GREATER
                            ;
                    }
                    if (isOk) {
                        PoolMaxArrayLength = (nint)num;
                    } else {
                        Debug.WriteLine("EnvironmentVariable - {0}: `{1}` is error value!", key, str);
                    }
                }
            } catch(Exception ex) {
                Debug.WriteLine("Parse fail on EnvironmentVariable. {0}", ex.ToString());
            }
#endif // NETSTANDARD1_3_OR_GREATER || NETCOREAPP2_1_OR_GREATER || NET40_OR_GREATER

            // == InitCheckSum
            unchecked {
                m_InitCheckSum = 0;

                // IntPtrExtensions
                m_InitCheckSum += IntPtrExtensions.IntPtrMaxValue.GetHashCode();
            }
        }
    }
}