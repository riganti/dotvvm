using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.Javascript;
using DotVVM.Framework.Compilation.Javascript.Ast;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Compilation.Binding
{
    public class DotnetViewModuleMethodTranslator : IJavascriptMethodTranslator
    {
        public JsExpression? TryTranslateCall(LazyTranslatedExpression? context, LazyTranslatedExpression[] arguments, MethodInfo method)
        {
            // ignore static methods
            if (context == null)
            {
                return null;
            }

            // check whether we have the annotation - otherwise the type is not used in the _dotnet context and will not be translated
            var target = context.JsExpression();
            if (target.Annotation<DotnetExtensionParameter.ViewModuleAnnotation>() is not { } annotation)
            {
                return null;
            }

            // check that the method is callable
            if (!method.IsPublic)
            {
                throw new DotvvmCompilationException($"Cannot call non-public method {method.DeclaringType!.FullName}.{method.Name} on a @dotnet module!");
            }
            if (method.IsAbstract)
            {
                throw new DotvvmCompilationException($"Cannot call abstract method {method.DeclaringType!.FullName}.{method.Name} on a @dotnet module!");
            }
            if (method.IsGenericMethod || method.IsGenericMethodDefinition)
            {
                throw new DotvvmCompilationException($"Cannot call generic method {method.DeclaringType!.FullName}.{method.Name} on a @dotnet module!");
            }

            // check that there are not more overloads
            var allOverloads = context.NotNull().OriginalExpression.Type
                .GetMethods()
                .Where(m => m.Name == method.Name && m.IsPublic);
            if (allOverloads.Count() > 1)
            {
                throw new DotvvmCompilationException($"There are multiple methods named {method.Name} on a @dotnet module {context.OriginalExpression.Type}! Overloads are not supported on @dotnet modules.");
            }

            // translate the method
            var viewIdOrElementExpr = annotation.IsMarkupControl ? new JsSymbolicParameter(JavascriptTranslator.CurrentElementParameter) : (JsExpression)new JsLiteral(annotation.Id);

            return new JsIdentifierExpression("dotvvm").Member("viewModules").Member("call")
                .Invoke(
                    viewIdOrElementExpr,
                    new JsLiteral("dotnetWasmInvoke"),
                    new JsArrayExpression(
                        new[] { new JsLiteral(method.Name) }
                            .Concat(arguments.Select(a => a.JsExpression()))
                            .ToArray()),
                    new JsLiteral(true)
                )
                .WithAnnotation(new ResultIsPromiseAnnotation(e => e));
        }

    }
}
