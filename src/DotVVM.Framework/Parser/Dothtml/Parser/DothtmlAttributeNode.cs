using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Parser.Dothtml.Tokenizer;
using System.Diagnostics;

namespace DotVVM.Framework.Parser.Dothtml.Parser
{
    [DebuggerDisplay("{debuggerDisplay,nq}{ValueNode}")]
    public class DothtmlAttributeNode : DothtmlNode
    {
        #region debbuger display
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
        public string AttributePrefix => AttributePrefixNode?.Text;

        public string AttributeName => AttributeNameNode.Text;

        public DothtmlValueNode ValueNode { get; set; }

        public DothtmlNameNode AttributePrefixNode { get; set; }

        public DothtmlNameNode AttributeNameNode { get; set; }

        public DothtmlToken PrefixSeparatorToken { get; set; }
        public DothtmlToken ValueSeparatorToken { get; set; }
        public List<DothtmlToken> ValueStartTokens { get; set; } = new List<DothtmlToken>();
        public List<DothtmlToken> ValueEndTokens { get; set; } = new List<DothtmlToken>();

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
                node.Accept(visitor);
            }
        }

        public override IEnumerable<DothtmlNode> EnumerateNodes()
        {
            return base.EnumerateNodes().Concat(EnumerateChildNodes().SelectMany(node => node.EnumerateNodes()));
        }
    }
}