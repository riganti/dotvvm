using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.Compilation.Javascript.Ast;

namespace DotVVM.Framework.Compilation.Javascript
{
    public class KnockoutObservableHandlingVisitor : JsNodeVisitor
    {
        private readonly bool AllowObservableResult;

        public KnockoutObservableHandlingVisitor(bool allowObservableResult)
        {
            this.AllowObservableResult = allowObservableResult;
        }

        private bool IsObservableResult(JsNode node) => node.HasAnnotation<ResultIsObservableAnnotation>() || node.HasAnnotation<ResultMayBeObservableAnnotation>();

        protected override void DefaultVisit(JsNode node)
        {
            if (node is JsExpression expression && IsObservableResult(node) && !node.Parent.HasAnnotation<ObservableUnwrapInvocationAnnotation>() && !(node.Role == JsAssignmentExpression.LeftRole && node.Parent is JsAssignmentExpression) && node.Parent != null)
            {
                if (ShouldUnwrap(node))
                {
                    // may be null is copied to the observable result
                    node.ReplaceWith(_ => KoUnwrap(expression, expression, !node.HasAnnotation<ResultIsObservableAnnotation>()));
                    node.RemoveAnnotations<MayBeNullAnnotation>();
                }
                else
                {
                    // may be null means that the value in the observable may be null. Which is not unwrapped, so the annotation is removed.
                    node.RemoveAnnotations<MayBeNullAnnotation>();
                }
            }
            else if (node is JsSymbolicParameter sp && sp.Symbol == JavascriptTranslator.KnockoutViewModelParameter && !ShouldUnwrap(node))
            {
                node.ReplaceWith(new JsSymbolicParameter(JavascriptTranslator.KnockoutContextParameter).Member("$rawData"));
            }
            base.DefaultVisit(node);
        }

        private bool ShouldUnwrap(JsNode node) =>
            !(AllowObservableResult && node.IsRootResultExpression()) &&
            !node.SatisfyResultCondition(n => n.HasAnnotation<ShouldBeObservableAnnotation>());

        public override void VisitAssignmentExpression(JsAssignmentExpression assignmentExpression)
        {
            base.VisitAssignmentExpression(assignmentExpression);

            // change assignment to observable property to observable invocation
            // only do for RestultIsObservable, not ResultMayBeObservable
            if (assignmentExpression.Left.HasAnnotation<ResultIsObservableAnnotation>())
            {
                var value = assignmentExpression.Right.Detach();
                var assignee = assignmentExpression.Left.Detach();
                assignee.RemoveAnnotations<ResultIsObservableAnnotation>();
                var resultType = value.GetResultType() ?? assignee.GetResultType();
                if (value.IsComplexType() || assignee.IsComplexType())
                    assignmentExpression.ReplaceWith(_ => new JsIdentifierExpression("dotvvm").Member("serialization").Member("deserialize").Invoke(value, assignee));
                else
                {
                    if (resultType.Type == typeof(DateTime) || resultType.Type == typeof(DateTime?))
                        value = new JsIdentifierExpression("dotvvm").Member("serialization").Member("serializeDate").Invoke(value);

                    // A = B -> A(B)
                    assignee.RemoveAnnotations<MayBeNullAnnotation>();
                    JsExpression newExpression = new JsInvocationExpression(assignee, value)
                        .WithAnnotation(ObservableSetterInvocationAnnotation.Instance);
                    if (assignmentExpression.Parent is JsExpression resultConsumer)
                    {
                        // assignment's result
                        if (assignee is JsMemberAccessExpression memberAccess)
                            // x.A(B) -> x.A(B).A
                            newExpression = AddAnnotations(newExpression.Member(memberAccess.MemberName).Invoke().WithAnnotation(ObservableUnwrapInvocationAnnotation.Instance), value);
                        else
                        {
                            // f() = CC -> f()(_a = CC), _a
                            var tmp = new JsTemporaryVariableParameter();
                            value.ReplaceWith(_ => new JsAssignmentExpression(new JsSymbolicParameter(tmp), value));
                            newExpression = AddAnnotations(new JsBinaryExpression(newExpression, BinaryOperatorType.Sequence, new JsSymbolicParameter(tmp)), value);
                        }
                    }
                    assignmentExpression.ReplaceWith(newExpression);
                }
            }
        }

        private JsExpression KoUnwrap(JsExpression expr, JsExpression rootExpression, bool weakObservable) =>
            AddAnnotations(weakObservable ? new JsIdentifierExpression("ko").Member("unwrap").Invoke(expr) : expr.Invoke(), rootExpression);

        private JsExpression AddAnnotations(JsExpression expr, JsExpression originalNode) =>
            expr.WithAnnotation(expr is JsInvocationExpression ? ObservableUnwrapInvocationAnnotation.Instance : null)
                .WithAnnotation(originalNode.Annotation<VMPropertyInfoAnnotation>())
                .WithAnnotation(originalNode.Annotation<ViewModelInfoAnnotation>())
                .WithAnnotation(originalNode.Annotation<MayBeNullAnnotation>());
    }
}
