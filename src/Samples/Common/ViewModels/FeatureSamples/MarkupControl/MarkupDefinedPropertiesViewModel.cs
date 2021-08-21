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
        public IEnumerable<string> Buildings { get; set; } = new[] { "Barn", "House", "Skyscraper", "Town Hall" };
    }
}

