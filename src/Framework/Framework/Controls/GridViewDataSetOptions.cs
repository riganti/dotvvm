namespace DotVVM.Framework.Controls
{
    public class GridViewDataSetOptions<TFilteringOptions, TSortingOptions, TPagingOptions>
        where TFilteringOptions : IFilteringOptions
        where TSortingOptions : ISortingOptions
        where TPagingOptions : IPagingOptions
    {
        public TFilteringOptions? FilteringOptions { get; init; } = default;

        public TSortingOptions? SortingOptions { get; init; } = default;

        public TPagingOptions? PagingOptions { get; init; } = default;
    }
}