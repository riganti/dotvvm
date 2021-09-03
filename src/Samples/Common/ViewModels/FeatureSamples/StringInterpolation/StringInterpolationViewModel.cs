using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.StringInterpolation
{
    public class StringInterpolationViewModel : DotvvmViewModelBase
    {
        public string Name { get; set; } = "Mark";
        public string Name2 { get; set; } = "John";
        public int Age { get; set; } = 24;
        public int IntNumber { get; set; } = -1508;
        public double DoubleNumber { get; set; } = 15.0896;
        public DateTime Date { get; set; } = new DateTime(2016, 7, 15, 3, 15, 0);
    }
}

