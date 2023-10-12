using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Compilation.ControlTree;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;
using DirectiveDictionary = System.Collections.Generic.Dictionary<string, System.Collections.Generic.IReadOnlyList<DotVVM.Framework.Compilation.Parser.Dothtml.Parser.DothtmlDirectiveNode>>;

namespace DotVVM.Framework.Compilation.Directives
{
    public abstract class MarkupDirectiveCompilerPipelineBase : IMarkupDirectiveCompilerPipeline
    {
        public MarkupPageMetadata Compile(DothtmlRootNode dothtmlRoot, string fileName)
        {
            var directivesByName = dothtmlRoot.Directives
                .GroupBy(d => d.Name, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(d => d.Key, d => (IReadOnlyList<DothtmlDirectiveNode>)d.ToList(), StringComparer.OrdinalIgnoreCase);

            var resolvedDirectives = new Dictionary<string, IReadOnlyList<IAbstractDirective>>();

            var importCompiler = CreateImportCompiler(directivesByName);
            var importResult = importCompiler.Compile();
            var imports = importResult.Artefact;
            resolvedDirectives.AddIfAny(importCompiler.DirectiveName, importResult.Directives);

            var viewModelDirectiveCompiler = CreateViewModelDirectiveCompiler(fileName, directivesByName, imports);
            var viewModelTypeResult = viewModelDirectiveCompiler.Compile();
            var viewModelType = viewModelTypeResult.Artefact;
            if (!string.IsNullOrEmpty(viewModelType.Error)) { dothtmlRoot.AddError(viewModelType.Error!); }
            resolvedDirectives.AddIfAny(viewModelDirectiveCompiler.DirectiveName, viewModelTypeResult.Directives);

            var masterPageDirectiveCompiler = CreateMasterPageDirectiveCompiler(directivesByName);
            var masterPageDirectiveResult = masterPageDirectiveCompiler.Compile();
            var masterPage = masterPageDirectiveResult.Artefact;
            resolvedDirectives.AddIfAny(masterPageDirectiveCompiler.DirectiveName, masterPageDirectiveResult.Directives);

            var serviceCompiler = CreateServiceCompiler(directivesByName, imports);
            var injectedServicesResult = serviceCompiler.Compile();
            resolvedDirectives.AddIfAny(serviceCompiler.DirectiveName, injectedServicesResult.Directives);

            var baseTypeCompiler = CreateBaseTypeCompiler(fileName, directivesByName, imports);
            var baseTypeResult = baseTypeCompiler.Compile();
            var baseType = baseTypeResult.Artefact;
            resolvedDirectives.AddIfAny(baseTypeCompiler.DirectiveName, baseTypeResult.Directives);

            var viewModuleDirectiveCompiler = CreateViewModuleDirectiveCompiler(directivesByName, baseType);
            var viewModuleResult = viewModuleDirectiveCompiler.Compile();
            resolvedDirectives.AddIfAny(viewModuleDirectiveCompiler.DirectiveName, viewModuleResult.Directives);

            var propertyDirectiveCompiler = CreatePropertyDirectiveCompiler(directivesByName, imports, baseType);
            var propertyResult = propertyDirectiveCompiler.Compile();
            resolvedDirectives.AddIfAny(propertyDirectiveCompiler.DirectiveName, propertyResult.Directives);

            var defaultResolver = CreateDefaultResolver(directivesByName);

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

        protected abstract DefaultDirectiveResolver CreateDefaultResolver(DirectiveDictionary directivesByName);
        protected abstract PropertyDeclarationDirectiveCompiler CreatePropertyDirectiveCompiler(DirectiveDictionary directivesByName, ImmutableList<NamespaceImport> imports, ITypeDescriptor baseType);
        protected abstract ViewModuleDirectiveCompiler CreateViewModuleDirectiveCompiler(DirectiveDictionary directivesByName, ITypeDescriptor baseType);
        protected abstract MasterPageDirectiveCompiler CreateMasterPageDirectiveCompiler(DirectiveDictionary directivesByName);
        protected abstract ServiceDirectiveCompiler CreateServiceCompiler(DirectiveDictionary directivesByName, ImmutableList<NamespaceImport> imports);
        protected abstract BaseTypeDirectiveCompiler CreateBaseTypeCompiler(string fileName, DirectiveDictionary directivesByName, ImmutableList<NamespaceImport> imports);
        protected abstract ImportDirectiveCompiler CreateImportCompiler(DirectiveDictionary directivesByName);
        protected abstract ViewModelDirectiveCompiler CreateViewModelDirectiveCompiler(string fileName, DirectiveDictionary directivesByName, ImmutableList<NamespaceImport> imports);
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
