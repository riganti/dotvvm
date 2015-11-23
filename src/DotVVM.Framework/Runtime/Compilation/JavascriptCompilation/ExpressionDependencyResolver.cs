using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Runtime.Compilation.JavascriptCompilation
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
