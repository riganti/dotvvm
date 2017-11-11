using System;
using System.Collections.Generic;
using System.Globalization; 
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotVVM.Framework.Controls;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.DoublePostBackPrevention
{
    public class DoublePostBackPreventionViewModel : DotvvmViewModelBase
    {

        public int CurrentIndex { get; set; }

        public string LastAction { get; set; }

        [FromQuery("concurrency")]
        [Bind(Direction.None)]
        public PostbackConcurrencyMode ConcurrencyMode { get; set; }


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