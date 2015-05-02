using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Redwood.Framework.ViewModel;

namespace Redwood.Samples.BasicSamples.ViewModels
{
    public class Sample8ViewModel: RedwoodViewModelBase
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