using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DotVVM.Framework.Controls
{
    public class GenericGridViewDataSet<
        T,
        TFilteringOptions,
        TSortingOptions,
        TPagingOptions,
        TRowInsertOptions,
        TRowEditOptions>
        : IGridViewDataSet<T>,
            IFilterableGridViewDataSet<TFilteringOptions>,
            ISortableGridViewDataSet<TSortingOptions>,
            IPageableGridViewDataSet<TPagingOptions>,
            IRowInsertGridViewDataSet<TRowInsertOptions>,
            IRowEditGridViewDataSet<TRowEditOptions>,
            IRefreshableGridViewDataSet
        where TFilteringOptions : IFilteringOptions
        where TSortingOptions : ISortingOptions
        where TPagingOptions : IPagingOptions
        where TRowInsertOptions : IRowInsertOptions
        where TRowEditOptions : IRowEditOptions
    {

        /// <summary>
        /// Gets or sets the items for the current page.
        /// </summary>
        public virtual IList<T> Items { get; set; } = new List<T>();

        /// <summary>
        /// Gets or sets whether the data should be refreshed. This property is set to true automatically
        /// when paging, sorting or other options change.
        /// </summary>
        public bool IsRefreshRequired { get; set; } = true;

        /// <summary>
        /// Gets or sets the settings for filtering.
        /// </summary>
        public TFilteringOptions FilteringOptions { get; set; }

        /// <summary>
        /// Gets or sets the settings for sorting.
        /// </summary>
        public TSortingOptions SortingOptions { get; set; }

        /// <summary>
        /// Gets or sets the settings for paging.
        /// </summary>
        public TPagingOptions PagingOptions { get; set; }

        /// <summary>
        /// Gets or sets the settings for row (item) insert feature.
        /// </summary>
        public TRowInsertOptions RowInsertOptions { get; set; }

        /// <summary>
        /// Gets or sets the settings for row (item) edit feature.
        /// </summary>
        public TRowEditOptions RowEditOptions { get; set; }


        IList IBaseGridViewDataSet.Items => Items is IList list ? list : new ReadOnlyCollection<T>(Items);

        IFilteringOptions IBaseGridViewDataSet.FilteringOptions => this.FilteringOptions;

        ISortingOptions IBaseGridViewDataSet.SortingOptions => this.SortingOptions;

        IPagingOptions IBaseGridViewDataSet.PagingOptions => this.PagingOptions;

        IRowInsertOptions IBaseGridViewDataSet.RowInsertOptions => this.RowInsertOptions;

        IRowEditOptions IBaseGridViewDataSet.RowEditOptions => this.RowEditOptions;

        TFilteringOptions IFilterableGridViewDataSet<TFilteringOptions>.FilteringOptions => this.FilteringOptions;

        TSortingOptions ISortableGridViewDataSet<TSortingOptions>.SortingOptions => this.SortingOptions;

        TPagingOptions IPageableGridViewDataSet<TPagingOptions>.PagingOptions => this.PagingOptions;

        TRowInsertOptions IRowInsertGridViewDataSet<TRowInsertOptions>.RowInsertOptions => this.RowInsertOptions;

        TRowEditOptions IRowEditGridViewDataSet<TRowEditOptions>.RowEditOptions => this.RowEditOptions;



        public GenericGridViewDataSet(TFilteringOptions filteringOptions, TSortingOptions sortingOptions, TPagingOptions pagingOptions, TRowInsertOptions rowInsertOptions, TRowEditOptions rowEditOptions)
        {
            FilteringOptions = filteringOptions;
            SortingOptions = sortingOptions;
            PagingOptions = pagingOptions;
            RowInsertOptions = rowInsertOptions;
            RowEditOptions = rowEditOptions;
        }


        /// <summary>
        /// Requests to reload data into the <see cref="GridViewDataSet{T}" />.
        /// </summary>
        public void RequestRefresh()
        {
            IsRefreshRequired = true;
        }

        /// <summary>
        /// Applies the options from the specified <see cref="GridViewDataSetOptions{TFilteringOptions, TSortingOptions, TPagingOptions}" /> to this instance.
        /// </summary>
        public void ApplyOptions(GridViewDataSetOptions<TFilteringOptions, TSortingOptions, TPagingOptions> options)
        {
            if (options.FilteringOptions != null)
            {
                FilteringOptions = options.FilteringOptions;
            }

            if (options.SortingOptions != null)
            {
                SortingOptions = options.SortingOptions;
            }

            if (options.PagingOptions != null)
            {
                PagingOptions = options.PagingOptions;
            }
        }

        public GridViewDataSetOptions<TFilteringOptions, TSortingOptions, TPagingOptions> GetOptions()
        {
            return new()
            {
                FilteringOptions = FilteringOptions,
                SortingOptions = SortingOptions,
                PagingOptions = PagingOptions
            };
        }

        /// <summary> Sets new items + filtering, sorting, paging options. </summary>
        public void ApplyResult(GridViewDataSetResult<T, TFilteringOptions, TSortingOptions, TPagingOptions> result)
        {
            Items = result.Items.ToList();
            if (result.FilteringOptions is {})
                FilteringOptions = result.FilteringOptions;
            if (result.SortingOptions is {})
                SortingOptions = result.SortingOptions;
            if (result.PagingOptions is {})
                PagingOptions = result.PagingOptions;
        }
    }
}
