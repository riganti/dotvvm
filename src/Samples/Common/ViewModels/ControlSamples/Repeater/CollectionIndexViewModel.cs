using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.ControlSamples.Repeater
{
    public class CollectionIndexViewModel : DotvvmViewModelBase
    {
        public List<int> Collection { get; set; } = Enumerable.Range(0, 4).ToList();

        public int Counter { get; set; }
    }
}
