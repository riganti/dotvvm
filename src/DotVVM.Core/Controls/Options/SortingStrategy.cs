﻿using System;
using System.Collections.Generic;
using System.Text;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Customizable sorting strategy. Uses default if no strategy provided.
    /// </summary>
    public class SortingStrategy : ISortingStrategy
    {
        public Action<ISortableGridViewDataSet, string?> Strategy { get; set; } = GetDefaultSortingStrategy();

        static Action<ISortableGridViewDataSet, string?> GetDefaultSortingStrategy()
        {
            return (sortableSet, expr) => {
                var options = sortableSet.SortingOptions;

                if (options.SortExpression == expr)
                {
                    options.SortDescending ^= true;
                }
                else
                {
                    options.SortExpression = expr;
                    options.SortDescending = false;
                }

                (sortableSet as IPageableGridViewDataSet)?.GoToFirstPage();
            };
        }

        /// <summary>
        /// Applies custom properties before sorting
        /// </summary>
        /// <param name="sortableDataSet">DataSet to sort</param>
        /// <param name="expression">Expression to sort by. When null, no sorting should be performed.</param>
        public void Apply(ISortableGridViewDataSet sortableDataSet, string? expression)
        {
            Strategy(sortableDataSet, expression);
        }
    }
}
