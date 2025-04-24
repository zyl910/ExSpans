using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zyl.ExSpans.Tests.Fake.Attributes {
    /// <summary>
    /// Fake ActiveIssueAttribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class ActiveIssueAttribute : Attribute {
        public ActiveIssueAttribute(string url) { }
    }
}
