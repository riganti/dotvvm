using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DotVVM.Framework.Compilation.Binding
{
    /// <summary> Takes all variables from block expressions and hoists them into a top-level expression.
    /// This prevents glitches with undefined variables that would be introduced by following steps in static command translation. </summary>
    public class VariableHoistingVisitor: ExpressionVisitor
    {
        public List<ParameterExpression> Variables { get; } = new List<ParameterExpression>();
        protected override Expression VisitBlock(BlockExpression node)
        {
            Variables.AddRange(node.Variables);

            if (node.Expressions.Count == 1)
                return node.Expressions.Single();
            return node.Update(Enumerable.Empty<ParameterExpression>(), node.Expressions);
        }

        /// <summary> Takes all variables from block expressions and hoists them into a top-level expression.
        /// This prevents glitches with undefined variables that would be introduced by following steps in static command translation. </summary>
        public static Expression HoistVariables(Expression expression)
        {
            var v = new VariableHoistingVisitor();
            expression = v.Visit(expression);

            if (v.Variables.Count == 0)
            {
                return expression;
            }
            else if (expression is BlockExpression blockExpression)
            {
                return blockExpression.Update(v.Variables, blockExpression.Expressions);
            }
            else
            {
                return Expression.Block(v.Variables, new [] { expression });
            }
        }
    }
}
