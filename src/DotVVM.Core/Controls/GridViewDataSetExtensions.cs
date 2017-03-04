using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Controls
{
    public static class GridViewDataSetExtensions
    {
        public static IQueryable<T> ApplyGridViewDataSetOptions<T>(this IQueryable<T> query,
            IGridViewDataSetLoadOptions gridViewDataSetLoadOptions)
        {
            return
                query.ApplySortOptions(gridViewDataSetLoadOptions.SortOptions)
                    .ApplyPagingOptions(gridViewDataSetLoadOptions.PagingOptions);
        }

        public static IQueryable<T> ApplySortOptions<T>(this IQueryable<T> query,
            IGridViewDataSetLoadOptions gridViewDataSetLoadOptions)
        {
            return query.ApplySortOptions(gridViewDataSetLoadOptions.SortOptions);
        }

        public static IQueryable<T> ApplyPagingOptions<T>(this IQueryable<T> query,
            IGridViewDataSetLoadOptions gridViewDataSetLoadOptions)
        {
            return query.ApplyPagingOptions(gridViewDataSetLoadOptions.PagingOptions);
        }

        public static IQueryable<T> ApplyPagingOptions<T>(this IQueryable<T> query, IPagingOptions pagingOptions)
        {
            if (pagingOptions.PageSize <= 0)
            {
                return query;
            }
            return query.Skip(pagingOptions.PageIndex * pagingOptions.PageSize)
                .Take(pagingOptions.PageSize);
        }


        public static IQueryable<T> ApplySortOptions<T>(this IQueryable<T> query, ISortOptions sortOptions)
        {
            if (!string.IsNullOrEmpty(sortOptions.SortExpression))
            {
                var type = typeof(T);
                var property = type.GetTypeInfo().GetProperty(sortOptions.SortExpression);
                if (property == null)
                {
                    throw new Exception(
                        $"Could not sort by property '{sortOptions.SortExpression}', since it does not exists.");
                }
                var parameterExpression = Expression.Parameter(type, "p");
                var lambdaExpression = Expression.Lambda(Expression.MakeMemberAccess(parameterExpression, property),
                    parameterExpression);
                var methodCallExpression = Expression.Call(typeof(Queryable),
                    GetSortingMethodName(sortOptions),
                    new Type[2]
                    {
                        type,
                        property.PropertyType
                    },
                    query.Expression,
                    Expression.Quote(lambdaExpression));

                return query.Provider.CreateQuery<T>(methodCallExpression);
            }
            return query;
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


        public static GridViewDataSetLoadedData<T> GetDataFromQueryable<T>(this IBaseGridViewDataSet baseGridViewDataSet,
            IQueryable<T> queryable)
        {
            var totalItemsCount = queryable.Count();
           
            if (baseGridViewDataSet is ISortableGridViewDataSet sortableDataSet)
            {
                queryable = queryable.ApplySortOptions(sortableDataSet.SortOptions);
            }
            if (baseGridViewDataSet is IPageableGridViewDataSet pageableDataSet)
            {
                queryable = queryable.ApplyPagingOptions(pageableDataSet.PagingOptions);
            }
            var items = queryable.ToList();
            return new GridViewDataSetLoadedData<T>(items, totalItemsCount);
           
        }
    }
}