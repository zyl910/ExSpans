# ExSpans

Extended spans of nint index range (nint 索引范围的扩展跨度).

Span uses an index of type Int32, which results in an index range of up to 2G (2^31). ExSpan solves this drawback by using an index of type IntPtr, which can have an index range of more than 2G. And on 64-bit systems, it can access data with a 64-bit index. This makes it suitable for video processing, deep learning, and other areas of large-scale data (Span使用 Int32 类型的索引，其索引范围最多为2G (2^31)。ExSpans解决了这一缺点，它使用 IntPtr 类型的索引，索引范围能超过2G。且在64位系统上，它能以64位的索引来访问数据。这使得它适用于视频处理、深度学习等大规模数据的领域).

Package:

- ExSpans
- ExSpans.Core
- ExSpans.Buffers
- ExSpans.Tensors
