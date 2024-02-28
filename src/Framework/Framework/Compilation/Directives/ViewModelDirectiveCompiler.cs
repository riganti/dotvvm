using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.Parser;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Controls;
using System.Linq;
using System.Collections.Immutable;

namespace DotVVM.Framework.Compilation.Directives
{
    using DirectiveDictionary = System.Collections.Immutable.ImmutableDictionary<string, System.Collections.Immutable.ImmutableList<DotVVM.Framework.Compilation.Parser.Dothtml.Parser.DothtmlDirectiveNode>>;

    public record ViewModelCompilationResult(ITypeDescriptor? TypeDescriptor, string? Error);
    public class ViewModelDirectiveCompiler : DirectiveCompiler<IAbstractViewModelDirective, ViewModelCompilationResult>
    {
        private readonly string fileName;
        private readonly ImmutableList<NamespaceImport> imports;

        public override string DirectiveName => ParserConstants.ViewModelDirectiveName;

        public ViewModelDirectiveCompiler(DirectiveDictionary directiveNodesByName, IAbstractTreeBuilder treeBuilder, string fileName, ImmutableList<NamespaceImport> imports)
            : base(directiveNodesByName, treeBuilder)
        {
            this.fileName = fileName;
            this.imports = imports;
        }

        protected override ViewModelCompilationResult CreateArtefact(ImmutableList<IAbstractViewModelDirective> resolvedDirectives)
        {
            if (resolvedDirectives.Count == 0)
            {
                return new ViewModelCompilationResult(null, $"The @viewModel directive is missing in the page '{fileName}'!");
            }

            var viewModelDirective = resolvedDirectives.First();
            if (viewModelDirective?.ResolvedType is object && viewModelDirective.ResolvedType.IsAssignableTo(new ResolvedTypeDescriptor(typeof(DotvvmBindableObject))))
            {
                return new ViewModelCompilationResult(null, $"The @viewModel directive cannot contain type that derives from DotvvmBindableObject!");
            }

            return new ViewModelCompilationResult(viewModelDirective?.ResolvedType, null);
        }

        protected override IAbstractViewModelDirective Resolve(DothtmlDirectiveNode directive)
            => TreeBuilder.BuildViewModelDirective(directive, ParseDirective(directive, p => p.ReadDirectiveTypeName()), imports);
    }

}
