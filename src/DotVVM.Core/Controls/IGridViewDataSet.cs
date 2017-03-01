namespace DotVVM.Framework.Controls
{
    public interface IGridViewDataSet : IBaseGridViewDataSet, IPageableGridViewDataSet, ISortableGridViewDataSet, IRowEditGridViewDataSet
    {
        void Reset();
    }
}