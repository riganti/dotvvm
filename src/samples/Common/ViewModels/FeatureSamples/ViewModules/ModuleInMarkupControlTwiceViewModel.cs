using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.ViewModules
{
    public class ModuleInMarkupControlTwiceViewModel : DotvvmViewModelBase
    {

        public ViewModulesViewModel Page { get; set; } = new ViewModulesViewModel();

        public ViewModulesViewModel Page2 { get; set; }


        public void ToggleSecond()
        {
            Page2 = Page2 == null ? new ViewModulesViewModel() : null;
        }
    }
}

