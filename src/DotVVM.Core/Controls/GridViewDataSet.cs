using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Represents a collection of items with paging, sorting and row edit capabilities.
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="Items" /> elements.</typeparam>
    public class GridViewDataSet<T> : IGridViewDataSet
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
        /// Navigates to the first page.
        /// </summary>
        public void GoToFirstPage()
        {
            PagingOptions.PageIndex = 0;
            RequestRefresh();
        }

        /// <summary>
        /// Navigates to the last page.
        /// </summary>
        public void GoToLastPage()
        {
            PagingOptions.PageIndex = PagingOptions.PagesCount - 1;
            RequestRefresh();
        }

        /// <summary>
        /// Navigates to the next page if possible.
        /// </summary>
        public void GoToNextPage()
        {
            if (!PagingOptions.IsLastPage)
            {
                PagingOptions.PageIndex++;
                RequestRefresh();
            }
        }

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
        /// Navigates to the previous page if possible.
        /// </summary>
        public void GoToPreviousPage()
        {
            if (!PagingOptions.IsFirstPage)
            {
                PagingOptions.PageIndex--;
                RequestRefresh();
            }
        }

        /// <summary>
        /// Loads data into the <see cref="GridViewDataSet{T}"/> from the given <see cref="IQueryable{T}"/> source.
        /// </summary>
        /// <param name="source">The source to load data from.</param>
        public void LoadFromQueryable(IQueryable<T> source)
        {
            source = ApplyFiltersToQueryable(source);
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
            GoToFirstPage();
        }

        /// <summary>
        /// Applies filters to the <paramref name="queryable" /> before the total number
        /// of items is retrieved.
        /// </summary>
        /// <param name="queryable">The <see cref="IQueryable{T}" /> to modify.</param>
        protected virtual IQueryable<T> ApplyFiltersToQueryable(IQueryable<T> queryable)
        {
            return queryable;
        }

        /// <summary>
        /// Applies options to the <paramref name="queryable" /> after the total number
        /// of items is retrieved.
        /// </summary>
        /// <param name="queryable">The <see cref="IQueryable{T}" /> to modify.</param>
        protected virtual IQueryable<T> ApplyOptionsToQueryable(IQueryable<T> queryable)
        {
            if (SortingOptions?.SortExpression != null)
            {
                queryable = SortingOptions.ApplyToQueryable(queryable);
            }

            if (PagingOptions != null)
            {
                queryable = PagingOptions.ApplyToQueryable(queryable);
            }

            return queryable;
        }
    }
}
