#if NET9_0_OR_GREATER
#define ALLOWS_REF_STRUCT // C# 13 - ref struct interface; allows ref struct. https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-13#ref-struct-interfaces
#endif // NET9_0_OR_GREATER

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Zyl.SizableSpans.Reflection;

namespace Zyl.SizableSpans.Tests.Reflection {
    public class TypeNameUtilTest {

        private readonly ITestOutputHelper _output;

        public TypeNameUtilTest(ITestOutputHelper output) {
            _output = output;
        }

        private ITestOutputHelper Output => _output;


        private static readonly TypeNameFlags[] _flagsArray = {
            TypeNameFlags.Default,
            TypeNameFlags.Raw,
            TypeNameFlags.Raw | TypeNameFlags.ShowNamespace,
            TypeNameFlags.SubShowNamespace,
            TypeNameFlags.SubShowNamespace | TypeNameFlags.SubShowNamespace,
            TypeNameFlags.NoKeyword,
            TypeNameFlags.NoKeyword | TypeNameFlags.SubShowNamespace,
            TypeNameFlags.NoKeyword | TypeNameFlags.ShowNamespace,
            TypeNameFlags.NoKeyword | TypeNameFlags.ShowNamespace | TypeNameFlags.SubShowNamespace,
        };

        [Fact]
        public void ItemsToStringTest() {
            CallItem<double>();
            CallItem<Tuple<int>>();
            CallItem<List<Tuple<int, string>>>();
            CallItem<List<Tuple<int?, string>[]>>();

            void CallItem<T>()
#if ALLOWS_REF_STRUCT
                    where T : allows ref struct
#endif // ALLOWS_REF_STRUCT
                    {
                foreach(var flags in _flagsArray) {
                    string name = TypeNameUtil.GetName<T>(flags);
                    Output.WriteLine("{0}:\t{1}", flags, name);
                }
                Output.WriteLine("");
            }
        }

    }
}
