using DotVVM.Framework.Compilation.Parser.Binding.Parser;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;

namespace DotVVM.Framework.Compilation.ControlTree.Resolved
{
    public abstract class ResolvedTypeSpecificationDirective : ResolvedDirective, IAbstractTypeSpecificationDirective
    {
        public BindingParserNode NameSyntax { get; }
        public ResolvedTypeDescriptor? ResolvedType { get; }

        ITypeDescriptor IAbstractTypeSpecificationDirective.ResolvedType { get; }

        public ResolvedTypeSpecificationDirective(DirectiveCompilationService compilationService, DothtmlDirectiveNode dothtmlNode, BindingParserNode nameSyntax)
        {
            NameSyntax = nameSyntax;
            ResolvedType = compilationService.ResolveType(dothtmlNode, nameSyntax);
            DothtmlNode = dothtmlNode;
        }
    }
    public sealed class ResolvedViewModelDirective : ResolvedTypeSpecificationDirective, IAbstractViewModelDirective
    {
        public ResolvedViewModelDirective(DirectiveCompilationService directiveCompilationService, DothtmlDirectiveNode dothtmlNode, BindingParserNode nameSyntax)
            : base(directiveCompilationService, dothtmlNode, nameSyntax) {

        }
    }
    public sealed class ResolvedBaseTypeDirective : ResolvedTypeSpecificationDirective, IAbstractBaseTypeDirective
    {
        public ResolvedBaseTypeDirective(DirectiveCompilationService directiveCompilationService, DothtmlDirectiveNode dothtmlNode, BindingParserNode nameSyntax)
            : base(directiveCompilationService, dothtmlNode, nameSyntax) {

        }
    }
}
