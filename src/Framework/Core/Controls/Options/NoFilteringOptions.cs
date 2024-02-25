using System.Linq;

namespace DotVVM.Framework.Controls
{
    public class NoFilteringOptions : IFilteringOptions, IApplyToQueryable
    {
        public IQueryable<T> ApplyToQueryable<T>(IQueryable<T> queryable) => queryable;
    }
}
