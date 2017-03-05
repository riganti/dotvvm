using System;
using System.Collections;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Framework.Controls
{
    public delegate GridViewDataSetLoadedData RequestRefresh(IGridViewDataSetLoadOptions gridViewDataSetLoadOptions);

    /// <summary>
    /// Represents a common core functionality for GridViewDataSets.
    /// </summary>
    public interface IBaseGridViewDataSet
    {
        /// <summary>
        /// Called when the GridViewDataSet should be refreshed (on initial page load and when paging or sort options change).
        /// </summary>
        [Bind(Direction.None)]
        RequestRefresh RequestRefresh { get; }

        /// <summary>
        /// Requests to refresh the GridViewDataSet.
        /// </summary>
        void ReloadData();

        /// <summary>
        /// Gets or sets the items for the current page.
        /// </summary>
        IList Items { get;}
    }
}