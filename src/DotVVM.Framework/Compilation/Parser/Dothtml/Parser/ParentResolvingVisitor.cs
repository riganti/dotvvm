namespace DotVVM.Framework.Compilation.Parser.Dothtml.Parser
{
    public class ParentResolvingVisitor : IDothtmlSyntaxTreeVisitor
    {
        public bool Condition(DothtmlNode node)
        {
            return true;
        }

        public void Visit(DothtmlAttributeNode attribute)
        {
            ResolveFromParent(attribute);
        }

        public void Visit(DothtmlValueBindingNode bindingValue)
        {
            ResolveFromParent(bindingValue);
        }

        public void Visit(DotHtmlCommentNode comment)
        {
            ResolveFromParent(comment);
        }

        public void Visit(DothtmlDirectiveNode directive)
        {
            ResolveFromParent(directive);
        }

        public void Visit(DothtmlLiteralNode literal)
        {
            ResolveFromParent(literal);
        }

        public void Visit(DothtmlBindingNode binding)
        {
            ResolveFromParent(binding);
        }

        public void Visit(DothtmlNameNode name)
        {
            ResolveFromParent(name);
        }

        public void Visit(DothtmlValueTextNode textValue)
        {
            ResolveFromParent(textValue);
        }

        public void Visit(DothtmlElementNode element)
        {
            ResolveFromParent(element);
        }

        public void Visit(DothtmlRootNode root)
        {
            ResolveFromParent(root);
        }

        private void ResolveFromParent(DothtmlNode parentNode)
        {
            foreach (var childNode in parentNode.EnumerateChildNodes() )
            {
                childNode.ParentNode = parentNode;
            }
        }
    }
}
