using System.Collections.Generic;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.LateContentPlaceHolders
{
    public class LateContentPlaceHolderViewModel : DotvvmViewModelBase
    {
    }

    /// <summary>Viewmodel for Repeater tests - items list is set per subclass.</summary>
    public abstract class RepeaterContentPlaceHolderViewModel : DotvvmViewModelBase
    {
        public List<string> Items { get; set; } = new List<string>();
    }

    public class RepeaterOneItemViewModel : RepeaterContentPlaceHolderViewModel
    {
        public RepeaterOneItemViewModel() { Items = new List<string> { "Item 1" }; }
    }

    public class RepeaterZeroItemsViewModel : RepeaterContentPlaceHolderViewModel
    {
        public RepeaterZeroItemsViewModel() { Items = new List<string>(); }
    }

    public class RepeaterMultipleItemsViewModel : RepeaterContentPlaceHolderViewModel
    {
        public RepeaterMultipleItemsViewModel() { Items = new List<string> { "Item 1", "Item 2" }; }
    }
}
