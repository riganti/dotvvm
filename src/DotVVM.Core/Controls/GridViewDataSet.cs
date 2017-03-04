using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Framework.Controls
{
    public delegate GridViewDataSetLoadedData<T> RequestRefresh<T>(IGridViewDataSetLoadOptions gridViewDataSetLoadOptions);

    public class GridViewDataSet<T> : IGridViewDataSet
    {
        [Bind(Direction.None)]
        public RequestRefresh<T> RequestRefresh { get; set; }

        public IList<T> Items { get; set; } = new List<T>();

        public bool IsRefreshRequired { get; set; }

        IList IBaseGridViewDataSet.Items => (IList) Items;

        RequestRefresh IBaseGridViewDataSet.RequestRefresh => RequestRefresh as RequestRefresh;


        public IPagingOptions PagingOptions { get; set; } = new PagingOptions();


        public ISortOptions SortOptions { get; set; } = new SortOptions();



        protected virtual GridViewDataSetLoadOptions CreateGridViewDataSetLoadOptions()
        {
            return new GridViewDataSetLoadOptions()
            {
                PagingOptions = PagingOptions,
                SortOptions = SortOptions
            };
        }

        public string PrimaryKeyPropertyName { get; set; }
        public object EditRowId { get; set; }

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
    }
}