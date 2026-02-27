namespace DotVVM.Framework.Controls
{
    public interface IRowEditGridViewDataSet<out TRowEditOptions> : IBaseGridViewDataSet
        where TRowEditOptions : IRowEditOptions
    {
        /// <summary>
        /// Gets the settings for row (item) edit feature.
        /// </summary>
        new TRowEditOptions RowEditOptions { get; }
    }
}
