using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.ConditionalCssClasses
{
    public class ConditionalCssClassViewModel : DotvvmViewModelBase
    {
        public bool Italic { get; set; } = false;
        public bool Blue { get; set; } = false;
        public bool Bordered { get; set; } = false;

        public void SwitchItalic()
        {
            Italic = !Italic;
        }

        public void SwitchBlue()
        {
            Blue = !Blue;
        }

        public void SwitchBordered()
        {
            Bordered = !Bordered;
        }
    }
}
