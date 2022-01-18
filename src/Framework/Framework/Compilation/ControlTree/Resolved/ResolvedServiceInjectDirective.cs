
using System;
using DotVVM.Framework.Compilation.Parser.Binding.Parser;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;

namespace DotVVM.Framework.Compilation.ControlTree.Resolved
{
    public class ResolvedServiceInjectDirective : ResolvedDirective, IAbstractServiceInjectDirective
    {
        public SimpleNameBindingParserNode NameSyntax { get; }
        public BindingParserNode TypeSyntax { get; }
        public ResolvedTypeDescriptor? Type { get; }

        ITypeDescriptor? IAbstractServiceInjectDirective.Type => Type;

        public ResolvedServiceInjectDirective(
            DirectiveCompilationService directiveService,
            DothtmlDirectiveNode node,
            SimpleNameBindingParserNode nameSyntax,
            BindingParserNode typeSyntax)
            : base(node)
        {
            NameSyntax = nameSyntax;
            TypeSyntax = typeSyntax;
            Type = directiveService.ResolveType(node, typeSyntax);

        }
    }
}
