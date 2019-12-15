namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Represents settings for sorting.
    /// </summary>
    public interface ISortingOptions
    {
        /// <summary>
        /// Gets or sets whether the sort order should be descending.
        /// </summary>
        bool SortDescending { get; set; }

        /// <summary>
        /// Gets or sets the name of the property that is used for sorting.
        /// </summary>
        string SortExpression { get; set; }
    }
}