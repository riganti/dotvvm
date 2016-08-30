using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.ControlSamples.TextBox
{
	public class TextBox_FormatDoublePropertyViewModel : DotvvmViewModelBase
	{
        public double DoubleProperty { get; set; }
        public void SetDoubleProperty() { DoubleProperty = 10.5; }
    }
}
