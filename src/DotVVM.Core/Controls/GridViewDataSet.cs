using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Framework.Controls
{
    public delegate GridViewDataSetLoadedData<T> GridViewDataSetLoadDelegate<T>(IGridViewDataSetLoadOptions gridViewDataSetLoadOptions);

    /// <summary>
    /// Represents a collection of items with paging, sorting and row edit capabilities.
    /// </summary>
    /// <typeparam name="T">Type of the collection element.</typeparam>
    public class GridViewDataSet<T> : IGridViewDataSet
    {

        public GridViewDataSet()
        {
            IsRefreshRequired = true;
        }

        /// <summary>
        /// Called when the GridViewDataSet should be refreshed (on initial page load and when paging or sort options change).
        /// </summary>
        [Bind(Direction.None)]
        public GridViewDataSetLoadDelegate<T> OnLoadingData { get; set; }

        /// <summary>
        /// Called when the GridViewDataSet should be refreshed (on initial page load and when paging or sort options change).
        /// </summary>
        GridViewDataSetLoadDelegate IBaseGridViewDataSet.OnLoadingData => OnLoadingData as GridViewDataSetLoadDelegate;
        
        /// <summary>
        /// Gets or sets the items for the current page.
        /// </summary>
        public IList<T> Items { get; set; } = new List<T>();

        /// <summary>
        /// Gets or sets the items for the current page.
        /// </summary>
        IList IBaseGridViewDataSet.Items => (IList)Items;

        /// <summary>
        /// Gets or sets whether the GridViewDataSet should be refreshed. This property is set to true automatically when paging or sort options change.
        /// </summary>
        public bool IsRefreshRequired { get; set; }

        
        /// <summary>
        /// Gets or sets an object that represents the settings for paging.
        /// </summary>
        public IPagingOptions PagingOptions { get; set; } = new PagingOptions();

        /// <summary>
        /// Gets or sets an object that represents the settings for sorting.
        /// </summary>
        public ISortOptions SortOptions { get; set; } = new SortOptions();

        /// <summary>
        /// Gets or sets an object that represents the settings for row edits.
        /// </summary>
        public IRowEditOptions RowEditOptions { get; set; } = new RowEditOptions();


        /// <summary>
        /// Requests to refresh the GridViewDataSet.
        /// </summary>
        public virtual void RequestRefresh(bool forceRefresh = false)
        {
            if (forceRefresh || IsRefreshRequired)
            {
                NotifyRefreshRequired();
            }
        }
        
        /// <summary>
        /// Navigates to the first page.
        /// </summary>
        public void GoToFirstPage()
        {
            PagingOptions.PageIndex = 0;
            NotifyRefreshRequired();
        }

        /// <summary>
        /// Navigates to the previous page (if possible).
        /// </summary>
        public void GoToPreviousPage()
        {
            if (!PagingOptions.IsFirstPage)
            {
                PagingOptions.PageIndex--;
                NotifyRefreshRequired();
            }
        }

        /// <summary>
        /// Navigates to the last page.
        /// </summary>
        public void GoToLastPage()
        {
            PagingOptions.PageIndex = PagingOptions.PagesCount - 1;
            NotifyRefreshRequired();
        }

        /// <summary>
        /// Navigates to the next page (if possible).
        /// </summary>
        public void GoToNextPage()
        {
            if (!PagingOptions.IsLastPage)
            {
                PagingOptions.PageIndex++;
                NotifyRefreshRequired();
            }
        }

        /// <summary>
        /// Navigates to the specific page.
        /// </summary>
        public void GoToPage(int index)
        {
            PagingOptions.PageIndex = index;
            NotifyRefreshRequired();
        }

        /// <summary>
        /// Sets the sort expression. If the specified expression is already active, switches the sort direction.
        /// </summary>
        public virtual void SetSortExpression(string expression)
        {
            if (SortOptions.SortExpression == expression)
            {
                SortOptions.SortDescending = !SortOptions.SortDescending;
                GoToFirstPage();
            }
            else
            {
                SortOptions.SortExpression = expression;
                SortOptions.SortDescending = false;
                GoToFirstPage();
            }
        }



        /// <summary>
        /// Creates a GridViewDataSetLoadOptions object which provides information for loading the data.
        /// </summary>
        protected virtual GridViewDataSetLoadOptions CreateGridViewDataSetLoadOptions()
        {
            return new GridViewDataSetLoadOptions
            {
                PagingOptions = PagingOptions,
                SortOptions = SortOptions
            };
        }

        /// <summary>
        /// Refreshes the GridViewDataSet immediately, or switches the flag that the refresh is needed.
        /// </summary>
        protected virtual void NotifyRefreshRequired()
        {
            if (OnLoadingData != null)
            {
                var gridViewDataSetLoadedData = OnLoadingData(CreateGridViewDataSetLoadOptions());
                FillDataSet(gridViewDataSetLoadedData);
                IsRefreshRequired = false;
            }
            else
            {
                IsRefreshRequired = true;
            }
        }

        /// <summary>
        /// Fills the GridViewDataSet with specified data.
        /// </summary>
        protected virtual void FillDataSet(GridViewDataSetLoadedData data)
        {
            Items = data.Items.OfType<T>().ToList();
            PagingOptions.TotalItemsCount = data.TotalItemsCount;
            IsRefreshRequired = false;
        }

        /// <summary>
        /// Loads the GridViewDataSet using provided IQueryable object.
        /// </summary>
        /// <param name="queryable"></param>
        public void LoadFromQueryable(IQueryable<T> queryable)
        {
            var gridViewDataSetLoadedData = this.GetDataFromQueryable(queryable);
            FillDataSet(gridViewDataSetLoadedData);
        }

        /// <summary>
        /// Gets or sets a zero-based index of the current page.
        /// </summary>
        [Bind(Direction.None)]
        [Obsolete("Use PagingOptions.PageIndex instead. This property will be removed in future versions.")]
        public int PageIndex { get => PagingOptions.PageIndex; set => PagingOptions.PageIndex = value; }

        /// <summary>
        /// Gets or sets the size of page.
        /// </summary>
        [Bind(Direction.None)]
        [Obsolete("Use PagingOptions.PageSize instead. This property will be removed in future versions.")]
        public int PageSize { get => PagingOptions.PageSize; set => PagingOptions.PageSize = value; }

        /// <summary>
        /// Gets or sets the total number of items in the data store without respect to paging.
        /// </summary>
        [Bind(Direction.None)]
        [Obsolete("Use PagingOptions.TotalItemsCount instead. This property will be removed in future versions.")]
        public int TotalItemsCount { get => PagingOptions.TotalItemsCount; set => PagingOptions.TotalItemsCount = value; }

        /// <summary>
        /// Determines whether the PageIndex represents the first page.
        /// </summary>
        [Bind(Direction.None)]
        [Obsolete("Use PagingOptions.IsFirstPage instead. This property will be removed in future versions.")]
        public bool IsFirstPage { get => PagingOptions.IsFirstPage; }

        /// <summary>
        /// Determines whether the PageIndex represents the last page.
        /// </summary>
        [Bind(Direction.None)]
        [Obsolete("Use PagingOptions.IsLastPage instead. This property will be removed in future versions.")]
        public bool IsLastPage { get => PagingOptions.IsLastPage; }

        /// <summary>
        /// Calcualtes the total number of pages.
        /// </summary>
        [Bind(Direction.None)]
        [Obsolete("Use PagingOptions.PagesCount instead. This property will be removed in future versions.")]
        public int PagesCount { get => PagingOptions.PagesCount; }

        /// <summary>
        /// Calculates a list of page indexes for the pager controls.
        /// </summary>
        [Bind(Direction.None)]
        [Obsolete("Use PagingOptions.NearPageIndexes instead. This property will be removed in future versions.")]
        public IList<int> NearPageIndexes { get => PagingOptions.NearPageIndexes; }

        /// <summary>
        /// Gets or sets whether the sort order should be descending.
        /// </summary>
        [Bind(Direction.None)]
        [Obsolete("Use SortOptions.SortDescending instead. This property will be removed in future versions.")]
        public bool SortDescending { get => SortOptions.SortDescending; set => SortOptions.SortDescending = value; }

        /// <summary>
        /// Gets or sets the name of the property that is used for sorting.
        /// </summary>
        [Bind(Direction.None)]
        [Obsolete("Use SortOptions.SortExpression instead. This property will be removed in future versions.")]
        public string SortExpression { get => SortOptions.SortExpression; set => SortOptions.SortExpression = value; }


        /// <summary>
        /// Gets or sets the name of property that uniquely identifies the row - unique row ID, primary key etc.
        /// </summary>
        [Bind(Direction.None)]
        [Obsolete("Use RowEditOptions.PrimaryKeyPropertyName instead. This property will be removed in future versions.")]
        public string PrimaryKeyPropertyName { get => RowEditOptions.PrimaryKeyPropertyName; set => RowEditOptions.PrimaryKeyPropertyName = value; }

        /// <summary>
        /// Gets or sets the value of PrimaryKeyPropertyName property for the row that is currently edited.
        /// </summary>
        [Bind(Direction.None)]
        [Obsolete("Use RowEditOptions.EditRowId instead. This property will be removed in future versions.")]
        public object EditRowId { get => RowEditOptions.EditRowId; set => RowEditOptions.EditRowId = value; }
    }
}