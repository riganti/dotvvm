namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Represents a marker interface for sorting options.
    /// </summary>
    /// <seealso cref="SortingOptions" />
    /// <seealso cref="MultiCriteriaSortingOptions" />
    /// <seealso cref="NoSortingOptions" />
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
}
