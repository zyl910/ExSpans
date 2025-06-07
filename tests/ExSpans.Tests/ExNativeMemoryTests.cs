using Xunit;
using Zyl.ExSpans.Extensions;

namespace Zyl.ExSpans.Tests {
    public unsafe class ExNativeMemoryTests {
        /// <inheritdoc cref="IntPtrExtensions.UIntPtrMaxValue"/>
        private static readonly nuint NUMaxValue = IntPtrExtensions.UIntPtrMaxValue; // nuint.MaxValue

#if NOT_RELATED
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(4)]
        [InlineData(8)]
        [InlineData(16)]
        [InlineData(32)]
        [InlineData(64)]
        [InlineData(128)]
        [InlineData(256)]
        [InlineData(512)]
        [InlineData(1 * 1024)]
        [InlineData(2 * 1024)]
        [InlineData(4 * 1024)]
        [InlineData(8 * 1024)]
        [InlineData(16 * 1024)]
        [InlineData(64 * 1024)]
        [InlineData(1 * 1024 * 1024)]
        [InlineData(2 * 1024 * 1024)]
        [InlineData(4 * 1024 * 1024)]
        public void AlignedAllocTest(uint alignment) {
            void* ptr = ExNativeMemory.AlignedAlloc(1, alignment);

            Assert.True(ptr != null);
            Assert.True((nuint)ptr % alignment == 0);

            ExNativeMemory.AlignedFree(ptr);
        }

        [Fact]
        public void AlignedAllocLessThanVoidPtrAlignmentTest() {
            void* ptr = ExNativeMemory.AlignedAlloc(1, 1);
            Assert.True(ptr != null);
            ExNativeMemory.AlignedFree(ptr);
        }

        [Fact]
        public void AlignedAllocOOMTest() {
            Assert.Throws<OutOfMemoryException>(() => ExNativeMemory.AlignedAlloc(NUMaxValue - ((uint)sizeof(nuint) - 1), (uint)sizeof(nuint)));
        }

        [Fact]
        public void AlignedAllocZeroAlignmentTest() {
            Assert.Throws<ArgumentException>(() => ExNativeMemory.AlignedAlloc((uint)sizeof(nuint), 0));
        }

        [Fact]
        public void AlignedAllocNonPowerOfTwoAlignmentTest() {
            Assert.Throws<ArgumentException>(() => ExNativeMemory.AlignedAlloc((uint)sizeof(nuint), (uint)sizeof(nuint) + 1));
            Assert.Throws<ArgumentException>(() => ExNativeMemory.AlignedAlloc((uint)sizeof(nuint), (uint)sizeof(nuint) * 3));
        }

        [Fact]
        public void AlignedAllocOverflowByteCountTest() {
            // POSIX requires byteCount to be a multiple of alignment and so we will internally upsize.
            // This upsizing can overflow for certain values since we do (byteCount + (alignment - 1)) & ~(alignment - 1)
            //
            // However, this overflow is "harmless" since it will result in a value that is less than alignment
            // given that alignment is a power of two and will ultimately be a value less than alignment which
            // will be treated as invalid and result in OOM.
            //
            // Take for example a 64-bit system where the max power of two is (1UL << 63): 9223372036854775808
            // * 9223372036854775808 + 9223372036854775807 == ulong.MaxValue, so no overflow
            // * 9223372036854775809 + 9223372036854775807 == 0, so overflows and is less than alignment
            // *      ulong.MaxValue + 9223372036854775807 == 9223372036854775806, so overflows and is less than alignment
            //
            // Likewise, for small alignments such as 8 (which is the smallest on a 64-bit system for POSIX):
            // * 18446744073709551608 + 7 == ulong.MaxValue, so no overflow
            // * 18446744073709551609 + 7 == 0, so overflows and is less than alignment
            // *       ulong.MaxValue + 7 == 6, so overflows and is less than alignment

            nuint maxAlignment = (nuint)1 << ((sizeof(nuint) * 8) - 1);
            Assert.Throws<OutOfMemoryException>(() => ExNativeMemory.AlignedAlloc(maxAlignment + 1, maxAlignment));

            Assert.Throws<OutOfMemoryException>(() => ExNativeMemory.AlignedAlloc(NUMaxValue, (uint)sizeof(nuint)));
        }

        [Fact]
        public void AlignedAllocZeroSizeTest() {
            void* ptr = ExNativeMemory.AlignedAlloc(0, (uint)sizeof(nuint));

            Assert.True(ptr != null);
            Assert.True((nuint)ptr % (uint)sizeof(nuint) == 0);

            ExNativeMemory.AlignedFree(ptr);
        }

