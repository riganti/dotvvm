using DotVVM.Framework.Binding;
using DotVVM.Framework.Runtime.Compilation.JavascriptCompilation;
using DotVVM.Framework.Runtime.Compilation.ResolvedControlTree;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Runtime.Compilation
{
    public class StaticCommandBindingCompilationAttribute : BindingCompilationAttribute
    {
        public override string CompileToJs(ResolvedBinding binding, CompiledBindingExpression compiledExpression)
        {
            var methodExpression = binding.GetExpression() as MethodCallExpression;
            if (methodExpression == null)
            {
                throw new NotSupportedException("static command binding must be method call");
            }
            var argsScript = GetArgsScript(methodExpression, binding.DataContextTypeStack);
            return $"dotvvm.staticCommandPostbackScript('{GetMethodName(methodExpression)}', [{ argsScript }])";
        }

        public static string GetArgsScript(MethodCallExpression expression, DataContextStack dataContext)
        {
            var target = expression.Object == null ? "null" : JavascriptTranslator.CompileToJavascript(expression.Object, dataContext);
            var arguments = new[] { target }.Concat(expression.Arguments.Select(a => JavascriptTranslator.CompileToJavascript(a, dataContext)));
            return "[" + String.Join(", ", arguments.Select(a => JsonConvert.SerializeObject(a))) + "]";
        }

        public static string GetMethodName(MethodCallExpression methodInvocation)
        {
            return methodInvocation.Method.DeclaringType.AssemblyQualifiedName + "." + methodInvocation.Method.Name;
        }
    }
}
