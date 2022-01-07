using System;
using System.Linq;
using DotVVM.Framework.Compilation.Javascript.Ast;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Utils;
using DotVVM.Framework.ViewModel.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DotVVM.Framework.Compilation.Javascript
{
    public static class JavascriptCompilationHelper
    {
        public static string CompileConstant(object? obj) =>
            obj switch {
                null => "null",
                true => "true",
                false => "false",
                string s => JsonConvert.ToString(s),
                int i => JsonConvert.ToString(i),
                _ => JsonConvert.SerializeObject(obj, DefaultSerializerSettingsProvider.Instance.Settings)
            };

        public static ViewModelInfoAnnotation? GetResultType(this JsExpression expr)
        {
            ViewModelInfoAnnotation? combine2(ViewModelInfoAnnotation? a, ViewModelInfoAnnotation? b)
            {
                if (a == null || b == null) return a ?? b;
                else if (a.Type.IsAssignableFrom(b.Type)) return a;
                else if (b.Type.IsAssignableFrom(a.Type)) return b;
                else return null;
            }
            if (expr.TryGetAnnotation<ViewModelInfoAnnotation>(out var vmInfo)) return vmInfo;
            else if (expr is JsParenthesizedExpression parens) return parens.Expression.GetResultType();
            else if (expr is JsAssignmentExpression assignment && assignment.Operator == null) return GetResultType(assignment.Right) ?? GetResultType(assignment.Left);
            else if (expr is JsBinaryExpression binary)
            {
                switch (binary.Operator)
                {
                    case BinaryOperatorType.ConditionalAnd:
                    case BinaryOperatorType.ConditionalOr:
                    case BinaryOperatorType.NullishCoalescing:
                        return combine2(
                            GetResultType(binary.Left),
                            GetResultType(binary.Right));
                    case BinaryOperatorType.Sequence:
                        return GetResultType(binary.Right);
                    default:
                        return null;
                }
            }
            else if (expr is JsConditionalExpression conditional)
                return combine2(
                    GetResultType(conditional.TrueExpression),
                    GetResultType(conditional.FalseExpression));
            else if (expr is JsLiteral literal) return literal.Value != null ? new ViewModelInfoAnnotation(literal.Value.GetType(), containsObservables: false) : null;
            // match IIFE (function () { return X })()
            else if (expr is JsInvocationExpression invocationExpression &&
                invocationExpression.Target is JsFunctionExpression functionExpression)
            {
                var returnStatements = functionExpression.Block.Body.OfType<JsReturnStatement>().ToArray();
                if (returnStatements.Length == 1)
                    return returnStatements[0].Expression.GetResultType();
                else
                    return null;
            }
            else return null;
        }

        public static bool IsComplexType(this JsExpression expr) =>
            GetResultType(expr) is { Type: var type } && ReflectionUtils.IsComplexType(type);

        public static bool IsRootResultExpression(this JsNode node) =>
            SatisfyResultCondition(node, n => n.Parent == null || n.Parent is JsExpressionStatement);

        public static bool IsResultIgnored(this JsExpression e) =>
            e.SatisfyResultCondition(
                e => e.Parent is JsExpressionStatement ||
                     e.Parent is JsBinaryExpression { Operator: BinaryOperatorType.Sequence } && e.Role == JsBinaryExpression.LeftRole ||
                     e.Parent is JsUnaryExpression { Operator: UnaryOperatorType.Void }
            );
        
        public static bool SatisfyResultCondition(this JsNode node, Func<JsNode, bool> predicate) =>
            predicate(node) ||
            (node.Parent is JsParenthesizedExpression ||
                node.Role == JsConditionalExpression.FalseRole ||
                node.Role == JsConditionalExpression.TrueRole ||
                node.Role == JsBinaryExpression.RightRole && node.Parent is JsBinaryExpression { OperatorString: "," or "&&" or "||" or "??" }
            ) && node.Parent!.SatisfyResultCondition(predicate);

    }
}
