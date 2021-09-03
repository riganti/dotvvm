using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.BindableCssStyles
{
    public class BindableCssStylesViewModel : DotvvmViewModelBase
    {
        public int Width { get; set; } = 50;
        public int FontSize { get; set; } = 14;
        public string Color { get; set; } = "red";
        public bool Condition { get; set; }
    }
}
