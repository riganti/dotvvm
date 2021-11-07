using System.Linq.Expressions;
using System.Threading.Tasks;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.Utils;

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
                    return VisitBlock(Expression.Block(first, second));
                }
            }

            return base.VisitMethodCall(expression);
        }

    }
}
