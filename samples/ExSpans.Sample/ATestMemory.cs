using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Zyl.ExSpans;
using Zyl.ExSpans.Buffers;

namespace Zyl.ExSpans.Sample {
    internal class ATestMemory {
        const int bufferSize = 16;

        static void Main0(string[] args) {
            TextWriter writer = Console.Out;
            Program.OutputHeader(writer);

            // Test some.
            TestMain(writer);
        }

        /// <summary>
        /// Test main (测试主入口).
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/>.</param>
        internal static void TestMain(TextWriter writer) {
            TestSimple(writer);
        }

        /// <summary>
        /// Test simple (简单测试).
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/>.</param>
        static void TestSimple(TextWriter writer) {
            // Create ExMemory by Array.
            int[] sourceArray = new int[bufferSize];
            ExMemory<int> exMemory = new ExMemory<int>(sourceArray); // Use constructor method.
            //ExMemory<int> exMemory = sourceArray.AsExMemory(); // Or use extension method.
            TestExMemory(writer, "Array", exMemory);
            writer.WriteLine(string.Format("ExMemory: {0}", exMemory));

            // Cast ExMemory to ReadOnlyExMemory.
            ReadOnlyExMemory<int> readOnlyExMemory = exMemory.AsReadOnlyExMemory();
            writer.WriteLine(string.Format("ReadOnlyExMemory: {0}", readOnlyExMemory));

            // Cast ExMemory to Memory. The length will saturating limited.
            Memory<int> memory = exMemory.AsMemory();
            writer.WriteLine(string.Format("Memory: {0}", memory));

            // Call ExMemoryMarshal.
            if (ExMemoryMarshal.TryGetArray(readOnlyExMemory, out ArraySegment<int> segment1)) {
                writer.WriteLine(string.Format("ExMemoryMarshal.TryGetArray: {0}", segment1));
            }

            // Call MemoryMarshal.
            if (MemoryMarshal.TryGetArray(memory, out ArraySegment<int> segment2)) {
                writer.WriteLine(string.Format("MemoryMarshal.TryGetArray: {0}", segment2));
            }

            // Done.
            writer.WriteLine();

            // Output:
            // [TestExMemory-Array]
            // Data[0]: 305419896 // 0x12345678
            // Data[1]: 16909060 // 0x1020304
            // Data[^1]: 2018915346 // 0x78563412
            // Count(Data[1]): 14 // 0xE
            // ExMemory: Zyl.ExSpans.ExMemory<Int32>[16]
            // ReadOnlyExMemory: Zyl.ExSpans.ReadOnlyExMemory<Int32>[16]
            // Memory: System.Memory<Int32>[16]
            // ExMemoryMarshal.TryGetArray: System.ArraySegment`1[System.Int32]
            // MemoryMarshal.TryGetArray: System.ArraySegment`1[System.Int32]
        }

        /// <summary>
        /// Test <see cref="ExMemory{T}"/> (测试 <see cref="ExMemory{T}"/>).
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/>.</param>
        /// <param name="title">The title (标题).</param>
        /// <param name="memory">This memory (当前内存).</param>
        static void TestExMemory(TextWriter writer, string title, ExMemory<int> memory) {
            try {
                writer.WriteLine($"[TestExMemory-{title}]");
                TestExSpan(writer, title, memory.ExSpan);
            } catch (Exception ex) {
                writer.WriteLine(string.Format("Run TestExMemory fail! {0}", ex.ToString()));
            }
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
                //writer.WriteLine($"[TestExSpan-{title}]");
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

    }
}
