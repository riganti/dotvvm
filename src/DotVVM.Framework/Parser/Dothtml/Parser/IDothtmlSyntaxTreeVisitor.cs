using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Parser.Dothtml.Parser
{
    public interface IDothtmlSyntaxTreeVisitor
    {
        void Visit(DothtmlAttributeNode attribute);
        void Visit(DothtmlBindingNode binding);
        void Visit(DothtmlDirectiveNode directive);
        void Visit(DothtmlElementNode element);
        void Visit(DothtmlLiteralNode literal);
    }
}
