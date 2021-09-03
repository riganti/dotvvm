using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.ComplexSamples.ClassBindings
{
    public class ClassBindingsViewModel : DotvvmViewModelBase
    {
        public string Classes { get; set; }

        public bool IsBorderEnabled { get; set; }

        public bool IsInvertedEnabled { get; set; }
    }
}

