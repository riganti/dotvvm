using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DotVVM.Framework.Controls;

namespace DotVVM.Core.Controls
{
    public static class GridViewDataSetExtensions
    {

        public static IQueryable<T> ApplySortExpression<T>(this ISortOptions options, IQueryable<T> queryable)
        {
            if (string.IsNullOrEmpty(options.SortExpression))
            {
                return queryable;
            }

            var type = typeof(T);
            var property = type.GetTypeInfo().GetProperty(options.SortExpression);

            if (property == null)
            {
                throw new Exception($"Could not sort by property '{options.SortExpression}', since it does not exists.");
            }

            var parameter = Expression.Parameter(type, "p");
            var propertyAccess = Expression.MakeMemberAccess(parameter, property);
            var orderBy = Expression.Lambda(propertyAccess, parameter);

            var result = Expression.Call(typeof(Queryable),
                GetSortingMethodName(options),
                new[] { type, property.PropertyType },
                queryable.Expression,
                Expression.Quote(orderBy));

            return queryable.Provider.CreateQuery<T>(result);
        }

        private static string GetSortingMethodName(ISortOptions options)
        {
            return options.SortDescending ? "OrderByDescending" : "OrderBy";
        }




        public static IQueryable<T> ApplyPaging<T>(this IPagingOptions pagingOptions, IQueryable<T> queryable)
        {
            if (pagingOptions.PageSize <= 0)
            {
                return queryable;
            }

            return queryable.Skip(pagingOptions.PageSize * pagingOptions.PageIndex).Take(pagingOptions.PageSize);
        }

    }
}
