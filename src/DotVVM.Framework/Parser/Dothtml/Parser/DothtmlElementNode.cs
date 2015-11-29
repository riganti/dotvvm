using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Parser.Dothtml.Tokenizer;
using System.Diagnostics;

namespace DotVVM.Framework.Parser.Dothtml.Parser
{
    [DebuggerDisplay("{debuggerDisplay,nq}")]
    public class DothtmlElementNode : DothtmlNodeWithContent
    {
        #region debbuger display
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string debuggerDisplay
        {
            get
            {
                return "<" + (IsClosingTag ? "/" : "") + FullTagName + ( Attributes.Any() ? " ..." : "" ) + (IsSelfClosingTag ? " /" : "") +  ">";
            }
        }
        #endregion

        public string TagName => TagNameNode.Text;
        public string TagPrefix => TagPrefixNode?.Text;
        public string FullTagName
        {
            get { return string.IsNullOrEmpty(TagPrefix) ? TagName : (TagPrefix + ":" + TagName); }
        }

        public bool IsClosingTag { get; set; }

        public bool IsSelfClosingTag { get; set; } 

        public DothtmlNameNode TagPrefixNode { get; set; }
        public DothtmlNameNode TagNameNode { get; set; }
        public List<DothtmlAttributeNode> Attributes { get; set; } = new List<DothtmlAttributeNode>();
        public List<DotHtmlCommentNode> InnerComments { get; set; } = new List<DotHtmlCommentNode>();

        public DothtmlToken PrefixSeparator { get; set; }
        public List<DothtmlToken> AttributeSeparators { get; set; } = new List<DothtmlToken>();
        public DothtmlElementNode CorrespondingEndTag { get; internal set; }

        public override IEnumerable<DothtmlNode> EnumerateChildNodes()
        {
            var enumetarion = new List<DothtmlNode>();

            if (TagPrefixNode != null)
            {
                enumetarion.Add(TagPrefixNode);
            }
            enumetarion.Add(TagNameNode);
            enumetarion.AddRange(Attributes);
            enumetarion.AddRange(InnerComments);
            enumetarion.AddRange(base.EnumerateChildNodes());

            return enumetarion;
        }

        public override void Accept(IDothtmlSyntaxTreeVisitor visitor)
        {
            visitor.Visit(this);

            foreach (var node in EnumerateChildNodes() )
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