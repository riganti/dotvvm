namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Extends the <see cref="IGridViewDataSet" /> with specific implementation of paging functionality.
    /// </summary>
    public interface IPageableGridViewDataSet<out TPagingOptions> : IGridViewDataSet
        where TPagingOptions : IPagingOptions
    {
        /// <summary>
        /// Gets the settings for paging.
        /// </summary>
        new TPagingOptions PagingOptions { get; }
    }
}
