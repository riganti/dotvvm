using System.Collections;
using System.Collections.Generic;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Defines the basic contract for DataSets.
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="Items" /> elements.</typeparam>
    public interface IBaseGridViewDataSet<T> : IBaseGridViewDataSet
    {
        /// <summary>
        /// Gets the items for the current page.
        /// </summary>
        new IList<T> Items { get; set; }
    }

    /// <summary>
    /// Defines the basic contract for DataSets.
    /// </summary>
    public interface IBaseGridViewDataSet
    {
        /// <summary>
        /// Gets the items for the current page.
        /// </summary>
        IList Items { get; }

        /// <summary>
        /// Gets the settings for filtering.
        /// </summary>
        /// <remarks> Return the <see cref="NoFilteringOptions"/> instance to disable this feature.</remarks>
        IFilteringOptions FilteringOptions
#if NET8_0_OR_GREATER
            => NoFilteringOptions.Instance;
#else
            { get; }
#endif

        /// <summary>
        /// Gets the settings for sorting.
        /// </summary>
        /// <remarks> Return the <see cref="NoSortingOptions"/> instance to disable this feature.</remarks>
        ISortingOptions SortingOptions
#if NET8_0_OR_GREATER
            => NoSortingOptions.Instance;
#else
            { get; }
#endif

        /// <summary>
        /// Gets the settings for paging.
        /// </summary>
        /// <remarks> Return the <see cref="NoPagingOptions"/> instance to disable this feature.</remarks>
        IPagingOptions PagingOptions
#if NET8_0_OR_GREATER
            => NoPagingOptions.Instance;
#else
            { get; }
#endif

        /// <summary>
        /// Gets the settings for row (item) insert feature.
        /// </summary>
        /// <remarks> Return the <see cref="NoRowInsertOptions"/> instance to disable this feature.</remarks>
        IRowInsertOptions RowInsertOptions
#if NET8_0_OR_GREATER
            => NoRowInsertOptions.Instance;
#else
            { get; }
#endif

        /// <summary>
        /// Gets the settings for row (item) edit feature.
        /// </summary>
        /// <remarks> Return the <see cref="NoRowEditOptions"/> instance to disable this feature.</remarks>
        IRowEditOptions RowEditOptions
#if NET8_0_OR_GREATER
            => NoRowEditOptions.Instance;
#else
            { get; }
#endif

        /// <summary>
        /// Sets the refresh flag to true, if the data set supports it.
        /// </summary>
#if NET8_0_OR_GREATER
        void RequestRefresh()
        {
            if (this is IRefreshableGridViewDataSet refreshable)
                refreshable.IsRefreshRequired = true;
        }
#else
        void RequestRefresh();
#endif
    }
}
