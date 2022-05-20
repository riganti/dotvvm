using System.Collections.Generic;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.AutoUI.Annotations;

public interface ISelectionProvider<TItem>
{
    [AllowStaticCommand]
    Task<List<TItem>> GetSelectorItems();
}

public interface ISelectionProvider<TItem, TParam>
{
    [AllowStaticCommand]
    Task<List<TItem>> GetSelectorItems(TParam parameter);
}
