using System;
using System.Linq.Expressions;
using System.Reflection;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.Javascript.Ast;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Compilation.Javascript
{

    class PrimitiveToStringTranslator : IJavascriptMethodTranslator
    {
        internal static bool CanBeNull(Expression expr)
        {
            if (expr.GetParameterAnnotation() is { } annotation)
            {
                // view model and extension parameters can't be null 
                return false;
            }

            if (expr.NodeType == ExpressionType.Convert)
            {
                var cast = (UnaryExpression)expr;
                return CanBeNull(cast.Operand);
            }

            if (expr is MemberExpression member)
            {
                if (!expr.Type.IsValueType || expr.Type.IsNullable())
                    return true;
                return member.Expression is {} && CanBeNull(member.Expression);
            }



            return true; // assume it can be
            
        }
        static (bool canConvert, bool isNullable, bool isStringAlready) ToStringCheck(Expression expr)
        {
            while (expr.NodeType == ExpressionType.Convert) expr = ((UnaryExpression)expr).Operand;
            var type = expr.Type.UnwrapNullableType();
            var isStringRepresentedType =
                type == typeof(string) || type == typeof(TimeSpan) || type == typeof(Guid) || type.IsEnum;
            return (
                type.IsPrimitive || type.IsEnum || type == typeof(Enum) || isStringRepresentedType,
                CanBeNull(expr),
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
                js = new JsIdentifierExpression("window").Member("String").Invoke(js);
            return js;
        }
    }
}
