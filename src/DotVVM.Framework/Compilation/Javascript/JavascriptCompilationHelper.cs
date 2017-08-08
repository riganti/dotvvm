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

        private static readonly object indexerTargetParameter = new object();
        private static readonly object indexerExpressionParameter = new object();
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

        public static bool IsComplexType(this JsExpression expr)
        {
            if (expr.TryGetAnnotation<ViewModelInfoAnnotation>(out var vmInfo)) return ViewModelJsonConverter.IsComplexType(vmInfo.Type);
            if (expr is JsAssignmentExpression assignment && assignment.Operator == null) return IsComplexType(assignment.Right);
            if (expr is JsBinaryExpression binary && (binary.Operator == BinaryOperatorType.ConditionalAnd || binary.Operator == BinaryOperatorType.ConditionalOr))
                return IsComplexType(binary.Left) && IsComplexType(binary.Right);
            if (expr is JsConditionalExpression conditional) return IsComplexType(conditional.TrueExpression) && IsComplexType(conditional.FalseExpression);
            if (expr is JsLiteral literal) return literal.Value == null || ViewModelJsonConverter.IsComplexType(literal.Value.GetType());
            return false;
        }

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
