using System;
using System.Globalization;
using DotVVM.Framework.Hosting;

namespace DotVVM.Samples.BasicSamples.ViewModels.ControlSamples.TextBox
{
    public class TextBoxFormatViewModel : QueryStringLocalizableViewModel
    {

        public DateTime DateValue { get; set; } = DateTime.Parse("2015-12-27T00:00:00.0000000");
        public double NumberValue { get; set; } = 123.123456789;
        public double BigNumberValue { get; set; } = 12356789.987654;
        public string CurrentCulture => CultureInfo.CurrentUICulture.Name;
        public string DateResult1 => $"{DateValue:d}";
        public string DateResult2 => $"{DateTime.Parse("2018-12-27T00:00:00.0000000"):d}";
        public string DateResult3 => $"{DateTime.Parse("2018-01-01T00:00:00.0000000"):d}";



    }
}