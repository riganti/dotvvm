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
        private readonly IControlBuilderFactory controlBuilderFactory;
        private readonly DotvvmResourceRepository resourceRepository;

        public MarkupDirectiveCompilerPipeline(IAbstractTreeBuilder treeBuilder, IControlBuilderFactory controlBuilderFactory, DotvvmResourceRepository resourceRepository)
        {
            this.treeBuilder = treeBuilder;
            this.controlBuilderFactory = controlBuilderFactory;
            this.resourceRepository = resourceRepository;
        }

        public MarkupPageMetadata Compile(DothtmlRootNode dothtmlRoot, string fileName)
        {
            var directivesByName = dothtmlRoot.Directives.GroupBy(d => d.Name, StringComparer.OrdinalIgnoreCase).ToDictionary(d => d.Key, d => (IReadOnlyList<DothtmlDirectiveNode>)d.ToList());

            var resolvedDirectives = new Dictionary<string, IReadOnlyList<IAbstractDirective>>();

            var importCompiler = new ImportDirectiveCompiler(directivesByName, treeBuilder);
            var importResult = importCompiler.Compile();
            resolvedDirectives.AddIfAny(importCompiler.DirectiveName, importResult.Diractives);

            var viewModelDirectiveCompiler = new ViewModelDirectiveCompiler(directivesByName, treeBuilder, fileName);
            var viewModelTypeResult = viewModelDirectiveCompiler.Compile();
            var viewModelType = viewModelTypeResult.Artefact;
            if (!string.IsNullOrEmpty(viewModelType.Error)) { dothtmlRoot.AddError(viewModelType.Error); }
            resolvedDirectives.AddIfAny(viewModelDirectiveCompiler.DirectiveName, viewModelTypeResult.Diractives);

            var masterPageDirectiveCompiler = new MasterPageDirectiveCompiler(directivesByName, treeBuilder, controlBuilderFactory, viewModelType.TypeDescriptor);
            var masterPageDirectiveResult = masterPageDirectiveCompiler.Compile();
            var masterPage = masterPageDirectiveResult.Artefact;
            resolvedDirectives.AddIfAny(masterPageDirectiveCompiler.DirectiveName, masterPageDirectiveResult.Diractives);

            var serviceCompiler = new ServiceDirectiveCompiler(directivesByName, treeBuilder);
            var injectedServicesResult = serviceCompiler.Compile();
            resolvedDirectives.AddIfAny(serviceCompiler.DirectiveName, injectedServicesResult.Diractives);

            var baseTypeCompiler = new BaseTypeDirectiveCompiler(directivesByName, treeBuilder, fileName);
            var baseTypeResult = baseTypeCompiler.Compile();
            var baseType = baseTypeResult.Artefact;
            resolvedDirectives.AddIfAny(baseTypeCompiler.DirectiveName, baseTypeResult.Diractives);

            var viewModuleDirectiveCompiler = new ViewModuleDirectiveCompiler(
                directivesByName,
                treeBuilder,
                masterPage,
                baseType?.IsEqualTo(ResolvedTypeDescriptor.Create(typeof(DotvvmView))) == true,
                resourceRepository);
            var viewModuleResult = viewModuleDirectiveCompiler.Compile();
            resolvedDirectives.AddIfAny(viewModuleDirectiveCompiler.DirectiveName, viewModuleResult.Diractives);

            var propertyDirectiveCompiler = new PropertyDeclarationDirectiveCompiler(directivesByName, treeBuilder, baseType);
            var propertyResult = propertyDirectiveCompiler.Compile();
            resolvedDirectives.AddIfAny(propertyDirectiveCompiler.DirectiveName, propertyResult.Diractives);

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
                importResult.Artefact,
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
