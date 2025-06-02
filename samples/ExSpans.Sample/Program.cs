using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

namespace Zyl.ExSpans.Sample {

    internal class Program {
        static void Main(string[] args) {
            TextWriter writer = Console.Out;
            writer.WriteLine("ExSpans.Sample");
            EnvironmentOutput.OutputEnvironment(writer);
            writer.WriteLine();

            // Test some.
            TestSimple(writer);
            Test2GB(writer);
            TestMemoryMappedFile(writer);
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
                span[(nint)0] = 0x12345678;
                // Read.
                writer.WriteLine(string.Format("Data[0]: {0} // 0x{0:X}", span[(nint)0]));
                writer.WriteLine(string.Format("Data[1]: {0} // 0x{0:X}", span[(nint)1]));
            } catch (Exception ex) {
                writer.WriteLine(string.Format("Run TestExSpan fail! {0}", ex.ToString()));
            }
        }

        private static int SumExSpan(ReadOnlyExSpan<int> span) {
            int rt = 0; // Result.
            for (nint i = (nint)0; i < span.Length; i++) {
                rt += span[i];
            }
            return rt;
        }

        /// <summary>
        /// Test 2GB data (测试2GB数据).
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/>.</param>
        static unsafe void Test2GB(TextWriter writer) {
#if NET6_0_OR_GREATER
            const uint byteSize = 2U * 1024 * 1024 * 1024; // 2GB
            nint bufferSize = (nint)(byteSize / sizeof(int));
            // Create ExSpan by Pointer.
            try {
                void* buffer = NativeMemory.Alloc(byteSize);
                try {
                    ExSpan<int> sizableSpan = new ExSpan<int>(buffer, bufferSize);
                    TestExSpan(writer, "2GB", sizableSpan);
                    writer.WriteLine(string.Format("ItemsToString: {0}", sizableSpan.ItemsToString((nint)16)));
                    writer.WriteLine();
                } finally {
                    NativeMemory.Free(buffer);
                }
            } catch (Exception ex) {
                writer.WriteLine(string.Format("Run Test2GB fail! {0}", ex.ToString()));
            }
#endif // NET6_0_OR_GREATER
        }

        /// <summary>
        /// Test MemoryMappedFile (测试内存映射文件).
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/>.</param>
        static void TestMemoryMappedFile(TextWriter writer) {
            try {
                const string MemoryMappedFileMapName = "ExSpans.Sample.tmp";
                const string MemoryMappedFilePath = MemoryMappedFileMapName;
                const long MemoryMappedFileSize = 1 * 1024 * 1024; // 1MB
                using MemoryMappedFile mappedFile = MemoryMappedFile.CreateFromFile(MemoryMappedFilePath, FileMode.Create, MemoryMappedFileMapName, MemoryMappedFileSize);
                using MemoryMappedViewAccessor accessor = mappedFile.CreateViewAccessor();
                using SafeBufferSpanProvider spanProvider = accessor.SafeMemoryMappedViewHandle.CreateSpanProvider();
                // Write.
                writer.WriteLine("[TestMemoryMappedFile]");
                ExSpan<int> spanInt = spanProvider.CreateExSpan<int>();
                spanInt.Fill(0x01020304);
                spanInt[(nint)0] = 0x12345678;
                // Read.
                writer.WriteLine(string.Format("Data[0]: {0} // 0x{0:X}", spanInt[(nint)0]));
                writer.WriteLine(string.Format("Data[1]: {0} // 0x{0:X}", spanInt[(nint)1]));
                // Extension methods provided by ExSpanExtensions.
                writer.WriteLine(string.Format("ItemsToString: {0}", spanProvider.ItemsToString(spanProvider.GetPinnableReadOnlyReference(), (nint)16)));
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
