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

        public string TagName { get; set; }

        public List<DothtmlAttributeNode> Attributes { get; set; }

        public bool IsClosingTag { get; set; }

        public bool IsSelfClosingTag { get; set; }

        public string TagPrefix { get; set; }

        public string FullTagName 
        {
            get { return string.IsNullOrEmpty(TagPrefix) ? TagName : (TagPrefix + ":" + TagName); }
        }

        public DothtmlElementNode ParentElement { get; set; }

        public DothtmlToken TagPrefixToken { get; set; }

        public DothtmlToken TagNameToken { get; set; }

        public DothtmlElementNode CorrespondingEndTag { get; internal set; }

        public DothtmlElementNode()
        {
            Attributes = new List<DothtmlAttributeNode>();
        }

        public override IEnumerable<DothtmlNode> EnumerateNodes()
        {
            return base.EnumerateNodes().Concat(Attributes.SelectMany(a => a.EnumerateNodes()));
        }

        public override void AddHierarchyByPosition(IList<DothtmlNode> hierarchy, int position)
        {
            var attr = Attributes.FirstOrDefault(a => a.StartPosition <= position && a.StartPosition + a.Length > position);
            if (attr != null)
            {
                hierarchy.Add(this);
                attr.AddHierarchyByPosition(hierarchy, position);
            }
            else base.AddHierarchyByPosition(hierarchy, position);
        }
    }
}