using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DotVVM.Framework.Compilation.Parser.Dothtml.Tokenizer;

namespace DotVVM.Framework.Compilation.Parser.Dothtml.Parser
{
    [DebuggerDisplay("{debuggerDisplay,nq}")]
    public sealed class DothtmlElementNode : DothtmlNodeWithContent
    {
        #region debugger display
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string debuggerDisplay
        {
            get
            {
                return "<" + (IsClosingTag ? "/" : "") + FullTagName + (Attributes.Any() ? " ..." : "") + (IsSelfClosingTag ? " /" : "") + ">";
            }
        }
        #endregion

        public string TagName => TagNameNode.Text;
        public string? TagPrefix => TagPrefixNode?.Text;
        public string FullTagName
        {
            get { return string.IsNullOrEmpty(TagPrefix) ? TagName : (TagPrefix + ":" + TagName); }
        }

        public bool IsClosingTag { get; set; }

        public bool IsSelfClosingTag { get; set; }

        public DothtmlNameNode? TagPrefixNode { get; set; }
        public DothtmlNameNode TagNameNode { get; set; } = null!;
        public List<DothtmlAttributeNode> Attributes { get; set; } = new List<DothtmlAttributeNode>();
        public List<DotHtmlCommentNode>? InnerComments { get; set; }

        public DothtmlToken? PrefixSeparator { get; set; }
        public List<DothtmlToken>? AttributeSeparators { get; set; }
        public DothtmlElementNode? CorrespondingEndTag { get; internal set; }

        public override IEnumerable<DothtmlNode> EnumerateChildNodes()
        {
            var result = new List<DothtmlNode>(
                capacity:
                    (InnerComments?.Count ?? 0) + 
                    (CorrespondingEndTag is null ? 1 : 0) +
                    (TagPrefixNode is null ? 1 : 0) +
                    1 +
                    Attributes.Count +
                    Content.Count
            );

            if (TagPrefixNode != null)
                result.Add(TagPrefixNode);

            result.Add(TagNameNode);
            foreach (var a in Attributes)
                result.Add(a);
            if (InnerComments != null)
                foreach (var c in InnerComments)
                    result.Add(c);
            foreach (var c in Content)
                result.Add(c);
            if (CorrespondingEndTag != null) result.Add(CorrespondingEndTag);

            return result;
        }

        public override void Accept(IDothtmlSyntaxTreeVisitor visitor)
        {
            visitor.Visit(this);

            TagPrefixNode?.AcceptIfCondition(visitor);

            TagNameNode.AcceptIfCondition(visitor);
            foreach (var a in Attributes)
                a.AcceptIfCondition(visitor);
            if (InnerComments != null)
                foreach (var c in InnerComments)
                    c.AcceptIfCondition(visitor);
            foreach (var c in Content)
                c.AcceptIfCondition(visitor);
            CorrespondingEndTag?.AcceptIfCondition(visitor);
        }
        internal new void AcceptIfCondition(IDothtmlSyntaxTreeVisitor visitor)
        {
            if (visitor.Condition(this))
                this.Accept(visitor);
        }
        public override IEnumerable<DothtmlNode> EnumerateNodes()
        {
            return base.EnumerateNodes().Concat(EnumerateChildNodes().SelectMany(node => node.EnumerateNodes()));
        }
    }
}
