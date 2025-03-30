using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using System.IO.MemoryMappedFiles;

namespace Zyl.SizableSpans.Sample {
    internal class Program {
        static void Main(string[] args) {
            TextWriter writer = Console.Out;
            writer.WriteLine("SizableSpans.Sample");
            EnvironmentOutput.OutputEnvironment(writer);
            writer.WriteLine();

            writer.WriteLine("ArrayMaxLengthSafe:\t{0} // 0x{0:X}", SizableMemoryMarshal.ArrayMaxLengthSafe);
            writer.WriteLine();

            // Test some.
            try {
                TestMemoryMappedFile(writer);
            } catch (Exception ex) {
                writer.WriteLine(string.Format("Run TestMemoryMappedFile fail! {0}", ex.ToString()));
            }
        }

        /// <summary>
        /// Test MemoryMappedFile (测试内存映射文件).
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/>.</param>
        static void TestMemoryMappedFile(TextWriter writer) {
            const string MemoryMappedFileMapName = "SizableSpans.Sample.tmp";
            const string MemoryMappedFilePath = MemoryMappedFileMapName;
            const long MemoryMappedFileSize = 1 * 1024 * 1024; // 1MB
            writer.WriteLine("[TestMemoryMappedFile]");
            using MemoryMappedFile mappedFile = MemoryMappedFile.CreateFromFile(MemoryMappedFilePath, FileMode.Create, MemoryMappedFileMapName, MemoryMappedFileSize);
            using MemoryMappedViewAccessor accessor = mappedFile.CreateViewAccessor();
            using SafeBufferSpanProvider<SafeMemoryMappedViewHandle> spanProvider = accessor.SafeMemoryMappedViewHandle.CreateSpanProvider();
            // Write.
            SizableSpan<int> spanInt = spanProvider.CreateSizableSpan<int>();
            spanInt.Fill(0x01020304);
            spanInt[(nuint)0] = 0x12345678;
            // Read.
            writer.WriteLine(string.Format("Data[0]: {0} // 0x{0:X}", spanInt[(nuint)0]));
            writer.WriteLine(string.Format("Data[1]: {0} // 0x{0:X}", spanInt[(nuint)1]));
            // done.
            writer.WriteLine();
            // Output:
            // [TestMemoryMappedFile]
            // Data[0]: 305419896 // 0x12345678
            // Data[1]: 16909060 // 0x1020304

        }
    }
}
