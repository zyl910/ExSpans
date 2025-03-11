using System;

namespace Zyl.SizableSpans.Sample {
    internal class Program {
        static void Main(string[] args) {
            Console.WriteLine("SizableSpans.Sample");
            Console.WriteLine("SpanMaxLengthSafe:\t{0} // 0x{0:X}", SizableSpanHelpers.SpanMaxLengthSafe);
        }
    }
}
