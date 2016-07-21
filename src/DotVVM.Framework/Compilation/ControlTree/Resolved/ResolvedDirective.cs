using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;

namespace DotVVM.Framework.Compilation.ControlTree.Resolved
{
    public class ResolvedDirective : IAbstractDirective
    {
        public DothtmlDirectiveNode DirectiveNode { get; set; }

        public ResolvedTreeRoot Parent { get; set; }
        IAbstractTreeRoot IAbstractDirective.Parent => Parent;

        public string Value => DirectiveNode.Value;
    }
}
