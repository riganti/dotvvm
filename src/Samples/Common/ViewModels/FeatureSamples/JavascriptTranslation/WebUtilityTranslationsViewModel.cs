using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.JavascriptTranslation
{
    public class WebUtilityTranslationsViewModel : DotvvmViewModelBase
    {
        public string InputString { get; set; } = "Encoding test \"&?<>";
    }
}

