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

    }
}
