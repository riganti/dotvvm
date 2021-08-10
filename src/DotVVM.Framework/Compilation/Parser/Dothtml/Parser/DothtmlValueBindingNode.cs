#nullable enable
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Compilation.Parser.Dothtml.Tokenizer;

namespace DotVVM.Framework.Compilation.Parser.Dothtml.Parser
{
    public class DothtmlValueBindingNode : DothtmlValueNode
    {
        public DothtmlValueBindingNode(DothtmlBindingNode bindingNode, IReadOnlyList<DothtmlToken> valueTokens)
        {
            BindingNode = bindingNode;
            ValueTokens = valueTokens;
        }

        public DothtmlBindingNode BindingNode { get; set; }

        public IReadOnlyList<DothtmlToken> ValueTokens { get; set; }

        public override IEnumerable<DothtmlNode> EnumerateChildNodes()
        {
            if(BindingNode != null)
            {
                yield return BindingNode;
            }
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
            return base.EnumerateNodes().Concat(EnumerateChildNodes().SelectMany(n=> n.EnumerateNodes()));
        }
    }
}
