using System;
using System.Collections.Generic;
using System.Linq;

namespace Redwood.Framework.Parser.RwHtml.Parser
{
    public class RwHtmlRootNode : RwHtmlNodeWithContent
    {
        
        public List<RwHtmlDirectiveNode> Directives { get; private set; }



        public RwHtmlRootNode()
        {
            Directives = new List<RwHtmlDirectiveNode>();
        }

    }
}
