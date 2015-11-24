using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Parser.Dothtml.Tokenizer;
using System.Diagnostics;

namespace DotVVM.Framework.Parser.Dothtml.Parser
{
    [DebuggerDisplay("{debuggerDisplay,nq}{Literal}")]
    public class DothtmlAttributeNode : DothtmlNode
    {
        #region debbuger display
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string debuggerDisplay
        {
            get
            {
                return (string.IsNullOrWhiteSpace(AttributePrefix) ? "" : AttributePrefix + ":") + AttributeName
                    + ( ValueNode == null ? "" : "=" );
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

        public override IEnumerable<DothtmlNode> EnumerateNodes()
        {
            var enumeration = base.EnumerateNodes();
            if (AttributePrefixNode != null)
            {
                enumeration = enumeration.Concat(AttributePrefixNode.EnumerateNodes());
            }
            enumeration = enumeration.Concat(AttributeNameNode.EnumerateNodes());
            if (ValueNode != null)
            {
                enumeration = enumeration.Concat(ValueNode.EnumerateNodes());
            }
            return enumeration;
        }
    }
}