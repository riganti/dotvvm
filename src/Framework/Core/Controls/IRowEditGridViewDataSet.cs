namespace DotVVM.Framework.Controls
{
    public interface IRowEditGridViewDataSet<out TRowEditOptions> : IRowEditGridViewDataSet
        where TRowEditOptions : IRowEditOptions
    {
        /// <summary>
        /// Gets the settings for row (item) edit feature.
        /// </summary>
        new TRowEditOptions RowEditOptions { get; }
    }

    public interface IRowEditGridViewDataSet : IBaseGridViewDataSet
    {
        /// <summary>
        /// Gets the settings for row (item) edit feature.
        /// </summary>
        IRowEditOptions RowEditOptions { get; }
    }
}
