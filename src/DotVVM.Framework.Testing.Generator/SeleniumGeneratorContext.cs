using System.Collections.Generic;
using DotVVM.Framework.Compilation.ControlTree.Resolved;

namespace DotVVM.Framework.Testing.Generator
{
    public class SeleniumGeneratorContext
    {
        public ResolvedControl Control { get; set; }

        public string UniqueName { get; set; }
        public string Selector { get; set; }

        public HashSet<string> UsedNames { get; set; } = new HashSet<string>();

        public HashSet<string> ExistingUsedSelectors { get; set; }

        public ISeleniumPageObjectVisitor Visitor { get; set; }
    }
}
