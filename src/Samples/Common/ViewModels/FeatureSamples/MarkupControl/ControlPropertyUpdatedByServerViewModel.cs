using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.MarkupControl
{
	public class ControlPropertyUpdatedByServerViewModel : DotvvmViewModelBase
	{
        public string SimpleProperty { get; set; }

        public ControlUpdatingPropertyChildViewModel ChildViewModel { get; set; }

        public void AddViewModel()
        {
            ChildViewModel = new ControlUpdatingPropertyChildViewModel() { Property = "TEST" };
        }
    }

    public class ControlUpdatingPropertyChildViewModel
    {
        public string Property { get; set; }
    }
}

