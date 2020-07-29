using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;
using DotVVM.Samples.BasicSamples.Controls;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.CommandArguments
{
    public class CommandArgumentsViewModel : DotvvmViewModelBase
    {
        public string Value { get; set; } = "Nothing here";

        [Bind(Direction.ServerToClientFirstRequest)]
        public ButtonParameter Parameter { get; set; } = new ButtonParameter {
            MyProperty = "Sample text"
        };

        public void Command(string arg)
        {
            Value = arg;
        }
    }
}

