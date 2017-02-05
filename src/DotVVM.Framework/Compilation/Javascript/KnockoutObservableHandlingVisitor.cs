using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.Compilation.Javascript.Ast;

namespace DotVVM.Framework.Compilation.Javascript
{
    public class KnockoutObservableHandlingVisitor : JsNodeVisitor
    {
        protected override void DefaultVisit(JsNode node)
        {
            if (node is JsExpression expression && node.HasAnnotation<ResultIsObservableAnnotation>() && !node.Parent.HasAnnotation<ObservableUnwrapInvocationAnnotation>() && node.Parent != null)
            {
                if (node.Role == JsAssignmentExpression.LeftRole && node.Parent is JsAssignmentExpression parentAssignment)
                {
                    if (expression.IsComplexType())
                        parentAssignment.ReplaceWith(_ => new JsIdentifierExpression("dotvvm").Member("serialization").Member("deserialize").Invoke(parentAssignment.Right, parentAssignment.Left));
                    else
                    {
                        var assignment = (JsExpression)parentAssignment.ReplaceWith(_ => new JsInvocationExpression(parentAssignment.Left, parentAssignment.Right)
                            .WithAnnotation(ObservableSetterInvocationAnnotation.Instance));
                        if (assignment.Parent is JsExpression resultConsumer)
                        {
                            // assignment's result
                            if (resultConsumer is JsMemberAccessExpression memberAccess)
                                assignment.ReplaceWith(_ => AddAnnotations(assignment.Member(memberAccess.MemberName).Invoke().WithAnnotation(ObservableUnwrapInvocationAnnotation.Instance), expression));
                            else
                            {
                                var tmp = new JsTemporaryVariableParameter();
                                parentAssignment.Left.ReplaceWith(l => new JsAssignmentExpression(new JsSymbolicParameter(tmp), (JsExpression)l));
                                assignment.ReplaceWith(_ => AddAnnotations(new JsBinaryExpression(assignment, BinaryOperatorType.Sequence, new JsSymbolicParameter(tmp)), expression));
                            }
                        }
                    }
                }
                else if (node.Parent is JsExpression parent)
                {
                    var isRoot = node.IsRootResultExpression();
                    node.ReplaceWith(_ => KoUnwrap(expression, expression, isRoot));
                }
                node.RemoveAnnotations<MayBeNullAnnotation>();

            }
            base.DefaultVisit(node);
        }

        private JsExpression KoUnwrap(JsExpression expr, JsExpression rootExpression, bool isRootResult) =>
            isRootResult ? (rootExpression == expr ? expr : AddAnnotations(expr, rootExpression)) : AddAnnotations(expr.Invoke(), rootExpression);

        private JsExpression AddAnnotations(JsExpression expr, JsExpression originalNode) =>
            expr.WithAnnotation(expr is JsInvocationExpression ? ObservableUnwrapInvocationAnnotation.Instance: null)
                .WithAnnotation(originalNode.Annotation<VMPropertyInfoAnnotation>())
                .WithAnnotation(originalNode.Annotation<ViewModelInfoAnnotation>())
                .WithAnnotation(originalNode.Annotation<MayBeNullAnnotation>());
    }
}
