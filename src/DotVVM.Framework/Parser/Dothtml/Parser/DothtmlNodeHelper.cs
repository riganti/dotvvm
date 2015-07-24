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
            return !(node is DothtmlLiteralNode && string.IsNullOrWhiteSpace(((DothtmlLiteralNode)node).Value));
        }
    }
}
