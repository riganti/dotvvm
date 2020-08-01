using System;
using System.Collections.Generic;
using DotVVM.Framework.Query;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Framework.Controls
{

    public class DefaultGridPager<T, TNearPageIndexesProvider> : IDataSetIndexPager<T>
        where TNearPageIndexesProvider : INearPageIndexesProvider<T, DefaultGridPager<T, TNearPageIndexesProvider>>
    {
        public DefaultGridPager(TNearPageIndexesProvider nearPageIndexesProvider)
        {
            this.NearPageIndexesProvider = nearPageIndexesProvider;
        }

        /// <summary>
        /// Gets or sets the object that provides a list of page indexes near the current page.
        /// </summary>
        public TNearPageIndexesProvider NearPageIndexesProvider { get; set; }

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

        public IQuery<T> Apply(IQuery<T> query)
        {
            return PageSize > 0 ?
                   query.Skip(PageSize * PageIndex).Take(PageSize) :
                   query;
        }

        public void GoToPage(int index)
        {
            if (index < 0 || index >= PagesCount)
                throw new ArgumentException($"Page index {index} out of range [0, {PagesCount}).");
            PageIndex = index;
        }

        [Obsolete("Please use the IDataSetPager or the DefaultGridPager in the GridViewDataSet.Pager property")]
        public PagingOptions ToPagingOptions() => new PagingOptions(
                                                      this.IsFirstPage,
                                                      this.IsLastPage,
                                                      this.PagesCount,
                                                      this.PageIndex,
                                                      this.PageSize,
                                                      this.TotalItemsCount,
                                                      this.NearPageIndexes
                                                  );
    }

    /// <summary>
    /// Represents settings for paging.
    /// </summary>
    [Obsolete("Please use the IDataSetPager or the DefaultGridPager in the GridViewDataSet.Pager property")]
    public class PagingOptions : IPagingOptions
    {
        public PagingOptions(bool isFirstPage, bool isLastPage, int pagesCount, int pageIndex, int pageSize, int totalItemsCount, IList<int> nearPageIndexes)
        {
            this.IsFirstPage = isFirstPage;
            this.IsLastPage = isLastPage;
            this.PagesCount = pagesCount;
            this.PageIndex = pageIndex;
            this.PageSize = pageSize;
            this.TotalItemsCount = totalItemsCount;
            this.TotalItemsCount = totalItemsCount;
            this.NearPageIndexes = nearPageIndexes;
        }

        /// <summary>
        /// Gets whether the <see cref="PageIndex" /> represents the first page.
        /// </summary>

        [Obsolete("Please use the IDataSetPager or the DefaultGridPager in the GridViewDataSet.Pager property")]
        public bool IsFirstPage { get; }

        /// <summary>
        /// Gets whether the <see cref="PageIndex" /> represents the last page.
        /// </summary>
        [Obsolete("Please use the IDataSetPager or the DefaultGridPager in the GridViewDataSet.Pager property")]
        public bool IsLastPage { get; }

        /// <summary>
        /// Gets the total number of pages.
        /// </summary>
        [Obsolete("Please use the IDataSetPager or the DefaultGridPager in the GridViewDataSet.Pager property")]
        public int PagesCount { get; }

        /// <summary>
        /// Gets or sets a zero-based index of the current page.
        /// </summary>
        [Obsolete("Please use the IDataSetPager or the DefaultGridPager in the GridViewDataSet.Pager property")]
        public int PageIndex { get; }

        /// <summary>
        /// Gets or sets the maximum number of items on a page.
        /// </summary>
        [Obsolete("Please use the IDataSetPager or the DefaultGridPager in the GridViewDataSet.Pager property")]
        public int PageSize { get; }

        /// <summary>
        /// Gets or sets the total number of items in the data store without respect to paging.
        /// </summary>
        [Obsolete("Please use the IDataSetPager or the DefaultGridPager in the GridViewDataSet.Pager property")]
        public int TotalItemsCount { get; }

        /// <summary>
        /// Gets a list of page indexes near the current page. It can be used to build data pagers.
        /// </summary>
        [Obsolete("Please use the IDataSetPager or the DefaultGridPager in the GridViewDataSet.Pager property")]
        public IList<int> NearPageIndexes { get; }
    }
}
