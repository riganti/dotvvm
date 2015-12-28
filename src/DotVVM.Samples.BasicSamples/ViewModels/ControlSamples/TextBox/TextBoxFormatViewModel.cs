using System;

namespace DotVVM.Samples.BasicSamples.ViewModels.ControlSamples.TextBox
{
    public class TextBoxFormatViewModel : QueryStringLocalizableViewModel
    {

        public DateTime DateValue { get; set; } = DateTime.Parse("2015-12-27T00:00:00.0000000");
        public double NumberValue { get; set; } = 123.123456789;
        public string CurrentCulture => Context.GetCurrentUICulture().Name;


    }
}