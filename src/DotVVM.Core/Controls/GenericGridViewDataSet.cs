using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DotVVM.Framework.Query;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Framework.Controls
{
    public interface IQueryModifier<T>
    {
        IQuery<T> Apply(IQuery<T> query);
    }
    public interface IDataSetSorter<T>: IQueryModifier<T>
    {
        void ColumnSortClick(IBaseGridViewDataSet<T> dataSet, string? columnName);
    }
    public interface ISingleColumnSorter<T>: IDataSetSorter<T>
    {
        /// <summary>
        /// Gets or sets whether the sort order should be descending.
        /// </summary>
        bool SortDescending { get; }

        /// <summary>
        /// Gets or sets the name of the property that is used for sorting. Null means the grid should not be sorted.
        /// </summary>
        string? SortExpression { get; }
    }
    public interface IDataSetFilter<T>: IQueryModifier<T>
    {
    }
    public interface IDataSetPager<T>: IQueryModifier<T>
    {
    }
    public interface IDataSetIndexPager<T>: IDataSetPager<T>
    {
        int PageIndex { get; }
        int PagesCount { get; }
        /// <summary>
        /// Navigates to the specific page.
        /// </summary>
        /// <param name="index">The zero-based index of the page to navigate to.</param>
        void GoToPage(int index);
    }

    public interface IDataSetReloader<T, DataSet>
        where DataSet: IComposableDataSetOptions<T>
    {
        List<T> LoadItems(DataSet dataSet);
    }

    public interface IComposableDataSetOptions<T>
    {
        IEnumerable<IQueryModifier<T>> QueryModifiers { get; }
    }

    public abstract class GenericGridViewDataSet<T, TSorter, TPager, TFilter> : IGridViewDataSet<T, TSorter, TPager, TFilter>, IPageableGridViewDataSet<T, TPager>
        where TSorter: IDataSetSorter<T>
        where TPager: IDataSetPager<T>
        where TFilter: IDataSetFilter<T>
    {
        public List<T> Items { get; set; } = new List<T>();

        IReadOnlyList<T> IBaseGridViewDataSet<T>.Items => this.Items;

        /// <summary>
        /// Gets or sets whether the data should be refreshed. This property is set to true automatically
        /// when paging, sorting or other options change.
        /// </summary>
        public bool IsRefreshRequired { get; set; } = true;

        /// <summary>
        /// Requests to reload data into the <see cref="GridViewDataSet{T}" />.
        /// </summary>
        public virtual void RequestRefresh()
        {
            IsRefreshRequired = true;
        }

        protected virtual IQuery<T> ApplyToQuery(IQuery<T> q)
        {
            q = Filter.Apply(q);
            q = Sorter.Apply(q);
            q = Pager.Apply(q);
            return q;
        }

        public TSorter Sorter { get; set; }
        public TPager Pager { get; set; }
        public TFilter Filter { get; set; }

        public IRowEditOptions RowEditOptions { get; set; } // TODO: editing?

        protected GenericGridViewDataSet(TSorter sorter, TPager pager, TFilter filter)
        {
            this.Sorter = sorter;
            this.Pager = pager;
            this.Filter = filter;
            this.RowEditOptions = new RowEditOptions();
        }
    }
}
