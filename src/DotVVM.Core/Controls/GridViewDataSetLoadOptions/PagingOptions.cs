using System;
using System.Collections.Generic;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Framework.Controls
{
    public class PagingOptions : IPagingOptions
    {
        [Bind(Direction.None)]
        public INearPageIndexesProvider NearPageIndexesProvider { get; set; } = new DistanceNearPageIndexesProvider(5);

        public bool IsFirstPage => PageIndex == 0;

        public bool IsLastPage => PageIndex == PagesCount - 1;

        public int PagesCount
        {
            get
            {
                if (TotalItemsCount == 0 || PageSize == 0)
                {
                    return 1;
                }
                return (int) Math.Ceiling((double) TotalItemsCount / PageSize);
            }
        }

        public int PageIndex { get; set; }

        public int PageSize { get; set; }

        public int TotalItemsCount { get; set; }

        public IList<int> NearPageIndexes => NearPageIndexesProvider.GetIndexes(this);
    }
}