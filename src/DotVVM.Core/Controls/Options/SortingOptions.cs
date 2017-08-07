using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DotVVM.Framework.ViewModel;

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
            var parameterExpression = Expression.Parameter(typeof(T), "p");
            Expression sortByExpression = parameterExpression;
            foreach (var prop in (SortExpression ?? "").Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var property = sortByExpression.Type.GetTypeInfo().GetProperty(prop);
                if (property == null)
                    throw new Exception($"Could not sort by property '{prop}', since it does not exists.");
                if (property.GetCustomAttribute<BindAttribute>() is BindAttribute bind && bind.Direction == Direction.None)
                    throw new Exception($"Can not sort by an property '{prop}' that has [Bind(Direction.None)]");
                if (property.GetCustomAttribute<ProtectAttribute>() is ProtectAttribute protect && protect.Settings == ProtectMode.EncryptData)
                    throw new Exception($"Can not sort by an property '{prop}' that is encrypted");

                sortByExpression = Expression.Property(sortByExpression, property);
            }
            if (sortByExpression == parameterExpression) // no sorting
                return queryable;
            else
            {
                var lambdaExpression = Expression.Lambda(sortByExpression, parameterExpression);
                var methodCallExpression = Expression.Call(typeof(Queryable), GetSortingMethodName(),
                    new Type[] { parameterExpression.Type, sortByExpression.Type },
                    queryable.Expression,
                    Expression.Quote(lambdaExpression));
                return queryable.Provider.CreateQuery<T>(methodCallExpression);
            }
        }


        private string GetSortingMethodName()
        {
            return SortDescending ? "OrderByDescending" : "OrderBy";
        }
    }
}