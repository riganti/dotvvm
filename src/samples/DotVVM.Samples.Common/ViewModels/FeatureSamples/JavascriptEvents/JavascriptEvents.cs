using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.JavascriptEvents
{
    public class JavascriptEventsViewModel: DotvvmViewModelBase
    {

        public void Test()
        {

        }

        public void TestError()
        {
            throw new Exception("This is an error!");
        }

    }
}