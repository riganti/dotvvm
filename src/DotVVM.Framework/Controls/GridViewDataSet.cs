using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Controls
{
    public class GridViewDataSet<T> : IGridViewDataSet
    {

        public string SortExpression { get; set; }

        public bool SortDescending { get; set; }

        public int PageIndex { get; set; }

        public int PageSize { get; set; }

        public int TotalItemsCount { get; set; }

        public IList<T> Items { get; set; }

        public IList<int> NearPageIndexes
        {
            get { return Enumerable.Range(0, PagesCount).Where(n => Math.Abs(n - PageIndex) <= 5).ToList(); }
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

        IList IGridViewDataSet.Items
        {
            get { return (IList)Items; }
        }

        public GridViewDataSet()
        {
            Items = new List<T>();
        }


        public void GoToFirstPage()
        {
            PageIndex = 0;
        }

        public void GoToPreviousPage()
        {
            if (!IsFirstPage)
            {
                PageIndex--;
            }
        }

        public void GoToLastPage()
        {
            PageIndex = PagesCount - 1;
        }

        public void GoToNextPage()
        {
            if (!IsLastPage)
            {
                PageIndex++;
            }
        }

        public void GoToPage(int index)
        {
            PageIndex = index;
        }

        public void Reset()
        {
            PageIndex = 0;
        }

        public void SetSortExpression(string expression)
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


        public void LoadFromQueryable(IQueryable<T> queryable)
        {
            TotalItemsCount = queryable.Count();

            if (!string.IsNullOrEmpty(SortExpression))
            {
                queryable = ApplySortExpression(queryable);
            }

            if (PageSize > 0)
            {
                queryable = queryable.Skip(PageSize * PageIndex).Take(PageSize);
            }

            Items = queryable.ToList();
        }


        public IQueryable<T> ApplySortExpression(IQueryable<T> queryable)
        {
            var type = typeof(T);
            var property = type.GetProperty(SortExpression);
            var parameter = Expression.Parameter(type, "p");
            var propertyAccess = Expression.MakeMemberAccess(parameter, property);
            var orderBy = Expression.Lambda(propertyAccess, parameter);

            var result = Expression.Call(
                typeof(Queryable), 
                SortDescending ? "OrderByDescending" : "OrderBy", 
                new[] { type, property.PropertyType }, 
                queryable.Expression, 
                Expression.Quote(orderBy));

            return queryable.Provider.CreateQuery<T>(result);
        }
    }
}
