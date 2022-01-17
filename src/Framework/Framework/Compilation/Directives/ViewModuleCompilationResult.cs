using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Binding;

namespace DotVVM.Framework.Compilation.Directives
{
    public record ViewModuleCompilationResult
    {
        public JsExtensionParameter ExtensionParameter { get; }
        public ViewModuleReferenceInfo Reference { get; }

        public ViewModuleCompilationResult(JsExtensionParameter extensionParameter, ViewModuleReferenceInfo viewModuleReferenceInfo)
        {
            ExtensionParameter = extensionParameter;
            Reference = viewModuleReferenceInfo;
        }
    }

}
