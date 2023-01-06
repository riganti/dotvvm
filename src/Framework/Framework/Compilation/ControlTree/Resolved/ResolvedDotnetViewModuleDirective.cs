using System.Collections.Immutable;
using DotVVM.Framework.Compilation.Parser.Binding.Parser;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;

namespace DotVVM.Framework.Compilation.ControlTree.Resolved;

/// <summary> Represents the @dotnet directive - import .NET WASM module on the client side </summary>
public class ResolvedDotnetViewModuleDirective : ResolvedDirective, IAbstractDotnetViewModuleDirective
{
    /// <summary>Full .NET type of the module</summary>
    public ITypeDescriptor? ModuleType { get; }
        
    public ResolvedDotnetViewModuleDirective(DirectiveCompilationService directiveCompilationService, DothtmlDirectiveNode node, BindingParserNode typeName, ImmutableList<NamespaceImport> imports)
        : base(node)
    {
        ModuleType = directiveCompilationService.ResolveType(node, typeName, imports);
    }
}
