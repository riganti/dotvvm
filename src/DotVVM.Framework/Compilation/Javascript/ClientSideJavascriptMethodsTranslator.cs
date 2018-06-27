using System.Linq;
using System.Reflection;
using DotVVM.Framework.Compilation.Javascript.Ast;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Framework.Compilation.Javascript
{
    public class ClientSideJavascriptMethodsTranslator : IJavascriptMethodTranslator
    {
        public JsExpression TryTranslateCall(LazyTranslatedExpression context, LazyTranslatedExpression[] arguments,
            MethodInfo method)
        {
            if (method.GetCustomAttribute<ClientSideMethodAttribute>() != null)
            {
                var methodAccessExpression = new JsMemberAccessExpression(context.JsExpression(), method.Name);
                var argumentJsExpressions = arguments.Select(a => a.JsExpression());
                return new JsInvocationExpression(methodAccessExpression, argumentJsExpressions);
            }
            return null;
        }
    }
}