using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.Attribute
{
    public class ToStringConversionViewModel : DotvvmViewModelBase
    {

        public double NumberValue { get; set; } = 45.3;

        public bool IsOpen { get; set; }
    }
}

