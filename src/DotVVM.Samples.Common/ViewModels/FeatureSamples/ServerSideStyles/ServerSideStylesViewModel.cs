using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.ServerSideStyles
{
    public class ServerSideStylesViewModel : DotvvmViewModelBase
    {
        public string CustomPropertyText { get; set; } = "Default value";

        public object Object { get; set; } = new TestingObject();

        public class TestingObject
        {
            public string Pangram { get; set; } = "Pack my box with five dozen liquor jugs.";
        }
    }
}

