using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.ResourceManagement;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;
using DirectiveDictionary = System.Collections.Generic.Dictionary<string, System.Collections.Generic.IReadOnlyList<DotVVM.Framework.Compilation.Parser.Dothtml.Parser.DothtmlDirectiveNode>>;

namespace DotVVM.Framework.Compilation.Directives
{
    public class MarkupDirectiveCompilerPipeline : MarkupDirectiveCompilerPipelineBase
    {
        private readonly IAbstractTreeBuilder treeBuilder;
        private readonly DotvvmResourceRepository resourceRepository;

        public MarkupDirectiveCompilerPipeline(IAbstractTreeBuilder treeBuilder, DotvvmResourceRepository resourceRepository) : base()
        {
            this.treeBuilder = treeBuilder;
            this.resourceRepository = resourceRepository;
        }

        protected override DefaultDirectiveResolver CreateDefaultResolver(DirectiveDictionary directivesByName)
            => new(directivesByName, treeBuilder);

        protected override PropertyDeclarationDirectiveCompiler CreatePropertyDirectiveCompiler(DirectiveDictionary directivesByName, ImmutableList<NamespaceImport> imports, ITypeDescriptor baseType)
            => new ResolvedPropertyDeclarationDirectiveCompiler (directivesByName, treeBuilder, baseType, imports);

        protected override ViewModuleDirectiveCompiler CreateViewModuleDirectiveCompiler(DirectiveDictionary directivesByName, ITypeDescriptor baseType)
            => new(
        directivesByName,
                        treeBuilder,
                        !baseType.IsEqualTo(ResolvedTypeDescriptor.Create(typeof(DotvvmView))),
                        resourceRepository);

        protected override BaseTypeDirectiveCompiler CreateBaseTypeCompiler(string fileName, DirectiveDictionary directivesByName, ImmutableList<NamespaceImport> imports)
            => new ResolvedBaseTypeDirectiveCompiler(directivesByName, treeBuilder, fileName, imports);
        protected override ServiceDirectiveCompiler CreateServiceCompiler(DirectiveDictionary directivesByName, ImmutableList<NamespaceImport> imports)
            => new(directivesByName, treeBuilder, imports);
        protected override MasterPageDirectiveCompiler CreateMasterPageDirectiveCompiler(DirectiveDictionary directivesByName)
            => new(directivesByName, treeBuilder);
        protected override ViewModelDirectiveCompiler CreateViewModelDirectiveCompiler( string fileName, DirectiveDictionary directivesByName, ImmutableList<NamespaceImport> imports)
            => new(directivesByName, treeBuilder, fileName, imports);
        protected override ImportDirectiveCompiler CreateImportCompiler(DirectiveDictionary directivesByName)
            => new(directivesByName, treeBuilder);
    }
}
