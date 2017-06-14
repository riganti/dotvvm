using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.CommandArguments
{
    public class CommandArgumentsViewModel : DotvvmViewModelBase
    {
        public string Value { get; set; } = "Nothing here";

        public void Command(string arg)
        {
            Value = arg;
        }
    }
}

