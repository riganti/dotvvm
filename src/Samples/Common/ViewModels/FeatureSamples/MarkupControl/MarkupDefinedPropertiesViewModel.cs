using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.MarkupControl
{
    public class MarkupDefinedPropertiesViewModel : DotvvmViewModelBase
    {
        public IList<string> Buildings { get; set; } = new List<string> { "Barn", "House", "Skyscraper", "Town Hall" };

        public int Counter { get; set; }

        public string Name { get; set; }
        public void AddBuilding()
        {
            Buildings.Add(Name);
        }
    }
}

