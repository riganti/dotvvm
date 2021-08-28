using System;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.ControlSamples.HtmlLiteral
{
    public class HtmlLiteralViewModel : DotvvmViewModelBase
    {

        public string Html => "Hello <b>value</b>";
        
    }
}