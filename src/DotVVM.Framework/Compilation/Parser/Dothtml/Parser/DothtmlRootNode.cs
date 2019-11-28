#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Framework.Compilation.Parser.Dothtml.Parser
{
    public class DothtmlRootNode : DothtmlNodeWithContent
    {
        public List<DothtmlDirectiveNode> Directives { get; } = new List<DothtmlDirectiveNode>();

        public override IEnumerable<DothtmlNode> EnumerateChildNodes()
        {
            return Directives.Concat(base.EnumerateChildNodes());
        }

        public override void Accept(IDothtmlSyntaxTreeVisitor visitor)
        {
            visitor.Visit(this);

            foreach (var node in EnumerateChildNodes())
            {
                if (visitor.Condition(node))
                {
                    node.Accept(visitor);
                }
            }
        }

        public override IEnumerable<DothtmlNode> EnumerateNodes()
        {
            return base.EnumerateNodes().Concat(EnumerateChildNodes().SelectMany(d => d.EnumerateNodes()));
        }


        public string GetDirectiveValue(string directiveName)
        {
            return Directives.Where(d => string.Equals(d.Name, directiveName, StringComparison.OrdinalIgnoreCase))
                .Select(d => d.Value)
                .FirstOrDefault();
        }
    }
}
