using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Framework.Controls
{

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
        GridViewDataSetLoadDelegate IRefreshableGridViewDataSet.OnLoadingData
        {
            get
            {
                if (OnLoadingData == null)
                {
                    return null;
                }
                return options => OnLoadingData(options);
            }
        }

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
        public ISortingOptions SortingOptions { get; set; } = new SortingOptions();

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
            if (SortingOptions.SortExpression == expression)
            {
                SortingOptions.SortDescending = !SortingOptions.SortDescending;
                GoToFirstPage();
            }
            else
            {
                SortingOptions.SortExpression = expression;
                SortingOptions.SortDescending = false;
                GoToFirstPage();
            }
        }

        /// <summary>
        /// Creates a GridViewDataSetLoadOptions object which provides information for loading the data.
        /// </summary>
        protected virtual GridViewDataSetLoadOptions<T> CreateGridViewDataSetLoadOptions()
        {
            return new GridViewDataSetLoadOptions<T>
            {
                PagingOptions = PagingOptions,
                SortingOptions = SortingOptions
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
            var options = CreateGridViewDataSetLoadOptions();
            var gridViewDataSetLoadedData = queryable.GetDataFromQueryable(options);
            FillDataSet(gridViewDataSetLoadedData);
        }
    }

    public class GridViewDataSet
    {
        public static GridViewDataSet<T> Create<T>(GridViewDataSetLoadDelegate<T> gridViewDataSetLoadDelegate,
            int pageSize)
        {
            return new GridViewDataSet<T>
            {
                OnLoadingData = gridViewDataSetLoadDelegate,
                PagingOptions = new PagingOptions
                {
                    PageSize = pageSize
                }
            };
        }
    }
}