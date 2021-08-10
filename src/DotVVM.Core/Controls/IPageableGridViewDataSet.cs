namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Extends the <see cref="IBaseGridViewDataSet" /> with paging functionality.
    /// </summary>
    public interface IPageableGridViewDataSet : IBaseGridViewDataSet
    {
        /// <summary>
        /// Gets the settings for paging.
        /// </summary>
        IPagingOptions PagingOptions { get; }

        /// <summary>
        /// Navigates to the specific page.
        /// </summary>
        /// <param name="index">The zero-based index of the page to navigate to.</param>
        void GoToPage(int index);
    }
}