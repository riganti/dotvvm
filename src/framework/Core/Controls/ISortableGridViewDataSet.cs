namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Extends the base GridViewDataSet with sorting functionality.
    /// </summary>
    public interface ISortableGridViewDataSet : IBaseGridViewDataSet
    {
        /// <summary>
        /// Gets the settings for sorting.
        /// </summary>
        ISortingOptions SortingOptions { get; }

        /// <summary>
        /// Sets the sort expression. If the specified expression is already set, switches the sort direction.
        /// </summary>
        void SetSortExpression(string expression);
    }
}