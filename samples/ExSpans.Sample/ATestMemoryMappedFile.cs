using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Threading.Tasks;
using Zyl.ExSpans;
using Zyl.ExSpans.Buffers;

namespace Zyl.ExSpans.Sample {
    internal class ATestMemoryMappedFile {

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
            TestMemoryMappedFile(writer);
            TestMemoryMappedFileOnAsync(writer);
        }

        /// <summary>
        /// Test MemoryMappedFile (测试内存映射文件).
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/>.</param>
        static void TestMemoryMappedFile(TextWriter writer) {
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

        /// <summary>
        /// Test MemoryMappedFile on asynchronous code (异步代码中测试内存映射文件).
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/>.</param>
        static void TestMemoryMappedFileOnAsync(TextWriter writer) {
            try {
                const string MemoryMappedFilePath = "ExSpans.Sample.tmp";
                const string? MemoryMappedFileMapName = null; // If it is not null, MacOS will throw an exception. System.PlatformNotSupportedException: Named maps are not supported.
                const long MemoryMappedFileSize = 1 * 1024 * 1024; // 1MB
                using MemoryMappedFile mappedFile = MemoryMappedFile.CreateFromFile(MemoryMappedFilePath, FileMode.Create, MemoryMappedFileMapName, MemoryMappedFileSize);
                using MemoryMappedViewAccessor accessor = mappedFile.CreateViewAccessor();
                SafeBufferSpanProvider spanProvider = accessor.SafeMemoryMappedViewHandle.CreateSpanProvider(); // Using by memoryManager.
                using PointerExMemoryManager<byte, SafeBufferSpanProvider> memoryManager = spanProvider.CreatePointerExMemoryManager();
                //using PointerExMemoryManager<byte, SafeBufferSpanProvider> memoryManager = accessor.SafeMemoryMappedViewHandle.CreatePointerExMemoryManager(); // PointerExMemoryManager also support SafeBuffer.
                Task task = Task.Run(() => {
                    // Write.
                    writer.WriteLine("[TestMemoryMappedFileOnAsync]");
                    ExSpan<int> spanInt = spanProvider.CreateExSpan<int>();
                    //ExSpan<int> spanInt = ExMemoryMarshal.Cast<byte, int>(memoryManager.GetExSpan()); // It can be used instead of `spanProvider.CreateExSpan<int>()`.
                    spanInt.Fill(0x01020304);
                    spanInt[0] = 0x12345678;
                    // Read.
                    writer.WriteLine(string.Format("Data[0]: {0} // 0x{0:X}", spanInt[0]));
                    writer.WriteLine(string.Format("Data[1]: {0} // 0x{0:X}", spanInt[1]));
                    // Output ExSpan.
                    writer.WriteLine(string.Format("ExSpan: {0}", spanInt.ToString()));
                    // Output ExMemory.
                    ExMemory<byte> memory = memoryManager.ExMemory;
                    writer.WriteLine(string.Format("ExMemory: {0}", memory));
                });
                // done.
                task.Wait();
                writer.WriteLine();
            } catch (Exception ex) {
                writer.WriteLine(string.Format("Run TestMemoryMappedFile fail! {0}", ex.ToString()));
            }
            // Output:
            // [TestMemoryMappedFileOnAsync]
            // Data[0]: 305419896 // 0x12345678
            // Data[1]: 16909060 // 0x1020304
            // ExSpan: Zyl.ExSpans.ExSpan<Int32>[262144]
            // ExMemory: Zyl.ExSpans.ExMemory<Byte>[1048576]
        }
    }
}
