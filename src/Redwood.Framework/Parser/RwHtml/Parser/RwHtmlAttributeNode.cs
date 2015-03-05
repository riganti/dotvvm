using System;
using System.Collections.Generic;
using System.Linq;

namespace Redwood.Framework.Parser.RwHtml.Parser
{
    public class RwHtmlAttributeNode : RwHtmlNode
    {
        public string Prefix { get; set; }

        public string Name { get; set; }

        public RwHtmlLiteralNode Literal { get; set; }
        public RwHtmlElementNode ParentElement { get; set; }

        public override IEnumerable<RwHtmlNode> EnumerateNodes()
        {
            return base.EnumerateNodes().Concat(Literal.EnumerateNodes());
        }
    }
}