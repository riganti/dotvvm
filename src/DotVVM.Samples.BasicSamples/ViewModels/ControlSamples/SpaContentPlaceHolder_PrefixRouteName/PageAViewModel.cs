using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.ControlSamples.SpaContentPlaceHolder_PrefixRouteName
{
	public class PageAViewModel : SpaMasterViewModel
	{

        public PageAViewModel()
        {
            HeaderText = "Sample 1";
        }

        public int Value { get; set; }

        public void IncreaseValue()
        {
            Value++;
        }


    }
}

