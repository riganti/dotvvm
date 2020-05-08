using DotVVM.Compiler.Compilation;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;

namespace DotVVM.Compiler.Resolving
{
    class OfflineCompilationControlResolver : DefaultControlResolver
    {
        private readonly DotvvmConfiguration config;
        private readonly IMarkupFileLoader markupFileLoader;
        private ViewStaticCompiler compiler;

        public OfflineCompilationControlResolver(DotvvmConfiguration config, IControlBuilderFactory controlBuilderFactory, IMarkupFileLoader markupFileLoader, ViewStaticCompiler compiler)
            : base(config.Markup, controlBuilderFactory)
        {
            this.config = config;
            this.markupFileLoader = markupFileLoader;
            this.compiler = compiler;
        }

        protected override IControlType FindMarkupControl(string fileName)
        {
            var file = markupFileLoader.GetMarkup(config, fileName);
            var cr = compiler.CompileFile(file);
            // the control builder type is not used anywhere...
            return new ControlType(cr.ControlType, fileName, cr.DataContextType);
        }
    }
}
