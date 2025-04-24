using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace Zyl.ExSpans.Tests.Fake.Attributes {
    // From: https://github.com/dotnet/arcade/blob/b902fd6b6948e689a5128fa6d94dc7de13e6af84/src/Microsoft.DotNet.XUnitExtensions/src/Attributes/PlatformSpecificAttribute.cs

    [Flags]
    public enum TestPlatforms {
        Windows = 1,
        Linux = 2,
        OSX = 4,
        FreeBSD = 8,
        NetBSD = 16,
        AnyUnix = FreeBSD | Linux | NetBSD | OSX,
        Any = ~0
    }

    /// <summary>
    /// Apply this attribute to your test method to specify this is a platform specific test.
    /// </summary>
    [TraitDiscoverer("Microsoft.DotNet.XUnitExtensions.PlatformSpecificDiscoverer", "Microsoft.DotNet.XUnitExtensions")]
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class PlatformSpecificAttribute : Attribute, ITraitAttribute {
        public PlatformSpecificAttribute(TestPlatforms platforms) { }
    }
}
