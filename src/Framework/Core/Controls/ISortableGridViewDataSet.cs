namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Extends the <see cref="IGridViewDataSet" /> with specific implementation of sorting functionality.
    /// </summary>
    public interface ISortableGridViewDataSet<out TSortingOptions> : IGridViewDataSet
        where TSortingOptions : ISortingOptions
    {
        /// <summary>
        /// Gets the settings for sorting.
        /// </summary>
        new TSortingOptions SortingOptions { get; }
    }
}
