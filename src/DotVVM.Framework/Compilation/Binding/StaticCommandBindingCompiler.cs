using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.Javascript;
using DotVVM.Framework.Compilation.Javascript.Ast;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.ViewModel.Serialization;

namespace DotVVM.Framework.Compilation.Binding
{
    public class StaticCommandBindingCompiler
    {
        private readonly JavascriptTranslator javascriptTranslator;

        public StaticCommandBindingCompiler(JavascriptTranslator javascriptTranslator)
        {
            this.javascriptTranslator = javascriptTranslator;
        }

        public JsExpression CompileToJavascript(DataContextStack dataContext, Expression expression)
        {
            var currentContextVariable = new JsTemporaryVariableParameter(new JsIdentifierExpression("ko").Member("contextFor").Invoke(new JsSymbolicParameter(CommandBindingExpression.SenderElementParameter)));
            var resultPromiseVariable = new JsTemporaryVariableParameter(new JsNewExpression("DotvvmPromise"));
            var senderVariable = new JsTemporaryVariableParameter(new JsSymbolicParameter(CommandBindingExpression.SenderElementParameter));
            var visitor = new ExtractExpressionVisitor(ex => ex.NodeType == ExpressionType.Call && ex is MethodCallExpression methodCall && javascriptTranslator.TryTranslateMethodCall(methodCall.Object, methodCall.Arguments.ToArray(), methodCall.Method, dataContext) == null);
            var rootCallback = visitor.Visit(expression);
            var js = SouldCompileCallback(rootCallback) ? new JsSymbolicParameter(resultPromiseVariable).Member("resolve").Invoke(javascriptTranslator.CompileToJavascript(rootCallback, dataContext)) : null;
            foreach (var param in visitor.ParameterOrder.Reverse<ParameterExpression>())
            {
                js = js ?? new JsSymbolicParameter(resultPromiseVariable).Member("resolve").Invoke(new JsIdentifierExpression(param.Name));
                var callback = new JsFunctionExpression(new[] { new JsIdentifier(param.Name) }, new JsBlockStatement(new JsExpressionStatement(js)));
                var method = visitor.Replaced[param] as MethodCallExpression;
                js = CompileMethodCall(method, dataContext, callback);
            }
            foreach(var sp in js.Descendants.OfType<JsSymbolicParameter>())
            {
                if (sp.Symbol == JavascriptTranslator.KnockoutContextParameter) sp.Symbol = currentContextVariable;
                else if (sp.Symbol == JavascriptTranslator.KnockoutViewModelParameter) sp.ReplaceWith(new JsSymbolicParameter(currentContextVariable).Member("$data"));
                else if (sp.Symbol == CommandBindingExpression.SenderElementParameter) sp.Symbol = senderVariable;
            }
            return new JsBinaryExpression(js, BinaryOperatorType.Sequence, new JsSymbolicParameter(resultPromiseVariable));
        }

        protected virtual bool SouldCompileCallback(Expression c)
        {
            if (c.NodeType == ExpressionType.Parameter) return false;
            return true;
        }

        protected virtual JsExpression CompileMethodCall(MethodCallExpression methodExpression, DataContextStack dataContext, JsExpression callbackFunction = null)
        {
            if (!methodExpression.Method.IsDefined(typeof(AllowStaticCommandAttribute)))
                throw new Exception($"Method '{methodExpression.Method.DeclaringType.Name}.{methodExpression.Method.Name}' used in static command has to be marked with [AllowStaticCommand] attribute.");

            if (callbackFunction == null) callbackFunction = new JsLiteral(null);
            if (methodExpression == null) throw new NotSupportedException("Static command binding must be a method call!");

            var argsScript = GetArgsScript(methodExpression, dataContext);
            return new JsIdentifierExpression("dotvvm").Member("staticCommandPostback").Invoke(new JsSymbolicParameter(CommandBindingExpression.ViewModelNameParameter), new JsSymbolicParameter(CommandBindingExpression.SenderElementParameter), new JsLiteral(GetMethodName(methodExpression)), argsScript, callbackFunction);
        }

        public JsExpression GetArgsScript(MethodCallExpression expression, DataContextStack dataContext)
        {
            var arguments = (expression.Object == null ? new Expression[0] : new[] { expression.Object })
                .Concat(expression.Arguments).Select(a => javascriptTranslator.CompileToJavascript(a, dataContext));
            return new JsArrayExpression(arguments);
        }

        public static string GetMethodName(MethodCallExpression methodInvocation)
        {
            return methodInvocation.Method.DeclaringType.AssemblyQualifiedName + "." + methodInvocation.Method.Name;
        }
    }
}
