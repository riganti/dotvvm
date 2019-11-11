#nullable enable
using System.Linq;

namespace DotVVM.Framework.Compilation.Parser.Dothtml.Parser
{
    public static class DothtmlNodeHelper
    {
        public static bool IsNotEmpty(this DothtmlNode node)
        {
            return !(node is DotHtmlCommentNode) &&
                   !(node is DothtmlLiteralNode literalNode && string.IsNullOrWhiteSpace(literalNode.Value));
        }

        public static int GetContentStartPosition(this DothtmlElementNode node)
        {
            return node.Content.FirstOrDefault()?.StartPosition ?? node.EndPosition;
        }

        public static int GetContentEndPosition(this DothtmlElementNode node)
        {
            var lastNode = node.Content.LastOrDefault();

            if(lastNode == null)
            {
                return node.EndPosition;
            }

            if (lastNode is DothtmlElementNode lastElement)
            {
                if (lastElement.IsSelfClosingTag)
                {
                    return lastElement.EndPosition;
                }
                else if (lastElement.CorrespondingEndTag != null)
                {
                    var closingTag = lastElement.CorrespondingEndTag;
                    return closingTag.EndPosition;
                }
                else if (lastElement.Content.Count == 0)
                {
                    return lastElement.EndPosition;
                }
                else
                {
                    return lastElement.GetContentEndPosition();
                }

            }
            else
            {
                return lastNode.EndPosition;
            }
        }
    }
}
