using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Javascript;
using DotVVM.Framework.Compilation.Javascript.Ast;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Security;
using DotVVM.Framework.Testing;
using DotVVM.Framework.Utils;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.ViewModel.Serialization;

namespace DotVVM.Framework.Compilation.Binding
{
    public class StaticCommandBindingCompiler
    {
        private readonly IViewModelSerializationMapper vmMapper;
        private readonly IViewModelProtector protector;
        public StaticCommandBindingCompiler(IViewModelSerializationMapper vmMapper, IViewModelProtector protector)
        {
            this.vmMapper = vmMapper;
            this.protector = protector;
        }

        public JsExpression CompileToJavascript(DataContextStack dataContext, Expression expression)
        {
            var currentContextVariable = new JsTemporaryVariableParameter(new JsIdentifierExpression("ko").Member("contextFor").Invoke(new JsSymbolicParameter(CommandBindingExpression.SenderElementParameter)));
            var resultPromiseVariable = new JsTemporaryVariableParameter(new JsNewExpression("DotvvmPromise"));
            var senderVariable = new JsTemporaryVariableParameter(new JsSymbolicParameter(CommandBindingExpression.SenderElementParameter));
            var visitor = new ExtractExpressionVisitor(ex => ex.NodeType == ExpressionType.Call && ex is MethodCallExpression methodCall && JavascriptTranslator.FindMethodTranslator(methodCall.Method, methodCall.Object, methodCall.Arguments.ToArray()) == null);
            var rootCallback = visitor.Visit(expression);
            var js = SouldCompileCallback(rootCallback) ? new JsSymbolicParameter(resultPromiseVariable).Member("resolve").Invoke(JavascriptTranslator.CompileToJavascript(rootCallback, dataContext, vmMapper)) : null;
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

            string methodName = GetMethodName(methodExpression.Method);
            var argsScript = GetArgsScript(methodExpression, dataContext, GetArgumentEncryptionPurposes(methodName));
            return new JsIdentifierExpression("dotvvm").Member("staticCommandPostback").Invoke(new JsSymbolicParameter(CommandBindingExpression.ViewModelNameParameter), new JsSymbolicParameter(CommandBindingExpression.SenderElementParameter), new JsLiteral(methodName), argsScript, callbackFunction);
        }

        private JsExpression TranslateArgument(Expression expr, DataContextStack dataContext, string[] encryptionPurpose)
        {
            if (expr.GetParameterAnnotation() is BindingParameterAnnotation annotation && annotation.ExtensionParameter is InjectedServiceExtensionParameter service)
                return new JsObjectExpression(new JsObjectProperty("@service", new JsLiteral(
                    protector.Protect(
                        ((ResolvedTypeDescriptor)service.ParameterType).Type.AssemblyQualifiedName.Apply(Encoding.UTF8.GetBytes),
                        encryptionPurpose
                    )
                    .Apply(Convert.ToBase64String)
                )));
            else
                return JavascriptTranslator.CompileToJavascript(expr, dataContext, vmMapper);
        }

        public JsExpression GetArgsScript(MethodCallExpression expression, DataContextStack dataContext, IEnumerable<string[]> encryptionPurpose) =>
            (expression.Object == null ? new Expression[0] : new[] { expression.Object })
            .Concat(expression.Arguments).Zip(encryptionPurpose, (a, e) => TranslateArgument(a, dataContext, e))
            .Apply(a => new JsArrayExpression(a));

        public static string GetMethodName(MethodInfo method)
        {
            return method.DeclaringType.AssemblyQualifiedName + "." + method.Name;
        }

        public static IEnumerable<string[]> GetArgumentEncryptionPurposes(string methodName)
        {
            for (int i = 0; true; i++)
            {
                yield return new [] {
                    "StaticCommand",
                    methodName, // different method calls should have different keys, so you can't swap arguments between calls of different methods
                    i.ToString(), // different arguments should have different keys, so you can't swap arguments in one method call
                };
            }
        }
    }
}
