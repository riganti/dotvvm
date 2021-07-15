using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.JavascriptTranslation
{
    public class ArrayTranslationViewModel : DotvvmViewModelBase
    {
        public string[] Array { get; set; } = new string[] { "value1", "value2", "value3" };

        public int Index { get; set; }
        public string Value { get; set; }
    }
}

