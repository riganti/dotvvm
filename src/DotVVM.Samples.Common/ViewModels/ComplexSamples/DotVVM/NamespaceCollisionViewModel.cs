using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.ComplexSamples.DotVVM
{
    public class NamespaceCollisionViewModel : DotvvmViewModelBase
    {
        public string Test { get; set; } = "Hello from DotVVM!";
    }
}
