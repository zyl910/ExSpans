#if SIZE_UINTPTR
global using TSize = System.UIntPtr;
global using TSize32 = System.UInt32;
global using MyCLSCompliantAttribute = System.CLSCompliantAttribute;
#else
global using TSize = System.IntPtr;
global using TSize32 = System.Int32;
global using MyCLSCompliantAttribute = Zyl.ExSpans.Impl.FakeCLSCompliantAttribute;
#endif // SIZE_UINTPTR
global using TUSize = System.UIntPtr;

using System;
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
