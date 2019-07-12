using System.Linq.Expressions;
using DotVVM.Framework.Runtime;

namespace DotVVM.Framework.Compilation.Javascript
{
    public class TaskSequenceRewriterExpressionVisitor : ExpressionVisitor
    {

        protected override Expression VisitMethodCall(MethodCallExpression expression)
        {
            if (expression.Method.DeclaringType == typeof(CommandTaskSequenceHelper))
            {
                // replace helper methods for joining tasks and handling async assignments
                if (expression.Method.Name == nameof(CommandTaskSequenceHelper.JoinTasks))
                {
                    var first = expression.Arguments[0];
                    var second = ((LambdaExpression)expression.Arguments[1]).Body;
                    if (second is MethodCallExpression secondMethod)
                    {
                        if (secondMethod.Method.DeclaringType == typeof(CommandTaskSequenceHelper)
                            && secondMethod.Method.Name == nameof(CommandTaskSequenceHelper.WrapAsTask))
                        {
                            second = ((LambdaExpression)secondMethod.Arguments[0]).Body;
                        }
                    }
                    return VisitBlock(Expression.Block(first, second));
                }
                else if (expression.Method.Name == nameof(CommandTaskSequenceHelper.AssignTaskResult))
                {
                    // decompose assignment of async method
                    var rightSide = Expression.Property(expression.Arguments[0], "Result");
                    var assignmentBody = ((LambdaExpression)expression.Arguments[1]).Body;
                    var leftSide = ((BinaryExpression)assignmentBody).Left;
                    return VisitBinary(Expression.MakeBinary(ExpressionType.Assign, leftSide, rightSide));
                }
            }

            return base.VisitMethodCall(expression);
        }

    }
}
