using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.ResourceManagement;
using System.Collections.Immutable;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Configuration;
using System;
using System.Linq;

namespace DotVVM.Framework.Compilation.Directives
{
    using DirectiveDictionary = ImmutableDictionary<string, ImmutableList<DothtmlDirectiveNode>>;

    public class MarkupDirectiveCompilerPipeline : MarkupDirectiveCompilerPipelineBase
    {
        private readonly IAbstractTreeBuilder treeBuilder;
        private readonly DotvvmResourceRepository resourceRepository;
        private readonly DotvvmConfiguration configuration;

        public MarkupDirectiveCompilerPipeline(IAbstractTreeBuilder treeBuilder, DotvvmConfiguration configuration) : base()
        {
            this.treeBuilder = treeBuilder;
            this.resourceRepository = configuration.Resources;
            this.configuration = configuration;
        }

        protected override bool IsMarkupControl(string fileName)
        {
            if (fileName.EndsWith(".dotcontrol", StringComparison.OrdinalIgnoreCase))
                return true;
            return configuration.Markup.Controls.Any(c => c.Src == fileName);
        }

        protected override DefaultDirectiveResolver CreateDefaultResolver(DirectiveDictionary directivesByName)
            => new(directivesByName, treeBuilder);

        protected override PropertyDeclarationDirectiveCompiler CreatePropertyDirectiveCompiler(DirectiveDictionary directivesByName, bool isMarkupControl, ImmutableList<NamespaceImport> imports, ITypeDescriptor baseType)
            => new ResolvedPropertyDeclarationDirectiveCompiler (directivesByName, treeBuilder, isMarkupControl, baseType, imports);

        protected override ViewModuleDirectiveCompiler CreateViewModuleDirectiveCompiler(DirectiveDictionary directivesByName, ITypeDescriptor baseType)
            => new(
        directivesByName,
                        treeBuilder,
                        !baseType.IsEqualTo(ResolvedTypeDescriptor.Create(typeof(DotvvmView))),
                        resourceRepository);

        protected override BaseTypeDirectiveCompiler CreateBaseTypeCompiler(bool isMarkupControl, DirectiveDictionary directivesByName, ImmutableList<NamespaceImport> imports)
            => new BaseTypeDirectiveCompiler (directivesByName, treeBuilder, isMarkupControl, imports);
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
