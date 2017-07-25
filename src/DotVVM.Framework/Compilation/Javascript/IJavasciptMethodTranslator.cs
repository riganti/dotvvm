using System.Linq.Expressions;
using System.Reflection;
using DotVVM.Framework.Compilation.Javascript.Ast;

namespace DotVVM.Framework.Compilation.Javascript
{
    public interface IJavascriptMethodTranslator
    {
        JsExpression TryTranslateCall(HalfTranslatedExpression context, HalfTranslatedExpression[] arguments, MethodInfo method);
    }
}
