using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DotVVM.Samples.BasicSamples.ViewModels
{
    public class Sample50ViewModel
    {

        public Sample50Child Child { get; set; }

        public Sample50ViewModel()
        {
            Child = new Sample50Child()
            {
                Child2 = new Sample50Child2()
                {
                    Child3 = new Sample50Child3()
                    {
                        Test = "Hello"
                    }
                }
            };
        }

    }

    public class Sample50Child
    {

        public Sample50Child2 Child2 { get; set; }

    }

    public class Sample50Child2
    {

        public Sample50Child3 Child3 { get; set; }


    }

    public class Sample50Child3
    {

        public string Test { get; set; }

    }
}