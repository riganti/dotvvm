using System;
using System.Collections.Generic;
using System.Linq;

namespace Redwood.Framework.Parser.RwHtml.Parser
{
    public class RwHtmlNodeWithContent : RwHtmlNode
    {

        public List<RwHtmlNode> Content { get; private set; }

        public RwHtmlNodeWithContent()
        {
            Content = new List<RwHtmlNode>();
        }


        public override IEnumerable<RwHtmlNode> EnumerateNodes()
        {
            return base.EnumerateNodes().Concat(Content.SelectMany(n => n.EnumerateNodes()));
        }
    }
}