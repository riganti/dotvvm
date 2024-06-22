using System.Linq;

namespace DotVVM.Framework.Controls
{
    public static class PagingImplementation
    {

        /// <summary>
        /// Applies paging to the <paramref name="queryable" /> after the total number
        /// of items is retrieved.
        /// </summary>
        /// <param name="queryable">The <see cref="IQueryable{T}" /> to modify.</param>
        public static IQueryable<T> ApplyPagingToQueryable<T, TPagingOptions>(IQueryable<T> queryable, TPagingOptions options)
            where TPagingOptions : IPagingPageSizeCapability, IPagingPageIndexCapability
        {
            return options.PageSize > 0
                ? queryable.Skip(options.PageSize * options.PageIndex).Take(options.PageSize)
                : queryable;
        }
    }

}
