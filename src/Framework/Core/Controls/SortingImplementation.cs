using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Framework.Controls
{
    public static class SortingImplementation
    {

        /// <summary>
        /// Applies sorting to the <paramref name="queryable" /> after the total number
        /// of items is retrieved.
        /// </summary>
        /// <param name="queryable">The <see cref="IQueryable{T}" /> to modify.</param>
        public static IQueryable<T> ApplySortingToQueryable<T>(IQueryable<T> queryable, ISortingSingleCriterionCapability options)
        {
            if (options.SortExpression == null)
            {
                return queryable;
            }

            var parameterExpression = Expression.Parameter(typeof(T), "p");
            Expression sortByExpression = parameterExpression;
            foreach (var prop in (options.SortExpression ?? "").Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var property = sortByExpression.Type.GetProperty(prop);
                if (property == null)
                {
                    throw new Exception($"Could not sort by property '{prop}', since it does not exists.");
                }
                if (property.GetCustomAttribute<BindAttribute>() is BindAttribute bind && bind.Direction == Direction.None)
                {
                    throw new Exception($"Cannot sort by an property '{prop}' that has [Bind(Direction.None)].");
                }
                if (property.GetCustomAttribute<ProtectAttribute>() is ProtectAttribute protect && protect.Settings == ProtectMode.EncryptData)
                {
                    throw new Exception($"Cannot sort by an property '{prop}' that is encrypted.");
                }

                sortByExpression = Expression.Property(sortByExpression, property);
            }
            if (sortByExpression == parameterExpression) // no sorting
            {
                return queryable;
            }
            var lambdaExpression = Expression.Lambda(sortByExpression, parameterExpression);
            var methodCallExpression = Expression.Call(typeof(Queryable), GetSortingMethodName(options.SortDescending),
                new[] { parameterExpression.Type, sortByExpression.Type },
                queryable.Expression,
                Expression.Quote(lambdaExpression));
            return queryable.Provider.CreateQuery<T>(methodCallExpression);
        }

        private static string GetSortingMethodName(bool sortDescending)
        {
            return sortDescending ? "OrderByDescending" : "OrderBy";
        }

    }
}
