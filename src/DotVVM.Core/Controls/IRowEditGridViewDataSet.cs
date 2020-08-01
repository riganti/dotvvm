namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Extends the <see cref="IBaseGridViewDataSet{T}" /> with row edit functionality.
    /// </summary>
    public interface IRowEditGridViewDataSet<T> : IBaseGridViewDataSet<T>
    {
        /// <summary>
        /// Gets the settings for row (item) edit feature.
        /// </summary>
        IRowEditOptions RowEditOptions { get; }
    }
}
