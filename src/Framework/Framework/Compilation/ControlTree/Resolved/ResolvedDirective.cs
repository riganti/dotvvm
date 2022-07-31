using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;

namespace DotVVM.Framework.Compilation.ControlTree.Resolved
{
    public class ResolvedDirective : ResolvedTreeNode, IAbstractDirective
    {
        public string Value => ((DothtmlDirectiveNode)DothtmlNode!).Value;

        public ResolvedDirective(DothtmlDirectiveNode node)
        {
            DothtmlNode = node;
        }

        public override void Accept(IResolvedControlTreeVisitor visitor)
        {
            visitor.VisitDirective(this);
        }

        public override void AcceptChildren(IResolvedControlTreeVisitor visitor)
        {
        }
    }
}
