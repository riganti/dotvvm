using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DotVVM.Framework.Controls.Options;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Represents a default implementation of the sorting options.
    /// </summary>
    public class SortingOptions : ISortingOptions, ISortingStateCapability, ISortingSetSortExpressionCapability, IApplyToQueryable
    {
        /// <summary>
        /// Gets or sets whether the sort order should be descending.
        /// </summary>
        public bool SortDescending { get; set; }

        /// <summary>
        /// Gets or sets the name of the property that is used for sorting. Null means the grid should not be sorted.
        /// </summary>
        public string? SortExpression { get; set; }

        /// <summary>
        /// Determines whether the specified property can be used for sorting.
        /// </summary>
        public virtual bool IsSortingAllowed(string sortExpression) => true;

        /// <summary>
        /// Modifies the options to be sorted by the specified expression.
        /// </summary>
        public virtual void SetSortExpression(string? sortExpression)
        {
            if (sortExpression is {} && !IsSortingAllowed(sortExpression))
                throw new ArgumentException($"Sorting by column '{sortExpression}' is not allowed.");

            if (sortExpression == null)
            {
                SortExpression = null;
                SortDescending = false;
            }
            else if (sortExpression == SortExpression)
            {
                SortDescending = !SortDescending;
            }
            else
            {
                SortExpression = sortExpression;
                SortDescending = false;
            }
        }

        /// <summary>
        /// Applies the sorting options to the specified IQueryable expression.
        /// </summary>
        public virtual IQueryable<T> ApplyToQueryable<T>(IQueryable<T> queryable)
        {
            return SortingImplementation.ApplySortingToQueryable(queryable, SortExpression, SortDescending);
        }

        /// <inheritdoc />
        public bool IsColumnSortedAscending(string? sortExpression) => SortExpression == sortExpression && !SortDescending;

        /// <inheritdoc />
        public bool IsColumnSortedDescending(string? sortExpression) => SortExpression == sortExpression && SortDescending;
    }
}
