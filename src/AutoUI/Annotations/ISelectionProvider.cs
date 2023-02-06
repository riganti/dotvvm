using System.Collections.Generic;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.AutoUI.Annotations;

/// <summary> The service providing <see cref="Selection{TKey}" /> items. Automatically used from the SelectionViewModel, unless Items are set explicitly. </summary>
public interface ISelectionProvider<TItem>
{
    [AllowStaticCommand]
    Task<List<TItem>> GetSelectorItems();
}

/// <summary> The service providing <see cref="Selection{TKey}" /> items. Automatically used from the SelectionViewModel, unless Items are set explicitly. </summary>
public interface ISelectionProvider<TItem, TParam>
{
    [AllowStaticCommand]
    Task<List<TItem>> GetSelectorItems(TParam parameter);
}
