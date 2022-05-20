using System.Collections.Generic;

namespace DotVVM.AutoUI.ViewModel;

public interface ISelectorViewModel<TItem>
    where TItem : Annotations.Selection
{
    List<TItem>? Items { get; }
}
