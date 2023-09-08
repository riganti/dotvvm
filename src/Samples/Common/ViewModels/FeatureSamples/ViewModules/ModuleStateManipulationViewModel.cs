using System.Collections.Generic;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.ViewModules
{
    public class ModuleStateManipulationViewModel : DotvvmViewModelBase
    {
        public List<Item> Items { get; set; } = new () { new Item() { Value = 0, Label = "Item1" }, new Item() { Value = 0, Label = "Item2" } };

        public int IntProperty { get; set; } = 0;

        public class Item
        {
            public int Value { get; set; }
            public string Label  { get; set; }
        }
    }
}

