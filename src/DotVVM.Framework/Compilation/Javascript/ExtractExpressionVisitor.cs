using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Compilation.Javascript
{
    public class ExtractExpressionVisitor : ExpressionVisitor
    {
        public List<ParameterExpression> ParameterOrder { get; set; } = new List<ParameterExpression>();
        public Dictionary<ParameterExpression, Expression> Replaced { get; set; } = new Dictionary<ParameterExpression, Expression>();
        public Func<Expression, Func<ParameterExpression, BindingParameterAnnotation>> Predicate { get; set; }
        readonly string ParameterPrefix;

        public ExtractExpressionVisitor(Func<Expression, Func<ParameterExpression, BindingParameterAnnotation>> predicate, string parameterPrefix = "r_")
        {
            Predicate = predicate;
            ParameterPrefix = parameterPrefix;
        }

        public override Expression Visit(Expression node)
        {
            if (node == null) return null;
            node = base.Visit(node);
            var annotator = Predicate(node);
            if (annotator != null)
            {
                var par = Expression.Parameter(node.Type == typeof(void) ? typeof(object) : node.Type, "r_" + Replaced.Count);
                annotator(par)?.Apply(par.AddParameterAnnotation);
                Replaced.Add(par, node);
                ParameterOrder.Add(par);
                return par;
            }
            else return node;
        }
    }
}
