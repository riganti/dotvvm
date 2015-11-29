using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels
{
    public class Sample8ViewModel: DotvvmViewModelBase
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