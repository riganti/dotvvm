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
        public List<DothtmlAttributeNode> Attributes { get; set; }

        public DothtmlToken PrefixSeparator { get; set; }

        public List<DothtmlToken> AttributeSeparators { get; set; }

        public DothtmlElementNode CorrespondingEndTag { get; internal set; }

        public DothtmlElementNode()
        {
            Attributes = new List<DothtmlAttributeNode>();
        }

        public override IEnumerable<DothtmlNode> EnumerateNodes()
        {
            var enumaration = base.EnumerateNodes();

            if(TagPrefixNode != null)
            {
                enumaration = enumaration.Concat(TagPrefixNode.EnumerateNodes() );
            }

            return enumaration.Concat(TagNameNode.EnumerateNodes()).Concat(Attributes.SelectMany(a => a.EnumerateNodes()));
        }
    }
}