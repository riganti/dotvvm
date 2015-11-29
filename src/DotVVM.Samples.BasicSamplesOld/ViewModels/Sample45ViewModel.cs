using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels
{
    public class Sample45ViewModel : DotvvmViewModelBase
    {
        public int Num { get; set; }

        public void DoNothing()
        {
            
        }
    }
}