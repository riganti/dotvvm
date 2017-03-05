namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Extends the base GridViewDataSet with sorting functionality.
    /// </summary>
    public interface ISortableGridViewDataSet : IBaseGridViewDataSet
    {
        /// <summary>
        /// Gets or sets an object that represents the settings for sorting.
        /// </summary>
        ISortOptions SortOptions { get; set; }

        /// <summary>
        /// Sets the sort expression. If the specified expression is already active, switches the sort direction.
        /// </summary>
        void SetSortExpression(string expression);
    }
}