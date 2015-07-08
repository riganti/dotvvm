using DotVVM.Framework.Parser.Dothtml.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Runtime.Compilation.ResolvedControlTree
{
    public class ResolvedView: ResolvedContentNode
    {
        public Dictionary<string, string> Directives { get; set; } = new Dictionary<string, string>();

        public ResolvedView(ControlResolverMetadata metadata, DothtmlNode node)
            : base(metadata, node)
        { }

        public override void Accept(IResolvedControlTreeVisitor visitor)
        {
            visitor.VisitView(this);
        }
    }
}
