namespace DotVVM.Framework.Controls
{

    /// <summary>
    /// Extends the <see cref="IGridViewDataSet" /> with specific implementation of row insert functionality.
    /// </summary>
    public interface IRowInsertGridViewDataSet<out TRowInsertOptions> : IGridViewDataSet
        where TRowInsertOptions : IRowInsertOptions
    {

        /// <summary>
        /// Gets the settings for row (item) insert feature.
        /// </summary>
        new TRowInsertOptions RowInsertOptions { get; }
    }
}
