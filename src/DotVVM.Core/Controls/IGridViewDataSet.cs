namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Represents a collection of items with paging, sorting and row edit capabilities.
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="IBaseGridViewDataSet{T}.Items" /> elements.</typeparam>
    public interface IGridViewDataSet<T, out TSorter, out TPager, out TFilter> :
        IBaseGridViewDataSet<T>,
        ISortableGridViewDataSet<T, TSorter>,
        IPageableGridViewDataSet<T, TPager>,
        IFilterableGridViewDataSet<T, TFilter>,
        IRowEditGridViewDataSet<T>,
        IRefreshableGridViewDataSet<T>
        where TPager: IDataSetPager<T>
        where TSorter: IDataSetSorter<T>
        where TFilter: IDataSetFilter<T>
    {
    }
}
