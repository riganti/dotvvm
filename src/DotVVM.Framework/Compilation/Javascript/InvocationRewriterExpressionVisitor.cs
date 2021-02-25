using System;
using System.Linq.Expressions;

namespace DotVVM.Framework.Compilation.Javascript
{
    public class InvocationRewriterExpressionVisitor : ExpressionVisitor
    {
        protected override Expression VisitInvocation(InvocationExpression node)
        {
            var invokeMethod = node.Expression.Type.GetMethod("Invoke");

            return Expression.Call(node.Expression, invokeMethod, node.Arguments); ;
        }
    }
}
