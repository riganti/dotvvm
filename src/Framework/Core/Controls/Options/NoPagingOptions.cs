using System.Linq;

namespace DotVVM.Framework.Controls
{
    /// <summary> Dataset with NoPagingOptions does not support paging. </summary>
    public sealed class NoPagingOptions : IPagingOptions, IApplyToQueryable
    {
        public IQueryable<T> ApplyToQueryable<T>(IQueryable<T> queryable) => queryable;

        public static NoPagingOptions Instance { get; } = new();
    }
}
