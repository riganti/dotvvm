namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Extends the base GridViewDataSet with filtering functionality.
    /// </summary>
    public interface IFilterableGridViewDataSet<T, out TFilter> : IBaseGridViewDataSet<T>
        where TFilter: IDataSetFilter<T>
    {
        /// <summary>
        /// The settings and logic for filtering.
        /// </summary>
        TFilter Filter { get; }
    }
}
