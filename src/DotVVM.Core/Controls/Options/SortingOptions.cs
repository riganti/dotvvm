using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Represents a settings for sorting.
    /// </summary>
    public class SortingOptions : ISortingOptions
    {
        /// <summary>
        /// Gets or sets whether the sort order should be descending.
        /// </summary>
        public bool SortDescending { get; set; }

        /// <summary>
        /// Gets or sets the name of the property that is used for sorting.
        /// </summary>
        public string SortExpression { get; set; }



        /// <summary>
        /// Applies the sorting settings to the IQueryable object.
        /// </summary>
        public virtual IQueryable<T> ApplyToQueryable<T>(IQueryable<T> queryable)
        {
            if (!string.IsNullOrEmpty(SortExpression))
            {
                var type = typeof(T);
                var property = type.GetTypeInfo().GetProperty(SortExpression);
                if (property == null)
                {
                    throw new Exception($"Could not sort by property '{SortExpression}', since it does not exists.");
                }
                var parameterExpression = Expression.Parameter(type, "p");
                var lambdaExpression = Expression.Lambda(Expression.MakeMemberAccess(parameterExpression, property), parameterExpression);
                var methodCallExpression = Expression.Call(typeof(Queryable), GetSortingMethodName(),
                    new Type[2]
                    {
                        type,
                        property.PropertyType
                    },
                    queryable.Expression,
                    Expression.Quote(lambdaExpression));

                return queryable.Provider.CreateQuery<T>(methodCallExpression);
            }
            return queryable;
        }


        private string GetSortingMethodName()
        {
            return SortDescending ? "OrderByDescending" : "OrderBy";
        }
    }
}