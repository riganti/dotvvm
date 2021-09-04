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
        public static JsExpression Await(this JsExpression target) =>
            target.Unary(UnaryOperatorType.Await);

        /// Builds `return {expr}`
        public static JsReturnStatement Return(this JsExpression expr) =>
            new JsReturnStatement(expr);
        public static JsStatement AsStatement(this JsNode node) =>
            node is JsExpression expr ? new JsExpressionStatement(expr) :
            node is JsStatement s ? s :
            throw new NotSupportedException();
        public static JsBlockStatement AsBlock(this JsStatement statement) =>
            statement is JsBlockStatement block ? block :
            new JsBlockStatement(statement);
        public static JsBlockStatement AsBlock(this IEnumerable<JsStatement> statements) =>
            new JsBlockStatement(statements);

        
        public static bool ContainsAwait(this JsNode node) =>
            node.DescendantNodesAndSelf(child => !(child is JsFunctionExpression))
                .Any(child => child is JsUnaryExpression { Operator: UnaryOperatorType.Await });

        public static string FormatScript(this JsNode node, bool niceMode = false, string indent = "\t", bool isDebugString = false)
        {
            node.FixParenthesis();
            var visitor = new JsFormattingVisitor(niceMode, indent);
            node.AcceptVisitor(visitor);
            return isDebugString ? visitor.ToString() : visitor.GetParameterlessResult();
        }

        public static ParametrizedCode FormatParametrizedScript(this JsNode node, bool niceMode = false, string indent = "\t")
        {
            if (node is null)
                throw new ArgumentNullException(nameof(node));
            node = node.FixParenthesis();
            var visitor = new JsFormattingVisitor(niceMode, indent);
            node.AcceptVisitor(visitor);
            var precedence = node is JsExpression expr ? JsParensFixingVisitor.GetOperatorPrecedence(expr) : OperatorPrecedence.Max;
            return visitor.GetResult(precedence);
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

        public static TNewNode ReplaceWith<TNode, TNewNode>(this TNode node, Func<TNode, TNewNode> replaceFunction)
            where TNode: JsNode
            where TNewNode: JsNode
        {
            if (replaceFunction == null)
            throw new ArgumentNullException("replaceFunction");
            if (node.Parent == null) {
                throw new InvalidOperationException("Cannot replace the root node");
            }
            var oldParent = node.Parent;
            var oldSuccessor = node.NextSibling;
            var oldRole = node.Role!;
            node.Remove();
            var replacement = replaceFunction(node);
            if (oldSuccessor != null && oldSuccessor.Parent != oldParent)
                throw new InvalidOperationException("replace function changed nextSibling of node being replaced?");
            if (replacement != null) {
                if (replacement.Parent != null)
                    throw new InvalidOperationException("replace function must return the root of a tree");
                if (!oldRole.IsValid(replacement)) {
                    throw new InvalidOperationException(string.Format("The new node '{0}' is not valid in the role {1}", replacement.GetType().Name, oldRole.ToString()));
                }

                if (oldSuccessor != null)
                    oldParent.InsertChildBeforeUnsafe(oldSuccessor, replacement, oldRole);
                else
                    oldParent.AddChildUnsafe(replacement, oldRole);
            }
            return replacement!;
        }


        /// <summary>
        /// Clones the whole subtree starting at this AST node.
        /// </summary>
        /// <remarks>Annotations are copied over to the new nodes; and any annotations implementing ICloneable will be cloned.</remarks>
        public static TNode Clone<TNode>(this TNode node)
            where TNode: JsNode
        {
            return (TNode)node.CloneImpl();
        }

        public static JsNode AssignParameters(this JsNode node, Func<CodeSymbolicParameter, JsNode?> parameterAssignment)
        {
            foreach (var sp in node.Descendants.OfType<JsSymbolicParameter>())
            {
                var assignment = parameterAssignment(sp.Symbol);
                if (assignment is JsSymbolicParameter assignmentS)
                {
                    sp.Symbol = assignmentS.Symbol;
                    sp.DefaultAssignment = assignmentS.DefaultAssignment;
                }
                else if (assignment != null)
                {
                    if (sp == node)
                    {
                        node = assignment;
                        if (sp.Parent != null)
                            sp.ReplaceWith(assignment);
                    }
                    else
                    {
                        sp.ReplaceWith(assignment);
                    }
                }
                else if (sp.GetDefaultAssignment() is { Code: {} defaultAssignment })
                {
                    var newDefault = defaultAssignment.AssignParameters(p =>
                        parameterAssignment(p)?.FormatParametrizedScript()
                    );
                    if (newDefault != defaultAssignment)
                        sp.DefaultAssignment = new CodeParameterAssignment(newDefault);
                }
            }
            return node;
        }

        /// Wraps the expression in `dotvvm.evaluator.wrapObservable` if needed
        /// Note that this method may be used to process nodes that are already fully handled by all the transformation regarding observables -> it's fine to use to post-process a generate expression, but you shall not use it in custom method translator (see <see cref="ObservableTransformationAnnotation.EnsureWrapped"/> annotation instead)
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
                    new JsArrowFunctionExpression(
                        Enumerable.Empty<JsIdentifier>(),
                        expression.WithAnnotation(ShouldBeObservableAnnotation.Instance)
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
                return new JsIdentifierExpression("ko").Member("pureComputed").Invoke(new JsArrowFunctionExpression(
                        Enumerable.Empty<JsIdentifier>(),
                        expression
                    ))
                    .WithAnnotation(ResultIsObservableAnnotation.Instance)
                    .WithAnnotation(ShouldBeObservableAnnotation.Instance);
            }
        }
    }
}
