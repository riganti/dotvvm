using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Provides a list of page indexes near current paged based on distance.
    /// </summary>
    public class DistanceNearPageIndexesProvider : INearPageIndexesProvider
    {
        private readonly int distance;

        /// <summary>
        /// Initializes a new instance of the <see cref="DistanceNearPageIndexesProvider" /> class.
        /// </summary>
        /// <param name="distance">The distance specifying the range of page indexes to return.</param>
        public DistanceNearPageIndexesProvider(int distance)
        {
            this.distance = distance;
        }

        /// <summary>
        /// Gets a list of page indexes near current page.
        /// </summary>
        /// <param name="pagingOptions">The settings for paging.</param>
        public IList<int> GetIndexes(IPagingOptions pagingOptions)
        {
            var count = pagingOptions.PagesCount;
            var index = Math.Max(0, Math.Min(count - 1, pagingOptions.PageIndex)); // clamp index to be a valid page
            var firstIndex = Math.Max(index - distance, 0);
            var lastIndex = Math.Min(index + distance + 1, count);

            return Enumerable
                .Range(firstIndex, lastIndex - firstIndex)
                .ToList();
        }
    }
}
