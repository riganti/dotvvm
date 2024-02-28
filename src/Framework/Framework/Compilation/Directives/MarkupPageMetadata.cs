using System.Collections.Immutable;
using DotVVM.Framework.Compilation.ControlTree;

namespace DotVVM.Framework.Compilation.Directives
{
    public record MarkupPageMetadata(
           ImmutableDictionary<string, ImmutableList<IAbstractDirective>> Directives,
           ImmutableList<NamespaceImport> Imports,
           IAbstractDirective? MasterPageDirective,
           ImmutableList<InjectedServiceExtensionParameter> InjectedServices,
           ITypeDescriptor BaseType,
           ITypeDescriptor? ViewModelType,
           ViewModuleCompilationResult? ViewModuleResult,
           ImmutableList<IPropertyDescriptor> Properties);
}
