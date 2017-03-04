namespace DotVVM.Framework.Controls
{
    public interface IGridViewDataSetLoadOptions
    {
        IPagingOptions PagingOptions { get; set; }
        ISortOptions SortOptions { get; set; }
        
    }
}