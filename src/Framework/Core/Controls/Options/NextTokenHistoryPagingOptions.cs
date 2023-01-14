using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Framework.Controls
{
    // TODO: comments
    public class NextTokenHistoryPagingOptions : IPagingFirstPageCapability, IPagingPreviousPageCapability, IPagingPageIndexCapability
    {
        public List<string?> TokenHistory { get; set; } = new List<string?> { null };

        public int PageIndex { get; set; } = 0;

        public bool IsFirstPage => PageIndex == 0;

        public void GoToFirstPage() => PageIndex = 0;

        public bool IsLastPage => PageIndex == TokenHistory.Count - 1;

        public void GoToNextPage() => PageIndex++;

        public void GoToPreviousPage() => PageIndex--;

        public void GoToPage(int pageIndex)
        {
            if (TokenHistory.Count <= pageIndex)
                throw new ArgumentOutOfRangeException(nameof(pageIndex));
            PageIndex = pageIndex;
        }

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
            var lastIndex = Math.Min(PageIndex + distance + 1, TokenHistory.Count);
            return Enumerable
                .Range(firstIndex, lastIndex - firstIndex)
                .ToList();
        }

    }
}
