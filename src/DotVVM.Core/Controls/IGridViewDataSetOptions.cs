namespace DotVVM.Framework.Controls
{
    public interface IGridViewDataSetOptions
    {

        IBaseGridViewDataSet DataSet { get; }
        
        IPagingOptions PagingOptions { get; }

        ISortOptions SortOptions { get; }
        
    }
}