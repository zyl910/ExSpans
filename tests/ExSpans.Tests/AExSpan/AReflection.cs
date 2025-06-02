using Xunit;
using System.Buffers;
using System.Buffers.Binary;
using System.Reflection;
using System.Runtime.InteropServices;
//using System.MemoryTests;

namespace Zyl.ExSpans.Tests.AExSpan {
#nullable disable
    public static partial class AReflection {
#if NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_0_OR_GREATER || NET40_OR_GREATER

        // Calling ExSpan APIs via Reflection is not supported yet.
        // These tests check that using reflection results in graceful failures. See https://github.com/dotnet/runtime/issues/10057
        // These tests are only relevant for fast span.

        [Fact]
        public static void MemoryExtensions_StaticReturningReadOnlyExSpan() {
            Type type = typeof(ExSpanExtensions);

            MethodInfo method = type.GetMethod(nameof(ExSpanExtensions.AsExSpan), new Type[] { typeof(string) });
            Assert.Throws<NotSupportedException>(() => method.Invoke(null, new object[] { "Hello" }));

            method = type.GetMethod(nameof(ExSpanExtensions.AsExSpan), new Type[] { typeof(string), typeof(int), typeof(int) });
            Assert.Throws<NotSupportedException>(() => method.Invoke(null, new object[] { "Hello", 1, 1 }));
        }

        [Fact]
        public static void MemoryExtensions_StaticWithExSpanArguments() {
            Type type = typeof(ExMemoryExtensions);
            MethodInfo method = type.GetMethod(nameof(ExMemoryExtensions.CompareTo));
            Assert.Throws<NotSupportedException>(() => method.Invoke(null, new object[] { default, default, StringComparison.Ordinal }));
        }

        [Fact]
        public static void BinaryPrimitives_StaticWithExSpanArgument() {
            Type type = typeof(BinaryPrimitives);

            MethodInfo method = type.GetMethod(nameof(BinaryPrimitives.ReadInt16LittleEndian));
            Assert.Throws<NotSupportedException>(() => method.Invoke(null, new object[] { default }));

            method = type.GetMethod(nameof(BinaryPrimitives.TryReadInt16LittleEndian));
            Assert.Throws<NotSupportedException>(() => method.Invoke(null, new object[] { default, null }));
        }

        [Fact]
        public static void MemoryMarshal_GenericStaticReturningExSpan() {
            MethodInfo createExSpanMethod = typeof(ExMemoryMarshal).GetMethod(nameof(ExMemoryMarshal.CreateExSpan));

            int value = 0;
            ref int refInt = ref value;
            Type refIntType = refInt.GetType();

            MethodInfo method = createExSpanMethod.MakeGenericMethod(refIntType);
            Assert.Throws<NotSupportedException>(() => method.Invoke(null, new object[] { null, 0 }));
        }

        [Fact]
        public static void ExSpan_Constructor() {
            Type type = typeof(ExSpan<int>);

            ConstructorInfo ctor = type.GetConstructor(new Type[] { typeof(int[]) });
            Assert.Throws<TargetException>(() => ctor.Invoke(new object[] { new int[10] }));

            ctor = type.GetConstructor(new Type[] { typeof(int[]), typeof(TSize), typeof(TSize) });
            Assert.Throws<TargetException>(() => ctor.Invoke(new object[] { new int[10], (TSize)1, (TSize)1 }));

            ctor = type.GetConstructor(new Type[] { typeof(void*), typeof(TSize) });
            Assert.Throws<TargetException>(() => ctor.Invoke(new object[] { null, (TSize)1 }));
        }

        [Fact]
        public static void ExSpan_Property() {
            Type type = typeof(ExSpan<int>);

            PropertyInfo property = type.GetProperty(nameof(ExSpan<int>.Empty));
            Assert.Throws<NotSupportedException>(() => property.GetValue(default));
        }

