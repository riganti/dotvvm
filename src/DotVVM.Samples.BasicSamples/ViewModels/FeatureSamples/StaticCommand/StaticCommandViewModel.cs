using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.Binding;

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.StaticCommand
{
    public class StaticCommandViewModel : DotvvmViewModelBase
    {
        public string Name { get; set; } = "Deep Thought";
        public string Greeting { get; set; }

        [StaticCommandCallable]
        public static string GetGreeting(string name)
        {
            return "Hello " + name + "!";
        }
    }
}
