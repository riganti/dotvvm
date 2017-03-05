using DotVVM.Framework.ViewModel;

namespace DotVVM.Framework.Controls
{

    /// <summary>
    /// Represents a collection of items with paging, sorting and row edit capabilities.
    /// </summary>
    public interface IGridViewDataSet : IPageableGridViewDataSet, ISortableGridViewDataSet, IRowEditGridViewDataSet
    {
    }
}