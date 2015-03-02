using System;
using System.Collections.Generic;
using System.Linq;

namespace Redwood.Framework.Parser.RwHtml.Parser
{
    public class RwHtmlElementNode : RwHtmlNodeWithContent
    {
        
        public string TagName { get; set; }

        public List<RwHtmlAttributeNode> Attributes { get; set; }

        public bool IsClosingTag { get; set; }

        public bool IsSelfClosingTag { get; set; }

        public string TagPrefix { get; set; }

        public string FullTagName 
        {
            get { return string.IsNullOrEmpty(TagPrefix) ? TagName : (TagPrefix + ":" + TagName); }
        }


        public RwHtmlElementNode()
        {
            Attributes = new List<RwHtmlAttributeNode>();
        }

        public override IEnumerable<RwHtmlNode> EnumerateNodes()
        {
            return base.EnumerateNodes().Concat(Attributes);
        }
    }
}