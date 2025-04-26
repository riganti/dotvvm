using System.Collections.Generic;

namespace DotVVM.Framework.Controls
{

    public class GridViewDataSetResult<TItem, TFilteringOptions, TSortingOptions, TPagingOptions>
        where TFilteringOptions : IFilteringOptions
        where TSortingOptions : ISortingOptions
        where TPagingOptions : IPagingOptions
    {
        public GridViewDataSetResult(IReadOnlyList<TItem> items, TFilteringOptions? filteringOptions = default, TSortingOptions? sortingOptions = default, TPagingOptions? pagingOptions = default)
        {
            Items = items;
            FilteringOptions = filteringOptions;
            SortingOptions = sortingOptions;
            PagingOptions = pagingOptions;
        }

        public GridViewDataSetResult(IReadOnlyList<TItem> items, GridViewDataSetOptions<TFilteringOptions, TSortingOptions, TPagingOptions> options)
        {
            Items = items;
            FilteringOptions = options.FilteringOptions;
            SortingOptions = options.SortingOptions;
            PagingOptions = options.PagingOptions;
        }

        /// <summary> New items to replace the old <see cref="IBaseGridViewDataSet{T}.Items"/> </summary>
        public IReadOnlyList<TItem> Items { get; }

        /// <summary> New filtering options to replace the old <see cref="IFilterableGridViewDataSet{T}.FilteringOptions"/>, if null the old options are left unchanged. </summary>
        public TFilteringOptions? FilteringOptions { get; }

        /// <summary> New sorting options to replace the old <see cref="ISortableGridViewDataSet{T}.SortingOptions"/>, if null the old options are left unchanged. </summary>
        public TSortingOptions? SortingOptions { get; }

        /// <summary> New paging options to replace the old <see cref="IPageableGridViewDataSet{T}.PagingOptions"/>, if null the old options are left unchanged. </summary>
        public TPagingOptions? PagingOptions { get; }
    }
}
