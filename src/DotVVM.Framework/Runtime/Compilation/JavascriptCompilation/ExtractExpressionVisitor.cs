using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Runtime.Compilation.JavascriptCompilation
{
    public class ExtractExpressionVisitor : ExpressionVisitor
    {
        public List<ParameterExpression> ParameterOrder { get; set; } = new List<ParameterExpression>();
        public Dictionary<ParameterExpression, Expression> Replaced { get; set; } = new Dictionary<ParameterExpression, Expression>();
        public Predicate<Expression> Predicate { get; set; }
        readonly string ParameterPrefix;

        public ExtractExpressionVisitor(Predicate<Expression> predicate, string parameterPrefix = "r_")
        {
            Predicate = predicate;
            ParameterPrefix = parameterPrefix;
        }

        public override Expression Visit(Expression node)
        {
            if (node == null) return null;
            node = base.Visit(node);
            if (Predicate(node))
            {
                var par = Expression.Parameter(node.Type, "r_" + Replaced.Count);
                Replaced.Add(par, node);
                ParameterOrder.Add(par);
                return par;
            }
            else return node;
        }
    }
}
