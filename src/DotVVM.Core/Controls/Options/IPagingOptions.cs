using System.Collections.Generic;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Represents settings for paging.
    /// </summary>
    public interface IPagingOptions
    {
        /// <summary>
        /// Gets or sets a zero-based index of the current page.
        /// </summary>
        int PageIndex { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of items on a page.
        /// </summary>
        int PageSize { get; set; }

        /// <summary>
        /// Gets or sets the total number of items in the data store without respect to paging.
        /// </summary>
        int TotalItemsCount { get; set; }

        /// <summary>
        /// Gets whether the <see cref="PageIndex" /> represents the first page.
        /// </summary>
        bool IsFirstPage { get; }

        /// <summary>
        /// Gets whether the <see cref="PageIndex" /> represents the last page.
        /// </summary>
        bool IsLastPage { get; }

        /// <summary>
        /// Gets the total number of pages.
        /// </summary>
        int PagesCount { get; }

        /// <summary>
        /// Gets a list of page indexes near the current page. It can be used to build data pagers.
        /// </summary>
        IList<int> NearPageIndexes { get; }
    }
}