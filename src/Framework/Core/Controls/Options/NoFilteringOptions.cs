using System.Linq;

namespace DotVVM.Framework.Controls
{
    /// <summary> Dataset with NoFilteringOptions does not support filtering. </summary>
    public sealed class NoFilteringOptions : IFilteringOptions, IApplyToQueryable
    {
        public IQueryable<T> ApplyToQueryable<T>(IQueryable<T> queryable) => queryable;

        public static NoFilteringOptions Instance { get; } = new();
    }
}
