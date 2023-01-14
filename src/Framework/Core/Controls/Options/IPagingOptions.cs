using System.Collections.Generic;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Represents a marker interface for GridViewDataSet paging options.
    /// </summary>
    public interface IPagingOptions
    {
    }

    public interface IPagingNextPageCapability : IPagingOptions
    {
        /// <summary> Sets the options to load the next page. </summary>
        void GoToNextPage();

        /// <summary>
        /// Gets whether the <see cref="PageIndex" /> represents the last page.
        /// </summary>
        bool IsLastPage { get; }
    }

    public interface IPagingPreviousPageCapability : IPagingOptions
    {
        /// <summary> Sets the options to load the previous page. </summary>
        void GoToPreviousPage();

        /// <summary>
        /// Gets whether the <see cref="PageIndex" /> represents the first page.
        /// </summary>
        bool IsFirstPage { get; }
    }

    public interface IPagingFirstPageCapability : IPagingOptions
    {
        /// <summary> Sets the options to load the first page. </summary>
        void GoToFirstPage();

        /// <summary>
        /// Gets whether the <see cref="PageIndex" /> represents the first page.
        /// </summary>
        bool IsFirstPage { get; }
    }

    public interface IPagingLastPageCapability : IPagingOptions
    {
        void GoToLastPage();

        /// <summary>
        /// Gets whether the <see cref="PageIndex" /> represents the last page.
        /// </summary>
        bool IsLastPage { get; }
    }

    public interface IPagingPageIndexCapability : IPagingOptions
    {
        /// <summary>
        /// Gets or sets a zero-based index of the current page.
        /// </summary>
        int PageIndex { get; }
        
        void GoToPage(int pageIndex);

        /// <summary>
        /// Gets a list of page indexes near the current page. It can be used to build data pagers.
        /// </summary>
        IList<int> NearPageIndexes { get; }
    }

    public interface IPagingPageSizeCapability : IPagingOptions
    {
        /// <summary>
        /// Gets or sets the maximum number of items on a page.
        /// </summary>
        int PageSize { get; }

    }

    public interface IPagingTotalItemsCountCapability : IPagingOptions
    {
        /// <summary>
        /// Gets or sets the total number of items in the data store without respect to paging.
        /// </summary>
        int TotalItemsCount { get; set; }
    }
}
