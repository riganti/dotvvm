using System.Linq;

namespace DotVVM.Framework.Controls
{
    public class GridViewDataSetLoadOptions<T> : IGridViewDataSetLoadOptions
    {
        public IPagingOptions PagingOptions { get; set; }

        public ISortingOptions SortingOptions { get; set; }


        public GridViewDataSetLoadedData<T> GetDataFromQueryable(IQueryable<T> queryable)
        {
            queryable = ApplyFiltersToQueryable(queryable);

            var totalItemsCount = queryable.Count();

            var items = ApplyOptionsToQueryable(queryable).ToList();

            return new GridViewDataSetLoadedData<T>(items, totalItemsCount);
        }

        GridViewDataSetLoadedData IGridViewDataSetLoadOptions.GetDataFromQueryable(IQueryable queryable)
        {
            return GetDataFromQueryable((IQueryable<T>)queryable);
        }


        protected virtual IQueryable<T> ApplyFiltersToQueryable(IQueryable<T> queryable)
        {
            // extensibility point for modification of queryable before counting (e.g. filtering)
            return queryable;
        }

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