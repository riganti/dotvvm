using System.Collections.Generic;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Represents settings for sorting.
    /// </summary>
    public interface ISortingOptions
    {
    }

    public interface ISortingSetSortExpressionCapability : ISortingOptions
    {
        bool IsSortingAllowed(string sortExpression);
        void SetSortExpression(string? sortExpression);
    }

    public interface ISortingSingleCriterionCapability : ISortingOptions
    {

        /// <summary>
        /// Gets or sets whether the sort order should be descending.
        /// </summary>
        bool SortDescending { get; }

        /// <summary>
        /// Gets or sets the name of the property that is used for sorting. Null means the grid should not be sorted.
        /// </summary>
        string? SortExpression { get; }

    }

    public interface ISortingMultipleCriteriaCapability : ISortingOptions
    {
        IList<SortCriterion> Criteria { get; }
    }

    public sealed record SortCriterion
    {
        /// <summary>
        /// Gets or sets whether the sort order should be descending.
        /// </summary>
        public bool SortDescending { get; set; }

        /// <summary>
        /// Gets or sets the name of the property that is used for sorting. Null means the grid should not be sorted.
        /// </summary>
        public string? SortExpression { get; set; }
    }
}
