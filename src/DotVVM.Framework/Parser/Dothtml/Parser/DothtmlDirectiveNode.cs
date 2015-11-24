using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Parser.Dothtml.Tokenizer;

namespace DotVVM.Framework.Parser.Dothtml.Parser
{
    public class DothtmlDirectiveNode : DothtmlNode
    {
        public string Name => NameNode.Text;
        public string Value => (ValueNode!=null) ? ValueNode.Text : string.Empty;
        public DothtmlToken DirectiveStartToken { get; set; }

        public DothtmlNameNode NameNode { get; set; }

        public DothtmlValueTextNode ValueNode { get; set; }

        public override IEnumerable<DothtmlNode> EnumerateNodes()
        {
            var enumeration = base.EnumerateNodes().Concat(NameNode.EnumerateNodes());
            if (ValueNode != null)
            {
                enumeration = enumeration.Concat(ValueNode.EnumerateNodes());
            }
            return enumeration;
        }
    }
}