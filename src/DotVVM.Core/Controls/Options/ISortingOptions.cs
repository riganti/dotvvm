using System.Linq;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Represents a settings for sorting.
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

        /// <summary>
        /// Applies the paging settings to the IQueryable object.
        /// </summary>
        IQueryable<T> ApplyToQueryable<T>(IQueryable<T> queryable);
    }
}