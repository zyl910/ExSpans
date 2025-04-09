#if SIZE_UINTPTR
global using TSize = System.UIntPtr;
#else
global using TSize = System.IntPtr;
#endif // SIZE_UINTPTR
global using TUSize = System.UIntPtr;

using System;

[assembly: CLSCompliant(true)]
