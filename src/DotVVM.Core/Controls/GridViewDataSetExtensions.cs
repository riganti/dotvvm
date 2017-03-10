using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;


namespace DotVVM.Framework.Controls
{
    public static class GridViewDataSetExtensions
    {
        public static GridViewDataSetLoadedData<T> GetDataFromQueryable<T>(this IQueryable<T> queryable, IGridViewDataSetLoadOptions options)
        {
            return (GridViewDataSetLoadedData<T>)options.GetDataFromQueryable(queryable);
        }
    }
}