using System;
using DotVVM.Framework.Query;

namespace DotVVM.Framework.Controls
{
    public class NopDataSetSorter<T> : IDataSetSorter<T>
    {
        public IQuery<T> Apply(IQuery<T> query) => query;
        public void ColumnSortClick(IBaseGridViewDataSet<T> dataSet, string? columnName) { }
    }

    public class NopDataSetPager<T> : IDataSetIndexPager<T>
    {
        int IDataSetIndexPager<T>.PageIndex => 0;

        int IDataSetIndexPager<T>.PagesCount => 1;

        public IQuery<T> Apply(IQuery<T> query) => query;
        public void GoToPage(int index)
        {
            if (index != 0) throw new Exception("Only Page 0 exists in NopDataSetPager.");
        }
    }
    public class NopDataSetFilter<T> : IDataSetFilter<T>
    {
        public IQuery<T> Apply(IQuery<T> query) => query;
    }
}
