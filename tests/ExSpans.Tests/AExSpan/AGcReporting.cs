using Xunit;

namespace Zyl.ExSpans.Tests.AExSpan {
    public static class AGcReporting {
        /// <summary>
        /// This is a simple sanity test to check that GC reporting for ExSpan is not completely broken, it is not meant to be
        /// comprehensive.
        /// </summary>
        [Theory]
        [InlineData(100_000, 10_000)]
        [InlineData(100, 100)]
        [OuterLoop]
        public static void DelegateTest(int iterationCount, int objectCount) {
            object[] objects = new object[objectCount];
            Random rng = new Random();
            var delegateTestCore =
                new DelegateTestCoreDelegate(DelegateTest_Core) +
                new DelegateTestCoreDelegate(DelegateTest_Core);

            for (int i = 0; i < iterationCount; i++) {
                DelegateTest_CreateSomeObjects(objects, rng);
                delegateTestCore(new ExSpan<int>(new int[] { 1, 2, 3 }), objects, rng);
            }
        }

        private delegate void DelegateTestCoreDelegate(ExSpan<int> span, object[] objects, Random rng);

        private static void DelegateTest_Core(ExSpan<int> span, object[] objects, Random rng) {
            ReadOnlyExSpan<int> initialExSpan = span;

            DelegateTest_CreateSomeObjects(objects, rng);

            int sum = 0;
            for (int i = 0; i < span.Length; ++i) {
                sum += span[i];
            }
            Assert.Equal(1 + 2 + 3, sum);

            Assert.True(span == initialExSpan);
        }

        private static void DelegateTest_CreateSomeObjects(object[] objects, Random rng) {
            for (int i = 0; i < 100; ++i) {
                objects[rng.Next(objects.Length)] = new object();
            }
        }
    }
}
