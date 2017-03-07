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

        protected override void DefaultVisit(JsNode node)
        {
            if (node is JsExpression expression && node.HasAnnotation<ResultIsObservableAnnotation>() && !node.Parent.HasAnnotation<ObservableUnwrapInvocationAnnotation>() && !(node.Role == JsAssignmentExpression.LeftRole && node.Parent is JsAssignmentExpression) && node.Parent != null)
            {
                if (!AllowObservableResult || !node.IsRootResultExpression())
                {
                    // may be null is copied to the observable result
                    node.RemoveAnnotations<MayBeNullAnnotation>();
                    node.ReplaceWith(_ => KoUnwrap(expression, expression, false));
                }
            }
            base.DefaultVisit(node);
        }

        public override void VisitAssignmentExpression(JsAssignmentExpression assignmentExpression)
        {
            base.VisitAssignmentExpression(assignmentExpression);

            if (assignmentExpression.Left.HasAnnotation<ResultIsObservableAnnotation>())
            {
                var value = assignmentExpression.Right.Detach();
                var assignee = assignmentExpression.Left.Detach();
                assignee.RemoveAnnotations<ResultIsObservableAnnotation>();
                if (value.IsComplexType())
                    assignmentExpression.ReplaceWith(_ => new JsIdentifierExpression("dotvvm").Member("serialization").Member("deserialize").Invoke(value, assignee));
                else
                {
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

        private JsExpression KoUnwrap(JsExpression expr, JsExpression rootExpression, bool isRootResult) =>
            isRootResult ? (rootExpression == expr ? expr : AddAnnotations(expr, rootExpression)) : AddAnnotations(expr.Invoke(), rootExpression);

        private JsExpression AddAnnotations(JsExpression expr, JsExpression originalNode) =>
            expr.WithAnnotation(expr is JsInvocationExpression ? ObservableUnwrapInvocationAnnotation.Instance : null)
                .WithAnnotation(originalNode.Annotation<VMPropertyInfoAnnotation>())
                .WithAnnotation(originalNode.Annotation<ViewModelInfoAnnotation>())
                .WithAnnotation(originalNode.Annotation<MayBeNullAnnotation>());
    }
}