        [Fact]
        public static void ExSpan_StaticOperator() {
            Type type = typeof(ExSpan<int>);

            MethodInfo method = type.GetMethod("op_Equality");
            Assert.Throws<NotSupportedException>(() => method.Invoke(null, new object[] { default, default }));

            method = type.GetMethod("op_Inequality");
            Assert.Throws<NotSupportedException>(() => method.Invoke(null, new object[] { default, default }));
        }

        [Fact]
        public static void ExSpan_InstanceMethod() {
            Type type = typeof(ExSpan<int>);

            MethodInfo method = type.GetMethod(nameof(ExSpan<int>.CopyTo), new Type[] { typeof(ExSpan<int>) });
            Assert.Throws<NotSupportedException>(() => method.Invoke(default, new object[] { default }));
        }

        [Fact]
        public static void ReadOnlyExSpan_Constructor() {
            Type type = typeof(ReadOnlyExSpan<int>);

            ConstructorInfo ctor = type.GetConstructor(new Type[] { typeof(int[]) });
            Assert.Throws<TargetException>(() => ctor.Invoke(new object[] { new int[10] }));

            ctor = type.GetConstructor(new Type[] { typeof(int[]), typeof(TSize), typeof(TSize) });
            Assert.Throws<TargetException>(() => ctor.Invoke(new object[] { new int[10], (TSize)1, (TSize)1 }));

            ctor = type.GetConstructor(new Type[] { typeof(void*), typeof(TSize) });
            Assert.Throws<TargetException>(() => ctor.Invoke(new object[] { null, (TSize)1 }));
        }

        [Fact]
        public static void ReadOnlyExSpan_Property() {
            Type type = typeof(ReadOnlyExSpan<int>);

            PropertyInfo property = type.GetProperty(nameof(ReadOnlyExSpan<int>.Empty));
            Assert.Throws<NotSupportedException>(() => property.GetValue(default));
        }

        [Fact]
        public static void ReadOnlyExSpan_Operator() {
            Type type = typeof(ReadOnlyExSpan<int>);

            MethodInfo method = type.GetMethod("op_Equality");
            Assert.Throws<NotSupportedException>(() => method.Invoke(null, new object[] { default, default }));

            method = type.GetMethod("op_Inequality");
            Assert.Throws<NotSupportedException>(() => method.Invoke(null, new object[] { default, default }));
        }

        [Fact]
        public static void ReadOnlyExSpan_InstanceMethod() {
            Type type = typeof(ReadOnlyExSpan<int>);

            MethodInfo method = type.GetMethod(nameof(ReadOnlyExSpan<int>.CopyTo), new Type[] { typeof(ExSpan<int>) });
            Assert.Throws<NotSupportedException>(() => method.Invoke(default, new object[] { default }));
        }

#if NOT_RELATED
        [Fact]
        public static void Memory_PropertyReturningExSpan() {
            Type type = typeof(Memory<int>);

            PropertyInfo property = type.GetProperty(nameof(Memory<int>.ExSpan));
            Assert.Throws<NotSupportedException>(() => property.GetValue(null));
        }

        [Fact]
        public static void ReadOnlyMemory_PropertyReturningReadOnlyExSpan() {
            Type type = typeof(ReadOnlyMemory<int>);

            PropertyInfo property = type.GetProperty(nameof(ReadOnlyMemory<int>.ExSpan));
            Assert.Throws<NotSupportedException>(() => property.GetValue(null));
        }

        [Fact]
        public static void MemoryManager_MethodReturningExSpan() {
            Type type = typeof(MemoryManager<int>);

            MemoryManager<int> manager = new CustomMemoryForTest<int>(new int[10]);
            MethodInfo method = type.GetMethod(nameof(MemoryManager<int>.GetExSpan), BindingFlags.Public | BindingFlags.Instance);
            Assert.Throws<NotSupportedException>(() => method.Invoke(manager, null));
        }
#endif // NOT_RELATED
#endif // NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_0_OR_GREATER || NET40_OR_GREATER
    }
#nullable restore
}
