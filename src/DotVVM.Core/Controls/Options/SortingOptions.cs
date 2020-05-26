using DotVVM.Framework.ViewModel;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Represents settings for sorting.
    /// </summary>
    public class SortingOptions : ISortingOptions
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
        /// Gets or sets a custom sorting strategy
        /// </summary>
        [Bind(Direction.None)]
        public ISortingStrategy? SortingStrategy { get; set; } = new SortingStrategy();
    }
}
