using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.MarkupControl
{
	public class ControlCommandBindingViewViewModel : DotvvmViewModelBase
	{
        public ControlCommandBindingDTO Data { get; set; }

        public override Task Load()
        {
            if (!Context.IsPostBack)
            {
                Data = new ControlCommandBindingDTO();
            }
            return base.Load();
        }
    }
}

