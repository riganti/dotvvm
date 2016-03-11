using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.ChildViewModelInvokeMethods
{
    public class ChildViewModelInvokeMethodsViewModel : DotvvmViewModelBase
    {
        public ChildViewModel ChildViewModel { get; set; } = new ChildViewModel();
        public NastyChildViewModel NastyChildViewModel { get; set; } = new NastyChildViewModel();
    }
}