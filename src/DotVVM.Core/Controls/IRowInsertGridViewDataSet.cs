namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Extends the base GridViewDataSet with row insert functionality.
    /// </summary>
    public interface IRowInsertGridViewDataSet<T> : IRowInsertGridViewDataSet
        where T : new()
    {
        /// <summary>
        /// Gets or sets the settings for row insert feature.
        /// </summary>
        new IRowInsertOptions<T> RowInsertOptions { get; }
    }

    /// <summary>
    /// Extends the base GridViewDataSet with row insert functionality.
    /// </summary>
    public interface IRowInsertGridViewDataSet : IBaseGridViewDataSet
    {
        /// <summary>
        /// Gets or sets the settings for row insert feature.
        /// </summary>
        IRowInsertOptions RowInsertOptions { get; }
    }
}