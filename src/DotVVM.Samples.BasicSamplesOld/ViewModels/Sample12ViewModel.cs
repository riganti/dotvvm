using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels 
{
    public class Sample12ViewModel : DotvvmViewModelBase 
    {
        
        public int Value { get; set; }

        public string Result { get; set; }



        public void Apply()
        {
            Result = string.Join("<br />", Enumerable.Range(0, Value).Select(i => "Lorem ipsum"));
        }
    }

}