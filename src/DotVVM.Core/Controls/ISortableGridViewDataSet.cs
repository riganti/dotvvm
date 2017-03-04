namespace DotVVM.Framework.Controls
{
    public interface ISortableGridViewDataSet : IBaseGridViewDataSet
    {
        ISortOptions SortOptions { get; set; }
        void SetSortExpression(string expression);
    }
}