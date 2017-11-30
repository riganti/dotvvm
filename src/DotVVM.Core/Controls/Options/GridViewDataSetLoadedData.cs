using System.Collections.Generic;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Represents data to be loaded into <see cref="GridViewDataSet{T}" />.
    /// </summary>
    /// <typeparam name="T">The type of data to be loaded.</typeparam>
    public class GridViewDataSetLoadedData<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GridViewDataSetLoadedData{T}"/> class.
        /// </summary>
        /// <param name="items">The data with paging, sorting and other options applied.</param>
        /// <param name="totalItemsCount">The total number of items in the data store without respect to paging.</param>
        public GridViewDataSetLoadedData(IEnumerable<T> items, int totalItemsCount)
        {
            Items = items;
            TotalItemsCount = totalItemsCount;
        }

        /// <summary>
        /// Gets or sets the loaded data with paging, sorting and other options applied.
        /// </summary>
        public IEnumerable<T> Items { get; }

        /// <summary>
        /// Gets the total number of items in the data store without respect to paging.
        /// </summary>
        public int TotalItemsCount { get; }
    }
}