#if SIZE_UINTPTR
global using TSize = System.UIntPtr;
global using MyCLSCompliantAttribute = System.CLSCompliantAttribute;
#else
global using TSize = System.IntPtr;
global using MyCLSCompliantAttribute = Zyl.SizableSpans.Impl.FakeCLSCompliantAttribute;
#endif // SIZE_UINTPTR
global using TUSize = System.UIntPtr;

using System;

[assembly: CLSCompliant(true)]
