using System.Linq;

namespace DotVVM.Framework.Controls
{
    /// <summary> Dataset with NoSortingOptions does not support sorting. </summary>
    public sealed class NoSortingOptions : ISortingOptions, IApplyToQueryable
    {
        public IQueryable<T> ApplyToQueryable<T>(IQueryable<T> queryable) => queryable;

        public static NoSortingOptions Instance { get; } = new();
    }
}
