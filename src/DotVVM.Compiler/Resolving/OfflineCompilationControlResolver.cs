using DotVVM.Compiler.Compilation;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Configuration;

namespace DotVVM.Compiler.Resolving
{
    class OfflineCompilationControlResolver : DefaultControlResolver
    {
        private ViewStaticCompiler compiler;

        public OfflineCompilationControlResolver(DotvvmConfiguration config, IControlBuilderFactory controlBuilderFactory, ViewStaticCompiler compiler, CompiledAssemblyCache compiledAssemblyCache)
            : base(config, controlBuilderFactory, compiledAssemblyCache)
        {
            this.compiler = compiler;
        }

        protected override IControlType FindMarkupControl(string file)
        {
            var cr = compiler.CompileFile(file);
            // the control builder type is not used anywhere...
            return new ControlType(cr.ControlType, file, cr.DataContextType);
        }
    }
}
