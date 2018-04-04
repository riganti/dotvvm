using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Framework.Compilation.Javascript.Ast
{
    public static class JsAstHelpers
    {
        public static JsExpression Member(this JsExpression target, string memberName)
        {
            if (target == null) return new JsIdentifierExpression(memberName);
            else return new JsMemberAccessExpression(target, memberName);
        }

        public static JsExpression Invoke(this JsExpression target, IEnumerable<JsExpression> arguments) =>
            new JsInvocationExpression(target, arguments);

        public static JsExpression Invoke(this JsExpression target, params JsExpression[] arguments) =>
            new JsInvocationExpression(target, arguments);

        public static JsExpression Indexer(this JsExpression target, JsExpression argument)
        {
            return new JsIndexerExpression(target, argument);
        }

        public static JsExpression Unary(this JsExpression target, UnaryOperatorType type, bool isPrefix = true) =>
            new JsUnaryExpression(type, target, isPrefix);

        public static string FormatScript(this JsNode node, bool niceMode = false, string indent = "\t", bool isDebugString = false)
        {
            node.FixParenthesis();
            var visitor = new JsFormattingVisitor(niceMode, indent);
            node.AcceptVisitor(visitor);
            return isDebugString ? visitor.ToString() : visitor.GetParameterlessResult();
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
            switch (expr)
            {
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

        public static T Detach<T>(this T node) where T : JsNode
        {
            node.Remove();
            return node;
        }

        /// Wraps the expression in `dotvvm.evaluator.wrapObservable` if needed
        public static JsExpression EnsureObservableWrapped(this JsExpression expression)
        {
            // It's not needed to wrap if none of the descendants return an observable
            if (!expression.DescendantNodes().Any(n => (n.HasAnnotation<ResultIsObservableAnnotation>() && !n.HasAnnotation<ShouldBeObservableAnnotation>()) || n.HasAnnotation<ObservableUnwrapInvocationAnnotation>()))
            {
                return expression.WithAnnotation(ShouldBeObservableAnnotation.Instance);
            }
            else if (expression.SatisfyResultCondition(n => n.HasAnnotation<ResultIsObservableAnnotation>()))
            {
                var arguments = new List<JsExpression>(2) {
                    new JsFunctionExpression(
                        parameters: Enumerable.Empty<JsIdentifier>(),
                        bodyBlock: new JsBlockStatement(new JsReturnStatement(expression.WithAnnotation(ShouldBeObservableAnnotation.Instance)))
                    )
                };

                if (expression.SatisfyResultCondition(n => n.HasAnnotation<ResultIsObservableArrayAnnotation>()))
                {
                    arguments.Add(new JsLiteral(true));
                }

                return new JsIdentifierExpression("dotvvm").Member("evaluator").Member("wrapObservable").Invoke(arguments)
                    .WithAnnotation(ResultIsObservableAnnotation.Instance)
                    .WithAnnotation(ShouldBeObservableAnnotation.Instance);
            }
            else
            {
                return new JsIdentifierExpression("ko").Member("pureComputed").Invoke(new JsFunctionExpression(
                        parameters: Enumerable.Empty<JsIdentifier>(),
                        bodyBlock: new JsBlockStatement(new JsReturnStatement(expression))
                    ))
                    .WithAnnotation(ResultIsObservableAnnotation.Instance)
                    .WithAnnotation(ShouldBeObservableAnnotation.Instance);
            }
        }
    }
}
