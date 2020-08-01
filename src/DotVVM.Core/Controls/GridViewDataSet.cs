using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.Query;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Represents a collection of items with paging, sorting and row edit capabilities.
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="GenericGridViewDataSet{T, TSorter, TPager, TFilter}.Items" /> elements.</typeparam>
    public class GridViewDataSet<T> : GenericGridViewDataSet<T, DefaultGridSorter<T>, DefaultGridPager<T, DistanceNearPageIndexesProvider<T>>, NopDataSetFilter<T>>
    {
        public GridViewDataSet(int nearPageDistance = 5) : base(
            new DefaultGridSorter<T>(),
            new DefaultGridPager<T, DistanceNearPageIndexesProvider<T>>(new DistanceNearPageIndexesProvider<T>(nearPageDistance)),
            new NopDataSetFilter<T>())
        {
        }

        /// <summary>
        /// Gets or sets the settings for paging.
        /// </summary>
        [Bind(Direction.None)]
        [Obsolete("Please use the IDataSetPager or the DefaultGridPager in the GridViewDataSet.Pager property")]
        public DefaultGridPager<T, DistanceNearPageIndexesProvider<T>> PagingOptions => Pager;

        /// <summary>
        /// Gets or sets the settings for row (item) edit feature.
        /// </summary>
        public IRowEditOptions RowEditOptions { get; set; } = new RowEditOptions();

        /// <summary>
        /// Gets or sets the settings for sorting.
        /// </summary>
        [Bind(Direction.None)]
        [Obsolete("Please use the IDataSetSorter or the DefaultDataSetSorter in the GridViewDataSet.Sorter property")]
        public DefaultGridSorter<T> SortingOptions => Sorter;

        /// <summary>
        /// Loads data into the <see cref="GridViewDataSet{T}" /> from the given <see cref="IQueryable{T}" /> source.
        /// </summary>
        /// <param name="source">The source to load data from.</param>
        public void LoadFromQueryable(IQueryable<T> source)
        {
            Items = ApplyOptionsToQueryable(source).ToList();
            Pager.TotalItemsCount = source.Count();
            IsRefreshRequired = false;
        }

        /// <summary>
        /// Applies options to the <paramref name="queryable" /> after the total number
        /// of items is retrieved.
        /// </summary>
        /// <param name="queryable">The <see cref="IQueryable{T}" /> to modify.</param>
        public virtual IQueryable<T> ApplyOptionsToQueryable(IQueryable<T> queryable) =>
            ApplyToQuery(queryable.AsQuery()).AsQueryable(queryable.Provider);

        /// <summary>
        /// Applies sorting to the <paramref name="queryable" /> after the total number
        /// of items is retrieved.
        /// </summary>
        /// <param name="queryable">The <see cref="IQueryable{T}" /> to modify.</param>
        [Obsolete("Please prefer to use a different pager and sorter to overriding the ApplyOptionsToQueryable method. When calling, prefer to invoke Sorter.Apply and Pager.Apply directly")]
        public virtual IQueryable<T> ApplySortingToQueryable(IQueryable<T> queryable) =>
            Sorter.Apply(queryable.AsQuery()).AsQueryable(queryable.Provider);

        /// <summary>
        /// Applies paging to the <paramref name="queryable" /> after the total number
        /// of items is retrieved.
        /// </summary>
        /// <param name="queryable">The <see cref="IQueryable{T}" /> to modify.</param>
        [Obsolete("Please prefer to use a different pager to overridint the ApplyPagingToQueryable method. When calling, prefer to invoke Pager.Apply directly")]
        public virtual IQueryable<T> ApplyPagingToQueryable(IQueryable<T> queryable)
        {
            return Pager.Apply(queryable.AsQuery()).AsQueryable(queryable.Provider);
        }
    }
}
