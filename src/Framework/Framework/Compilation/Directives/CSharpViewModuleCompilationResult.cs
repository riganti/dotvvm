using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ControlTree;

namespace DotVVM.Framework.Compilation.Directives;

public record CSharpViewModuleCompilationResult(CSharpExtensionParameter ExtensionParameter, ViewModuleReferenceInfo Reference);
