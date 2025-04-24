using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Zyl.ExSpans.Benchmarks.AExSpan;
using Zyl.VectorTraits.Impl;

namespace Zyl.ExSpans.Benchmarks
{
    internal class BenchmarkMain {

        private static bool _inited = false;

        /// <summary>
        /// Run benchmark.
        /// </summary>
        /// <param name="writer">Output <see cref="TextWriter"/>.</param>
        /// <param name="indent">The indent.</param>
        /// <param name="onBefore">The action on before call item. Prototype: <c>void onBefore(double percentage, string title)</c>.</param>
        public static void RunBenchmark(TextWriter writer, string indent, Action<double, string>? onBefore = null) {
            // info.
            // RunBenchmark.
            BenchmarkUtil.CurrentBenchmarkWriter.CurrentTextWriter = writer;
            BenchmarkUtil.RunBenchmark(BenchmarkUtil.CurrentBenchmarkWriter, typeof(BenchmarkMain).Assembly, onBefore);
        }

        /// <summary>
        /// Run benchmark - Async.
        /// </summary>
        /// <param name="writer">Output <see cref="TextWriter"/>.</param>
        /// <param name="indent">The indent.</param>
        /// <param name="onBefore">The action on before call item. Prototype: <c>Task onBefore(double percentage, string title)</c>.</param>
        public static async Task RunBenchmarkAsync(TextWriter writer, string indent, Func<double, string, Task>? onBefore = null) {
            // info.
            // RunBenchmark.
            BenchmarkUtil.CurrentBenchmarkWriter.CurrentTextWriter = writer;
            await BenchmarkUtil.RunBenchmarkAsync(BenchmarkUtil.CurrentBenchmarkWriter, typeof(BenchmarkMain).Assembly, onBefore);
        }

        /// <summary>
        /// Register benchmark types.
        /// </summary>
#if NET5_0_OR_GREATER
        [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "They are test")]
        [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "Allow exceptions during test")]
#endif // NET5_0_OR_GREATER
        private static void RegisterBenchmark() {
            if (_inited) return;
            _inited = true;
            WrappedType[] types = {
                // ABitMath
                typeof(SumBenchmark_Int32),
            };
            WrappedTypePool.Shared.RegisterAll(types);
        }

        static BenchmarkMain() {
            RegisterBenchmark();
        }

    }
}
