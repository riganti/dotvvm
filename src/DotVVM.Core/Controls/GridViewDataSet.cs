using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Framework.Controls
{
    public class GridViewDataSet<T> : IGridViewDataSet
    {
        
        public GridViewDataSet()
        {
            Items = new List<T>();
        }



        #region IBaseGridViewDataSet

        public IList<T> Items { get; set; }

        IList IBaseGridViewDataSet.Items => (IList)Items;

        public bool IsRefreshRequired { get; set; }

        public Func<IGridViewDataSetOptions, GridViewDataSetLoadedData> OnLoadingData { get; set; }
        

        public void Reset()
        {
            PageIndex = 0;
        }

        public void LoadFromQueryable(IQueryable<T> queryable)
        {
            var data = CreateGridViewDataSetLoadOptions();
            FillDataSet(data.LoadFromQueryable(queryable));
        }

        protected virtual void NotifyRefreshRequired()
        {
            if (OnLoadingData != null)
            {
                var data = OnLoadingData(CreateGridViewDataSetLoadOptions());
                FillDataSet(data);
            }
            else
            {
                IsRefreshRequired = true;
            }
        }

        protected virtual GridViewDataSetLoadOptions CreateGridViewDataSetLoadOptions()
        {
            return new GridViewDataSetLoadOptions(this);
        }

        protected virtual void FillDataSet(GridViewDataSetLoadedData data)
        {
            Items = data.Items.OfType<T>().ToList();
            TotalItemsCount = data.TotalItemsCount;
            IsRefreshRequired = false;
        }

        #endregion



        #region ISortableGridViewDataSet

        public string SortExpression { get; set; }

        public bool SortDescending { get; set; }

        public virtual void SetSortExpression(string expression)
        {
            if (SortExpression == expression)
            {
                SortDescending = !SortDescending;
                GoToFirstPage();
            }
            else
            {
                SortExpression = expression;
                SortDescending = false;
                GoToFirstPage();
            }
        }
        
        #endregion



        #region IRowEditGridViewDataSet 

        public string PrimaryKeyPropertyName { get; set; }

        public object EditRowId { get; set; }

        #endregion



        #region IPageableGridViewDataSet

        public int PageIndex { get; set; }

        public int PageSize { get; set; }

        public int TotalItemsCount { get; set; }

        [Bind(Direction.None)]
        public INearPageIndexesProvider NearPageIndexesProvider { get; set; } = new DistanceNearPageIndexesProvider(5);

        public IList<int> NearPageIndexes
        {
            get { return NearPageIndexesProvider.GetIndexes(this); }
        }

        public int PagesCount
        {
            get
            {
                if (TotalItemsCount == 0 || PageSize == 0) return 1;
                return (int)Math.Ceiling((double)TotalItemsCount / PageSize);
            }
        }

        public bool IsFirstPage
        {
            get { return PageIndex == 0; }
        }

        public bool IsLastPage
        {
            get { return PageIndex == PagesCount - 1; }
        }

        public void GoToFirstPage()
        {
            PageIndex = 0;
            NotifyRefreshRequired();
        }

        public void GoToPreviousPage()
        {
            if (!IsFirstPage)
            {
                PageIndex--;
                NotifyRefreshRequired();
            }
        }

        public void GoToLastPage()
        {
            PageIndex = PagesCount - 1;
            NotifyRefreshRequired();
        }

        public void GoToNextPage()
        {
            if (!IsLastPage)
            {
                PageIndex++;
                NotifyRefreshRequired();
            }
        }

        public void GoToPage(int index)
        {
            PageIndex = index;
            NotifyRefreshRequired();
        }

        #endregion

    }
}