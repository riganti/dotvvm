using DotVVM.Framework.Binding;
using DotVVM.Framework.Runtime.Compilation.JavascriptCompilation;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Runtime.ControlTree;
using DotVVM.Framework.Runtime.ControlTree.Resolved;

namespace DotVVM.Framework.Runtime.Compilation
{
    public class StaticCommandBindingCompilationAttribute : CommandBindingCompilationAttribute
    {
        public override string CompileToJs(ResolvedBinding binding, CompiledBindingExpression compiledExpression)
        {
            var expression = binding.GetExpression();

            var visitor = new ExtractExpressionVisitor(ex => ex.NodeType == ExpressionType.Call);
            var rootCallback = visitor.Visit(expression);
            var js = SouldCompileCallback(rootCallback) ? JavascriptTranslator.CompileToJavascript(rootCallback, binding.DataContextTypeStack) : null;
            foreach (var param in visitor.ParameterOrder)
            {
                var callback = js == null ? null : $"function({param.Name}){{{js}}}";
                var method = visitor.Replaced[param] as MethodCallExpression;
                js = CompileMethodCall(method, binding.DataContextTypeStack, callback);
            }
            return "var $context = ko.contextFor(this);var sender = this;(function(i_pageArea){with($context){" + js + "}})";
        }

        protected virtual bool SouldCompileCallback(Expression c)
        {
            if (c.NodeType == ExpressionType.Parameter) return false;
            return true;
        }

        protected virtual string CompileMethodCall(MethodCallExpression methodExpression, DataContextStack dataContext, string callbackFunction = null)
        {
            if (callbackFunction == null) callbackFunction = "null";
            if (methodExpression == null)
            {
                throw new NotSupportedException("Static command binding must be a method call!");
            }
            var argsScript = GetArgsScript(methodExpression, dataContext);
            return $"dotvvm.staticCommandPostback(i_pageArea, sender, '{GetMethodName(methodExpression)}', { argsScript }, {callbackFunction})";
        }

        public static string GetArgsScript(MethodCallExpression expression, DataContextStack dataContext)
        {
            var target = expression.Object == null ? "null" : JavascriptTranslator.CompileToJavascript(expression.Object, dataContext);
            var arguments = new[] { target }.Concat(expression.Arguments.Select(a => JavascriptTranslator.CompileToJavascript(a, dataContext)));
            return "[" + String.Join(", ", arguments) + "]";
        }

        public static string GetMethodName(MethodCallExpression methodInvocation)
        {
            return methodInvocation.Method.DeclaringType.AssemblyQualifiedName + "." + methodInvocation.Method.Name;
        }
    }
}
