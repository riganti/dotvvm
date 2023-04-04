using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.ResourceManagement;
using System;
using System.Linq;
using System.Collections.Generic;

namespace DotVVM.Framework.Compilation.Directives
{
    public class MarkupDirectiveCompilerPipeline : IMarkupDirectiveCompilerPipeline
    {
        private readonly IAbstractTreeBuilder treeBuilder;
        private readonly DotvvmResourceRepository resourceRepository;

        public MarkupDirectiveCompilerPipeline(IAbstractTreeBuilder treeBuilder, DotvvmResourceRepository resourceRepository)
        {
            this.treeBuilder = treeBuilder;
            this.resourceRepository = resourceRepository;
        }

        public MarkupPageMetadata Compile(DothtmlRootNode dothtmlRoot, string fileName)
        {
            var directivesByName = dothtmlRoot.Directives
                .GroupBy(d => d.Name, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(d => d.Key, d => (IReadOnlyList<DothtmlDirectiveNode>)d.ToList(), StringComparer.OrdinalIgnoreCase);

            var resolvedDirectives = new Dictionary<string, IReadOnlyList<IAbstractDirective>>();

            var importCompiler = new ImportDirectiveCompiler(directivesByName, treeBuilder);
            var importResult = importCompiler.Compile();
            var imports = importResult.Artefact;
            resolvedDirectives.AddIfAny(importCompiler.DirectiveName, importResult.Directives);

            var viewModelDirectiveCompiler = new ViewModelDirectiveCompiler(directivesByName, treeBuilder, fileName, imports);
            var viewModelTypeResult = viewModelDirectiveCompiler.Compile();
            var viewModelType = viewModelTypeResult.Artefact;
            if (!string.IsNullOrEmpty(viewModelType.Error)) { dothtmlRoot.AddError(viewModelType.Error!); }
            resolvedDirectives.AddIfAny(viewModelDirectiveCompiler.DirectiveName, viewModelTypeResult.Directives);

            var masterPageDirectiveCompiler = new MasterPageDirectiveCompiler(directivesByName, treeBuilder);
            var masterPageDirectiveResult = masterPageDirectiveCompiler.Compile();
            var masterPage = masterPageDirectiveResult.Artefact;
            resolvedDirectives.AddIfAny(masterPageDirectiveCompiler.DirectiveName, masterPageDirectiveResult.Directives);

            var serviceCompiler = new ServiceDirectiveCompiler(directivesByName, treeBuilder, imports);
            var injectedServicesResult = serviceCompiler.Compile();
            resolvedDirectives.AddIfAny(serviceCompiler.DirectiveName, injectedServicesResult.Directives);

            var baseTypeCompiler = new BaseTypeDirectiveCompiler(directivesByName, treeBuilder, fileName, imports);
            var baseTypeResult = baseTypeCompiler.Compile();
            var baseType = baseTypeResult.Artefact;
            resolvedDirectives.AddIfAny(baseTypeCompiler.DirectiveName, baseTypeResult.Directives);

            var viewModuleDirectiveCompiler = new ViewModuleDirectiveCompiler(
                directivesByName,
                treeBuilder,
                !baseType.IsEqualTo(ResolvedTypeDescriptor.Create(typeof(DotvvmView))),
                resourceRepository);
            var viewModuleResult = viewModuleDirectiveCompiler.Compile();
            resolvedDirectives.AddIfAny(viewModuleDirectiveCompiler.DirectiveName, viewModuleResult.Directives);

            var propertyDirectiveCompiler = new PropertyDeclarationDirectiveCompiler(directivesByName, treeBuilder, baseType, imports);
            var propertyResult = propertyDirectiveCompiler.Compile();
            resolvedDirectives.AddIfAny(propertyDirectiveCompiler.DirectiveName, propertyResult.Directives);

            var defaultResolver = new DefaultDirectiveResolver(directivesByName, treeBuilder);

            foreach (var directiveGroup in directivesByName)
            {
                if (!resolvedDirectives.ContainsKey(directiveGroup.Key))
                {
                    resolvedDirectives.Add(directiveGroup.Key, defaultResolver.ResolveAll(directiveGroup.Key));
                }
            }

            return new MarkupPageMetadata(
                resolvedDirectives,
                imports,
                masterPageDirectiveResult.Artefact,
                injectedServicesResult.Artefact,
                baseType,
                viewModelType.TypeDescriptor,
                viewModuleResult.Artefact,
                propertyResult.Artefact);
        }
    }

    internal static class DirectivesExtensions
    {
        internal static void AddIfAny(this Dictionary<string, IReadOnlyList<IAbstractDirective>> resolvedDirectives, string directiveName, IReadOnlyList<IAbstractDirective> newDirectives)
        {
            if (newDirectives.Any())
            {
                resolvedDirectives.Add(directiveName, newDirectives);
            }
        }
    }

}
