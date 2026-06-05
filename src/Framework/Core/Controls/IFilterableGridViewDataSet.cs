namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Extends the <see cref="IGridViewDataSet" /> with specific implementation of filtering options.
    /// </summary>
    public interface IFilterableGridViewDataSet<out TFilteringOptions> : IGridViewDataSet
        where TFilteringOptions : IFilteringOptions
    {
        /// <summary>
        /// Gets the settings for filtering.
        /// </summary>
        new TFilteringOptions FilteringOptions { get; }
    }
}
