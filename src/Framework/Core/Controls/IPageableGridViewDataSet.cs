namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Extends the <see cref="IBaseGridViewDataSet" /> with paging functionality.
    /// </summary>
    public interface IPageableGridViewDataSet<out TPagingOptions> : IPageableGridViewDataSet
        where TPagingOptions : IPagingOptions
    {
        /// <summary>
        /// Gets the settings for paging.
        /// </summary>
        new TPagingOptions PagingOptions { get; }
    }

    public interface IPageableGridViewDataSet : IBaseGridViewDataSet
    {
        /// <summary>
        /// Gets the settings for paging.
        /// </summary>
        IPagingOptions PagingOptions { get; }
    }
}
