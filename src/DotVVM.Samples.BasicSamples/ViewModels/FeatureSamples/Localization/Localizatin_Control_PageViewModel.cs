using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.Controls;

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.Localization
{
    public class Localizatin_Control : DotvvmMarkupControl
    {
    }

    public class Localizatin_ControlViewModel : DotvvmViewModelBase
    {
        public bool Checked { get; set; }
    }

    public class Localizatin_Control_PageViewModel : DotvvmViewModelBase
	{
        public Localizatin_ControlViewModel Control { get; set; } = new Localizatin_ControlViewModel();
    }
}

