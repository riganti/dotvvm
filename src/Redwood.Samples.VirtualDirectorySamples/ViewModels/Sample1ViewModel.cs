using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Redwood.Samples.VirtualDirectorySamples.ViewModels
{
    public class Sample1ViewModel : MasterViewModel
    {

        public int Num1 { get; set; }

        public int Num2 { get; set; }

        public int Result { get; set; }


        public void Calculate()
        {
            Result = Num1 + Num2;
        }

    }
}