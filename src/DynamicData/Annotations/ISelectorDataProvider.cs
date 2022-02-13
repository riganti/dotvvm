using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotVVM.Framework.Controls.DynamicData.Annotations;

public interface ISelectorDataProvider<TItem>
{
    Task<List<TItem>> GetItems();
}

public interface ISelectorDataProvider<TItem, TParam>
{
    Task<List<TItem>> GetItems(TParam parameter);
}
