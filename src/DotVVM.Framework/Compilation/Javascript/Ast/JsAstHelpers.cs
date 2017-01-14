using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotVVM.Framework.Compilation.Javascript.Ast
{
    public static class JsAstHelpers
    {
        public static JsExpression Member(this JsExpression target, string memberName)
        {
            if (target == null) return new JsIdentifierExpression(memberName);
            else return new JsMemberAccessExpression(target, memberName);
        }

        public static JsExpression Invoke(this JsExpression target, params JsExpression[] arguments)
        {
            return new JsInvocationExpression(target, arguments);
        }

        public static JsExpression Indexer(this JsExpression target, JsExpression argument)
        {
            return new JsIndexerExpression(target, argument);
        }

        public static string FormatScript(this JsNode node, bool niceMode = false, string indent = "\t")
        {
            node.FixParenthesis();
            var visitor = new JsFormattingVisitor(niceMode, indent);
            node.AcceptVisitor(visitor);
            return visitor.GetParameterlessResult();
        }

        public static ParametrizedCode FormatParametrizedScript(this JsNode node, bool niceMode = false, string indent = "\t")
        {
            node.FixParenthesis();
            var visitor = new JsFormattingVisitor(niceMode, indent);
            node.AcceptVisitor(visitor);
            return visitor.GetResult(JsParensFixingVisitor.GetOperatorPrecedence(node as JsExpression));
        }

        public static JsNode FixParenthesis(this JsNode node)
        {
            var visitor = new JsParensFixingVisitor();
            node.AcceptVisitor(visitor);
            return node;
        }

        /// <summary>
        /// Gets nodes for the expression that can be result.
        /// for `a + b` return `a +b`
        /// for `a ? b : c` returns `b` and `c`
        /// for `a || b` returns `a` and `b`
        /// </summary>
        public static IEnumerable<JsExpression> GetLeafResultNodes(this JsExpression expr)
        {
            switch (expr) {
                case JsConditionalExpression condition:
                    return condition.TrueExpression.GetLeafResultNodes()
                        .Concat(condition.FalseExpression.GetLeafResultNodes());
                case JsBinaryExpression binary:
                    if (binary.Operator == BinaryOperatorType.ConditionalAnd || binary.Operator == BinaryOperatorType.ConditionalOr)
                        return binary.Left.GetLeafResultNodes()
                        .Concat(binary.Right.GetLeafResultNodes());
                    else goto default;
                case JsParenthesizedExpression p: return p.Expression.GetLeafResultNodes();
                default:
                    return new[] { expr };
            }
        }
    }
}
