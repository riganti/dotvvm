using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Compilation.Parser.Dothtml.Tokenizer;

namespace DotVVM.Framework.Compilation.Parser.Dothtml.Parser
{
    public class DothtmlDirectiveNode : DothtmlNode
    {
        public DothtmlDirectiveNode(DothtmlToken directiveStartToken, DothtmlNameNode nameNode, DothtmlValueTextNode valueNode)
        {
            DirectiveStartToken = directiveStartToken;
            NameNode = nameNode;
            ValueNode = valueNode;
        }

        public string Name => NameNode.Text;
        public string Value => (ValueNode!=null) ? ValueNode.Text : string.Empty;
        public DothtmlToken DirectiveStartToken { get; set; }
        public DothtmlNameNode NameNode { get; set; }
        public DothtmlValueTextNode ValueNode { get; set; }

        public override IEnumerable<DothtmlNode> EnumerateChildNodes()
        {
            yield return NameNode;
            if (ValueNode != null)
            {
                yield return ValueNode;
            }
        }

        public override void Accept(IDothtmlSyntaxTreeVisitor visitor)
        {
            visitor.Visit(this);
            NameNode.AcceptIfCondition(visitor);
            ValueNode?.AcceptIfCondition(visitor);
        }

        public override IEnumerable<DothtmlNode> EnumerateNodes()
        {
            return base.EnumerateNodes().Concat( EnumerateChildNodes().SelectMany(node => node.EnumerateNodes() ) );
        }
    }
}
