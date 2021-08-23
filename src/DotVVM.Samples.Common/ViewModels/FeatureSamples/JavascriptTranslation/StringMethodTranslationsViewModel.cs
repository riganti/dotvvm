using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.JavascriptTranslation
{
    public class StringMethodTranslationsViewModel : DotvvmViewModelBase
    {
        public string Joke { get; set; } = "Why do Java programmers have to wear glasses? Because they do not C#.";

        public string OperationResult { get; set; }
        public string Value { get; set; } = "";

        public string[] SplitArray { get; set; }
        public int Index { get; set; }

        public List<string> JoinList { get; set; } = new List<string> { "Real", "programmers", "count", "from", "0" };

        public string[] JoinArray { get; set; }  = { "Real", "programmers", "count", "from", "0" };
    }
}

