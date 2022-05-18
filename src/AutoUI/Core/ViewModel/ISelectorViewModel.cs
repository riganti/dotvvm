using System.Collections.Generic;

namespace DotVVM.AutoUI.ViewModel;

public interface ISelectorViewModel<TItem>
    where TItem : Annotations.SelectorItem
{
    List<TItem>? Items { get; }
}
