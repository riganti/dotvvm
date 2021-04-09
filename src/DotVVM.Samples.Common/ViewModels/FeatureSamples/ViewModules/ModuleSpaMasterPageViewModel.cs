using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.ViewModules
{
    public class ModuleSpaMasterPageViewModel : DotvvmViewModelBase
    {

        public ViewModulesViewModel Page { get; set; } = new ViewModulesViewModel();
    }
}

