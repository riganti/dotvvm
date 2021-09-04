using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Framework.Compilation.Parser.Dothtml.Parser
{
    public class HierarchyBuildingVisitor : IDothtmlSyntaxTreeVisitor
    {
        public int CursorPosition { get; set; }

        public DothtmlNode? LastFoundNode { get; set; }


        public bool Condition(DothtmlNode node)
        {
            int tagEnd = node.EndPosition;

            if(node is DothtmlElementNode element)
            {
                tagEnd = element.GetContentEndPosition() + (element.CorrespondingEndTag?.Length ?? 0);
            }

            //This is also enough for RootNode
            return node.StartPosition <= CursorPosition && (CursorPosition < tagEnd || (node.Tokens.Last().Length == 0 && node.Tokens.Last().StartPosition == tagEnd)); 
        }

        public void Visit(DothtmlAttributeNode attribute)
        {
            LastFoundNode = attribute;
        }

        public void Visit(DothtmlValueBindingNode bindingValue)
        {
            LastFoundNode = bindingValue;
        }

        public void Visit(DotHtmlCommentNode comment)
        {
            LastFoundNode = comment;
        }

        public void Visit(DothtmlDirectiveNode directive)
        {
            LastFoundNode = directive;
        }

        public void Visit(DothtmlLiteralNode literal)
        {
            LastFoundNode = literal;
        }

        public void Visit(DothtmlBindingNode binding)
        {
            LastFoundNode = binding;
        }

        public void Visit(DothtmlNameNode name)
        {
            LastFoundNode = name;
        }

        public void Visit(DothtmlValueTextNode textValue)
        {
            LastFoundNode = textValue;
        }

        public void Visit(DothtmlElementNode element)
        {
            LastFoundNode = element;
        }

        public void Visit(DothtmlRootNode root)
        {
            LastFoundNode = root;
        }

        public List<DothtmlNode> GetHierarchy()
        {
            return GetHierarchyPrivate().ToList();
        }

        private IEnumerable<DothtmlNode> GetHierarchyPrivate()
        {
            var currentNode = LastFoundNode;
            while (currentNode != null)
            {
                yield return currentNode;
                currentNode = currentNode.ParentNode;
            }
        }
    }
}
