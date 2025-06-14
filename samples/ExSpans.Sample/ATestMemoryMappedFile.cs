using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using Zyl.ExSpans;

namespace Zyl.ExSpans.Sample {
    internal class ATestMemoryMappedFile {

        static void Main0(string[] args) {
            TextWriter writer = Console.Out;
            Program.OutputHeader(writer);

            // Test some.
            TestMemoryMappedFile(writer);
        }

        /// <summary>
        /// Test MemoryMappedFile (测试内存映射文件).
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/>.</param>
        internal static void TestMemoryMappedFile(TextWriter writer) {
            try {
                const string MemoryMappedFilePath = "ExSpans.Sample.tmp";
                const string? MemoryMappedFileMapName = null; // If it is not null, MacOS will throw an exception. System.PlatformNotSupportedException: Named maps are not supported.
                const long MemoryMappedFileSize = 1 * 1024 * 1024; // 1MB
                using MemoryMappedFile mappedFile = MemoryMappedFile.CreateFromFile(MemoryMappedFilePath, FileMode.Create, MemoryMappedFileMapName, MemoryMappedFileSize);
                using MemoryMappedViewAccessor accessor = mappedFile.CreateViewAccessor();
                using SafeBufferSpanProvider spanProvider = accessor.SafeMemoryMappedViewHandle.CreateSpanProvider();
                // Write.
                writer.WriteLine("[TestMemoryMappedFile]");
                ExSpan<int> spanInt = spanProvider.CreateExSpan<int>();
                spanInt.Fill(0x01020304);
                spanInt[0] = 0x12345678;
                // Read.
                writer.WriteLine(string.Format("Data[0]: {0} // 0x{0:X}", spanInt[0]));
                writer.WriteLine(string.Format("Data[1]: {0} // 0x{0:X}", spanInt[1]));
                // Extension methods provided by ExSpanExtensions.
                writer.WriteLine(string.Format("ItemsToString: {0}", spanProvider.ItemsToString(spanProvider.GetPinnableReadOnlyReference(), 16)));
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
