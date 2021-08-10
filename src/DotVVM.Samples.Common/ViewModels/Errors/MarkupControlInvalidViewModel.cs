using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;
using DotVVM.Samples.BasicSamples.TestNamespace3;

namespace DotVVM.Samples.BasicSamples.ViewModels.Errors
{
    public class MarkupControlInvalidViewModel : DotvvmViewModelBase
    {
        public TestModel TestModel { get; set; } = new TestModel();
        public WrongTestModel WrongTestModel { get; set; } = new WrongTestModel();
    }
}
