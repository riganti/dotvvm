using System;
using System.Collections.Generic;
using DotVVM.Framework.Parser.Dothtml.Parser;
using DotVVM.Framework.Runtime.Compilation;

namespace DotVVM.Framework.Runtime.ControlTree.Resolved
{
    public class ResolvedTreeRoot: ResolvedContentNode, IAbstractTreeRoot
    {
        public Dictionary<string, string> Directives { get; set; } = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

        public ResolvedTreeRoot(ControlResolverMetadata metadata, DothtmlNode node, DataContextStack dataContext)
            : base(metadata, node, dataContext)
        {
        }
        

        public override void Accept(IResolvedControlTreeVisitor visitor)
        {
            visitor.VisitView(this);
        }
    }
}
