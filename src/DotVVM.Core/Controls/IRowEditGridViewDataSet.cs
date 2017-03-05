namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Extends the base GridViewDataSet with row edit functionality.
    /// </summary>
    public interface IRowEditGridViewDataSet : IBaseGridViewDataSet
    {
        /// <summary>
        /// Gets or sets an object that represents the settings for row edits.
        /// </summary>
        IRowEditOptions RowEditOptions { get; set; }
    }
}