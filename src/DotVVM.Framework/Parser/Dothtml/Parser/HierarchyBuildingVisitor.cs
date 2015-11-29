using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Parser.Dothtml.Parser
{
    public class HierarchyBuildingVisitor : IDothtmlSyntaxTreeVisitor
    {
        public int CursorPosition { get; set; }

        public DothtmlNode LastFoundNode { get; set; }


        public bool Condition(DothtmlNode node)
        {
            return node.StartPosition <= CursorPosition && CursorPosition < node.StartPosition + node.Length; 
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
            DothtmlNode currentNode = LastFoundNode;
            while (currentNode != null)
            {
                yield return currentNode;
                currentNode = currentNode.ParentNode;
            }
        }
    }
}
