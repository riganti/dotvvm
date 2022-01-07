using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using DotVVM.Framework.Compilation.Javascript.Ast;
using DotVVM.Framework.ViewModel.Serialization;

namespace DotVVM.Framework.Compilation.Javascript
{
    public class JavascriptNullCheckAdder : JsNodeVisitor
    {
        private JsNode? root;
        public JsNode? NewExpression { get; set; }

        public JavascriptNullCheckAdder(JsNode? root)
        {
            this.root = root;
        }

        public override void VisitConditionalExpression(JsConditionalExpression conditionalExpression)
        {
            base.VisitConditionalExpression(conditionalExpression);
            if (conditionalExpression.TrueExpression.HasAnnotation(MayBeNullAnnotation.Instance) ||
                conditionalExpression.FalseExpression.HasAnnotation(MayBeNullAnnotation.Instance))
                conditionalExpression.AddAnnotation(MayBeNullAnnotation.Instance);
        }

        public override void VisitInvocationExpression(JsInvocationExpression invocationExpression)
        {
            base.VisitInvocationExpression(invocationExpression);
            ProcessTargetedExpression(invocationExpression, null);
        }

        public override void VisitIndexerExpression(JsIndexerExpression indexerExpression)
        {
            base.VisitIndexerExpression(indexerExpression);
            ProcessTargetedExpression(indexerExpression, new JsArrayExpression());
        }

        public override void VisitMemberAccessExpression(JsMemberAccessExpression memberAccessExpression)
        {
            base.VisitMemberAccessExpression(memberAccessExpression);
            ProcessTargetedExpression(memberAccessExpression, new JsObjectExpression());
        }

        protected void ProcessTargetedExpression(JsExpression expression, JsExpression? defaultValue)
        {
            var target = expression.GetChildByRole(JsTreeRoles.TargetExpression)!;
            if (target.HasAnnotation(MayBeNullAnnotation.Instance))
            {
                // A().B -> A()?.B
                if (expression is JsMemberAccessExpression memberAccessExpression)
                {
                    memberAccessExpression.IsOptional = true;
                }
                // A()[index] -> (A() || []).[index]
                else if (defaultValue != null &&
                    (expression.HasAnnotation(MayBeNullAnnotation.Instance) || expression.IsRootResultExpression()) &&
                    IntroduceVariableFor(target, 1))
                {
                    target.ReplaceWith(_ =>
                        new JsBinaryExpression(target, BinaryOperatorType.NullishCoalescing, defaultValue));
                }
                // {A().B}.MORESTUFF -> (a=A()) == null : null ? a.B.MORESTUFF
                else
                {
                    var dependentExpression = GetDependentAncestorNode(expression);
                    if (IntroduceVariableFor(target))
                    {
                        var variable = new JsSymbolicParameter(new JsTemporaryVariableParameter());
                        target.ReplaceWith(variable);
                        target = new JsAssignmentExpression(variable.Clone(), target);
                    }
                    else target = target.Clone();

                    ReplaceOrEmit(dependentExpression, _ => {
                        return CreateNullCondition(target, dependentExpression).WithAnnotation(MayBeNullAnnotation.Instance);
                    });
                }
            }
        }

        protected JsExpression CreateNullCondition(JsExpression target, JsExpression expression)
        {
            if (target.IsComplexType())
                return new JsBinaryExpression(target, BinaryOperatorType.ConditionalAnd, expression);
            else
                return new JsConditionalExpression(new JsBinaryExpression(target, BinaryOperatorType.Equal, new JsLiteral(null)), new JsLiteral(null), expression);
        }

        protected bool IntroduceVariableFor(JsExpression expression, int limit = 2)
        {
            if (expression is JsIdentifierExpression) return false;
            if (expression is JsSymbolicParameter symbol) return false;
            if (limit > 0 && (expression is JsMemberAccessExpression memberAccess ||
                              expression is JsInvocationExpression invocation && invocation.Arguments.Count == 0 && invocation.HasAnnotation(ObservableUnwrapInvocationAnnotation.Instance)))
                return IntroduceVariableFor(expression.GetChildByRole(JsTreeRoles.TargetExpression)!, limit - 1);
            return true;
        }

        private void ReplaceOrEmit<T>(T node, Func<T, JsNode> replacer)
            where T : JsNode
        {
            if (node.Parent == null)
            {
                Debug.Assert(node.Parent == root);
                node.Remove();
                NewExpression = replacer(node);
            }
            else
            {
                var e = node.ReplaceWith(n => replacer((T)n));
                if (node.Parent == root) NewExpression = e;
            }
        }

        JsExpression GetDependentAncestorNode(JsExpression expr)
        {
            while (expr.Parent is JsExpression parent && !expr.HasAnnotation(MayBeNullAnnotation.Instance) && expr.Role == JsTreeRoles.TargetExpression)
                expr = parent;
            return expr;
        }

        public static JsExpression AddNullChecks(JsExpression expression)
        {
            var visitor = new JavascriptNullCheckAdder(expression.Parent);
            expression.AcceptVisitor(visitor);
            return (JsExpression?)visitor.NewExpression ?? expression;
        }
    }

    public sealed class MayBeNullAnnotation
    {
        public static MayBeNullAnnotation Instance = new MayBeNullAnnotation();
        MayBeNullAnnotation() { }
    }
}
