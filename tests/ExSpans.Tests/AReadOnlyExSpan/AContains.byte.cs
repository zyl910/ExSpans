using System.Numerics;
using Xunit;

namespace Zyl.ExSpans.Tests.AReadOnlyExSpan {
    // Adapted from IndexOf.byte.cs
    public static partial class AContains // .Contains<Byte>
    {
        [Fact]
        public static void ZeroLengthContains_Byte() {
            ReadOnlyExSpan<byte> span = new ReadOnlyExSpan<byte>(ArrayHelper.Empty<byte>());

            bool found = span.Contains<byte>(0);
            Assert.False(found);
        }

        [Fact]
        public static void DefaultFilledContains_Byte() {
            for (int length = 0; length <= byte.MaxValue; length++) {
                byte[] a = new byte[length];
                ReadOnlyExSpan<byte> span = new ReadOnlyExSpan<byte>(a);

                for (int i = 0; i < length; i++) {
                    byte target0 = default;

                    bool found = span.Contains(target0);
                    Assert.True(found);
                }
            }
        }

        [Fact]
        public static void TestContains_Byte() {
            for (int length = 0; length <= byte.MaxValue; length++) {
                byte[] a = new byte[length];
                for (int i = 0; i < length; i++) {
                    a[i] = (byte)(i + 1);
                }
                ReadOnlyExSpan<byte> span = new ReadOnlyExSpan<byte>(a);

                for (int targetIndex = 0; targetIndex < length; targetIndex++) {
                    byte target = a[targetIndex];

                    bool found = span.Contains(target);
                    Assert.True(found);
                }
            }
        }

        [Fact]
        public static void TestNotContains_Byte() {
            var rnd = new Random(42);
            for (int length = 0; length <= byte.MaxValue; length++) {
                byte[] a = new byte[length];
                byte target = (byte)rnd.Next(0, 256);
                for (int i = 0; i < length; i++) {
                    byte val = (byte)(i + 1);
                    a[i] = val == target ? (byte)(target + 1) : val;
                }
                ReadOnlyExSpan<byte> span = new ReadOnlyExSpan<byte>(a);

                bool found = span.Contains(target);
                Assert.False(found);
            }
        }

        [Fact]
        public static void TestAlignmentNotContains_Byte() {
            byte[] array = new byte[4 * Vector<byte>.Count];
            for (var i = 0; i < Vector<byte>.Count; i++) {
                var span = new ReadOnlyExSpan<byte>(array, i, 3 * Vector<byte>.Count);

                bool found = span.Contains((byte)'1');
                Assert.False(found);

                span = new ReadOnlyExSpan<byte>(array, i, 3 * Vector<byte>.Count - 3);

                found = span.Contains((byte)'1');
                Assert.False(found);
            }
        }

        [Fact]
        public static void TestAlignmentContains_Byte() {
            byte[] array = new byte[4 * Vector<byte>.Count];
            for (int i = 0; i < array.Length; i++) {
                array[i] = 5;
            }
            for (var i = 0; i < Vector<byte>.Count; i++) {
                var span = new ReadOnlyExSpan<byte>(array, i, 3 * Vector<byte>.Count);

                bool found = span.Contains<byte>(5);
                Assert.True(found);

                span = new ReadOnlyExSpan<byte>(array, i, 3 * Vector<byte>.Count - 3);

                found = span.Contains<byte>(5);
                Assert.True(found);
            }
        }

        [Fact]
        public static void TestMultipleContains_Byte() {
            for (int length = 2; length <= byte.MaxValue; length++) {
                byte[] a = new byte[length];
                for (int i = 0; i < length; i++) {
                    byte val = (byte)(i + 1);
                    a[i] = val == 200 ? (byte)201 : val;
                }

                a[length - 1] = 200;
                a[length - 2] = 200;

                ReadOnlyExSpan<byte> span = new ReadOnlyExSpan<byte>(a);

                bool found = span.Contains<byte>(200);
                Assert.True(found);
            }
        }

        [Fact]
        public static void MakeSureNoContainsChecksGoOutOfRange_Byte() {
            for (int length = 0; length <= byte.MaxValue; length++) {
                byte[] a = new byte[length + 2];
                a[0] = 99;
                a[length + 1] = 99;
                ReadOnlyExSpan<byte> span = new ReadOnlyExSpan<byte>(a, 1, length);

                bool found = span.Contains<byte>(99);
                Assert.False(found);
            }
        }
    }
}
