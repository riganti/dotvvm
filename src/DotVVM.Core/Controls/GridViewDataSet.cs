using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Framework.Controls
{
    public delegate GridViewDataSetLoadedData<T> RequestRefresh<T>(GridViewDataSetLoadOptions gridViewDataSetLoadOptions);

    public class GridViewDataSet<T> : IGridViewDataSet
    {
        [Bind(Direction.None)]
        public RequestRefresh<T> RequestRefresh { get; set; }

        RequestRefresh IBaseGridViewDataSet.RequestRefresh => RequestRefresh as RequestRefresh;
        
        public IList<T> Items { get; set; } = new List<T>();

        IList IBaseGridViewDataSet.Items => (IList)Items;

        public bool IsRefreshRequired { get; set; }

        

        public IPagingOptions PagingOptions { get; set; } = new PagingOptions();

        public ISortOptions SortOptions { get; set; } = new SortOptions();
        
        public IRowEditOptions RowEditOptions { get; set; } = new RowEditOptions();



        public virtual void ReloadData()
        {
            NotifyRefreshRequired();
        }
        
        public void GoToFirstPage()
        {
            PagingOptions.PageIndex = 0;
            NotifyRefreshRequired();
        }

        public void GoToPreviousPage()
        {
            if (!PagingOptions.IsFirstPage)
            {
                PagingOptions.PageIndex--;
                NotifyRefreshRequired();
            }
        }

        public void GoToLastPage()
        {
            PagingOptions.PageIndex = PagingOptions.PagesCount - 1;
            NotifyRefreshRequired();
        }

        public void GoToNextPage()
        {
            if (!PagingOptions.IsLastPage)
            {
                PagingOptions.PageIndex++;
                NotifyRefreshRequired();
            }
        }

        public void GoToPage(int index)
        {
            PagingOptions.PageIndex = index;
            NotifyRefreshRequired();
        }


        public virtual void SetSortExpression(string expression)
        {
            if (SortOptions.SortExpression == expression)
            {
                SortOptions.SortDescending = !SortOptions.SortDescending;
                GoToFirstPage();
            }
            else
            {
                SortOptions.SortExpression = expression;
                SortOptions.SortDescending = false;
                GoToFirstPage();
            }
        }




        protected virtual GridViewDataSetLoadOptions CreateGridViewDataSetLoadOptions()
        {
            return new GridViewDataSetLoadOptions
            {
                PagingOptions = PagingOptions,
                SortOptions = SortOptions
            };
        }

        protected virtual void NotifyRefreshRequired()
        {
            if (RequestRefresh != null)
            {
                var gridViewDataSetLoadedData = RequestRefresh(CreateGridViewDataSetLoadOptions());
                FillDataSet(gridViewDataSetLoadedData);
            }
            else
            {
                IsRefreshRequired = true;
            }
        }

        protected virtual void FillDataSet(GridViewDataSetLoadedData data)
        {
            Items = data.Items.OfType<T>().ToList();
            PagingOptions.TotalItemsCount = data.TotalItemsCount;
            IsRefreshRequired = false;
        }

        public void LoadFromQueryable(IQueryable<T> queryable)
        {
            var gridViewDataSetLoadedData = this.GetDataFromQueryable(queryable);
            FillDataSet(gridViewDataSetLoadedData);
        }


        [Bind(Direction.None)]
        [Obsolete("Use PagingOptions.PageIndex instead. This property will be removed in future versions.")]
        public int PageIndex { get => PagingOptions.PageIndex; set => PagingOptions.PageIndex = value; }

        [Bind(Direction.None)]
        [Obsolete("Use PagingOptions.PageSize instead. This property will be removed in future versions.")]
        public int PageSize { get => PagingOptions.PageSize; set => PagingOptions.PageSize = value; }

        [Bind(Direction.None)]
        [Obsolete("Use PagingOptions.TotalItemsCount instead. This property will be removed in future versions.")]
        public int TotalItemsCount { get => PagingOptions.TotalItemsCount; set => PagingOptions.TotalItemsCount = value; }

        [Bind(Direction.None)]
        [Obsolete("Use PagingOptions.IsFirstPage instead. This property will be removed in future versions.")]
        public bool IsFirstPage { get => PagingOptions.IsFirstPage; }

        [Bind(Direction.None)]
        [Obsolete("Use PagingOptions.IsLastPage instead. This property will be removed in future versions.")]
        public bool IsLastPage { get => PagingOptions.IsLastPage; }

        [Bind(Direction.None)]
        [Obsolete("Use PagingOptions.PagesCount instead. This property will be removed in future versions.")]
        public int PagesCount { get => PagingOptions.PagesCount; }

        [Bind(Direction.None)]
        [Obsolete("Use PagingOptions.NearPageIndexes instead. This property will be removed in future versions.")]
        public IList<int> NearPageIndexes { get => PagingOptions.NearPageIndexes; }


        [Bind(Direction.None)]
        [Obsolete("Use SortOptions.SortDescending instead. This property will be removed in future versions.")]
        public bool SortDescending { get => SortOptions.SortDescending; set => SortOptions.SortDescending = value; }

        [Bind(Direction.None)]
        [Obsolete("Use SortOptions.SortExpression instead. This property will be removed in future versions.")]
        public string SortExpression { get => SortOptions.SortExpression; set => SortOptions.SortExpression = value; }


        [Bind(Direction.None)]
        [Obsolete("Use RowEditOptions.PrimaryKeyPropertyName instead. This property will be removed in future versions.")]
        public string PrimaryKeyPropertyName { get => RowEditOptions.PrimaryKeyPropertyName; set => RowEditOptions.PrimaryKeyPropertyName = value; }

        [Bind(Direction.None)]
        [Obsolete("Use RowEditOptions.EditRowId instead. This property will be removed in future versions.")]
        public object EditRowId { get => RowEditOptions.EditRowId; set => RowEditOptions.EditRowId = value; }
    }
}