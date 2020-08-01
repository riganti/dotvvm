using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DotVVM.Framework.Query
{
    public interface IScalarQuery<out T>
    {
        Expression Expression { get; }
    }
    public sealed class ScalarQuery<T>: IScalarQuery<T>
    {
        public ScalarQuery(Expression expression)
        {
            Expression = expression;
            if (!typeof(T).GetTypeInfo().IsAssignableFrom(expression.Type))
                throw new ArgumentException($"Expression type {expression.Type} is not assignable to {typeof(T)}", nameof(expression));
        }

        public Expression Expression { get; }
    }

    public static class ScalarQueryHelper
    {
        // class CaptureExpressionQueryProvider : IQueryProvider
        // {
        //     public IQueryable CreateQuery(Expression expression) => CreateQuery<object>(expression);
        //     public IQueryable<TElement> CreateQuery<TElement>(Expression expression) =>
        //         new HackQueryable<TElement>(
        //             NonExecutableQueryProvider.FindGenericType(typeof(IQueryable<>), expression.Type) ?? typeof(TElement),
        //             expression,
        //             this
        //         );
        //     public Expression? LastExecutedExpression { get; private set; }
        //     public object Execute(Expression expression) => Execute<object>(expression);
        //     public TResult Execute<TResult>(Expression expression)
        //     {
        //         LastExecutedExpression = expression;
        //         return default!;
        //     }
        // }

        private class ReplaceAndInlineParametersVisitor : ExpressionVisitor
        {
            public Dictionary<ParameterExpression, Expression> Params { get; } = new Dictionary<ParameterExpression, Expression>();
            protected override Expression VisitParameter(ParameterExpression node)
            {
                if (Params.TryGetValue(node, out var r)) return r;
                else return base.VisitParameter(node);
            }
            protected override Expression VisitMember(MemberExpression node)
            {
                var e = Visit(node.Expression);
                if (e is ConstantExpression constant && node.Member is FieldInfo field)
                {
                    var v = field.GetValue(constant.Value);
                    if (v is Expression vExpression)
                        return Expression.Quote(vExpression);
                    else return Expression.Constant(v);
                }
                return node.Update(e);
            }
        }

        // public ScalarQuery<U> Make<T, U>(IQuery<T> q, Func<IQueryable<T>, U> applier)
        // {

        // }

        public static ScalarQuery<U> Make<T, U>(IQuery<T> q, Expression<Func<IQueryable<T>, U>> applier)
        {
            var v = new ReplaceAndInlineParametersVisitor { Params = {
                [applier.Parameters.Single()] = q.Expression
            } };
            var nextExpr = v.Visit(q.Expression);
            return new ScalarQuery<U>(nextExpr);
        }
        public static ScalarQuery<U> Make<T, U>(IScalarQuery<T> q, Expression<Func<T, U>> applier)
        {
            var v = new ReplaceAndInlineParametersVisitor { Params = {
                [applier.Parameters.Single()] = q.Expression
            } };
            var nextExpr = v.Visit(q.Expression);
            return new ScalarQuery<U>(nextExpr);
        }
    }
}
