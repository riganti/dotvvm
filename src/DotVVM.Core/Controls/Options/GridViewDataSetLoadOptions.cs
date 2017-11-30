using System.Linq;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Represents options required to reload data in <see cref="GridViewDataSet{T}" />.
    /// </summary>
    /// <typeparam name="T">The type of data to be loaded.</typeparam>
    public class GridViewDataSetLoadOptions<T> : IGridViewDataSetLoadOptions<T>
    {
        /// <summary>
        /// Gets or sets the settings for paging.
        /// </summary>
        public IPagingOptions PagingOptions { get; set; }

        /// <summary>
        /// Gets or sets the settings for sorting.
        /// </summary>
        public ISortingOptions SortingOptions { get; set; }

        /// <summary>
        /// Loads data from the <paramref name="source" /> queryable.
        /// </summary>
        /// <param name="source">The source to load data from.</param>
        public GridViewDataSetLoadedData<T> LoadDataFromQueryable(IQueryable<T> source)
        {
            source = ApplyFiltersToQueryable(source);

            var totalItemsCount = source.Count();

            var items = ApplyOptionsToQueryable(source).ToList();

            return new GridViewDataSetLoadedData<T>(items, totalItemsCount);
        }

        /// <summary>
        /// Applies filters to the <paramref name="queryable" /> before the total number
        /// of items is retrieved.
        /// </summary>
        /// <param name="queryable">The <see cref="IQueryable{T}" /> to modify.</param>
        protected virtual IQueryable<T> ApplyFiltersToQueryable(IQueryable<T> queryable)
        {
            return queryable;
        }

        /// <summary>
        /// Applies options to the <paramref name="queryable" /> after the total number
        /// of items is retrieved.
        /// </summary>
        /// <param name="queryable">The <see cref="IQueryable{T}" /> to modify.</param>
        protected virtual IQueryable<T> ApplyOptionsToQueryable(IQueryable<T> queryable)
        {
            if (SortingOptions?.SortExpression != null)
            {
                queryable = SortingOptions.ApplyToQueryable(queryable);
            }

            if (PagingOptions != null)
            {
                queryable = PagingOptions.ApplyToQueryable(queryable);
            }

            return queryable;
        }
    }
}