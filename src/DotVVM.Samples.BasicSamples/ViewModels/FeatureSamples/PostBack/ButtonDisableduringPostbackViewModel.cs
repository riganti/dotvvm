using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;
using System.Threading;

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.PostBack
{
	public class ButtonDisableduringPostbackViewModel : DotvvmViewModelBase
	{
        public void Postback()
        {
            Thread.Sleep(1000);
            PostbackCounter++;
        }

        public int PostbackCounter { get; set; }
    }
}

