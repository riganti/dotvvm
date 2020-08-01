using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DotVVM.Framework.Query;

namespace DotVVM.Framework.Query
{
    /// <summary> Like IQueryable, but can't be executed without the IQueryProvider </summary>
    public interface IQuery<out T>
    {
        Type ElementType { get; }
        Expression Expression { get; }
    }

    public class Query<T> : IQuery<T>
    {
        public Query(Type elementType, Expression expression)
        {
            this.ElementType = elementType;
            this.Expression = expression;

        }
        public Type ElementType { get; }

        public Expression Expression { get; }
    }

    class HackQueryable<T> : IQueryable<T>
    {
        internal HackQueryable(Type elementType, Expression expression, IQueryProvider provider)
        {
            this.ElementType = elementType;
            this.Expression = expression;
            this.Provider = provider;
        }
        public Type ElementType { get; }

        public Expression Expression { get; }

        public IQueryProvider Provider { get; }

        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }


    internal class NonExecutableQueryProvider : IQueryProvider
    {
        public static NonExecutableQueryProvider Instance = new NonExecutableQueryProvider();
        // copy from https://source.dot.net/#System.Linq.Queryable/System/Linq/TypeHelper.cs,13
        internal static Type? FindGenericType(Type definition, Type type)
        {
            bool? definitionIsInterface = null;
            while (type != null && type != typeof(object))
            {
                if (type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == definition)
                    return type;
                if (!definitionIsInterface.HasValue)
                    definitionIsInterface = definition.GetTypeInfo().IsInterface;
                if (definitionIsInterface.GetValueOrDefault())
                {
                    foreach (Type ifc in type.GetTypeInfo().GetInterfaces())
                    {
                        Type? found = FindGenericType(definition, ifc);
                        if (found != null)
                            return found;
                    }
                }
                type = type.GetTypeInfo().BaseType!;
            }
            return null;
        }

        public IQueryable CreateQuery(Expression expression)
        {
            var elementType = FindGenericType(typeof(IQueryable<>), expression.Type);
            if (elementType is null)
                throw new ArgumentException($"Expression must be of type IQueryable<T>, but is {expression.Type}", nameof(expression));
            return new HackQueryable<object>(elementType, expression, this);
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            var elementType = FindGenericType(typeof(IQueryable<>), expression.Type) ?? typeof(TElement);
            return new HackQueryable<TElement>(elementType, expression, this);
        }

        public object Execute(Expression expression)
        {
            throw new NotImplementedException();
        }

        public TResult Execute<TResult>(Expression expression)
        {
            throw new NotImplementedException();
        }
    }
}

public static partial class DotvvmFrameworkQueryExtensions
{
    public static IQuery<T> AsQuery<T>(this IQueryable<T> q) =>
        new Query<T>(q.ElementType, q.Expression);

    public static IQuery<T> AsConstantQuery<T>(this IEnumerable<T> collection) =>
        new Query<T>(typeof(T), Expression.Constant(collection));

    public static IQuery<object> AsQuery(this IQueryable q) =>
        new Query<object>(q.ElementType, q.Expression);

    public static IQueryable<T> AsQueryable<T>(this IQuery<T> q, IQueryProvider qp) =>
        qp.CreateQuery<T>(q.Expression);

    internal static IQueryable<T> AsNonExecutableQueryable<T>(this IQuery<T> q) =>
        NonExecutableQueryProvider.Instance.CreateQuery<T>(q.Expression);

    public static IQuery<U> ApplyQueryableTransform<T, U>(this IQuery<T> q, Func<IQueryable<T>, IQueryable<U>> transform) =>
        transform(q.AsNonExecutableQueryable())
        .AsQuery();
}
