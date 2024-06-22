namespace DotVVM.Framework.Controls
{
    /// <summary> Contains filtering, sorting, and paging options of a <see cref="GridViewDataSet{T}" />. </summary>
    public class GridViewDataSetOptions<TFilteringOptions, TSortingOptions, TPagingOptions>
        where TFilteringOptions : IFilteringOptions
        where TSortingOptions : ISortingOptions
        where TPagingOptions : IPagingOptions
    {
        public TFilteringOptions FilteringOptions { get; set; } = default!;

        public TSortingOptions SortingOptions { get; set; } = default!;

        public TPagingOptions PagingOptions { get; set; } = default!;
    }

    /// <summary> Contains filtering, sorting, and paging options of a <see cref="GridViewDataSet{T}" /> </summary>
    public class GridViewDataSetOptions : GridViewDataSetOptions<NoFilteringOptions, SortingOptions, PagingOptions>
    {
    }
}
