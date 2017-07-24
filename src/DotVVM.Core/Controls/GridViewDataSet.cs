using DotVVM.Framework.ViewModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        /// Gets or sets the value of PrimaryKeyPropertyName property for the row that is currently edited.
        /// </summary>
        [Bind(Direction.None)]
        [Obsolete("Use RowEditOptions.EditRowId instead. This property will be removed in future versions.")]
        public object EditRowId { get => RowEditOptions.EditRowId; set => RowEditOptions.EditRowId = value; }

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
        /// Gets or sets whether the GridViewDataSet should be refreshed. This property is set to true automatically when paging or sort options change.
        /// </summary>
        public bool IsRefreshRequired { get; set; }

        /// <summary>
        /// Gets or sets the items for the current page.
        /// </summary>
        public IList<T> Items { get; set; } = new List<T>();

        /// <summary>
        /// Gets or sets the items for the current page.
        /// </summary>
        IList IBaseGridViewDataSet.Items => (IList)Items;

        /// <summary>
        /// Calculates a list of page indexes for the pager controls.
        /// </summary>
        [Bind(Direction.None)]
        [Obsolete("Use PagingOptions.NearPageIndexes instead. This property will be removed in future versions.")]
        public IList<int> NearPageIndexes { get => PagingOptions.NearPageIndexes; }

        /// <summary>
        /// Called when the GridViewDataSet should be refreshed (on initial page load and when paging or sort options change).
        /// </summary>
        /// <remarks>
        /// Either <see cref="OnLoadingData"/> or <see cref="OnLoadingDataAsync"/> can be set but not both.
        /// </remarks>
        [Bind(Direction.None)]
        public GridViewDataSetLoadDelegate<T> OnLoadingData { get; set; }

        /// <summary>
        /// Called when the GridViewDataSet should be refreshed (on initial page load and when paging or sort options change).
        /// </summary>
        /// <remarks>
        /// Either <see cref="OnLoadingData"/> or <see cref="OnLoadingDataAsync"/> can be set but not both.
        /// </remarks>
        [Bind(Direction.None)]
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
        /// Called when the GridViewDataSet should be refreshed (on initial page load and when paging or sort options change).
        /// </summary>
        /// <remarks>
        /// Either <see cref="OnLoadingData"/> or <see cref="OnLoadingDataAsync"/> can be set but not both.
        /// </remarks>
        [Bind(Direction.None)]
        public GridViewDataSetLoadAsyncDelegate<T> OnLoadingDataAsync { get; set; }

        GridViewDataSetLoadAsyncDelegate IRefreshableGridViewDataSet.OnLoadingDataAsync
        {
            get
            {
                if (OnLoadingDataAsync == null)
                {
                    return null;
                }
                return async options => await OnLoadingDataAsync(options);
            }
        }

        /// <summary>
        /// Gets or sets a zero-based index of the current page.
        /// </summary>
        [Bind(Direction.None)]
        [Obsolete("Use PagingOptions.PageIndex instead. This property will be removed in future versions.")]
        public int PageIndex { get => PagingOptions.PageIndex; set => PagingOptions.PageIndex = value; }

        /// <summary>
        /// Calcualtes the total number of pages.
        /// </summary>
        [Bind(Direction.None)]
        [Obsolete("Use PagingOptions.PagesCount instead. This property will be removed in future versions.")]
        public int PagesCount { get => PagingOptions.PagesCount; }

        /// <summary>
        /// Gets or sets the size of page.
        /// </summary>
        [Bind(Direction.None)]
        [Obsolete("Use PagingOptions.PageSize instead. This property will be removed in future versions.")]
        public int PageSize { get => PagingOptions.PageSize; set => PagingOptions.PageSize = value; }

        /// <summary>
        /// Gets or sets an object that represents the settings for paging.
        /// </summary>
        public IPagingOptions PagingOptions { get; set; } = new PagingOptions();

        /// <summary>
        /// Gets or sets the name of property that uniquely identifies the row - unique row ID, primary key etc.
        /// </summary>
        [Bind(Direction.None)]
        [Obsolete("Use RowEditOptions.PrimaryKeyPropertyName instead. This property will be removed in future versions.")]
        public string PrimaryKeyPropertyName { get => RowEditOptions.PrimaryKeyPropertyName; set => RowEditOptions.PrimaryKeyPropertyName = value; }

        /// <summary>
        /// Gets or sets an object that represents the settings for row edits.
        /// </summary>
        public IRowEditOptions RowEditOptions { get; set; } = new RowEditOptions();

        /// <summary>
        /// Gets or sets whether the sort order should be descending.
        /// </summary>
        [Bind(Direction.None)]
        [Obsolete("Use SortingOptions.SortDescending instead. This property will be removed in future versions.")]
        public bool SortDescending { get => SortingOptions.SortDescending; set => SortingOptions.SortDescending = value; }

        /// <summary>
        /// Gets or sets the name of the property that is used for sorting.
        /// </summary>
        [Bind(Direction.None)]
        [Obsolete("Use SortingOptions.SortExpression instead. This property will be removed in future versions.")]
        public string SortExpression { get => SortingOptions.SortExpression; set => SortingOptions.SortExpression = value; }

        /// <summary>
        /// Gets or sets an object that represents the settings for sorting.
        /// </summary>
        public ISortingOptions SortingOptions { get; set; } = new SortingOptions();

        /// <summary>
        /// Gets or sets the total number of items in the data store without respect to paging.
        /// </summary>
        [Bind(Direction.None)]
        [Obsolete("Use PagingOptions.TotalItemsCount instead. This property will be removed in future versions.")]
        public int TotalItemsCount { get => PagingOptions.TotalItemsCount; set => PagingOptions.TotalItemsCount = value; }


        /// <summary>
        /// Navigates to the first page.
        /// </summary>
        public void GoToFirstPage() => GoToFirstPageAsync().Wait();

        /// <summary>
        /// Navigates to the first page.
        /// </summary>
        public Task GoToFirstPageAsync()
        {
            PagingOptions.PageIndex = 0;
            return NotifyRefreshRequired();
        }

        /// <summary>
        /// Navigates to the last page.
        /// </summary>
        public void GoToLastPage() => GoToLastPageAsync().Wait();

        /// <summary>
        /// Navigates to the last page.
        /// </summary>
        public Task GoToLastPageAsync()
        {
            PagingOptions.PageIndex = PagingOptions.PagesCount - 1;
            return NotifyRefreshRequired();
        }

        /// <summary>
        /// Navigates to the next page (if possible).
        /// </summary>
        public void GoToNextPage() => GoToNextPageAsync().Wait();

        /// <summary>
        /// Navigates to the next page (if possible).
        /// </summary>
        public Task GoToNextPageAsync()
        {
            if (!PagingOptions.IsLastPage)
            {
                PagingOptions.PageIndex++;
                return NotifyRefreshRequired();
            }
            return Task.WhenAll();
        }

        /// <summary>
        /// Navigates to the specific page.
        /// </summary>
        public void GoToPage(int index) => GoToPageAsync(index).Wait();

        /// <summary>
        /// Navigates to the specific page.
        /// </summary>
        public Task GoToPageAsync(int index)
        {
            PagingOptions.PageIndex = index;
            return NotifyRefreshRequired();
        }

        /// <summary>
        /// Navigates to the previous page (if possible).
        /// </summary>
        public void GoToPreviousPage() => GoToPreviousPageAsync().Wait();

        /// <summary>
        /// Navigates to the previous page (if possible).
        /// </summary>
        public Task GoToPreviousPageAsync()
        {
            if (!PagingOptions.IsFirstPage)
            {
                PagingOptions.PageIndex--;
                return NotifyRefreshRequired();
            }
            return Task.WhenAll();
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

        /// <summary>
        /// Requests to refresh the GridViewDataSet.
        /// </summary>
        public void RequestRefresh(bool forceRefresh = false) => RequestRefreshAsync(forceRefresh).Wait();

        /// <summary>
        /// Requests to refresh the GridViewDataSet.
        /// </summary>
        public virtual Task RequestRefreshAsync(bool forceRefresh = false)
        {
            if (forceRefresh || IsRefreshRequired)
            {
                return NotifyRefreshRequired();
            }
            return Task.WhenAll();
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
        /// Fills the GridViewDataSet with specified data.
        /// </summary>
        protected virtual void FillDataSet(GridViewDataSetLoadedData data)
        {
            Items = data.Items.OfType<T>().ToList();
            PagingOptions.TotalItemsCount = data.TotalItemsCount;
            IsRefreshRequired = false;
        }

        /// <summary>
        /// Refreshes the GridViewDataSet immediately, or switches the flag that the refresh is needed.
        /// </summary>
        protected virtual async Task NotifyRefreshRequired()
        {
            var data = await LoadData();
            if (data != null)
            {
                FillDataSet(data);
                IsRefreshRequired = false;
            }
            else
            {
                IsRefreshRequired = true;
            }
        }

        private async Task<GridViewDataSetLoadedData<T>> LoadData()
        {
            if (OnLoadingData != null && OnLoadingDataAsync != null)
            {
                throw new InvalidOperationException($"The {nameof(OnLoadingData)} and {nameof(OnLoadingDataAsync)} properties may not be set both at once.");
            }
            else if (OnLoadingData != null)
            {
                return OnLoadingData(CreateGridViewDataSetLoadOptions());
            }
            else if (OnLoadingDataAsync != null)
            {
                return await OnLoadingDataAsync(CreateGridViewDataSetLoadOptions());
            }
            else
            {
                return null;
            }
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

        public static GridViewDataSet<T> Create<T>(GridViewDataSetLoadAsyncDelegate<T> asyncGridViewDataSetLoadDelegate,
            int pageSize)
        {
            return new GridViewDataSet<T>
            {
                OnLoadingDataAsync = asyncGridViewDataSetLoadDelegate,
                PagingOptions = new PagingOptions
                {
                    PageSize = pageSize
                }
            };
        }
    }
}