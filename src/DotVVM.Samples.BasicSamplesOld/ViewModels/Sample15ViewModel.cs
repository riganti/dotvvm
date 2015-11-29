using System;
using System.Collections.Generic;
using System.Globalization; 
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels
{
    public class Sample15ViewModel : DotvvmViewModelBase
    {

        public int CurrentIndex { get; set; }

        public string LastAction { get; set; }


        public void LongAction()
        {
            Thread.Sleep(5000);
            CurrentIndex++;
            LastAction = "long";
        }

        public void ShortAction()
        {
            CurrentIndex++;
            LastAction = "short";
        }
        

    }
}