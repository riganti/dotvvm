namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Extends the base GridViewDataSet with sorting functionality.
    /// </summary>
    public interface ISortableGridViewDataSet<T, out TSorter> : IBaseGridViewDataSet<T>
        where TSorter: IDataSetSorter<T>
    {
        /// <summary>
        /// The settings and logic for sorting.
        /// </summary>
        TSorter Sorter { get; }
    }
}
