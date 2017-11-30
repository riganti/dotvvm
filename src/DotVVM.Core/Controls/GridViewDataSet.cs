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
            var options = CreateLoadOptions();
            var data = options.LoadDataFromQueryable(source);
            FillDataSet(data);
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
        /// Creates a <see cref="IGridViewDataSetLoadOptions{T}" /> implmentation providing options required to reload data.
        /// </summary>
        protected virtual IGridViewDataSetLoadOptions<T> CreateLoadOptions()
        {
            return new GridViewDataSetLoadOptions<T> {
                PagingOptions = PagingOptions,
                SortingOptions = SortingOptions
            };
        }

        /// <summary>
        /// Fills the GridViewDataSet with specified data.
        /// </summary>
        protected virtual void FillDataSet(GridViewDataSetLoadedData<T> data)
        {
            Items = data.Items.ToList();
            PagingOptions.TotalItemsCount = data.TotalItemsCount;
            IsRefreshRequired = false;
        }
    }
}
