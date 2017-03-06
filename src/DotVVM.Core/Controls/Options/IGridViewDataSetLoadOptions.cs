using System.Linq;

namespace DotVVM.Framework.Controls
{
    public interface IGridViewDataSetLoadOptions
    {
        IPagingOptions PagingOptions { get; set; }

        ISortingOptions SortingOptions { get; set; }

        GridViewDataSetLoadedData GetDataFromQueryable(IQueryable queryable);
    }
}