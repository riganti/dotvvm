using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Binding;
using System.Collections.Immutable;
using DotVVM.Framework.Compilation.ViewCompiler;
using System.Collections.Generic;

namespace DotVVM.Framework.Compilation.Directives
{
    public record MarkupPageMetadata(
           IReadOnlyDictionary<string, IReadOnlyList<IAbstractDirective>> Directives,
           ImmutableList<NamespaceImport> Imports,
           IAbstractControlBuilderDescriptor? MasterPage,
           ImmutableList<InjectedServiceExtensionParameter> InjectedServices,
           ITypeDescriptor BaseType,
           ITypeDescriptor? ViewModelType,
           ViewModuleCompilationResult? ViewModuleResult,
           CSharpViewModuleCompilationResult? CSharpViewModuleResult,
           ImmutableList<DotvvmProperty> Properties);
}
