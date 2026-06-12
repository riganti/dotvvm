using System;

namespace DotVVM.Framework.Controls
{
    [Obsolete("Use IGridViewDataSet<T> instead.", true)]
    public interface IBaseGridViewDataSet<T> { }

    [Obsolete("Use IGridViewDataSet instead.", true)]
    public interface IBaseGridViewDataSet { }

    [Obsolete("Use IGridViewDataSet instead.", true)]
    public interface IRefreshableGridViewDataSet { }

    [Obsolete("Use IGridViewDataSet or IPageableGridViewDataSet<PagingOptions> instead.", true)]
    public interface IPageableGridViewDataSet { }

    [Obsolete("Use IGridViewDataSet or ISortableGridViewDataSet<SortingOptions> instead.", true)]
    public interface ISortableGridViewDataSet { }

    [Obsolete("Use IGridViewDataSet or IRowEditGridViewDataSet<RowEditOptions> instead.", true)]
    public interface IRowEditGridViewDataSet { }

    [Obsolete("Use IGridViewDataSet or IRowInsertGridViewDataSet<RowEditOptions> instead.", true)]
    public interface IRowInsertGridViewDataSet { }
}
