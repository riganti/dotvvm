using System;
using System.Linq;
using DotVVM.Core.Controls;

namespace DotVVM.Framework.Controls
{
    public class GridViewDataSetLoadOptions : IGridViewDataSetOptions
    {

        public GridViewDataSetLoadOptions(IBaseGridViewDataSet gridViewDataSet)
        {
            DataSet = gridViewDataSet;
        }

        public IBaseGridViewDataSet DataSet { get; }

        public IPagingOptions PagingOptions
        {
            get
            {
                var pagingOptions = DataSet as IPagingOptions;
                if (pagingOptions == null)
                {
                    throw new NotSupportedException("The DataSet doesn't support paging!");
                }
                return pagingOptions;
            }
        }

        public ISortOptions SortOptions
        {
            get
            {
                var sortOptions = DataSet as ISortOptions;
                if (sortOptions == null)
                {
                    throw new NotSupportedException("The DataSet doesn't support sorting!");
                }
                return sortOptions;
            }
        }


        public virtual GridViewDataSetLoadedData LoadFromQueryable<T>(IQueryable<T> queryable)
        {
            var count = queryable.Count();

            if (DataSet is ISortableGridViewDataSet sortableDataSet)
            {
                queryable = sortableDataSet.ApplySortExpression(queryable);
            }
            if (DataSet is IPageableGridViewDataSet pageableDataSet)
            {
                queryable = pageableDataSet.ApplyPaging(queryable);
            }
            var items = queryable.ToList();

            return new GridViewDataSetLoadedData<T>(items, count);
        }
    }
}