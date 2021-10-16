
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace DotVVM.Framework.Compilation.Static
{
    internal class UnallowedConstantRemover : ExpressionVisitor
    {
        public static (Expression, (ParameterExpression, object)[]) ReplaceBadConstants(Expression expr)
        {
            var v = new UnallowedConstantRemover();
            return (v.Visit(expr), v.Replacements.ToArray());
        }

        public readonly List<(ParameterExpression, object)> Replacements = new List<(ParameterExpression, object)>();

        private UnallowedConstantRemover()
        {
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (node.Value == null)
                return base.VisitConstant(node);
            var type = node.Value.GetType();
            if (type.IsPrimitive || type == typeof(string) || typeof(MemberInfo).IsAssignableFrom(type))
                return base.VisitConstant(node);
            var p = Expression.Parameter(node.Type);
            Replacements.Add((p, node.Value));
            return p;
        }
    }
}
