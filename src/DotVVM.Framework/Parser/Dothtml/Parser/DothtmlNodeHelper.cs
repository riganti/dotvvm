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
            return !(node is DotHtmlCommentNode) &&
                   !((node is DothtmlLiteralNode) && string.IsNullOrWhiteSpace((node as DothtmlLiteralNode).Value));
        }

        public static int GetContentStartPosition(this DothtmlElementNode node)
        {
            return node.Content.FirstOrDefault()?.StartPosition ?? (node.StartPosition + node.Length);
        }

        public static int GetContentEndPosition(this DothtmlElementNode node)
        {
            var lastNode = node.Content.LastOrDefault();

            if(lastNode == null)
            {
                return (node.StartPosition + node.Length);
            }

            if (lastNode is DothtmlElementNode)
            {
                var lastElement = lastNode as DothtmlElementNode;

                if (lastElement.IsSelfClosingTag)
                {
                    return lastElement.StartPosition + lastElement.Length;
                }
                else if (lastElement.CorrespondingEndTag != null)
                {
                    var closingTag = lastElement.CorrespondingEndTag;
                    return closingTag.StartPosition + closingTag.Length;
                }
                else if (lastElement.Content.Count == 0)
                {
                    return lastElement.StartPosition + lastElement.Length;
                }
                else
                {
                    return lastElement.GetContentEndPosition();
                }

            }
            else
            {
                return lastNode.StartPosition + lastNode.Length;
            }
        }
    }
}
