using System.Collections;
using System.Collections.Generic;

namespace DotVVM.Framework.Controls
{
    public interface IGridViewDataSet
    {
        bool IsFirstPage { get; }
        bool IsLastPage { get; }
        IList Items { get; }
        int PageIndex { get; set; }
        int PagesCount { get; }
        int PageSize { get; set; }
        bool SortDescending { get; set; }
        string SortExpression { get; set; }
        int TotalItemsCount { get; set; }
        IList<int> NearPageIndexes { get; } 
        void GoToFirstPage();
        void GoToLastPage();
        void GoToNextPage();
        void GoToPage(int index);
        void GoToPreviousPage();
        void Reset();
    }
}