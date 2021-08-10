using System;
using System.Linq;
using System.Reflection;
using DotVVM.Framework.Compilation.Javascript.Ast;

namespace DotVVM.Framework.Compilation.Javascript
{
    public class EnumToStringMethodTranslator : IJavascriptMethodTranslator
    {
        public JsExpression TryTranslateCall(LazyTranslatedExpression context, LazyTranslatedExpression[] arguments, MethodInfo method)
        {
            // Enum has totally strange behavior in reflection:
            // method.DeclaringType.IsEnum == false
            // method != typeof(Enum).GetMethod("ToString", Array.Empty<Type>())

            if (method?.DeclaringType == typeof(Enum) && method.Name == "ToString" && method.GetParameters().Length == 0)
                return context.JsExpression();
            else
                return null;
        }
    }
}
