using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.PageModules
{
    public class PageModulesViewModel : DotvvmViewModelBase
    {

        public int Value { get; set; }

        public string Result { get; set; }

        public TestObject ChildObject { get; set; } = new TestObject() { Test = "Hello" };
        
    }
    public class TestObject
    {
        public string Test { get; set; }
    }
}

