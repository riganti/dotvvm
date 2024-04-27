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

        public IReadOnlyList<TItem> Items { get; }

        public TFilteringOptions? FilteringOptions { get; }

        public TSortingOptions? SortingOptions { get; }

        public TPagingOptions? PagingOptions { get; }
    }
}
