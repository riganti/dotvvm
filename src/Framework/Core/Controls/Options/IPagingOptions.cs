using System.Collections.Generic;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Represents a marker interface for GridViewDataSet paging options.
    /// </summary>
    public interface IPagingOptions
    {
    }

    /// <summary>
    /// Represents a paging options which support navigating to the next page.
    /// </summary>
    public interface IPagingNextPageCapability : IPagingOptions
    {
        /// <summary>
        /// Modifies the options to load the next page.
        /// </summary>
        void GoToNextPage();

        /// <summary>
        /// Gets whether the current page is the last page.
        /// </summary>
        bool IsLastPage { get; }
    }

    /// <summary>
    /// Represents a paging options which support navigating to the previous page.
    /// </summary>
    public interface IPagingPreviousPageCapability : IPagingOptions
    {
        /// <summary>
        /// Modifies the options to load the previous page.
        /// </summary>
        void GoToPreviousPage();

        /// <summary>
        /// Gets whether the current page is the first page.
        /// </summary>
        bool IsFirstPage { get; }
    }

    /// <summary>
    /// Represents a paging options which support navigating to the first page.
    /// </summary>
    public interface IPagingFirstPageCapability : IPagingOptions
    {
        /// <summary>
        /// Modifies the options to load the first page.
        /// </summary>
        void GoToFirstPage();

        /// <summary>
        /// Gets whether the current page is the first page.
        /// </summary>
        bool IsFirstPage { get; }
    }

    /// <summary>
    /// Represents a paging options which support navigating to the last page.
    /// </summary>
    public interface IPagingLastPageCapability : IPagingOptions
    {
        /// <summary>
        /// Modifies the options to load the last page.
        /// </summary>
        void GoToLastPage();

        /// <summary>
        /// Gets whether the current page is the last page.
        /// </summary>
        bool IsLastPage { get; }
    }

    /// <summary>
    /// Represents a paging options which support navigating to the a specific page by its index.
    /// </summary>
    public interface IPagingPageIndexCapability : IPagingOptions
    {
        /// <summary>
        /// Gets or sets a zero-based index of the current page.
        /// </summary>
        int PageIndex { get; }

        /// <summary>
        /// Modifies the options to load the specific page.
        /// </summary>
        /// <param name="pageIndex">A zero-based index of the new page</param>
        void GoToPage(int pageIndex);

        /// <summary>
        /// Gets a list of page indexes near the current page. It can be used to tell the DataPager control which page numbers should be shown to the user.
        /// </summary>
        IList<int> NearPageIndexes { get; }
    }

    /// <summary>
    /// Represents a paging options which are aware of the page size.
    /// </summary>
    public interface IPagingPageSizeCapability : IPagingOptions
    {
        /// <summary>
        /// Gets or sets the maximum number of items on a page.
        /// </summary>
        int PageSize { get; set; }

    }

    /// <summary>
    /// Represents a paging options which are aware of the total number of items in the data set.
    /// </summary>
    public interface IPagingTotalItemsCountCapability : IPagingOptions
    {
        /// <summary>
        /// Gets or sets the total number of items in the data store without respect to paging.
        /// </summary>
        int TotalItemsCount { get; set; }
    }
}
