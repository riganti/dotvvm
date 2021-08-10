using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.MarkupControl
{
    public class CommandBindingInRepeater : DotvvmViewModelBase
    {
        public string Title { get; set; }

        public List<string> TestCollection { get; set; } = new List<string>() { "Item 1", "Item 2", "Item 3" };

        public CommandBindingInRepeater()
        {
            Title = "Hello from DotVVM!";
        }

        public void Action1(string value)
        {
            Title = "Action1 - " + value;
        }

        public void Action2(string value)
        {
            Title = "Action2 - " + value;
        }

    }
}
