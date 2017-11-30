using System.Linq;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Represents options required to reload data in <see cref="GridViewDataSet{T}" />.
    /// </summary>
    /// <typeparam name="T">The type of data to be loaded.</typeparam>
    public interface IGridViewDataSetLoadOptions<T>
    {
        /// <summary>
        /// Loads data from the <paramref name="source" /> queryable.
        /// </summary>
        /// <param name="source">The source to load data from.</param>
        GridViewDataSetLoadedData<T> LoadDataFromQueryable(IQueryable<T> source);
    }
}