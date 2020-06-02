﻿namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Strategy for customized sorting
    /// </summary>
    public interface ISortingStrategy
    {
        /// <summary>
        /// Applies custom properties before sorting
        /// </summary>
        /// <param name="sortableDataSet">DataSet to sort</param>
        /// <param name="expression">Expression to sort by. When null, no sorting should be performed.</param>
        void Apply(ISortableGridViewDataSet sortableDataSet, string? expression);
    }
}
