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
        public List<string> TestCollecion { get; set; } = new List<string>() { "ADRH" };

        public CommandBindingInRepeater()
        {
            Title = "Hello from DotVVM!";
        }

        public void Action1()
        {
            Title = "Action1";
        }

        public void Action2()
        {
            Title = "Action2";
        }

    }
}
