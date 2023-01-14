using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Represents settings for paging.
    /// </summary>
    public class PagingOptions : IPagingOptions, IPagingFirstPageCapability, IPagingLastPageCapability, IPagingPreviousPageCapability, IPagingNextPageCapability, IPagingPageIndexCapability, IPagingPageSizeCapability, IPagingTotalItemsCountCapability, IApplyToQueryable
    {
        /// <summary>
        /// Gets or sets the object that provides a list of page indexes near the current page.
        /// </summary>
        [Bind(Direction.None)]
        [Obsolete("This interface was removed. If you want to provide your custom page numbers, inherit the PagingOptions class and override the GetNearPageIndexes method.", true)]
        public INearPageIndexesProvider NearPageIndexesProvider { get; set; }

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

        public void GoToFirstPage() => PageIndex = 0;

        public void GoToLastPage() => PageIndex = PagesCount - 1;

        public void GoToNextPage()
        {
            if (PageIndex < PagesCount - 1)
            {
                PageIndex++;
            }
        }
        public void GoToPreviousPage()
        {
            if (PageIndex > 0)
            {
                PageIndex--;
            }
        }

        public void GoToPage(int pageIndex)
        {
            if (PageIndex >= 0 && PageIndex < PagesCount)
            {
                PageIndex = pageIndex;
            }
        }


        /// <summary>
        /// Gets a list of page indexes near the current page. It can be used to build data pagers.
        /// </summary>
        public IList<int> NearPageIndexes => GetNearPageIndexes();

        /// <summary>
        /// Gets a list of page indexes near the current page. Override this method to provide your own strategy.
        /// </summary>
        public virtual IList<int> GetNearPageIndexes()
        {
            return GetDefaultNearPageIndexes(5);
        }

        /// <summary>
        /// Returns the near page indexes in the maximum specified distance from the current page index.
        /// </summary>
        protected IList<int> GetDefaultNearPageIndexes(int distance)
        {
            var firstIndex = Math.Max(PageIndex - distance, 0);
            var lastIndex = Math.Min(PageIndex + distance + 1, PagesCount);
            return Enumerable
                .Range(firstIndex, lastIndex - firstIndex)
                .ToList();
        }

        public IQueryable<T> ApplyToQueryable<T>(IQueryable<T> queryable)
        {
            return PagingImplementation.ApplyPagingToQueryable(queryable, this);
        }
    }
}
