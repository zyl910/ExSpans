using System;
using System.IO;

namespace Zyl.SizableSpans.Sample {
    internal class Program {
        static void Main(string[] args) {
            TextWriter writer = Console.Out;
            writer.WriteLine("SizableSpans.Sample");
            EnvironmentOutput.OutputEnvironment(writer);
            writer.WriteLine();

            writer.WriteLine("ArrayMaxLengthSafe:\t{0} // 0x{0:X}", SizableMemoryMarshal.ArrayMaxLengthSafe);
        }
    }
}
