using System.Linq.Expressions;
using System.Reflection;
using DotVVM.Framework.Compilation.Javascript.Ast;

namespace DotVVM.Framework.Compilation.Javascript
{
    public interface IJsMethodTranslator
    {
        JsExpression TranslateCall(JsExpression context, JsExpression[] arguments, MethodInfo method);
		bool CanTranslateCall(MethodInfo method, Expression context, Expression[] arguments);
    }
}