        [Fact]
        public void AlignedFreeTest() {
            // This should not throw
            ExNativeMemory.AlignedFree(null);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(4)]
        [InlineData(8)]
        [InlineData(16)]
        [InlineData(32)]
        [InlineData(64)]
        [InlineData(128)]
        [InlineData(256)]
        [InlineData(512)]
        [InlineData(1 * 1024)]
        [InlineData(2 * 1024)]
        [InlineData(4 * 1024)]
        [InlineData(8 * 1024)]
        [InlineData(16 * 1024)]
        [InlineData(64 * 1024)]
        [InlineData(1 * 1024 * 1024)]
        [InlineData(2 * 1024 * 1024)]
        [InlineData(4 * 1024 * 1024)]
        public void AlignedReallocTest(uint alignment) {
            void* ptr = ExNativeMemory.AlignedAlloc(1, alignment);

            Assert.True(ptr != null);
            Assert.True((nuint)ptr % alignment == 0);

            void* newPtr = ExNativeMemory.AlignedRealloc(ptr, 1, alignment);

            Assert.True(newPtr != null);
            Assert.True((nuint)newPtr % alignment == 0);

            ExNativeMemory.AlignedFree(newPtr);
        }

        [Fact]
        public void AlignedReallocLessThanVoidPtrAlignmentTest() {
            void* ptr = ExNativeMemory.AlignedAlloc(1, 1);
            Assert.True(ptr != null);

            void* newPtr = ExNativeMemory.AlignedRealloc(ptr, 1, 1);
            Assert.True(newPtr != null);
            ExNativeMemory.AlignedFree(newPtr);
        }

        [Fact]
        public void AlignedReallocNullPtrTest() {
            void* ptr = ExNativeMemory.AlignedRealloc(null, 1, (uint)sizeof(nuint));

            Assert.True(ptr != null);
            Assert.True((nuint)ptr % (uint)sizeof(nuint) == 0);

            ExNativeMemory.AlignedFree(ptr);
        }

        [Fact]
        public void AlignedReallocNullPtrOOMTest() {
            Assert.Throws<OutOfMemoryException>(() => ExNativeMemory.AlignedRealloc(null, NUMaxValue, (uint)sizeof(nuint)));
        }

        [Fact]
        public void AlignedReallocNullPtrZeroSizeTest() {
            void* ptr = ExNativeMemory.AlignedRealloc(null, 0, (uint)sizeof(nuint));

            Assert.True(ptr != null);
            Assert.True((nuint)ptr % (uint)sizeof(nuint) == 0);

            ExNativeMemory.AlignedFree(ptr);
        }

        [Fact]
        public void AlignedReallocZeroAlignmentTest() {
            void* ptr = ExNativeMemory.AlignedAlloc(1, (uint)sizeof(nuint));

            Assert.True(ptr != null);
            Assert.True((nuint)ptr % (uint)sizeof(nuint) == 0);

            Assert.Throws<ArgumentException>(() => ExNativeMemory.AlignedRealloc(ptr, (uint)sizeof(nuint), 0));
            ExNativeMemory.AlignedFree(ptr);
        }

        [Fact]
        public void AlignedReallocNonPowerOfTwoAlignmentTest() {
            void* ptr = ExNativeMemory.AlignedAlloc(1, (uint)sizeof(nuint));

            Assert.True(ptr != null);
            Assert.True((nuint)ptr % (uint)sizeof(nuint) == 0);

            Assert.Throws<ArgumentException>(() => ExNativeMemory.AlignedRealloc(ptr, (uint)sizeof(nuint), (uint)sizeof(nuint) + 1));
            Assert.Throws<ArgumentException>(() => ExNativeMemory.AlignedRealloc(ptr, (uint)sizeof(nuint), (uint)sizeof(nuint) * 3));
            ExNativeMemory.AlignedFree(ptr);
        }

        [Fact]
        public void AlignedReallocZeroSizeTest() {
            void* ptr = ExNativeMemory.AlignedAlloc(1, (uint)sizeof(nuint));

            Assert.True(ptr != null);
            Assert.True((nuint)ptr % (uint)sizeof(nuint) == 0);

            void* newPtr = ExNativeMemory.AlignedRealloc(ptr, 0, (uint)sizeof(nuint));

            Assert.True(newPtr != null);
            Assert.True((nuint)newPtr % (uint)sizeof(nuint) == 0);

            ExNativeMemory.AlignedFree(newPtr);
        }

