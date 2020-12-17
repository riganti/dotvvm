using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DotVVM.Framework.Query;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Framework.Controls
{
    public class DefaultGridSorter<T>: IDataSetSorter<T>
    {
        /// <summary>
        /// Gets or sets whether the sort order should be descending.
        /// </summary>
        public bool SortDescending { get; set; }

        /// <summary>
        /// Gets or sets the name of the property that is used for sorting. Null means the grid should not be sorted.
        /// </summary>
        public string? SortExpression { get; set; }

        public IQuery<T> Apply(IQuery<T> query)
        {
            if (SortExpression == null)
            {
                return query;
            }

            var parameterExpression = Expression.Parameter(query.ElementType, "p");
            Expression sortByExpression = parameterExpression;
            foreach (var prop in (SortExpression ?? "").Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var property = sortByExpression.Type.GetTypeInfo().GetProperty(prop);
                if (property == null)
                {
                    throw new Exception($"Could not sort by property '{prop}', since it does not exists.");
                }
                if (property.GetCustomAttribute<BindAttribute>() is BindAttribute bind && bind.Direction == Direction.None)
                {
                    throw new Exception($"Can not sort by an property '{prop}' that has [Bind(Direction.None)].");
                }
                if (property.GetCustomAttribute<ProtectAttribute>() is ProtectAttribute protect && protect.Settings == ProtectMode.EncryptData)
                {
                    throw new Exception($"Can not sort by an property '{prop}' that is encrypted.");
                }

                sortByExpression = Expression.Property(sortByExpression, property);
            }
            if (sortByExpression == parameterExpression) // no sorting
            {
                return query;
            }
            var lambdaExpression = Expression.Lambda(sortByExpression, parameterExpression);
            var methodCallExpression = Expression.Call(typeof(Queryable), GetSortingMethodName(),
                new[] { parameterExpression.Type, sortByExpression.Type },
                query.Expression,
                Expression.Quote(lambdaExpression));
            return new Query<T>(query.ElementType, methodCallExpression);
        }

        private string GetSortingMethodName() => SortDescending ? "OrderByDescending" : "OrderBy";

        public void ColumnSortClick(IBaseGridViewDataSet<T> dataSet, string? columnName)
        {
            if (SortExpression == columnName)
            {
                SortDescending ^= true;
            }
            else
            {
                SortExpression = columnName;
                SortDescending = false;
            }
            (dataSet as IPageableGridViewDataSet<T, IDataSetIndexPager<T>>)?.GoToFirstPage();
        }
    }
}
