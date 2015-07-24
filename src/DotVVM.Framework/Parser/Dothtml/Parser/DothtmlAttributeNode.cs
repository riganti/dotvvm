using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Parser.Dothtml.Tokenizer;

namespace DotVVM.Framework.Parser.Dothtml.Parser
{
    public class DothtmlAttributeNode : DothtmlNode
    {
        public string AttributePrefix { get; set; }

        public string AttributeName { get; set; }

        public DothtmlLiteralNode Literal { get; set; }

        public DothtmlElementNode ParentElement { get; set; }

        public DothtmlToken AttributePrefixToken { get; set; }

        public DothtmlToken AttributeNameToken { get; set; }

        public override IEnumerable<DothtmlNode> EnumerateNodes()
        {
            if (Literal != null)
                return base.EnumerateNodes().Concat(Literal.EnumerateNodes());
            else return base.EnumerateNodes();
        }
    }
}