namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Extends the <see cref="IBaseGridViewDataSet" /> with row insert functionality.
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="IBaseGridViewDataSet{T}.Items" /> elements.</typeparam>
    public interface IRowInsertGridViewDataSet<T> : IRowInsertGridViewDataSet, IBaseGridViewDataSet<T>
        where T : new()
    {
        /// <summary>
        /// Gets the settings for row (item) insert feature.
        /// </summary>
        new IRowInsertOptions<T> RowInsertOptions { get; }
    }

    /// <summary>
    /// Extends the <see cref="IBaseGridViewDataSet" /> with row insert functionality.
    /// </summary>
    public interface IRowInsertGridViewDataSet : IBaseGridViewDataSet
    {
        /// <summary>
        /// Gets the settings for row (item) insert feature.
        /// </summary>
        IRowInsertOptions RowInsertOptions { get; }
    }
}