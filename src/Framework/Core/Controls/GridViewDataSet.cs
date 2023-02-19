using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DotVVM.Framework.Querying;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Represents a collection of items with paging, sorting and row edit capabilities.
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="Items" /> elements.</typeparam>
    public class GridViewDataSet<T> : IGridViewDataSet<T>
    {
        /// <summary>
        /// Gets or sets whether the data should be refreshed. This property is set to true automatically
        /// when paging, sorting or other options change.
        /// </summary>
        public bool IsRefreshRequired { get; set; } = true;

        /// <summary>
        /// Gets or sets the items for the current page.
        /// </summary>
        public IList<T> Items { get; set; } = new List<T>();

        IList IBaseGridViewDataSet.Items => Items is List<T> list ? list : Items.ToList();

        /// <summary>
        /// Gets or sets the settings for paging.
        /// </summary>
        public IPagingOptions PagingOptions { get; set; } = new PagingOptions();

        /// <summary>
        /// Gets or sets the settings for row (item) edit feature.
        /// </summary>
        public IRowEditOptions RowEditOptions { get; set; } = new RowEditOptions();

        /// <summary>
        /// Gets or sets the settings for sorting.
        /// </summary>
        public ISortingOptions SortingOptions { get; set; } = new SortingOptions();

        /// <summary>
        /// Navigates to the specific page.
        /// </summary>
        /// <param name="index">The zero-based index of the page to navigate to.</param>
        public void GoToPage(int index)
        {
            PagingOptions.PageIndex = index;
            RequestRefresh();
        }

        /// <summary>
        /// Loads data into the <see cref="GridViewDataSet{T}" /> from the given <see cref="IQueryable{T}" /> source.
        /// </summary>
        /// <param name="source">The source to load data from.</param>
        public void LoadFromQueryable(IQueryable<T> source)
        {
            source = ApplyFilteringToQueryable(source);
            Items = ApplyOptionsToQueryable(source).ToList();
            PagingOptions.TotalItemsCount = source.Count();
            IsRefreshRequired = false;
        }

        /// <summary>
        /// Requests to reload data into the <see cref="GridViewDataSet{T}" />.
        /// </summary>
        public virtual void RequestRefresh()
        {
            IsRefreshRequired = true;
        }

        /// <summary>
        /// Sets the sort expression. If the specified expression is already set, switches the sort direction.
        /// </summary>
        [Obsolete("This method has strange side-effects, assign the expression yourself and reload the DataSet.")]
        public virtual void SetSortExpression(string expression)
        {
            if (SortingOptions.SortExpression == expression)
            {
                SortingOptions.SortDescending = !SortingOptions.SortDescending;
            }
            else
            {
                SortingOptions.SortExpression = expression;
                SortingOptions.SortDescending = false;
            }

            GoToPage(0);
        }

        /// <summary>
        /// Applies filtering to the <paramref name="queryable" /> before the total number
        /// of items is retrieved.
        /// </summary>
        /// <param name="queryable">The <see cref="IQueryable{T}" /> to modify.</param>
        public virtual IQueryable<T> ApplyFilteringToQueryable(IQueryable<T> queryable)
        {
            return queryable;
        }

        /// <summary>
        /// Applies options to the <paramref name="queryable" /> after the total number
        /// of items is retrieved.
        /// </summary>
        /// <param name="queryable">The <see cref="IQueryable{T}" /> to modify.</param>
        public virtual IQueryable<T> ApplyOptionsToQueryable(IQueryable<T> queryable)
        {
            queryable = ApplySortingToQueryable(queryable);
            queryable = ApplyPagingToQueryable(queryable);
            return queryable;
        }

        /// <summary>
        /// Applies sorting to the <paramref name="queryable" /> after the total number
        /// of items is retrieved.
        /// </summary>
        /// <param name="queryable">The <see cref="IQueryable{T}" /> to modify.</param>
        public virtual IQueryable<T> ApplySortingToQueryable(IQueryable<T> queryable)
        {
            if (SortingOptions?.SortExpression == null)
            {
                return queryable;
            }

            return DataSetHelpers.ApplySortExpressionToQueryable(queryable, SortingOptions.SortExpression ?? "", SortingOptions.SortDescending);
        }

        /// <summary>
        /// Applies paging to the <paramref name="queryable" /> after the total number
        /// of items is retrieved.
        /// </summary>
        /// <param name="queryable">The <see cref="IQueryable{T}" /> to modify.</param>
        public virtual IQueryable<T> ApplyPagingToQueryable(IQueryable<T> queryable)
        {
            return PagingOptions != null && PagingOptions.PageSize > 0 ?
                queryable.Skip(PagingOptions.PageSize * PagingOptions.PageIndex).Take(PagingOptions.PageSize) :
                queryable;
        }
    }
}
