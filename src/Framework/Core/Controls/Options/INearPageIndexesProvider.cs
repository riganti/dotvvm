using System;
using System.Collections.Generic;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Provides a list of page indexes near current page. It can be used to build data pagers.
    /// </summary>
    [Obsolete("This interface was removed. If you want to provide your custom page numbers, inherit the PagingOptions class and override the GetNearPageIndexes method.", true)]
    public interface INearPageIndexesProvider
    {
        /// <summary>
        /// Gets a list of page indexes near current page.
        /// </summary>
        /// <param name="pagingOptions">The settings for paging.</param>
        IList<int> GetIndexes(IPagingOptions pagingOptions);
    }
}
