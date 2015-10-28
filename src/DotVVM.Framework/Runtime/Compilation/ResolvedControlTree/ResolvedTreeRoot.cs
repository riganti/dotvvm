using DotVVM.Framework.Parser.Dothtml.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Runtime.Compilation.ResolvedControlTree
{
    public class ResolvedTreeRoot: ResolvedContentNode
    {
        public Dictionary<string, string> Directives { get; set; } = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

        public ResolvedTreeRoot(ControlResolverMetadata metadata, DothtmlNode node, DataContextStack dataContext)
            : base(metadata, node, dataContext)
        { }

        public override void Accept(IResolvedControlTreeVisitor visitor)
        {
            visitor.VisitView(this);
        }
    }
}
