using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Framework.Parser.Dothtml.Parser
{
    public class DothtmlRootNode : DothtmlNodeWithContent
    {
        public List<DothtmlDirectiveNode> Directives { get; private set; }

        public DothtmlRootNode()
        {
            Directives = new List<DothtmlDirectiveNode>();
        }

        public override IEnumerable<DothtmlNode> EnumerateChildNodes()
        {
            return Directives.Concat(base.EnumerateChildNodes());
        }

        public override void Accept(IDothtmlSyntaxTreeVisitor visitor)
        {
            visitor.Visit(this);

            foreach (var node in EnumerateChildNodes())
            {
                node.Accept(visitor);
            }
        }

        public override IEnumerable<DothtmlNode> EnumerateNodes()
        {
            return base.EnumerateNodes().Concat(EnumerateChildNodes().SelectMany(d => d.EnumerateNodes()));
        }
    }
}
