using System;
using System.Collections.Generic;
using System.Globalization; 
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Redwood.Framework.ViewModel;

namespace Redwood.Samples.BasicSamples.ViewModels
{
    public class Sample15ViewModel : RedwoodViewModelBase
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