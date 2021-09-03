using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.ControlSamples.TextBox
{
    public class SelectAllOnFocusViewModel : DotvvmViewModelBase
    {
        public bool SelectAllOnFocus { get; set; }

        public string Text { get; set; } = "Testing text";
    }
}
