using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using Zyl.SizableSpans.Extensions;

namespace Zyl.SizableSpans.Sample {
    internal class Program {
        static void Main(string[] args) {
            TextWriter writer = Console.Out;
            writer.WriteLine("SizableSpans.Sample");
            EnvironmentOutput.OutputEnvironment(writer);
            writer.WriteLine();

            // Test some.
            TestSimple(writer);
            TestMemoryMappedFile(writer);
        }

        /// <summary>
        /// Test simple (简单测试).
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/>.</param>
        static void TestSimple(TextWriter writer) {
            const int bufferSize = 16;
            // Create SizableSpan by Array.
            int[] sourceArray = new int[bufferSize];
            TestSizableSpan(writer, "Array", new SizableSpan<int>(sourceArray)); // Use constructor method.
            //TestSizableSpan(writer, "Array", sourceArray.AsSizableSpan()); // Or use extension method.
            writer.WriteLine();

            // Create SizableSpan by Span.
            Span<int> sourceSpan = stackalloc int[bufferSize];
            TestSizableSpan(writer, "Span", sourceSpan); // Use implicit conversion.
            //TestSizableSpan(writer, "Span", sourceSpan.AsSizableSpan()); // Or use extension method.

            // Convert SizableSpan to Span.
            SizableSpan<int> sizableSpan = sourceSpan; // Implicit conversion Span to SizableSpan.
            Span<int> span = (Span<int>)sizableSpan; // Use explicit conversion.
            //Span<int> span = sizableSpan.AsSpan(); // Or use extension method.
            writer.WriteLine(string.Format("Span[1]: {0} // 0x{0:X}", span[1]));
            int checkSum = SumSizableSpan(sizableSpan); // Implicit conversion SizableSpan to ReadOnlySizableSpan.
            writer.WriteLine(string.Format("CheckSum: {0} // 0x{0:X}", checkSum));
            writer.WriteLine();

            // Output:
            // [TestSizableSpan-Array]
            // Data[0]: 305419896 // 0x12345678
            // Data[1]: 16909060 // 0x1020304
            // 
            // [TestSizableSpan-Span]
            // Data[0]: 305419896 // 0x12345678
            // Data[1]: 16909060 // 0x1020304
            // Span[1]: 16909060 // 0x1020304
            // CheckSum: 559055796 // 0x215283B4
        }

        /// <summary>
        /// Test <see cref="SizableSpan{T}"/> (测试 <see cref="SizableSpan{T}"/>).
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/>.</param>
        /// <param name="title">The title (标题).</param>
        /// <param name="span">This span (当前跨度).</param>
        static void TestSizableSpan(TextWriter writer, string title, SizableSpan<int> span) {
            try {
                // Write.
                writer.WriteLine($"[TestSizableSpan-{title}]");
                span.Fill(0x01020304);
                span[(nuint)0] = 0x12345678;
                // Read.
                writer.WriteLine(string.Format("Data[0]: {0} // 0x{0:X}", span[(nuint)0]));
                writer.WriteLine(string.Format("Data[1]: {0} // 0x{0:X}", span[(nuint)1]));
            } catch (Exception ex) {
                writer.WriteLine(string.Format("Run TestMemoryMappedFile fail! {0}", ex.ToString()));
            }
        }

        private static int SumSizableSpan(ReadOnlySizableSpan<int> span) {
            int rt = 0; // Result.
            for (nuint i = (nuint)0; i.LessThan(span.Length); i += 1) { // The LessThan method is from `Zyl.SizableSpans.Extensions.IntPtrExtensions.LessThan`. Since .NET 7.0, the nuint type has only begin to support the less than operator.
                rt += span[i];
            }
            return rt;
        }

        /// <summary>
        /// Test MemoryMappedFile (测试内存映射文件).
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/>.</param>
        static void TestMemoryMappedFile(TextWriter writer) {
            try {
                const string MemoryMappedFileMapName = "SizableSpans.Sample.tmp";
                const string MemoryMappedFilePath = MemoryMappedFileMapName;
                const long MemoryMappedFileSize = 1 * 1024 * 1024; // 1MB
                using MemoryMappedFile mappedFile = MemoryMappedFile.CreateFromFile(MemoryMappedFilePath, FileMode.Create, MemoryMappedFileMapName, MemoryMappedFileSize);
                using MemoryMappedViewAccessor accessor = mappedFile.CreateViewAccessor();
                using SafeBufferSpanProvider<SafeMemoryMappedViewHandle> spanProvider = accessor.SafeMemoryMappedViewHandle.CreateSpanProvider();
                // Write.
                writer.WriteLine("[TestMemoryMappedFile]");
                SizableSpan<int> spanInt = spanProvider.CreateSizableSpan<int>();
                spanInt.Fill(0x01020304);
                spanInt[(nuint)0] = 0x12345678;
                // Read.
                writer.WriteLine(string.Format("Data[0]: {0} // 0x{0:X}", spanInt[(nuint)0]));
                writer.WriteLine(string.Format("Data[1]: {0} // 0x{0:X}", spanInt[(nuint)1]));
                // ItemsToString.
                writer.WriteLine(string.Format("ItemsToString: {0}", spanProvider.ItemsToString(spanProvider.GetPinnableReadOnlyReference(), (nuint)16)));
                // done.
                writer.WriteLine();
            } catch (Exception ex) {
                writer.WriteLine(string.Format("Run TestMemoryMappedFile fail! {0}", ex.ToString()));
            }
            // Output:
            // [TestMemoryMappedFile]
            // Data[0]: 305419896 // 0x12345678
            // Data[1]: 16909060 // 0x1020304
        }
    }
}
