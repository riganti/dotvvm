namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Extends the base GridViewDataSet with filtering options.
    /// </summary>
    public interface IFilterableGridViewDataSet<out TFilteringOptions> : IBaseGridViewDataSet
        where TFilteringOptions : IFilteringOptions
    {
        /// <summary>
        /// Gets the settings for filtering.
        /// </summary>
        new TFilteringOptions FilteringOptions { get; }
    }
}
