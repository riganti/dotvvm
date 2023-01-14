using System;
using System.Linq;

namespace DotVVM.Framework.Controls
{
    public static class GridViewDataSetExtensions
    {

        public static void LoadFromQueryable<T>(this IGridViewDataSet<T> dataSet, IQueryable<T> queryable)
        {
            if (dataSet.FilteringOptions is not IApplyToQueryable filteringOptions)
            {
                throw new ArgumentException($"The FilteringOptions of {dataSet.GetType()} must implement IApplyToQueryable!");
            }
            if (dataSet.SortingOptions is not IApplyToQueryable sortingOptions)
            {
                throw new ArgumentException($"The SortingOptions of {dataSet.GetType()} must implement IApplyToQueryable!");
            }
            if (dataSet.PagingOptions is not IApplyToQueryable pagingOptions)
            {
                throw new ArgumentException($"The PagingOptions of {dataSet.GetType()} must implement IApplyToQueryable!");
            }

            var filtered = filteringOptions.ApplyToQueryable(queryable);
            var sorted = sortingOptions.ApplyToQueryable(filtered);
            var paged = pagingOptions.ApplyToQueryable(sorted);
            dataSet.Items = paged.ToList();

            if (pagingOptions is IPagingTotalItemsCountCapability pagingTotalItemsCount)
            {
                pagingTotalItemsCount.TotalItemsCount = filtered.Count();
            }

            dataSet.IsRefreshRequired = false;
        }

        public static void GoToFirstPageAndRefresh(this IPageableGridViewDataSet<IPagingFirstPageCapability> dataSet)
        {
            dataSet.PagingOptions.GoToFirstPage();
            (dataSet as IRefreshableGridViewDataSet)?.RequestRefresh();
        }

        public static void GoToLastPageAndRefresh(this IPageableGridViewDataSet<IPagingLastPageCapability> dataSet)
        {
            dataSet.PagingOptions.GoToLastPage();
            (dataSet as IRefreshableGridViewDataSet)?.RequestRefresh();
        }

        public static void GoToPreviousPageAndRefresh(this IPageableGridViewDataSet<IPagingPreviousPageCapability> dataSet)
        {
            dataSet.PagingOptions.GoToPreviousPage();
            (dataSet as IRefreshableGridViewDataSet)?.RequestRefresh();
        }

        public static void GoToNextPageAndRefresh(this IPageableGridViewDataSet<IPagingNextPageCapability> dataSet)
        {
            dataSet.PagingOptions.GoToNextPage();
            (dataSet as IRefreshableGridViewDataSet)?.RequestRefresh();
        }

        public static void GoToPageAndRefresh(this IPageableGridViewDataSet<IPagingPageIndexCapability> dataSet, int pageIndex)
        {
            dataSet.PagingOptions.GoToPage(pageIndex);
            (dataSet as IRefreshableGridViewDataSet)?.RequestRefresh();
        }
    }
}
