using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Runtime.Compilation;
using DotVVM.Framework.Runtime.Compilation.ResolvedControlTree;

namespace DotVVM.Framework.Binding
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class CommandPostbackJsCompile: CompileJavascriptAttribute
    {
        public override string CompileToJs(ResolvedBinding binding, CompiledBindingExpression expression)
        {
            return $"dotvvm.postbackScript('{ expression.Id }')";
        }
    }
}
