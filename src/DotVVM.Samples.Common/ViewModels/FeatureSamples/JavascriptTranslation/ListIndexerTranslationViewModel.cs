using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.JavascriptTranslation
{
    public class ListIndexerTranslationViewModel : DotvvmViewModelBase
    {
        public List<string> List { get; set; } = new List<string>() { "value1", "value2", "value3" };

        public int Index { get; set; }
        public string Value { get; set; }


        public void NAME()
        {
            var a = List.Take(5);

        }
    }
}

