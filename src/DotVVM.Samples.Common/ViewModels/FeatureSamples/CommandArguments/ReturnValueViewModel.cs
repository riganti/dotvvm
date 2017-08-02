using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.CommandArguments
{
    public class ReturnValueViewModel : DotvvmViewModelBase
    {
        public int Counter { get; set; }

        public string RefreshText()
        {
            return "Text: " + Counter++;
        }

        [AllowStaticCommand]
        public static string JustGetTheText(int counter) =>  "Text: " + counter;
    }
}

