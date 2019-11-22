#nullable enable
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DotVVM.Framework.Compilation.Parser.Dothtml.Tokenizer;

namespace DotVVM.Framework.Compilation.Parser.Dothtml.Parser
{
    [DebuggerDisplay("{debuggerDisplay,nq}{ValueNode}")]
    public class DothtmlAttributeNode : DothtmlNode
    {
        #region debugger display
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string debuggerDisplay
        {
            get
            {
                return (string.IsNullOrWhiteSpace(AttributePrefix) ? "" : AttributePrefix + ":") + AttributeName
                    + (ValueNode == null ? "" : "=");
            }
        }
        #endregion
        public string? AttributePrefix => AttributePrefixNode?.Text;

        public string AttributeName => AttributeNameNode.Text;

        public DothtmlValueNode? ValueNode { get; set; }

        public DothtmlNameNode? AttributePrefixNode { get; set; }

        public DothtmlNameNode AttributeNameNode { get; set; }

        public DothtmlToken? PrefixSeparatorToken { get; set; }
        public DothtmlToken? ValueSeparatorToken { get; set; }
        public IEnumerable<DothtmlToken>? ValueStartTokens { get; set; }
        public IEnumerable<DothtmlToken>? ValueEndTokens { get; set; }

        public DothtmlAttributeNode(DothtmlNameNode attributeNameNode)
        {
            this.AttributeNameNode = attributeNameNode;
        }

        public override IEnumerable<DothtmlNode> EnumerateChildNodes()
        {
            if (AttributePrefixNode != null)
            {
                yield return AttributePrefixNode;
            }
            yield return AttributeNameNode;
            if (ValueNode != null)
            {
                yield return ValueNode;
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
            return base.EnumerateNodes().Concat(EnumerateChildNodes().SelectMany(node => node.EnumerateNodes()));
        }
    }
}
