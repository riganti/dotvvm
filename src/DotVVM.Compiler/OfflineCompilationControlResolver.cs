using DotVVM.Framework.Configuration;
using DotVVM.Framework.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.ControlTree;

namespace DotVVM.Compiler
{
    class OfflineCompilationControlResolver : DefaultControlResolver
    {
        private ViewStaticCompiler compiler;

        public OfflineCompilationControlResolver(DotvvmMarkupConfiguration config, IControlBuilderFactory controlBuilderFactory, ViewStaticCompiler compiler)
            : base(config, controlBuilderFactory)
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
