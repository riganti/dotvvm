namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Represents a marker interface for sorting options.
    /// </summary>
    public interface ISortingOptions
    {
    }

    /// <summary>
    /// Represents sorting options that support changing the sort expression by the user.
    /// </summary>
    public interface ISortingSetSortExpressionCapability : ISortingOptions
    {
        /// <summary>
        /// Determines whether the specified property can be used for sorting.
        /// </summary>
        bool IsSortingAllowed(string sortExpression);

        /// <summary>
        /// Modifies the options to be sorted by the specified expression.
        /// </summary>
        void SetSortExpression(string? sortExpression);
    }

    /// <summary>
    /// Represents sorting options that can specify one sorting criterion.
    /// </summary>
    public interface ISortingStateCapability : ISortingOptions
    {
        /// <summary>
        /// Determines whether the column with specified sort expression is sorted in ascending order.
        /// </summary>
        bool IsColumnSortedAscending(string? sortExpression);

        /// <summary>
        /// Determines whether the column with specified sort expression is sorted in descending order.
        /// </summary>
        bool IsColumnSortedDescending(string? sortExpression);

    }

    /// <summary>
    /// Represents a sort criterion.
    /// </summary>
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
