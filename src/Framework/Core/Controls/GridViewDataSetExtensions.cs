using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DotVVM.Framework.Controls
{
    public static class GridViewDataSetExtensions
    {
        /// <summary>
        /// Loads dataset from the specified <paramref name="queryable" />: Applies filtering, sorting and paging options, and collects the results. If <see cref="PagingOptions"/> is used, the total number of items (after applying filtering) is retrieved and stored in the PagingOptions property.
        /// </summary>
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

            if (pagingOptions is IPagingOptionsLoadingPostProcessor pagingOptionsLoadingPostProcessor)
            {
                pagingOptionsLoadingPostProcessor.ProcessLoadedItems(filtered, dataSet.Items);
            }

            dataSet.IsRefreshRequired = false;
        }

#if NET6_0_OR_GREATER
        /// <summary>
        /// Loads dataset from the specified <paramref name="queryable" />: Applies filtering, sorting and paging options, and collects the results using IAsyncEnumerable. If <see cref="PagingOptions"/> is used, the total number of items (after applying filtering) is retrieved and stored in the PagingOptions property.
        /// </summary>
        /// <exception cref="ArgumentException">The specified IQueryable does not support async enumeration.</exception>
        public static async Task LoadFromQueryableAsync<T>(this IGridViewDataSet<T> dataSet, IQueryable<T> queryable, CancellationToken cancellationToken = default)
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
            var result = (await AsyncQueryableImplementation.QueryableToListAsync(paged, cancellationToken)).ToList();
            dataSet.Items = result;

            if (pagingOptions is IPagingOptionsLoadingPostProcessor pagingOptionsLoadingPostProcessor)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await pagingOptionsLoadingPostProcessor.ProcessLoadedItemsAsync(filtered, result, cancellationToken);
            }

            dataSet.IsRefreshRequired = false;
        }
#endif

        /// <summary> Sets <see cref="IPagingOptions"/> to the first page, and sets the <see cref="IRefreshableGridViewDataSet.IsRefreshRequired"/> property to true. </summary>
        /// <remarks> To reload the dataset, you must then call <see cref="LoadFromQueryable{T}(IGridViewDataSet{T}, IQueryable{T})"/> or a similar method. </remarks>
        public static void GoToFirstPageAndRefresh(this IPageableGridViewDataSet<IPagingFirstPageCapability> dataSet)
        {
            dataSet.PagingOptions.GoToFirstPage();
            (dataSet as IRefreshableGridViewDataSet)?.RequestRefresh();
        }

        /// <summary> Sets <see cref="IPagingOptions"/> to the last page, and sets the <see cref="IRefreshableGridViewDataSet.IsRefreshRequired"/> property to true. </summary>
        /// <remarks> To reload the dataset, you must then call <see cref="LoadFromQueryable{T}(IGridViewDataSet{T}, IQueryable{T})"/> or a similar method. </remarks>
        public static void GoToLastPageAndRefresh(this IPageableGridViewDataSet<IPagingLastPageCapability> dataSet)
        {
            dataSet.PagingOptions.GoToLastPage();
            (dataSet as IRefreshableGridViewDataSet)?.RequestRefresh();
        }

        /// <summary> Sets <see cref="IPagingOptions"/> to the previous page, and sets the <see cref="IRefreshableGridViewDataSet.IsRefreshRequired"/> property to true. </summary>
        /// <remarks> To reload the dataset, you must then call <see cref="LoadFromQueryable{T}(IGridViewDataSet{T}, IQueryable{T})"/> or a similar method. </remarks>
        public static void GoToPreviousPageAndRefresh(this IPageableGridViewDataSet<IPagingPreviousPageCapability> dataSet)
        {
            dataSet.PagingOptions.GoToPreviousPage();
            (dataSet as IRefreshableGridViewDataSet)?.RequestRefresh();
        }

        /// <summary> Sets <see cref="IPagingOptions"/> to the next page, and sets the <see cref="IRefreshableGridViewDataSet.IsRefreshRequired"/> property to true. </summary>
        /// <remarks> To reload the dataset, you must then call <see cref="LoadFromQueryable{T}(IGridViewDataSet{T}, IQueryable{T})"/> or a similar method. </remarks>
        public static void GoToNextPageAndRefresh(this IPageableGridViewDataSet<IPagingNextPageCapability> dataSet)
        {
            dataSet.PagingOptions.GoToNextPage();
            (dataSet as IRefreshableGridViewDataSet)?.RequestRefresh();
        }

        /// <summary> Sets <see cref="IPagingOptions"/> to the page number <paramref name="pageIndex"/> (indexed from 0), and sets the <see cref="IRefreshableGridViewDataSet.IsRefreshRequired"/> property to true. </summary>
        /// <remarks> To reload the dataset, you must then call <see cref="LoadFromQueryable{T}(IGridViewDataSet{T}, IQueryable{T})"/> or a similar method. </remarks>
        public static void GoToPageAndRefresh(this IPageableGridViewDataSet<IPagingPageIndexCapability> dataSet, int pageIndex)
        {
            dataSet.PagingOptions.GoToPage(pageIndex);
            (dataSet as IRefreshableGridViewDataSet)?.RequestRefresh();
        }


        public static async Task LoadAsync<T, TFilteringOptions, TSortingOptions, TPagingOptions, TRowInsertOptions, TRowEditOptions>(
            this GenericGridViewDataSet<T, TFilteringOptions, TSortingOptions, TPagingOptions, TRowInsertOptions, TRowEditOptions> dataSet,
            Func<GridViewDataSetOptions<TFilteringOptions, TSortingOptions, TPagingOptions>, Task<GridViewDataSetResult<T, TFilteringOptions, TSortingOptions, TPagingOptions>>> loadMethod)
                where TFilteringOptions : IFilteringOptions
                where TSortingOptions : ISortingOptions
                where TPagingOptions : IPagingOptions
                where TRowInsertOptions : IRowInsertOptions
                where TRowEditOptions : IRowEditOptions
        {
            var options = dataSet.GetOptions();
            var result = await loadMethod(options);
            dataSet.ApplyResult(result);
        }

    }
}
