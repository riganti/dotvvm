using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.StaticCommand
{
    public class StaticCommand_NullBindingViewModel : DotvvmViewModelBase
    {
        public class ComplexObject
        {
            public string Greeting { get; set; }
        }

        public List<ComplexObject> ListComplexObject { get; set; } = new List<ComplexObject>()
        {
            new ComplexObject() { Greeting = "Hello 1" },
            new ComplexObject() { Greeting = "Hello 2" },
            new ComplexObject() { Greeting = "Hello 3" },
            null
        };

        public ComplexObject SelectedComplex { get; set; }
    }
}

