#nullable enable
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DotVVM.Framework.Compilation.Parser.Dothtml.Tokenizer;

namespace DotVVM.Framework.Compilation.Parser.Dothtml.Parser
{
    [DebuggerDisplay("{debuggerDisplay,nq}")]
    public class DothtmlElementNode : DothtmlNodeWithContent
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
            var result = new List<DothtmlNode>();

            if (TagPrefixNode != null)
            {
                result.Add(TagPrefixNode);
            }

            result.Add(TagNameNode);
            result.AddRange(Attributes);
            if (InnerComments != null) result.AddRange(InnerComments);
            result.AddRange(Content);
            if (CorrespondingEndTag != null) result.Add(CorrespondingEndTag);

            return result;
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
