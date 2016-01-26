using DotVVM.Framework.Configuration;
using DotVVM.Framework.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Runtime.ControlTree;

namespace DotVVM.Compiler
{
    class OfflineCompilationControlResolver : DefaultControlResolver
    {
        private ViewStaticCompilerCompiler compiler;

        public OfflineCompilationControlResolver(DotvvmConfiguration config, ViewStaticCompilerCompiler compiler)
            : base(config)
        {
            this.compiler = compiler;
        }

        protected override IControlType FindMarkupControl(string file)
        {
            var cr = compiler.CompileFile(file);
            // the control builder type is not used anywhere...
            return new ControlType(cr.ControlType, typeof(object), file, cr.DataContextType);
        }
    }
}
