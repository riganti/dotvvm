using System.Collections.Generic;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Represents a settings for paging.
    /// </summary>
    public interface IPagingOptions
    {
        /// <summary>
        /// Gets or sets a zero-based index of the current page.
        /// </summary>
        int PageIndex { get; set; }

        /// <summary>
        /// Gets or sets the size of page.
        /// </summary>
        int PageSize { get; set; }

        /// <summary>
        /// Gets or sets the total number of items in the data store without respect to paging.
        /// </summary>
        int TotalItemsCount { get; set; }

        /// <summary>
        /// Determines whether the PageIndex represents the first page.
        /// </summary>
        bool IsFirstPage { get; }

        /// <summary>
        /// Determines whether the PageIndex represents the last page.
        /// </summary>
        bool IsLastPage { get; }

        /// <summary>
        /// Calcualtes the total number of pages.
        /// </summary>
        int PagesCount { get; }

        /// <summary>
        /// Calculates a list of page indexes for the pager controls.
        /// </summary>
        IList<int> NearPageIndexes { get; }
    }
}