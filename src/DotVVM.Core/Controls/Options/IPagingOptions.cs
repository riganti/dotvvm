using System;
using System.Collections.Generic;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Represents settings for paging.
    /// </summary>
    [Obsolete("Please use the IDataSetPager or the DefaultGridPager in the GridViewDataSet.Pager property")]
    public interface IPagingOptions
    {
        /// <summary>
        /// Gets or sets a zero-based index of the current page.
        /// </summary>
        [Obsolete("Please use the IDataSetPager or the DefaultGridPager in the GridViewDataSet.Pager property")]
        int PageIndex { get; }

        /// <summary>
        /// Gets or sets the maximum number of items on a page.
        /// </summary>
        [Obsolete("Please use the IDataSetPager or the DefaultGridPager in the GridViewDataSet.Pager property")]
        int PageSize { get; }

        /// <summary>
        /// Gets or sets the total number of items in the data store without respect to paging.
        /// </summary>
        [Obsolete("Please use the IDataSetPager or the DefaultGridPager in the GridViewDataSet.Pager property")]
        int TotalItemsCount { get; }

        /// <summary>
        /// Gets whether the <see cref="PageIndex" /> represents the first page.
        /// </summary>
        [Obsolete("Please use the IDataSetPager or the DefaultGridPager in the GridViewDataSet.Pager property")]
        bool IsFirstPage { get; }

        /// <summary>
        /// Gets whether the <see cref="PageIndex" /> represents the last page.
        /// </summary>
        [Obsolete("Please use the IDataSetPager or the DefaultGridPager in the GridViewDataSet.Pager property")]
        bool IsLastPage { get; }

        /// <summary>
        /// Gets the total number of pages.
        /// </summary>
        [Obsolete("Please use the IDataSetPager or the DefaultGridPager in the GridViewDataSet.Pager property")]
        int PagesCount { get; }

        /// <summary>
        /// Gets a list of page indexes near the current page. It can be used to build data pagers.
        /// </summary>
        [Obsolete("Please use the IDataSetPager or the DefaultGridPager in the GridViewDataSet.Pager property")]
        IList<int> NearPageIndexes { get; }
    }
}
