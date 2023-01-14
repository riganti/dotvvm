namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Extends the <see cref="IBaseGridViewDataSet" /> with refresh functionality.
    /// </summary>
    public interface IRefreshableGridViewDataSet : IBaseGridViewDataSet
    {
        /// <summary>
        /// Gets whether the data should be refreshed. This property is set to true automatically
        /// when paging, sorting or other options change.
        /// </summary>
        bool IsRefreshRequired { get; set; }

        /// <summary>
        /// Requests to reload data into the <see cref="IRefreshableGridViewDataSet" />.
        /// </summary>
        void RequestRefresh();
    }
}
