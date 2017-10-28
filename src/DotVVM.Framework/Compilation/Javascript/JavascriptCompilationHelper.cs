using System;
using DotVVM.Framework.Compilation.Javascript.Ast;
using DotVVM.Framework.ViewModel.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DotVVM.Framework.Compilation.Javascript
{
    public static class JavascriptCompilationHelper
    {
        public static string CompileConstant(object obj) => JsonConvert.SerializeObject(obj, new StringEnumConverter());

        public static ViewModelInfoAnnotation GetResultType(this JsExpression expr)
        {
            ViewModelInfoAnnotation combine2(ViewModelInfoAnnotation a, ViewModelInfoAnnotation b)
            {
                if (a == null ||  b == null) return a ?? b;
                else if (a.Type.Equals(b.Type)) return b;
                else return null;
            }
            if (expr.TryGetAnnotation<ViewModelInfoAnnotation>(out var vmInfo)) return vmInfo;
            else if (expr is JsAssignmentExpression assignment && assignment.Operator == null) return GetResultType(assignment.Right);
            else if (expr is JsBinaryExpression binary && (binary.Operator == BinaryOperatorType.ConditionalAnd || binary.Operator == BinaryOperatorType.ConditionalOr))
                return combine2(
                    GetResultType(binary.Left),
                    GetResultType(binary.Right));
            else if (expr is JsConditionalExpression conditional)
                return combine2(
                    GetResultType(conditional.TrueExpression),
                    GetResultType(conditional.FalseExpression));
            else if (expr is JsLiteral literal) return literal.Value != null ? new ViewModelInfoAnnotation(literal.Value.GetType(), containsObservables: false) : null;
            else return null;
        }

        public static bool IsComplexType(this JsExpression expr) =>
            GetResultType(expr) is ViewModelInfoAnnotation vmInfo && ViewModelJsonConverter.IsComplexType(vmInfo.Type);

        public static bool IsRootResultExpression(this JsNode node) =>
            SatisfyResultCondition(node, n => n.Parent == null || n.Parent is JsExpressionStatement);
        public static bool SatisfyResultCondition(this JsNode node, Func<JsNode, bool> predicate) =>
            predicate(node) ||
            (node.Parent is JsParenthesizedExpression ||
                node.Role == JsConditionalExpression.FalseRole ||
                node.Role == JsConditionalExpression.TrueRole
            ) && node.Parent.SatisfyResultCondition(predicate);
    }
}
