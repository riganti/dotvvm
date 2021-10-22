using System;
using System.Linq.Expressions;
using System.Reflection;
using DotVVM.Framework.Compilation.Javascript.Ast;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Compilation.Javascript
{

    public partial class JavascriptTranslatableMethodCollection
    {
        class PrimitiveToStringTranslator : IJavascriptMethodTranslator
        {
            static (bool canConvert, bool isNullable, bool isStringAlready) ToStringCheck(Expression expr)
            {
                while (expr.NodeType == ExpressionType.Convert) expr = ((UnaryExpression)expr).Operand;
                var type = expr.Type.UnwrapNullableType();
                var isStringRepresentedType =
                    type == typeof(string) || type == typeof(TimeSpan) || type == typeof(Guid) || type.IsEnum;
                return (
                    type.IsPrimitive || type.IsEnum || type == typeof(Enum) || isStringRepresentedType,
                    expr.Type.IsNullable(),
                    isStringRepresentedType
                );
            }
            public JsExpression? TryTranslateCall(LazyTranslatedExpression? context, LazyTranslatedExpression[] arguments, MethodInfo method)
            {
                var arg = context ?? arguments[0];
                var (canConvert, isNullable, isStringAlready) = ToStringCheck(arg.OriginalExpression);
                if (!canConvert)
                    return null;
                var js = arg.JsExpression();
                // convert null to empty string, not "null"
                if (isNullable)
                    js = new JsBinaryExpression(js, BinaryOperatorType.NullishCoalescing, new JsLiteral(""));
                if (!isStringAlready)
                    js = new JsIdentifierExpression("String").Invoke(js);
                return js;
            }
        }
    }
}
