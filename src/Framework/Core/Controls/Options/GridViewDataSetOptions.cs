namespace DotVVM.Framework.Controls
{
    public class GridViewDataSetOptions<TFilteringOptions, TSortingOptions, TPagingOptions>
        where TFilteringOptions : IFilteringOptions
        where TSortingOptions : ISortingOptions
        where TPagingOptions : IPagingOptions
    {
        public TFilteringOptions FilteringOptions { get; set; } = default!;

        public TSortingOptions SortingOptions { get; set; } = default!;

        public TPagingOptions PagingOptions { get; set; } = default!;
    }

    public class GridViewDataSetOptions : GridViewDataSetOptions<NoFilteringOptions, SortingOptions, PagingOptions>
    {
    }
}
