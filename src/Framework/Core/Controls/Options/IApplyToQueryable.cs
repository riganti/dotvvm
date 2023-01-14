using System.Linq;

namespace DotVVM.Framework.Controls
{
    public interface IApplyToQueryable
    {
        IQueryable<T> ApplyToQueryable<T>(IQueryable<T> queryable);
    }
}
