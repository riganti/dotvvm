namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Extends the <see cref="IBaseGridViewDataSet{T}" /> with row insert functionality.
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="IBaseGridViewDataSet{T}.Items" /> elements.</typeparam>
    public interface IRowInsertGridViewDataSet<T> : IBaseGridViewDataSet<T>
        where T : class, new()
    {
        /// <summary>
        /// Gets the settings for row (item) insert feature.
        /// </summary>
        IRowInsertOptions<T> RowInsertOptions { get; }
    }
}
