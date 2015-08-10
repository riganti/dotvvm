using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Parser.Dothtml.Parser
{
    public static class DothtmlNodeHelper
    {
        public static bool IsNotEmpty(this DothtmlNode node)
        {
            if (node is DothtmlLiteralNode)
            {
                var literalNode = (DothtmlLiteralNode) node;
                return !literalNode.IsComment && !string.IsNullOrWhiteSpace(literalNode.Value);
            }
            return true;
        }
    }
}
