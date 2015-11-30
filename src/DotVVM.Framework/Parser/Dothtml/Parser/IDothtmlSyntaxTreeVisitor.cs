using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Parser.Dothtml.Parser
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
