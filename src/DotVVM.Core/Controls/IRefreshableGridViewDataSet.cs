using System;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Extends the IBaseGridViewDataSet with refresh functionality.
    /// </summary>
    public interface IRefreshableGridViewDataSet : IBaseGridViewDataSet
    {

        /// <summary>
        /// Called when the GridViewDataSet should be refreshed (on initial page load and when paging or sort options change).
        /// </summary>
        /// <remarks>
        /// Either <see cref="OnLoadingData"/> or <see cref="OnLoadingDataAsync"/> can be set but not both.
        /// </remarks>
        [Bind(Direction.None)]
        [Obsolete("Use IRefreshableGridViewDataSet.OnLoadingDataAsync instead.This property will be removed in future versions.")]
        GridViewDataSetLoadDelegate OnLoadingData { get; }

        /// <summary>
        /// Called when the GridViewDataSet should be refreshed (on initial page load and when paging or sort options change).
        /// </summary>
        /// <remarks>
        /// Either <see cref="OnLoadingData"/> or <see cref="OnLoadingDataAsync"/> can be set but not both.
        /// </remarks>
        [Bind(Direction.None)]
        GridViewDataSetLoadAsyncDelegate OnLoadingDataAsync { get; }

        /// <summary>
        /// Requests to refresh the GridViewDataSet.
        /// </summary>
        [Obsolete("Use IRefreshableGridViewDataSet.RequestRefreshAsync instead.This property will be removed in future versions.")]
        void RequestRefresh(bool forceRefresh = false);

        /// <summary>
        /// Requests to refresh the GridViewDataSet.
        /// </summary>
        Task RequestRefreshAsync(bool forceRefresh = false);

        /// <summary>
        /// Gets or sets whether the GridViewDataSet should be refreshed. This property is set to true automatically when paging or sort options change.
        /// </summary>
        bool IsRefreshRequired { get; }

    }
}