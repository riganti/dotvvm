using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Framework.Controls
{
    // TODO: comments
    public class NextTokenHistoryPagingOptions : IPagingFirstPageCapability, IPagingPreviousPageCapability, IPagingPageIndexCapability, IPagingNextPageCapability
    {
        public List<string?> TokenHistory { get; set; } = new();

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

        /// <summary>
        /// Gets the token for loading the current page. The first page token is always null.
        /// </summary>
        public virtual string? GetCurrentPageToken()
        {
            if (TokenHistory.Count == 0)
            {
                TokenHistory.Add(null);
            }

            return PageIndex < TokenHistory.Count ? TokenHistory[PageIndex] : throw new InvalidOperationException($"Cannot get token for page {PageIndex}, because the token history contains only {TokenHistory.Count} items.");
        }

        /// <summary>
        /// Saves the token for loading the next page to the token history.
        /// </summary>
        public virtual void SaveNextPageToken(string? nextPageToken)
        {
            if (TokenHistory.Count == 0)
            {
                TokenHistory.Add(null);
            }

            if (IsLastPage && nextPageToken != null)
            {
                TokenHistory.Add(nextPageToken);
            }
            else if (PageIndex > TokenHistory.Count)
            {
                throw new InvalidOperationException($"Cannot save token for page {PageIndex}, because the token history contains only {TokenHistory.Count} items..");
            }
        }

        public IList<int> NearPageIndexes => GetNearPageIndexes();

        /// <summary>
        /// Gets a list of page indexes near the current page. Override this method to provide your own strategy.
        /// </summary>
        protected virtual IList<int> GetNearPageIndexes()
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
