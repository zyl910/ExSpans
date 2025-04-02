//#define USE_ALL_ITEMS

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Zyl.SizableSpans.Extensions;
using Zyl.SizableSpans.Impl;

namespace Zyl.SizableSpans {
    internal sealed class SizableSpanDebugView<T> {
#if USE_ALL_ITEMS

        private readonly T[] _array;

        public SizableSpanDebugView(SizableSpan<T> span) {
            _array = span.ToArray();
        }

        public SizableSpanDebugView(ReadOnlySizableSpan<T> span) {
            _array = span.ToArray();
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Items => _array;

#else // USE_ALL_ITEMS

        private readonly TSize _length;
        private readonly nint _dataAddress;
        private readonly T[] _itemsHeader;
        private readonly T[] _itemsFooter;
        private readonly TSize _itemsFooterStart;
        private readonly string _stringHeader;

        public unsafe SizableSpanDebugView(SizableSpan<T> span) {
            int spanViewLength = SizableMemoryMarshal.SpanViewLength;
            _length = span.Length;
            _dataAddress = (nint)Unsafe.AsPointer(ref span.GetPinnableReference());
            _itemsHeader = span.ToArray(spanViewLength);
            if (_length.LessThanOrEqual((uint)spanViewLength)) {
                _itemsFooter = ArrayHelper.Empty<T>();
                _itemsFooterStart = default;
            } else {
                _itemsFooterStart = _length.Subtract((TSize)spanViewLength);
                if (_itemsFooterStart.LessThan((uint)spanViewLength)) _itemsFooterStart = (TSize)spanViewLength;
                _itemsFooter = span.Slice(_itemsFooterStart).ToArray(spanViewLength);
            }
            _stringHeader = string.Empty;
            try {
                if (typeof(char) == typeof(T)) {
                    _stringHeader = new string((char[])(object)_itemsHeader);
                } else if (typeof(byte) == typeof(T)) {
#if NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_0_OR_GREATER
                    _stringHeader = Encoding.Default.GetString((byte[])(object)_itemsHeader);
#else
                    _stringHeader = Encoding.UTF8.GetString((byte[])(object)_itemsHeader, 0, _itemsHeader.Length);
#endif
                }
            } catch (Exception ex) {
                Debug.WriteLine(ex);
            }
        }

        public unsafe SizableSpanDebugView(ReadOnlySizableSpan<T> span) {
            int spanViewLength = SizableMemoryMarshal.SpanViewLength;
            _length = span.Length;
            _dataAddress = (nint)Unsafe.AsPointer(ref Unsafe.AsRef(in span.GetPinnableReference()));
            _itemsHeader = span.ToArray(spanViewLength);
            if (_length.LessThanOrEqual((uint)spanViewLength)) {
                _itemsFooter = ArrayHelper.Empty<T>();
                _itemsFooterStart = default;
            } else {
                _itemsFooterStart = _length.Subtract((TSize)spanViewLength);
                _itemsFooter = span.Slice(_itemsFooterStart).ToArray(spanViewLength);
            }
            _stringHeader = string.Empty;
            try {
                if (typeof(char) == typeof(T)) {
                    _stringHeader = new string((char[])(object)_itemsHeader);
                } else if (typeof(byte) == typeof(T)) {
#if NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_0_OR_GREATER
                    _stringHeader = Encoding.Default.GetString((byte[])(object)_itemsHeader);
#else
                    _stringHeader = Encoding.UTF8.GetString((byte[])(object)_itemsHeader, 0, _itemsHeader.Length);
#endif
                }
            } catch (Exception ex) {
                Debug.WriteLine(ex);
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
        public TSize Length => _length;

        [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
        public nint DataAddress => _dataAddress;

        [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
        public T[] ItemsHeader => _itemsHeader;

        [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
        public T[] ItemsFooter => _itemsFooter;

        [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
        public TSize ItemsFooterStart => _itemsFooterStart;

        [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
        public string StringHeader => _stringHeader;

#endif // USE_ALL_ITEMS
    }

}
