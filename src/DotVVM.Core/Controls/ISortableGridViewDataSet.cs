namespace DotVVM.Framework.Controls
{
    public interface ISortableGridViewDataSet
    {
        ISortOptions SortOptions { get; set; }
        void SetSortExpression(string expression);
    }
}