using System.Collections.Generic;

namespace DotVVM.Framework.Controls
{
    public interface IPageableGridViewDataSet : IPagingOptions
    {
        bool IsFirstPage { get; }
        bool IsLastPage { get; }
        int PagesCount { get; }

        void GoToFirstPage();
        void GoToLastPage();
        void GoToNextPage();
        void GoToPage(int index);
        void GoToPreviousPage();

        int TotalItemsCount { get; set; }
        IList<int> NearPageIndexes { get; }
    }
}