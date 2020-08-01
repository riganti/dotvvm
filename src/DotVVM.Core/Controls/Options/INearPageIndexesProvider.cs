using System.Collections.Generic;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Provides a list of page indexes near current page. It can be used to build data pagers.
    /// </summary>
    public interface INearPageIndexesProvider<T, in TPager>
        where TPager: IDataSetPager<T>
    {
        /// <summary>
        /// Gets a list of page indexes near current page.
        /// </summary>
        /// <param name="pagingOptions">The settings for paging.</param>
        IList<int> GetIndexes(TPager pagingOptions);
    }
}
