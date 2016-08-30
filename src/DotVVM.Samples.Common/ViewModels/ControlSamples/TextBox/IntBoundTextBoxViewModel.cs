using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.ControlSamples.TextBox
{
    public class IntBoundTextBoxViewModel : DotvvmViewModelBase
    {
        public int Num { get; set; }

        public void DoNothing()
        {
            
        }
    }
}