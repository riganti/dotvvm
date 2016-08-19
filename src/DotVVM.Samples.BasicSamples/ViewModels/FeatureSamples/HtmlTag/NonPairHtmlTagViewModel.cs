using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;


namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.HtmlTag
{
    public class NonPairHtmlTagViewModel : DotvvmViewModelBase
    {
        public Child1 Child1 { get; set; }

        public NonPairHtmlTagViewModel()
        {
            Child1 = new Child1()
            {
                Child2 = new Child2()
                {
                    Child3 = new Child3()
                    {
                        Test = "Hello"
                    }
                }
            };
        }
    }

    public class Child1
    {

        public Child2 Child2 { get; set; }

    }

    public class Child2
    {

        public Child3 Child3 { get; set; }


    }

    public class Child3
    {

        public string Test { get; set; }

    }
}
