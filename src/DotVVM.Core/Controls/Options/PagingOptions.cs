using System;
using System.Collections.Generic;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Represents settings for paging.
    /// </summary>
    public class PagingOptions : IPagingOptions
    {
        /// <summary>
        /// Gets or sets the object that provides a list of page indexes near the current page.
        /// </summary>
        [Bind(Direction.None)]
        public INearPageIndexesProvider NearPageIndexesProvider { get; set; } = new DistanceNearPageIndexesProvider(5);

        /// <summary>
        /// Gets whether the <see cref="PageIndex" /> represents the first page.
        /// </summary>
        public bool IsFirstPage => PageIndex == 0;

        /// <summary>
        /// Gets whether the <see cref="PageIndex" /> represents the last page.
        /// </summary>
        public bool IsLastPage => PageIndex == PagesCount - 1;

        /// <summary>
        /// Gets the total number of pages.
        /// </summary>
        public int PagesCount
        {
            get
            {
                if (TotalItemsCount == 0 || PageSize == 0)
                {
                    return 1;
                }
                return (int)Math.Ceiling((double)TotalItemsCount / PageSize);
            }
        }

        /// <summary>
        /// Gets or sets a zero-based index of the current page.
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of items on a page.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Gets or sets the total number of items in the data store without respect to paging.
        /// </summary>
        public int TotalItemsCount { get; set; }

        /// <summary>
        /// Gets a list of page indexes near the current page. It can be used to build data pagers.
        /// </summary>
        public IList<int> NearPageIndexes => NearPageIndexesProvider.GetIndexes(this);
    }
}
