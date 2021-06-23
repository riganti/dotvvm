using System;
using System.Threading;
using DotVVM.Framework.Hosting;

namespace DotVVM.Samples.BasicSamples.ViewModels.ControlSamples.TextBox
{
    public class TextBoxFormatViewModel : QueryStringLocalizableViewModel
    {
        public TextBoxFormatViewModel()
        {
            BindingNumberValueNString = 0.ToString("N", Thread.CurrentThread.CurrentCulture);
        }
        public DateTime DateValue { get; set; } = DateTime.Parse("2015-12-27T00:00:00.0000000");
        public DateTime? NullableDateValue { get; set; } = DateTime.Parse("2015-12-27T00:00:00.0000000");
        public double NumberValue { get; set; } = 123.123456789;
        public double? NullableNumberValue { get; set; } = 123.123456789;
        public double BigNumberValue { get; set; } = 12356789.987654;
        public string CurrentCulture => Context.GetCurrentUICulture().Name;
        public string DateResult1 => $"{DateValue:d}";
        public string DateResult2 => $"{DateTime.Parse("2018-12-27T00:00:00.0000000"):d}";
        public string DateResult3 => $"{DateTime.Parse("2018-01-01T00:00:00.0000000"):d}";

        public double BindingNumberValue { get; set; }
        public string BindingNumberValueNString { get; set; }
        public double ResultNumberValue { get; set; }

        public void ChangedNumberValue()
        {
            ResultNumberValue = BindingNumberValue;
            BindingNumberValueNString = BindingNumberValue.ToString("N", Thread.CurrentThread.CurrentCulture);
        }
    }
}