        [Fact]
        public void AlignedReallocSmallerToLargerTest() {
            void* ptr = ExNativeMemory.AlignedAlloc(16, 16);

            Assert.True(ptr != null);
            Assert.True((nuint)ptr % 16 == 0);

            for (int i = 0; i < 16; i++) {
                ((byte*)ptr)[i] = (byte)i;
            }

            void* newPtr = ExNativeMemory.AlignedRealloc(ptr, 32, 16);

            Assert.True(newPtr != null);
            Assert.True((nuint)newPtr % 16 == 0);

            for (int i = 0; i < 16; i++) {
                Assert.True(((byte*)newPtr)[i] == i);
            }

            ExNativeMemory.AlignedFree(newPtr);
        }
#endif // NOT_RELATED

        [Fact]
        public void AllocByteCountTest() {
            void* ptr = ExNativeMemory.Alloc(1);
            Assert.True(ptr != null);
            ExNativeMemory.Free(ptr);
        }

        [Fact]
        public void AllocElementCountTest() {
            void* ptr = ExNativeMemory.Alloc(1, 1);
            Assert.True(ptr != null);
            ExNativeMemory.Free(ptr);
        }

        [Fact]
        public void AllocByteCountOOMTest() {
            Assert.Throws<OutOfMemoryException>(() => ExNativeMemory.Alloc(NUMaxValue));
        }

        [Fact]
        public void AllocElementCountOOMTest() {
            Assert.Throws<OutOfMemoryException>(() => ExNativeMemory.Alloc(1, NUMaxValue));
            Assert.Throws<OutOfMemoryException>(() => ExNativeMemory.Alloc(NUMaxValue, 1));
            Assert.Throws<OutOfMemoryException>(() => ExNativeMemory.Alloc(NUMaxValue, NUMaxValue));
        }

        [Fact]
        public void AllocZeroByteCountTest() {
            void* ptr = ExNativeMemory.Alloc(0);
            Assert.True(ptr != null);
            ExNativeMemory.Free(ptr);
        }

        [Fact]
        public void AllocZeroElementCountTest() {
            void* ptr = ExNativeMemory.Alloc(0, 1);
            Assert.True(ptr != null);
            ExNativeMemory.Free(ptr);
        }

        [Fact]
        public void AllocZeroElementSizeTest() {
            void* ptr = ExNativeMemory.Alloc(1, 0);
            Assert.True(ptr != null);
            ExNativeMemory.Free(ptr);
        }

        [Fact]
        public void AllocZeroedByteCountTest() {
            void* ptr = ExNativeMemory.AllocZeroed(1);

            Assert.True(ptr != null);
            Assert.Equal(expected: 0, actual: ((byte*)ptr)[0]);

            ExNativeMemory.Free(ptr);
        }

        [Fact]
        public void AllocZeroedElementCountTest() {
            void* ptr = ExNativeMemory.AllocZeroed(1, 1);

            Assert.True(ptr != null);
            Assert.Equal(expected: 0, actual: ((byte*)ptr)[0]);

            ExNativeMemory.Free(ptr);
        }

        [Fact]
        public void AllocZeroedByteCountOOMTest() {
            Assert.Throws<OutOfMemoryException>(() => ExNativeMemory.AllocZeroed(NUMaxValue));
        }

        [Fact]
        public void AllocZeroedElementCountOOMTest() {
            Assert.Throws<OutOfMemoryException>(() => ExNativeMemory.AllocZeroed(1, NUMaxValue));
            Assert.Throws<OutOfMemoryException>(() => ExNativeMemory.AllocZeroed(NUMaxValue, 1));
            Assert.Throws<OutOfMemoryException>(() => ExNativeMemory.AllocZeroed(NUMaxValue, NUMaxValue));
        }

        [Fact]
        public void AllocZeroedZeroByteCountTest() {
            void* ptr = ExNativeMemory.AllocZeroed(0);
            Assert.True(ptr != null);
            ExNativeMemory.Free(ptr);
        }

        [Fact]
        public void AllocZeroedZeroElementCountTest() {
            void* ptr = ExNativeMemory.AllocZeroed(0, 1);
            Assert.True(ptr != null);
            ExNativeMemory.Free(ptr);
        }

        [Fact]
        public void AllocZeroedZeroElementSizeTest() {
            void* ptr = ExNativeMemory.AllocZeroed(1, 0);
            Assert.True(ptr != null);
            ExNativeMemory.Free(ptr);
        }

        [Fact]
        public void FreeTest() {
            // This should not throw
            ExNativeMemory.Free(null);
        }

