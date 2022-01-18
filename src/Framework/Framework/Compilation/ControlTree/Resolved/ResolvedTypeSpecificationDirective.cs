using DotVVM.Framework.Compilation.Parser.Binding.Parser;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;

namespace DotVVM.Framework.Compilation.ControlTree.Resolved
{
    public abstract class ResolvedTypeSpecificationDirective : ResolvedDirective, IAbstractTypeSpecificationDirective
    {
        public BindingParserNode NameSyntax { get; }
        public ResolvedTypeDescriptor? ResolvedType { get; }

        ITypeDescriptor? IAbstractTypeSpecificationDirective.ResolvedType => ResolvedType;

        public ResolvedTypeSpecificationDirective(DirectiveCompilationService compilationService, DothtmlDirectiveNode dothtmlNode, BindingParserNode nameSyntax)
            : base(dothtmlNode)
        {
            NameSyntax = nameSyntax;
            ResolvedType = compilationService.ResolveType(dothtmlNode, nameSyntax);
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
