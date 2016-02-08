using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.ControlSamples.UpdateProgress
{
    public class UpdateProgressViewModel : DotvvmViewModelBase
    {

        public void Test()
        {
            Thread.Sleep(2000);
        }

        public void TestFileDownload()
        {
            Context.Redirect("/test.zip");
        }

    }
}