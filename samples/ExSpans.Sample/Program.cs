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
            ExSpan<int> sizableSpan = sourceSpan; // Implicit conversion Span to ExSpan.
            Span<int> span = (Span<int>)sizableSpan; // Use explicit conversion.
            //Span<int> span = sizableSpan.AsSpan(); // Or use extension method.
            writer.WriteLine(string.Format("Span[1]: {0} // 0x{0:X}", span[1]));
            int checkSum = SumExSpan(sizableSpan); // Implicit conversion ExSpan to ReadOnlyExSpan.
            writer.WriteLine(string.Format("CheckSum: {0} // 0x{0:X}", checkSum));
            writer.WriteLine();

            // Output:
            // [TestExSpan-Array]
            // Data[0]: 305419896 // 0x12345678
            // Data[1]: 16909060 // 0x1020304
            // 
            // [TestExSpan-Span]
            // Data[0]: 305419896 // 0x12345678
            // Data[1]: 16909060 // 0x1020304
            // Span[1]: 16909060 // 0x1020304
            // CheckSum: 559055796 // 0x215283B4
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
                // Read.
                writer.WriteLine(string.Format("Data[0]: {0} // 0x{0:X}", span[0]));
                writer.WriteLine(string.Format("Data[1]: {0} // 0x{0:X}", span[1]));
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
            const uint byteSize = 2U * 1024 * 1024 * 1024; // 2GB
            nint bufferSize = (nint)(byteSize / sizeof(int));
            // Create ExSpan by Pointer.
            try {
                void* buffer = ExNativeMemory.Alloc(byteSize);
                try {
                    ExSpan<int> sizableSpan = new ExSpan<int>(buffer, bufferSize);
                    TestExSpan(writer, "2GB", sizableSpan);
                    writer.WriteLine(string.Format("ItemsToString: {0}", sizableSpan.ItemsToString((nint)16)));
                    writer.WriteLine();
                } finally {
                    ExNativeMemory.Free(buffer);
                }
            } catch (Exception ex) {
                writer.WriteLine(string.Format("Run Test2GB fail! {0}", ex.ToString()));
            }
        }

    }
}
