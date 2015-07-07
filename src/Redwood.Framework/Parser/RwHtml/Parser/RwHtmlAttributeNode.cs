using System;
using System.Collections.Generic;
using System.Linq;
using Redwood.Framework.Parser.RwHtml.Tokenizer;

namespace Redwood.Framework.Parser.RwHtml.Parser
{
    public class RwHtmlAttributeNode : RwHtmlNode
    {
        public string AttributePrefix { get; set; }

        public string AttributeName { get; set; }

        public RwHtmlLiteralNode Literal { get; set; }

        public RwHtmlElementNode ParentElement { get; set; }

        public RwHtmlToken AttributePrefixToken { get; set; }

        public RwHtmlToken AttributeNameToken { get; set; }

        public override IEnumerable<RwHtmlNode> EnumerateNodes()
        {
            if (Literal != null)
                return base.EnumerateNodes().Concat(Literal.EnumerateNodes());
            else return base.EnumerateNodes();
        }
    }
}