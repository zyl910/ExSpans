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

[assembly: CLSCompliant(true)]
