namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Extends the <see cref="IBaseGridViewDataSet" /> with paging functionality.
    /// </summary>
    public interface IPageableGridViewDataSet<out TPagingOptions> : IBaseGridViewDataSet
        where TPagingOptions : IPagingOptions
    {
        /// <summary>
        /// Gets the settings for paging.
        /// </summary>
        new TPagingOptions PagingOptions { get; }
    }
}
