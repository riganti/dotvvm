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
        public Dictionary<string, bool> Rules { get; set; } = new Dictionary<string, bool>()
        {
            { "key1", true },
            { "key2", false }
        };

        public List<string> List { get; set; } = new List<string>()
        {
            "Val1", "Val2"
        };
    }
}

