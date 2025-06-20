using Xunit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Zyl.ExSpans.Tests.AExSpan {
    public static partial class AOverflow {
        // NOTE: IndexOverflow test is constrained to run on Windows and MacOSX because it
        //       causes problems on Linux due to the way deferred memory allocation works.
        //       On Linux, the allocation can succeed even if there is not enough memory
        //       but then the test may get killed by the OOM killer at the time the memory
        //       is accessed which triggers the full memory allocation.

        [Fact]
        [OuterLoop]
        [PlatformSpecific(TestPlatforms.Windows | TestPlatforms.OSX)]
        public static void IndexOverflow() {
            // If this test is run in a 32-bit process, the 3GB allocation will fail.
            if (Unsafe.SizeOf<IntPtr>() == sizeof(long)) {
                //
                // Although ExSpan constrains indexes to 0..2Gb, it does not similarly constrain index * sizeof(T).
                // Make sure that internal offset calculations handle the >2Gb case properly.
                //
                unsafe {
                    if (!AllocationHelper.TryAllocNative(unchecked((nint)ThreeGiB), out IntPtr memBlock)) {
                        Console.WriteLine($"ExSpan.Overflow test {nameof(IndexOverflow)} skipped (could not alloc memory).");
                        return; // It's not implausible to believe that a 3gb allocation will fail - if so, skip this test to avoid unnecessary test flakiness.

                    }

                    try {
                        ref Guid memory = ref Unsafe.AsRef<Guid>(memBlock.ToPointer());
                        var span = new ExSpan<Guid>(memBlock.ToPointer(), s_guidThreeGiBLimit);

                        int bigIndex = checked(s_guidTwoGiBLimit + 1);
                        uint byteOffset = checked((uint)bigIndex * (uint)sizeof(Guid));
                        Assert.True(byteOffset > int.MaxValue);  // Make sure byteOffset actually overflows 2Gb, or this test is pointless.
                        ref Guid expected = ref Unsafe.Add<Guid>(ref memory, bigIndex);

                        Assert.True(Unsafe.AreSame<Guid>(ref expected, ref span[bigIndex]));

                        ExSpan<Guid> slice = span.Slice(bigIndex);
                        Assert.True(Unsafe.AreSame<Guid>(ref expected, ref ExMemoryMarshal.GetReference(slice)));

                        slice = span.Slice(bigIndex, 1);
                        Assert.True(Unsafe.AreSame<Guid>(ref expected, ref ExMemoryMarshal.GetReference(slice)));
                    } finally {
                        AllocationHelper.ReleaseNative(ref memBlock);
                    }
                }
            }
        }

        // NOTE: SliceStartInt32Overflow test is constrained to run on Windows and MacOSX because it
        //       causes problems on Linux due to the way deferred memory allocation works.
        //       On Linux, the allocation can succeed even if there is not enough memory
        //       but then the test may get killed by the OOM killer at the time the memory
        //       is accessed which triggers the full memory allocation.

        [Fact]
        [OuterLoop()]
        [PlatformSpecific(TestPlatforms.Windows | TestPlatforms.OSX)]
        public static void SliceStartInt32Overflow() {
            // If this test is run in a 32-bit process, the 3GB allocation will fail.
            if (Unsafe.SizeOf<IntPtr>() == sizeof(long)) {
                unsafe {
                    if (!AllocationHelper.TryAllocNative(unchecked((nint)ThreeGiB), out IntPtr memory)) {
                        Console.WriteLine($"ExSpan.Overflow test {nameof(SliceStartInt32Overflow)} skipped (could not alloc memory).");
                        return;
                    }

                    try {
                        int
                            GuidThreeGiBLimit = (int)(ThreeGiB / sizeof(Guid)),
                            GuidTwoGiBLimit = (int)(TwoGiB / sizeof(Guid)),
                            GuidOneGiBLimit = (int)(OneGiB / sizeof(Guid));

                        ExSpan<Guid> span = new ExSpan<Guid>((void*)memory, GuidThreeGiBLimit);
                        Guid guid = Guid.NewGuid();
                        ExSpan<Guid> slice = span.Slice(GuidTwoGiBLimit + 1);
                        slice[0] = guid;
                        slice = span.Slice(GuidOneGiBLimit).Slice(1).Slice(GuidOneGiBLimit);
                        Assert.Equal(guid, slice[0]);
                    } finally {
                        AllocationHelper.ReleaseNative(ref memory);
                    }
                }
            }
        }

        // NOTE: ReadOnlySliceStartInt32Overflow test is constrained to run on Windows and MacOSX because it
        //       causes problems on Linux due to the way deferred memory allocation works.
        //       On Linux, the allocation can succeed even if there is not enough memory
        //       but then the test may get killed by the OOM killer at the time the memory
        //       is accessed which triggers the full memory allocation.

        [Fact]
        [OuterLoop()]
        [PlatformSpecific(TestPlatforms.Windows | TestPlatforms.OSX)]
        public static void ReadOnlySliceStartInt32Overflow() {
            // If this test is run in a 32-bit process, the 3GB allocation will fail.
            if (Unsafe.SizeOf<IntPtr>() == sizeof(long)) {
                unsafe {
                    if (!AllocationHelper.TryAllocNative(unchecked((nint)ThreeGiB), out IntPtr memory)) {
                        Console.WriteLine($"ExSpan.Overflow test {nameof(ReadOnlySliceStartInt32Overflow)} skipped (could not alloc memory).");
                        return;
                    }

                    try {
                        int
                            GuidThreeGiBLimit = (int)(ThreeGiB / sizeof(Guid)),
                            GuidTwoGiBLimit = (int)(TwoGiB / sizeof(Guid)),
                            GuidOneGiBLimit = (int)(OneGiB / sizeof(Guid));

                        ExSpan<Guid> mutable = new ExSpan<Guid>((void*)memory, GuidThreeGiBLimit);
                        ReadOnlyExSpan<Guid> span = new ReadOnlyExSpan<Guid>((void*)memory, GuidThreeGiBLimit);
                        Guid guid = Guid.NewGuid();
                        ReadOnlyExSpan<Guid> slice = span.Slice(GuidTwoGiBLimit + 1);
                        mutable[GuidTwoGiBLimit + 1] = guid;
                        Assert.Equal(guid, slice[0]);

                        slice = span.Slice(GuidOneGiBLimit).Slice(1).Slice(GuidOneGiBLimit);
                        Assert.Equal(guid, slice[0]);
                    } finally {
                        AllocationHelper.ReleaseNative(ref memory);
                    }
                }
            }
        }

        private const long ThreeGiB = 3L * 1024L * 1024L * 1024L;
        private const long TwoGiB = 2L * 1024L * 1024L * 1024L;
        private const long OneGiB = 1L * 1024L * 1024L * 1024L;

        private static readonly int s_guidThreeGiBLimit = (int)(ThreeGiB / Unsafe.SizeOf<Guid>());  // sizeof(Guid) requires unsafe keyword and I don't want to mark the entire class unsafe.
        private static readonly int s_guidTwoGiBLimit = (int)(TwoGiB / Unsafe.SizeOf<Guid>());
    }
}
