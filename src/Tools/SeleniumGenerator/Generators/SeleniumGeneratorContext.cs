using System.Collections.Generic;
using DotVVM.Framework.Compilation.ControlTree.Resolved;

namespace DotVVM.Framework.Tools.SeleniumGenerator.Generators
{
    public class SeleniumGeneratorContext
    {

        public ResolvedControl Control { get; set; }

        public string Selector { get; set; }

        public string UniqueName { get; set; }

        public HashSet<string> UsedNames { get; set; }

        public SeleniumHelperVisitor Visitor { get; set; }
    }
}
