using System;
using System.Collections;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Framework.Controls
{
    public delegate GridViewDataSetLoadedData GridViewDataSetLoadDelegate(IGridViewDataSetLoadOptions gridViewDataSetLoadOptions);

    /// <summary>
    /// Represents a common core functionality for GridViewDataSets.
    /// </summary>
    public interface IBaseGridViewDataSet
    {
        /// <summary>
        /// Called when the GridViewDataSet should be refreshed (on initial page load and when paging or sort options change).
        /// </summary>
        [Bind(Direction.None)]
        GridViewDataSetLoadDelegate OnLoadingData { get; }

        /// <summary>
        /// Requests to refresh the GridViewDataSet.
        /// </summary>
        void RequestRefresh(bool forceRefresh = false);

        /// <summary>
        /// Gets or sets whether the GridViewDataSet should be refreshed. This property is set to true automatically when paging or sort options change.
        /// </summary>
        bool IsRefreshRequired { get; }

        /// <summary>
        /// Gets or sets the items for the current page.
        /// </summary>
        IList Items { get;}
    }
}