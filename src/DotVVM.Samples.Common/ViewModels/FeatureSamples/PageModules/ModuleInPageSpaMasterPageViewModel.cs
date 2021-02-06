using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.PageModules
{
    public class ModuleInPageSpaMasterPageViewModel : ModuleSpaMasterPageViewModel
    {
        public PageModulesViewModel Page2 { get; set; } = new PageModulesViewModel();

    }
}

