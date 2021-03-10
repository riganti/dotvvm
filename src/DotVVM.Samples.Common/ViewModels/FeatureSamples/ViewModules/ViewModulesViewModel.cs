using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.ViewModules
{
    public class ViewModulesViewModel : DotvvmViewModelBase
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

