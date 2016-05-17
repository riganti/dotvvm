namespace DotVVM.Framework.Compilation.Parser.Dothtml.Parser
{
    public interface IDothtmlSyntaxTreeVisitor
    {
        void Visit(DothtmlRootNode root);
        void Visit(DothtmlElementNode element);
        void Visit(DothtmlAttributeNode attribute);
        void Visit(DothtmlValueTextNode textValue);
        void Visit(DothtmlValueBindingNode bindingValue);
        void Visit(DothtmlNameNode name);
        void Visit(DotHtmlCommentNode comment);
        void Visit(DothtmlBindingNode binding);
        void Visit(DothtmlDirectiveNode directive);
        void Visit(DothtmlLiteralNode literal);
        bool Condition(DothtmlNode node);
    }
}
