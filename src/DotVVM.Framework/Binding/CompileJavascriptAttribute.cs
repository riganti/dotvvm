using DotVVM.Framework.Runtime.Compilation;
using DotVVM.Framework.Runtime.Compilation.JavascriptCompilation;
using DotVVM.Framework.Runtime.Compilation.ResolvedControlTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Binding
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class CompileJavascriptAttribute : Attribute
    {
        public virtual string CompileToJs(ResolvedBinding binding, CompiledBindingExpression expression)
        {
            var javascript = JavascriptTranslator.CompileToJavascript(binding.GetExpression(), binding.DataContextTypeStack);

            if (javascript.StartsWith("$data.", StringComparison.Ordinal)) javascript = javascript.Substring("$data.".Length);
            // do not produce try/eval on single properties
            if (javascript.Contains(".") || javascript.Contains("("))
                return "dotvvm.tryEval(function(){return " + javascript + "})";
            else return javascript;
        }
    }
}
