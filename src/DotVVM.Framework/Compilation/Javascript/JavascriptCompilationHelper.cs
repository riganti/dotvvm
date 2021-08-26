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
        public static string CompileConstant(object? obj) => JsonConvert.SerializeObject(obj, DefaultSerializerSettingsProvider.Instance.Settings);

        private static readonly CodeSymbolicParameter indexerTargetParameter = new CodeSymbolicParameter("JavascriptCompilationHelper.indexerTargetParameter");
        private static readonly CodeSymbolicParameter indexerExpressionParameter = new CodeSymbolicParameter("JavascriptCompilationHelper.indexerExpressionParameter");
        private static readonly ParametrizedCode indexerCode =
            new JsIdentifierExpression("ko").Member("unwrap").Invoke(new JsSymbolicParameter(indexerTargetParameter)).Indexer(new JsSymbolicParameter(indexerExpressionParameter))
            .FormatParametrizedScript();
        [Obsolete]
        public static ParametrizedCode AddIndexerToViewModel(ParametrizedCode script, object index, bool unwrap = false) =>
            AddIndexerToViewModel(script, new JsLiteral(index), unwrap);
        [Obsolete]
        public static ParametrizedCode AddIndexerToViewModel(ParametrizedCode script, JsExpression indexer, bool unwrap = false)
        {
            return indexerCode.AssignParameters(o =>
                o == indexerTargetParameter ? new CodeParameterAssignment(script) :
                o == indexerExpressionParameter ? CodeParameterAssignment.FromExpression(indexer) :
                default(CodeParameterAssignment));
        }

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
        public static bool SatisfyResultCondition(this JsNode node, Func<JsNode, bool> predicate) =>
            predicate(node) ||
            (node.Parent is JsParenthesizedExpression ||
                node.Role == JsConditionalExpression.FalseRole ||
                node.Role == JsConditionalExpression.TrueRole
            ) && node.Parent!.SatisfyResultCondition(predicate);

    }
}
