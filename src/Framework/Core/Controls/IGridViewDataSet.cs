using System.Collections;
using System.Collections.Generic;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Represents a collection of items with paging, sorting and row edit capabilities.
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="Items" /> elements.</typeparam>
    public interface IGridViewDataSet<T> : IGridViewDataSet
    {
        /// <summary>
        /// Gets the items for the current page.
        /// </summary>
        new IList<T> Items { get; set; }
    }

    /// <summary>
    /// Represents a collection of items with paging, sorting and row edit capabilities.
    /// </summary>
    public interface IGridViewDataSet
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
        /// Gets whether the data should be refreshed.
        /// This property is set to true automatically by GridView / DataPager component when options are changed.
        /// </summary>
        bool IsRefreshRequired { get; set; }

        /// <summary>
        /// Sets the refresh flag to true, if the dataset supports refresh functionality.
        /// </summary>
#if NET8_0_OR_GREATER
        void RequestRefresh()
        {
            IsRefreshRequired = true;
        }
#else
        void RequestRefresh();
#endif
    }
}
