using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Zyl.ExSpans.Benchmarks.AExSpan;

namespace Zyl.ExSpans.Benchmarks.NetFw {
    class Program {
        static void Main(string[] args) {
            TextWriter writer = Console.Out;
            writer.WriteLine("ExSpans.Benchmarks.NetFw");
            // benchmarkMode
            // 0: Benchmark all with my BenchmarkMain.
            // 1: Benchmark all with BenchmarkDotNet.
            // 2: Benchmark item with BenchmarkDotNet.
            // 3: Running special method (AloneTest).
            int benchmarkMode = 0;
            if (args.Length >= 1) {
                if (!int.TryParse(args[0], out benchmarkMode)) {
                    benchmarkMode = 0;
                }
            }
            writer.WriteLine("benchmarkMode:\t{0}", benchmarkMode);
            if (benchmarkMode == 3) {
                AloneTestUtil.AloneTestByCommand(writer, args);
            } else if (benchmarkMode > 0) {
                Architecture architecture = RuntimeInformation.OSArchitecture;
                var config = DefaultConfig.Instance;
                if (architecture == Architecture.X86 || architecture == Architecture.X64) {
                    config = config.AddDiagnoser(new DisassemblyDiagnoser(new DisassemblyDiagnoserConfig(maxDepth: 3, printSource: true, printInstructionAddresses: true, exportGithubMarkdown: true, exportHtml: true)));
                } else {
                    // Message: Arm64 is not supported (Iced library limitation)
                }
                config = config.AddJob(Job.MediumRun
                    //.WithToolchain(InProcessEmitToolchain.Instance)
                    //.WithId("InProcess")
                    );
                if (benchmarkMode >= 2) {
                    var summary = BenchmarkRunner.Run<SumBenchmark_Int32>(config);
                    writer.WriteLine(summary);
                } else {
                    var summary = BenchmarkRunner.Run(typeof(SumBenchmark_Int32).Assembly, config);
                    writer.WriteLine("Length={0}, {1}", summary.Length, summary);
                }
            } else {
                string indent = "";
                writer.WriteLine();
                BenchmarkUtil.OutputEnvironment(writer, indent);
                writer.WriteLine();
                BenchmarkUtil.ParseCommand(args);
                BenchmarkMain.RunBenchmark(writer, indent);
                writer.WriteLine();
                AloneTestUtil.AloneTestByCommand(writer, args);
            }
            //Console.ReadLine();
        }
    }
}
