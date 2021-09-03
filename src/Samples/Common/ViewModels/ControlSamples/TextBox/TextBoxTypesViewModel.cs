using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.ControlSamples.TextBox
{
    public class TextBoxTypesViewModel : DotvvmViewModelBase
    {
        public DateTime Date { get; set; } = new DateTime(2017, 1, 1, 8, 8, 0);
        public DateTime? NullableDate { get; set; } = new DateTime(2017, 1, 1, 20, 10, 0);
        public double Number { get; set; } = 42.42;
        public double? NullableNumber { get; set; } = 42.42;
        public int Int { get; set; }
        public int? NullableInt { get; set; }
    }
}

