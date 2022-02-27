using System.Collections.Generic;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Framework.Controls.DynamicData.Annotations;

public interface ISelectorDataProvider<TItem>
{
    [AllowStaticCommand]
    Task<List<TItem>> GetSelectorItems();
}

public interface ISelectorDataProvider<TItem, TParam>
{
    [AllowStaticCommand]
    Task<List<TItem>> GetSelectorItems(TParam parameter);
}
