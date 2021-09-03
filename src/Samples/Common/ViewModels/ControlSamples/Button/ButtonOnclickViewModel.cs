using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.ControlSamples.Button
{
	public class ButtonOnclickViewModel : DotvvmViewModelBase
	{
        public string Result { get; set; }

	    public ButtonOnclickViewModel()
	    {
	        Result = "";
	    }

	    public void ChangeResult()
	    {
	        Result = "Changed from command binding";
	    }
	}
}

