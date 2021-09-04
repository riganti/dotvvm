using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.JavascriptTranslation
{
    public class DictionaryIndexerTranslationViewModel : DotvvmViewModelBase
    {
        public string Key { get; set; }
        public string Value { get; set; }

        public Dictionary<string, string> Dictionary { get; set; } = new Dictionary<string, string>()
        {
            { "key1", "value1" },
            { "key2", "value2" }
        };
    }
}

