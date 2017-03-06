using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Framework.Controls
{
    public class DistanceNearPageIndexesProvider : INearPageIndexesProvider
    {
        private readonly int distance;

        public DistanceNearPageIndexesProvider(int distance)
        {
            this.distance = distance;
        }

        public IList<int> GetIndexes(IPagingOptions pagingOptions)
        {
            return
                Enumerable.Range(0, pagingOptions.PagesCount)
                    .Where(n => Math.Abs(n - pagingOptions.PageIndex) <= distance)
                    .ToList();
        }
    }
}