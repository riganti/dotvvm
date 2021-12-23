﻿using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Compilation.Parser.Dothtml.Tokenizer;

namespace DotVVM.Framework.Compilation.Parser.Dothtml.Parser
{
    public class DotHtmlCommentNode : DothtmlNode
    {
        public DotHtmlCommentNode(bool isServerSide, DothtmlToken startToken, DothtmlToken endToken, DothtmlValueTextNode valueNode)
        {
            this.IsServerSide = isServerSide;
            this.StartToken = startToken;
            this.EndToken = endToken;
            this.ValueNode = valueNode;
        }
        public bool IsServerSide { get; set; }
        public string Value => ValueNode.Text;
        public DothtmlToken StartToken { get; set; }
        public DothtmlToken EndToken { get; set; }
        public DothtmlValueTextNode ValueNode { get; set; }

        public override IEnumerable<DothtmlNode> EnumerateChildNodes()
        {
            if (ValueNode != null)
            {
                return new DothtmlNode[] { ValueNode };
            }
            return Enumerable.Empty<DothtmlNode>();
        }

        public override void Accept(IDothtmlSyntaxTreeVisitor visitor)
        {
            visitor.Visit(this);

            ValueNode?.AcceptIfCondition(visitor);
        }

        public override IEnumerable<DothtmlNode> EnumerateNodes()
        {
            return base.EnumerateNodes().Concat(EnumerateChildNodes().SelectMany(node => node.EnumerateNodes()));
        }
    }
}