        [Fact]
        public void ReallocTest() {
            void* ptr = ExNativeMemory.Alloc(1);
            Assert.True(ptr != null);

            void* newPtr = ExNativeMemory.Realloc(ptr, 1);
            Assert.True(newPtr != null);
            ExNativeMemory.Free(newPtr);
        }

        [Fact]
        public void ReallocNullPtrTest() {
            void* ptr = ExNativeMemory.Realloc(null, 1);
            Assert.True(ptr != null);
            ExNativeMemory.Free(ptr);
        }

        [Fact]
        public void ReallocNullPtrOOMTest() {
            Assert.Throws<OutOfMemoryException>(() => ExNativeMemory.Realloc(null, NUMaxValue));
        }

        [Fact]
        public void ReallocNullPtrZeroSizeTest() {
            void* ptr = ExNativeMemory.Realloc(null, 0);
            Assert.True(ptr != null);
            ExNativeMemory.Free(ptr);
        }

        [Fact]
        public void ReallocZeroSizeTest() {
            void* ptr = ExNativeMemory.Alloc(1);
            Assert.True(ptr != null);

            void* newPtr = ExNativeMemory.Realloc(ptr, 0);
            Assert.True(newPtr != null);
            ExNativeMemory.Free(newPtr);
        }

        [Fact]
        public void ReallocSmallerToLargerTest() {
            void* ptr = ExNativeMemory.Alloc(16);
            Assert.True(ptr != null);

            for (int i = 0; i < 16; i++) {
                ((byte*)ptr)[i] = (byte)i;
            }

            void* newPtr = ExNativeMemory.Realloc(ptr, 32);
            Assert.True(newPtr != null);

            for (int i = 0; i < 16; i++) {
                Assert.True(((byte*)newPtr)[i] == i);
            }

            ExNativeMemory.Free(newPtr);
        }

#if NOT_RELATED
        [Theory]
        [InlineData(1, 0)]
        [InlineData(1, 1)]
        [InlineData(1, 2)]
        [InlineData(1, 3)]
        [InlineData(2, 0)]
        [InlineData(3, 0)]
        [InlineData(4, 0)]
        [InlineData(8, 0)]
        [InlineData(9, 0)]
        [InlineData(16, 0)]
        [InlineData(16, 1)]
        [InlineData(16, 3)]
        [InlineData(16, 7)]
        [InlineData(32, 0)]
        [InlineData(64, 0)]
        [InlineData(128, 0)]
        [InlineData(256, 0)]
        [InlineData(256, 1)]
        [InlineData(256, 2)]
        [InlineData(256, 3)]
        [InlineData(256, 5)]
        [InlineData(512, 0)]
        [InlineData(547, 0)]
        [InlineData(1 * 1024, 0)]
        public void ClearTest(int size, int offset) {
            byte* ptr = (byte*)ExNativeMemory.AlignedAlloc((nuint)(size + offset), 8);

            Assert.True(ptr != null);
            Assert.True((nuint)ptr % 8 == 0);

            new Span<byte>(ptr, size + offset).Fill(0b10101010);

            ExNativeMemory.Clear(ptr + offset, (nuint)size);

            Assert.Equal(-1, new Span<byte>(ptr + offset, size).IndexOfAnyExcept((byte)0));

            ExNativeMemory.AlignedFree(ptr);
        }

