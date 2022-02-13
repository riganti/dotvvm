using System.Collections.Generic;

namespace DotVVM.Framework.Controls.DynamicData.ViewModel;

public interface ISelectorViewModel<TItem>
    where TItem : Annotations.SelectorItem
{
    List<TItem> Items { get; }
}