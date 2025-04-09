#if SIZE_UINTPTR
global using TSize = System.UIntPtr;
global using MyCLSCompliantAttribute = System.CLSCompliantAttribute;
#else
global using TSize = System.IntPtr;
global using MyCLSCompliantAttribute = Zyl.SizableSpans.Impl.FakeCLSCompliantAttribute;
#endif // SIZE_UINTPTR
global using TUSize = System.UIntPtr;

using System;
using System.Runtime.CompilerServices;
using Zyl.SizableSpans;
using Zyl.SizableSpans.Extensions;

[assembly: CLSCompliant(true)]

// -- Zyl.SizableSpans
[assembly: TypeForwardedToAttribute(typeof(ReadOnlySizableSpan<>))]
[assembly: TypeForwardedToAttribute(typeof(SizableMemoryMarshal))]
[assembly: TypeForwardedToAttribute(typeof(SizableSpan<>))]
[assembly: TypeForwardedToAttribute(typeof(SizableSpanExtensions))]

// -- Zyl.SizableSpans.Extensions
[assembly: TypeForwardedToAttribute(typeof(IntPtrExtensions))]
[assembly: TypeForwardedToAttribute(typeof(NULengthExtensions))]
