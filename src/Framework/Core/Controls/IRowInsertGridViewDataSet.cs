namespace DotVVM.Framework.Controls
{

    /// <summary>
    /// Extends the <see cref="IBaseGridViewDataSet" /> with row insert functionality.
    /// </summary>
    public interface IRowInsertGridViewDataSet<out TRowInsertOptions> : IRowInsertGridViewDataSet
        where TRowInsertOptions : IRowInsertOptions
    {

        /// <summary>
        /// Gets the settings for row (item) insert feature.
        /// </summary>
        new TRowInsertOptions RowInsertOptions { get; }
    }

    public interface IRowInsertGridViewDataSet : IBaseGridViewDataSet
    {
        /// <summary>
        /// Gets the settings for row (item) insert feature.
        /// </summary>
        IRowInsertOptions RowInsertOptions { get; }
    }
}
