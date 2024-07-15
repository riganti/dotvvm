namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Represents a collection of items with paging, sorting and row edit capabilities.
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="IBaseGridViewDataSet{T}.Items" /> elements.</typeparam>
    public interface IGridViewDataSet<T> : IGridViewDataSet, IBaseGridViewDataSet<T>
    {
    }

    /// <summary>
    /// Represents a collection of items with paging, sorting and row edit capabilities.
    /// </summary>
    public interface IGridViewDataSet : IFilterableGridViewDataSet, ISortableGridViewDataSet, IPageableGridViewDataSet, IRowInsertGridViewDataSet, IRowEditGridViewDataSet, IRefreshableGridViewDataSet
    {
    }
}
