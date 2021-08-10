namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Extends the <see cref="IBaseGridViewDataSet" /> with row edit functionality.
    /// </summary>
    public interface IRowEditGridViewDataSet : IBaseGridViewDataSet
    {
        /// <summary>
        /// Gets the settings for row (item) edit feature.
        /// </summary>
        IRowEditOptions RowEditOptions { get; }
    }
}