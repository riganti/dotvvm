using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Compilation.Parser.Binding.Parser;
using DotVVM.Framework.Compilation.Parser;

namespace DotVVM.Framework.Compilation.ControlTree.Resolved
{
    public class ResolvedImportDirective : ResolvedDirective, IAbstractImportDirective
    {
        public BindingParserNode AliasSyntax { get; }
        public BindingParserNode NameSyntax { get; }

        public ITextRange NameRange { get; }

        public bool HasError => DothtmlNode.HasNodeErrors;
        public Type Type { get; }

        public bool HasAlias => AliasSyntax != null;
        public bool IsNamespace => Type == null && !HasError;
        public bool IsType => Type != null;

        public ResolvedImportDirective(BindingParserNode aliasSyntax, BindingParserNode nameSyntax, Type type)
        {
            AliasSyntax = aliasSyntax;
            NameSyntax = nameSyntax;
            Type = type;
        }
    }
}
