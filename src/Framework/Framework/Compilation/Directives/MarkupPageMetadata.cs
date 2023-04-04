using System.Collections.Generic;
using System.Collections.Immutable;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ControlTree;

namespace DotVVM.Framework.Compilation.Directives
{
    public record MarkupPageMetadata(
           IReadOnlyDictionary<string, IReadOnlyList<IAbstractDirective>> Directives,
           ImmutableList<NamespaceImport> Imports,
           IAbstractDirective? MasterPageDirective,
           ImmutableList<InjectedServiceExtensionParameter> InjectedServices,
           ITypeDescriptor BaseType,
           ITypeDescriptor? ViewModelType,
           ViewModuleCompilationResult? ViewModuleResult,
           ImmutableList<DotvvmProperty> Properties);
}
