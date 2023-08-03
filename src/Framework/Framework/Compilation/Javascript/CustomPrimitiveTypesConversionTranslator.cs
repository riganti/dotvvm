using System.Reflection;
using DotVVM.Framework.Compilation.Javascript.Ast;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Compilation.Javascript
{
    public class CustomPrimitiveTypesConversionTranslator : IJavascriptMethodTranslator
    {
        public JsExpression? TryTranslateCall(LazyTranslatedExpression? context, LazyTranslatedExpression[] arguments, MethodInfo method)
        {
            var type = context?.OriginalExpression.Type ?? method.DeclaringType!;
            type = type.UnwrapNullableType();
            if (method.Name is "ToString" or "Parse" && ReflectionUtils.IsCustomPrimitiveType(type))
            {
                if (method.Name == "ToString" && arguments.Length == 0 && context is {})
                {
                    return context.JsExpression();
                }
                else if (method.Name == "Parse" && arguments.Length == 1 && context is null)
                {
                    return arguments[0].JsExpression();
                }
            }
            return null;
        }
    }
}
