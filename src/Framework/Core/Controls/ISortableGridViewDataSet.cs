namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Extends the base GridViewDataSet with sorting functionality.
    /// </summary>
    public interface ISortableGridViewDataSet<out TSortingOptions> : ISortableGridViewDataSet
        where TSortingOptions : ISortingOptions
    {
        /// <summary>
        /// Gets the settings for sorting.
        /// </summary>
        new TSortingOptions SortingOptions { get; }
    }

    public interface ISortableGridViewDataSet : IBaseGridViewDataSet
    {
        ISortingOptions SortingOptions { get; }
    }
}
