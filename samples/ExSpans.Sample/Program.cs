using System;
using System.IO;
using Zyl.ExSpans;

namespace Zyl.ExSpans.Sample {

    internal class Program {
        static void Main(string[] args) {
            TextWriter writer = Console.Out;
            OutputHeader(writer);

            // Test some.
            TestSimple(writer);
            Test2GB(writer);
            ATestMemoryMappedFile.TestMemoryMappedFile(writer);
        }

        /// <summary>
        /// Output header (输出头部信息).
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/>.</param>
        internal static void OutputHeader(TextWriter writer) {
            writer.WriteLine("ExSpans.Sample");
            EnvironmentOutput.OutputEnvironment(writer);
            writer.WriteLine();
        }

        /// <summary>
        /// Test simple (简单测试).
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/>.</param>
        static void TestSimple(TextWriter writer) {
            const int bufferSize = 16;
            // Create ExSpan by Array.
            int[] sourceArray = new int[bufferSize];
            TestExSpan(writer, "Array", new ExSpan<int>(sourceArray)); // Use constructor method.
            //TestExSpan(writer, "Array", sourceArray.AsExSpan()); // Or use extension method.
            writer.WriteLine();

            // Create ExSpan by Span.
            Span<int> sourceSpan = stackalloc int[bufferSize];
            TestExSpan(writer, "Span", sourceSpan); // Use implicit conversion.
            //TestExSpan(writer, "Span", sourceSpan.AsExSpan()); // Or use extension method.

            // Convert ExSpan to Span.
            ExSpan<int> intSpan = sourceSpan; // Implicit conversion Span to ExSpan.
            Span<int> span = (Span<int>)intSpan; // Use explicit conversion.
            //Span<int> span = intSpan.AsSpan(); // Or use extension method.
            writer.WriteLine(string.Format("Span[1]: {0} // 0x{0:X}", span[1]));
            int checkSum = SumExSpan(intSpan); // Implicit conversion ExSpan to ReadOnlyExSpan.
            writer.WriteLine(string.Format("CheckSum: {0} // 0x{0:X}", checkSum));
            writer.WriteLine();

            // Output:
            // [TestExSpan-Array]
            // Data[0]: 305419896 // 0x12345678
            // Data[1]: 16909060 // 0x1020304
            // Data[^1]: 2018915346 // 0x78563412
            // Count(Data[1]): 14 // 0xE
            // 
            // [TestExSpan-Span]
            // Data[0]: 305419896 // 0x12345678
            // Data[1]: 16909060 // 0x1020304
            // Data[^1]: 2018915346 // 0x78563412
            // Count(Data[1]): 14 // 0xE
            // Span[1]: 16909060 // 0x1020304
            // CheckSum: -1733905214 // 0x98A6B4C2
        }

        /// <summary>
        /// Test <see cref="ExSpan{T}"/> (测试 <see cref="ExSpan{T}"/>).
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/>.</param>
        /// <param name="title">The title (标题).</param>
        /// <param name="span">This span (当前跨度).</param>
        static void TestExSpan(TextWriter writer, string title, ExSpan<int> span) {
            try {
                // Write.
                writer.WriteLine($"[TestExSpan-{title}]");
                span.Fill(0x01020304);
                span[0] = 0x12345678;
                span[span.Length - 1] = 0x78563412;
                // Read.
                writer.WriteLine(string.Format("Data[0]: {0} // 0x{0:X}", span[0]));
                writer.WriteLine(string.Format("Data[1]: {0} // 0x{0:X}", span[1]));
                writer.WriteLine(string.Format("Data[^1]: {0} // 0x{0:X}", span[span.Length - 1]));
                writer.WriteLine(string.Format("Count(Data[1]): {0} // 0x{0:X}", (long)span.Count(span[1])));
            } catch (Exception ex) {
                writer.WriteLine(string.Format("Run TestExSpan fail! {0}", ex.ToString()));
            }
        }

        static int SumExSpan(ReadOnlyExSpan<int> span) {
            int rt = 0; // Result.
            for (nint i = 0; i < span.Length; i++) {
                rt += span[i];
            }
            return rt;
        }

        /// <summary>
        /// Test 2GB data (测试2GB数据).
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/>.</param>
        static unsafe void Test2GB(TextWriter writer) {
            const nint OutputMaxLength = 8;
            nuint byteSize = 2U * 1024 * 1024 * 1024; // 2GB
            if (IntPtr.Size > sizeof(int)) {
                byteSize += sizeof(int);
            }
            nint bufferSize = (nint)(byteSize / sizeof(int));
            // Create ExSpan by Pointer.
            try {
                void* buffer = ExNativeMemory.Alloc(byteSize);
                try {
                    ExSpan<int> intSpan = new ExSpan<int>(buffer, bufferSize);
                    TestExSpan(writer, "2GB", intSpan);
                    writer.WriteLine(string.Format("ItemsToString: {0}", intSpan.ItemsToString(OutputMaxLength, OutputMaxLength)));
                    writer.WriteLine(string.Format("intSpan.Count(): {0} // 0x{0:X}", (long)intSpan.Count(intSpan[1])));
                    writer.WriteLine(string.Format("intSpan.Length: {0} // 0x{0:X}", (long)intSpan.Length));
                    // Cast to byte.
                    ExSpan<byte> byteSpan = ExMemoryMarshal.Cast<int, byte>(intSpan);
                    writer.WriteLine(string.Format("byteSpan.Length: {0} // 0x{0:X}", (long)byteSpan.Length));
                    writer.WriteLine(string.Format("byteSpan[0]: {0} // 0x{0:X}", byteSpan[0]));
                    writer.WriteLine(string.Format("byteSpan.ItemsToString: {0}", byteSpan.ItemsToString(OutputMaxLength, OutputMaxLength)));
                    writer.WriteLine(string.Format("byteSpan.Count(): {0} // 0x{0:X}", (long)byteSpan.Count(byteSpan[1])));
                    writer.WriteLine();
                } finally {
                    ExNativeMemory.Free(buffer);
                }
            } catch (Exception ex) {
                writer.WriteLine(string.Format("Run Test2GB fail! {0}", ex.ToString()));
            }

            // Output:
            // [TestExSpan-2GB]
            // Data[0]: 305419896 // 0x12345678
            // Data[1]: 16909060 // 0x1020304
            // Data[^1]: 2018915346 // 0x78563412
            // Count(Data[1]): 536870911 // 0x1FFFFFFF
            // ItemsToString: ExSpan<int>[536870913]{305419896, 16909060, 16909060, 16909060, 16909060, 16909060, 16909060, 16909060, ..., 16909060, 16909060, 16909060, 16909060, 16909060, 16909060, 16909060, 2018915346}
            // intSpan.Count(): 536870911 // 0x1FFFFFFF
            // intSpan.Length: 536870913 // 0x20000001
            // byteSpan.Length: 2147483652 // 0x80000004
            // byteSpan[0]: 120 // 0x78
            // byteSpan.ItemsToString: ExSpan<byte>[2147483652]{120, 86, 52, 18, 4, 3, 2, 1, ..., 4, 3, 2, 1, 18, 52, 86, 120}
            // byteSpan.Count(): 2 // 0x2
        }

    }
}
