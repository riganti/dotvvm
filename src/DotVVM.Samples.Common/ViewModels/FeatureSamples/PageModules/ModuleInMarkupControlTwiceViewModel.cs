using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.PageModules
{
    public class ModuleInMarkupControlTwiceViewModel : DotvvmViewModelBase
    {

        public PageModulesViewModel Page { get; set; } = new PageModulesViewModel();

        public PageModulesViewModel Page2 { get; set; }


        public void ToggleSecond()
        {
            Page2 = Page2 == null ? new PageModulesViewModel() : null;
        }
    }
}

