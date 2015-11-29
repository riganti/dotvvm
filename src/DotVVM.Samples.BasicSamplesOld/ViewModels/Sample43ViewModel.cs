using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels
{
    public class Sample43ViewModel : DotvvmViewModelBase
    {
        public int Sum { get; set; } = 0;
        [RegularExpression(@"^\d+$")]
        public int Number { get; set; }

        public void Add()
        {
            Sum += Number;
        }
        
    }
}