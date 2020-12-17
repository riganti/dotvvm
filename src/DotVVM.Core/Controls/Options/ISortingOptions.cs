using System;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Represents settings for sorting.
    /// </summary>
    [Obsolete("Please use the IDataSetSorter or the DefaultGridSorter in the GridViewDataSet.Sorter property")]
    public interface ISortingOptions
    {
        /// <summary>
        /// Gets or sets whether the sort order should be descending.
        /// </summary>
        [Obsolete("Please use the IDataSetSorter or the DefaultGridSorter in the GridViewDataSet.Sorter property")]
        bool SortDescending { get; set; }

        /// <summary>
        /// Gets or sets the name of the property that is used for sorting. Null means the grid should not be sorted.
        /// </summary>
        [Obsolete("Please use the IDataSetSorter or the DefaultGridSorter in the GridViewDataSet.Sorter property")]
        string? SortExpression { get; set; }
    }
}
