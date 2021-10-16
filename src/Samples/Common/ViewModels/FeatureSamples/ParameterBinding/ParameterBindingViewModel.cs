using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.ParameterBinding
{
    public class ParameterBindingViewModel : DotvvmViewModelBase
    {
        [FromRoute("A")]
        public int A { get; set; }

        [FromQuery("B")]
        public string B { get; set; }

        public NestedViewModel Nested { get; set; } = new NestedViewModel();

        public class NestedViewModel : DotvvmViewModelBase
        {
            [FromRoute("A")]
            public int A { get; set; }

            [FromQuery("B")]
            public string B { get; set; }
        }
    }
}
