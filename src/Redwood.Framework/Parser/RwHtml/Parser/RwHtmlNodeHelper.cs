using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redwood.Framework.Parser.RwHtml.Parser
{
    public static class RwHtmlNodeHelper
    {
        public static bool IsNotEmpty(this RwHtmlNode node)
        {
            return !(node is RwHtmlLiteralNode && string.IsNullOrWhiteSpace(((RwHtmlLiteralNode)node).Value));
        }
    }
}
