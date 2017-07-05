using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.ControlSamples.IncludeInPageProperty
{
    public class IncludeInPageViewModel : DotvvmViewModelBase
    {
        public bool IncludeInPage { get; set; } = true;

        public string Text { get; set; }

        public List<string> Texts { get; set; } =
            new List<string> { "Test 1", "Test 2", "Test 3" };
    }
}

