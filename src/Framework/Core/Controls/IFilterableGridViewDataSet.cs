namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Extends the base GridViewDataSet with filtering options.
    /// </summary>
    public interface IFilterableGridViewDataSet<out TFilteringOptions> : IFilterableGridViewDataSet
        where TFilteringOptions : IFilteringOptions
    {
        /// <summary>
        /// Gets the settings for filtering.
        /// </summary>
        new TFilteringOptions FilteringOptions { get; }
    }
	
	public interface IFilterableGridViewDataSet : IBaseGridViewDataSet
    {
        /// <summary>
        /// Gets the settings for filtering.
        /// </summary>
        IFilteringOptions FilteringOptions { get; }
    }
}