using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Binding;
using System.Collections.Immutable;
using DotVVM.Framework.Compilation.ViewCompiler;
using System.Collections.Generic;

namespace DotVVM.Framework.Compilation.Directives
{
    public record MarkupPageMetadata
    {
        public IReadOnlyDictionary<string, IReadOnlyList<IAbstractDirective>> Directives { get; }
        public ImmutableList<NamespaceImport> Imports { get; }
        public IAbstractControlBuilderDescriptor? MasterPage { get; }
        public ImmutableList<InjectedServiceExtensionParameter> InjectedServices { get; }
        public ITypeDescriptor BaseType { get; }
        public ITypeDescriptor? ViewModelType { get; }
        public ViewModuleCompilationResult? ViewModuleResult { get; }
        public ImmutableList<DotvvmProperty> Properties { get; }

        public MarkupPageMetadata(
           IReadOnlyDictionary<string, IReadOnlyList<IAbstractDirective>> directives,
           ImmutableList<NamespaceImport> imports,
           IAbstractControlBuilderDescriptor? masterPage,
           ImmutableList<InjectedServiceExtensionParameter> injectedServices,
           ITypeDescriptor baseType,
           ITypeDescriptor? viewModelType,
           ViewModuleCompilationResult? viewModuleResult,
           ImmutableList<DotvvmProperty> properties)
        {
            Directives = directives;
            Imports = imports;
            MasterPage = masterPage;
            InjectedServices = injectedServices;
            BaseType = baseType;
            ViewModelType = viewModelType;
            ViewModuleResult = viewModuleResult;
            Properties = properties;
        }
    }

}
