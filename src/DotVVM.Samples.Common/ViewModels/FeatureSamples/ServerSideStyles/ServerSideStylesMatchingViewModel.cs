using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.ServerSideStyles
{
	public class ServerSideStylesMatchingViewModel : DotvvmViewModelBase
	{
        public TestingObject Object { get; set; } = new TestingObject();

        public class TestingObject
        {
            public string Pangram { get; set; } = "Sphinx of black quartz, judge my vow.";
        }
	}
}

