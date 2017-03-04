using System.Collections.Generic;

namespace DotVVM.Framework.Controls
{
    public interface IPagingOptions
    {
        int PageIndex { get; set; }
        int PageSize { get; set; }
        int TotalItemsCount { get; set; }
        bool IsFirstPage { get; }
        bool IsLastPage { get; }
        int PagesCount { get; }
        IList<int> NearPageIndexes { get; }
    }
}