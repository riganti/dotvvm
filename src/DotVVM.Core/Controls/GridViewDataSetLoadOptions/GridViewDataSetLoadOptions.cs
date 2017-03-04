namespace DotVVM.Framework.Controls
{
    public class GridViewDataSetLoadOptions : IGridViewDataSetLoadOptions
    {
        public IPagingOptions PagingOptions { get; set; }

        public ISortOptions SortOptions { get; set; }
    }
}