        [Theory]
        [InlineData(1, 0)]
        [InlineData(1, 1)]
        [InlineData(1, 2)]
        [InlineData(1, 3)]
        [InlineData(1, 44)]
        [InlineData(1, 367)]
        [InlineData(2, 0)]
        [InlineData(3, 0)]
        [InlineData(4, 0)]
        [InlineData(8, 0)]
        [InlineData(9, 0)]
        [InlineData(9, 2)]
        [InlineData(9, 111)]
        [InlineData(9, 289)]
        [InlineData(16, 0)]
        [InlineData(16, 1)]
        [InlineData(16, 3)]
        [InlineData(16, 7)]
        [InlineData(32, 0)]
        [InlineData(64, 0)]
        [InlineData(128, 0)]
        [InlineData(256, 0)]
        [InlineData(256, 1)]
        [InlineData(256, 2)]
        [InlineData(256, 3)]
        [InlineData(256, 5)]
        [InlineData(256, 67)]
        [InlineData(256, 143)]
        public void ClearWithExactRangeTest(int size, int offset) {
            int headLength = offset;
            int bodyLength = size;
            int tailLength = 512 - headLength - bodyLength;
            int headOffset = 0;
            int bodyOffset = headLength;
            int tailOffset = headLength + bodyLength;

            byte* ptr = (byte*)ExNativeMemory.AlignedAlloc(512, 8);

            Assert.True(ptr != null);
            Assert.True((nuint)ptr % 8 == 0);

            new Span<byte>(ptr, 512).Fill(0b10101010);

            ExNativeMemory.Clear(ptr + bodyOffset, (nuint)bodyLength);

            Assert.Equal(-1, new Span<byte>(ptr + headOffset, headLength).IndexOfAnyExcept((byte)0b10101010));
            Assert.Equal(-1, new Span<byte>(ptr + bodyOffset, bodyLength).IndexOfAnyExcept((byte)0));
            Assert.Equal(-1, new Span<byte>(ptr + tailOffset, tailLength).IndexOfAnyExcept((byte)0b10101010));

            ExNativeMemory.AlignedFree(ptr);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(167)]
        public void ClearWithSizeEqualTo0ShouldNoOpTest(int offset) {
            byte* ptr = (byte*)ExNativeMemory.AlignedAlloc(512, 8);

            Assert.True(ptr != null);
            Assert.True((nuint)ptr % 8 == 0);

            new Span<byte>(ptr, 512).Fill(0b10101010);

            ExNativeMemory.Clear(ptr + offset, 0);

            Assert.Equal(-1, new Span<byte>(ptr, 512).IndexOfAnyExcept((byte)0b10101010));

            ExNativeMemory.AlignedFree(ptr);
        }
#endif // NOT_RELATED

        [Fact]
        public void ClearWithNullPointerAndZeroByteCountTest() {
            ExNativeMemory.Clear(null, 0);

            // This test method just needs to check that no exceptions are thrown
        }

        [Fact]
        public void CopyNullBlockShouldNoOpTest() {
            // This should not throw
            ExNativeMemory.Copy(null, null, 0);
        }

        [Fact]
        public void CopyEmptyBlockShouldNoOpTest() {
            int* source = stackalloc int[1] { 42 };
            int* destination = stackalloc int[1] { 0 };

            ExNativeMemory.Copy(source, destination, 0);

            Assert.Equal(0, destination[0]);
        }

        [Theory]
        [InlineData(1, 1, 1)]
        [InlineData(7, 9, 5)]
        [InlineData(1, 16, 1)]
        [InlineData(16, 16, 16)]
        [InlineData(29, 37, 19)]
        [InlineData(1024, 16, 16)]
        public void CopyTest(int sourceSize, int destinationSize, int byteCount) {
            void* source = ExNativeMemory.AllocZeroed((nuint)sourceSize);
            void* destination = ExNativeMemory.AllocZeroed((nuint)destinationSize);

            new Span<byte>(source, sourceSize).Fill(0b10101010);

            ExNativeMemory.Copy(source, destination, (nuint)byteCount);

            Equals(byteCount - 1, new Span<byte>(destination, destinationSize).LastIndexOf<byte>(0b10101010));

            ExNativeMemory.Free(source);
            ExNativeMemory.Free(destination);
        }

        [Theory]
        [InlineData(311, 100, 50)]
        [InlineData(33, 0, 12)]
        [InlineData(150, 50, 100)]
        public void CopyToOverlappedMemoryTest(int size, int offset, int byteCount) {
            byte* source = (byte*)ExNativeMemory.AllocZeroed((nuint)size);

            var expectedBlock = new byte[byteCount];
#if NET6_0_OR_GREATER
            Random.Shared.NextBytes(expectedBlock);
#else
            new Random().NextBytes(expectedBlock);
#endif // NET6_0_OR_GREATER
            expectedBlock.CopyTo(new Span<byte>(source, byteCount));

            ExNativeMemory.Copy(source, source + offset, (nuint)byteCount);

            Assert.True(expectedBlock.AsSpan().SequenceEqual(new ReadOnlySpan<byte>(source + offset, byteCount)));

            ExNativeMemory.Free(source);
        }

        [Fact]
        public void FillNullMemoryBlockShouldNoOpTest() {
            // This should not throw
            ExNativeMemory.Fill(null, 0, 42);
        }

        [Fact]
        public void FillEmptyMemoryBlockShouldNoOpTest() {
            void* source = stackalloc byte[7] { 0, 0, 0, 0, 0, 0, 0 };

            ExNativeMemory.Fill(source, 0, 42);

            Assert.Equal(-1, new Span<byte>(source, 7).IndexOf<byte>(42));
        }
    }
}
