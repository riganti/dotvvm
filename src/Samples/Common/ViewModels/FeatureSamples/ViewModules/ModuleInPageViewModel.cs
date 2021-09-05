using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.ViewModules
{
    public class ModuleInPageViewModel : DotvvmViewModelBase
    {

        public ViewModulesViewModel Page { get; set; } = new ViewModulesViewModel();

    }
}

