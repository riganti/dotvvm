using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.CompositeControls
{
    public class ControlPropertyNamingConflictViewModel : DotvvmViewModelBase
    {
        public string Hello { get; set; } = "<ul><li>Hello</li><li>there</li></ul>";
    }
}

