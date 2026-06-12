namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Extends the <see cref="IGridViewDataSet" /> with specific implementation of row edit functionality.
    /// </summary>
    public interface IRowEditGridViewDataSet<out TRowEditOptions> : IGridViewDataSet
        where TRowEditOptions : IRowEditOptions
    {
        /// <summary>
        /// Gets the settings for row (item) edit feature.
        /// </summary>
        new TRowEditOptions RowEditOptions { get; }
    }
}
