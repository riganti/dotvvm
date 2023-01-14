using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Represents a collection of items with paging, sorting and row edit capabilities.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the collection.</typeparam>
    public class GridViewDataSet<T>
        : GenericGridViewDataSet<T, NoFilteringOptions, SortingOptions, PagingOptions, NoRowInsertOptions, RowEditOptions>
    {
        public GridViewDataSet()
            : base(new NoFilteringOptions(), new SortingOptions(), new PagingOptions(), new NoRowInsertOptions(), new RowEditOptions())
        {
        }
    }
}
