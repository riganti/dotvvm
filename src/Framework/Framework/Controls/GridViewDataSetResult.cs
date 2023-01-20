using System.Collections.Generic;

namespace DotVVM.Framework.Controls
{
    public class GridViewDataSetResult<TItem, TFilteringOptions, TSortingOptions, TPagingOptions>
        where TFilteringOptions : IFilteringOptions
        where TSortingOptions : ISortingOptions
        where TPagingOptions : IPagingOptions
    {
        public GridViewDataSetResult(List<TItem> items, TFilteringOptions? filteringOptions = default, TSortingOptions? sortingOptions = default, TPagingOptions? pagingOptions = default)
        {
            Items = items;
            FilteringOptions = filteringOptions;
            SortingOptions = sortingOptions;
            PagingOptions = pagingOptions;
        }

        public List<TItem> Items { get; init; }

        public TFilteringOptions? FilteringOptions { get; init; }

        public TSortingOptions? SortingOptions { get; init; }

        public TPagingOptions? PagingOptions { get; init; }
    }
}