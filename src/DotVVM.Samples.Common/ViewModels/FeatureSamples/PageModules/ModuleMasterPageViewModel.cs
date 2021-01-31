using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.PageModules
{
    public class ModuleMasterPageViewModel : DotvvmViewModelBase
    {

        public PageModulesViewModel Page { get; set; } = new PageModulesViewModel();

    }
}

