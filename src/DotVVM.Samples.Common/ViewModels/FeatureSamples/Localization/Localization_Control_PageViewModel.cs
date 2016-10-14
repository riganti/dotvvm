using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.Controls;

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.Localization
{
    public class Localization_Control : DotvvmMarkupControl
    {
    }

    public class Localization_ControlViewModel : DotvvmViewModelBase
    { 
        public bool Checked { get; set; }
    }

    public class Localization_Control_PageViewModel : DotvvmViewModelBase
	{
        public Localization_ControlViewModel Control { get; set; } = new Localization_ControlViewModel();
    }
}

