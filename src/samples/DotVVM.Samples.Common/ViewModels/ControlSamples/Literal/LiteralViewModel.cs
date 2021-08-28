using System;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.ControlSamples.Literal
{
    public class LiteralViewModel : QueryStringLocalizableViewModel
    {
        public string Text => "Hello";

        public string Html => "Hello <b>value</b>";

        public DateTime CurrentDate => new DateTime(2000, 1, 1);
        public string CorrectCurrentDateFormatString_d => $"{CurrentDate:d}";
        public string CorrectCurrentDateFormatString_D => $"{CurrentDate:D}";
    }
}