#if SIZE_UINTPTR
global using TSize = nuint; //System.UIntPtr;
global using TSize32 = System.UInt32;
#else
global using TSize = nint; //System.IntPtr;
global using TSize32 = System.Int32;
#endif // SIZE_UINTPTR
global using TUSize = nuint; //System.UIntPtr;

global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Runtime.InteropServices;
#if NETCOREAPP3_0_OR_GREATER
global using System.Runtime.Intrinsics;
global using System.Runtime.Intrinsics.X86;
#endif // NETCOREAPP3_0_OR_GREATER
global using Zyl.ExSpans.Impl;
global using Zyl.ExSpans.Tests.Fake;
global using Zyl.ExSpans.Tests.Fake.Attributes;

