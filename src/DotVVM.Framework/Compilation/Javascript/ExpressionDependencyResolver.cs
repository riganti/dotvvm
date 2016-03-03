using System.Collections.Generic;
using System.Linq.Expressions;

namespace DotVVM.Framework.Compilation.Javascript
{
    public class ExpressionDependencyResolver: ExpressionVisitor
    {


        public static void DoTheDesiredStuff(Dictionary<ParameterExpression, Expression> references, Expression root)
        {
            var order = new List<Expression>();
            var set = new HashSet<ParameterExpression>();
        }


    }
}
