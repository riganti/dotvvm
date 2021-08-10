using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.RenderSettingsModeServer
{
    public class RepeaterCollectionExchangeViewModel : DotvvmViewModelBase
    {
        public bool UseNull { get; set; }
        public bool UseAlternativeCollection { get; set; }
        public List<string> Collection1 { get; set; } = new List<string> { "standard item 1", "standard item 2" };
        public List<string> Collection2 { get; set; } = new List<string> { "alternative item 1", "alternative item 2" };

        public string SelectedValue { get; set; }
    }
}

